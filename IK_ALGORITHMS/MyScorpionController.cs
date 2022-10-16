using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
  
    public class MyScorpionController
    {
        //TAIL
        Transform tailTarget;
        Transform tailEndEffector;
        MyTentacleController _tail;
        float animationRange;

        //LEGS
        Transform[] legTargets;
        Transform[] legFutureBases;
        MyTentacleController[] _legs = new MyTentacleController[6];

        float legDistanceToFutureBase;

        bool[] interpolatingLeg;


        #region public
        public void InitLegs(Transform[] LegRoots,Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            //Legs init
            for(int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                //TODO: initialize anything needed for the FABRIK implementation
            }

            legTargets = LegTargets;
            legFutureBases = LegFutureBases;

            legDistanceToFutureBase = Vector3.Distance(_legs[0].Bones[0].position, legFutureBases[0].position)*1.8f;
            interpolatingLeg = new bool[_legs.Length];

        }

        Vector3[] StartOffset;
        Vector3[] Axis;

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            //TODO: Initialize anything needed for the Gradient Descent implementation



            Solution = new float[_tail.Bones.Length];
            for (int i = 0; i < Solution.Length; i++)
            {
                Solution[i] = 0f;
            }

            StartOffset = new Vector3[_tail.Bones.Length];
            for (int i = 0; i < StartOffset.Length; i++)
            {
                StartOffset[i] = _tail.Bones[i].localPosition;
            }

            // Queremos que el primer joint rote en y, el resto en x
            // ergo el primer joint tiene axis vector up y el otro right
            Axis = new Vector3[_tail.Bones.Length];
            Axis[0] = new Vector3(0f, 1f, 0f);
            for (int i = 1; i < Axis.Length; i++)
            {
                Axis[i] = new Vector3(1f,0f,0f);
            }
        }

        //TODO: Check when to start the animation towards target and implement Gradient Descent method to move the joints.
        public void NotifyTailTarget(Transform target)
        {
            tailTarget = target;
        }

        bool firstWalkDone;
        //TODO: Notifies the start of the walking animation
        public void NotifyStartWalk()
        {

            firstWalkDone = true;
        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()
        {
            updateTail();
            updateLegs();
        }
        #endregion


        #region private
        //TODO: Implement the leg base animations and logic
        private void updateLegPos()
        {
            //check for the distance to the futureBase, then if it's too far away start moving the leg towards the future base position
            //
        }
        //TODO: implement Gradient Descent method to move tail if necessary
        float[] Solution;
        bool firstTailUpdateDone;
        private void updateTail()
        {

            Vector3 targetPos = tailTarget.position;

            const float delta = 0.001f;
            const float L = 0.6f;
            const int iterations = 100;
            // En el primer frame hacemos más iteraciones para evitar movimientos poco naturales.
            // a partir de entonces nos aprovechamos de las iteraciones como transicion.
            for (int iteration = 0; iteration < (firstTailUpdateDone ? iterations: 5000); iteration++)
            {
                for (int i = 0; i < Solution.Length; i++)
                {
                    float gradientDecent = CalculateGradient(targetPos, Solution, i, delta);

                    Solution[i] = Solution[i] - L * gradientDecent;
                }
            }

            ApplyForwardKinematics(Solution);
            firstTailUpdateDone = true;
        }



        public void ApplyForwardKinematics(float[] Solution)
        {
            Vector3 prevPoint = _tail.Bones[0].transform.position;

            // Takes object initial rotation into account
            Quaternion rotation = _tail.Bones[0].rotation;


            for (int i = 1; i < _tail.Bones.Length; i++)
            {
                // Rotates around a new axis
                rotation *= Quaternion.AngleAxis(Solution[i - 1], Axis[i - 1]);
                Vector3 nextPoint = prevPoint + rotation * StartOffset[i];

                prevPoint = nextPoint;

                _tail.Bones[i - 1].transform.rotation = rotation;

            }


        }

        public float DistanceFromTarget(Vector3 target, float[] Solution)
        {
            Vector3 point = ForwardKinematics(Solution);
            Debug.DrawLine(point, Vector3.zero,Color.red);
            return Vector3.Distance(point, target);
        }

        public PositionRotation ForwardKinematics(float[] Solution)
        {
            Vector3 prevPoint = _tail.Bones[0].transform.position;

            // Takes object initial rotation into account
            Quaternion rotation = _tail.Bones[0].rotation;

            float scorpionScale = _tail.Bones[0].parent.parent.parent.localScale.x;


            for (int i = 1; i < _tail.Bones.Length; i++)
            {
                // Rotates around a new axis
                rotation *= Quaternion.AngleAxis(Solution[i - 1], Axis[i - 1]);
                Vector3 nextPoint = prevPoint + rotation * StartOffset[i] * scorpionScale;

                Debug.DrawLine(_tail.Bones[i].transform.position, _tail.Bones[i - 1].transform.position, Color.white);
                Debug.DrawLine(prevPoint, nextPoint, Color.blue);

                prevPoint = nextPoint;
            }


            // The end of the effector
            return new PositionRotation(prevPoint, rotation);
        }


        public struct PositionRotation
        {
            Vector3 position;
            Quaternion rotation;

            public PositionRotation(Vector3 position, Quaternion rotation)
            {
                this.position = position;
                this.rotation = rotation;
            }

            // PositionRotation to Vector3
            public static implicit operator Vector3(PositionRotation pr)
            {
                return pr.position;
            }
            // PositionRotation to Quaternion
            public static implicit operator Quaternion(PositionRotation pr)
            {
                return pr.rotation;
            }
        }


        public float CalculateGradient(Vector3 target, float[] Solution, int i, float delta)
        {
            float f2 = DistanceFromTarget(target, Solution);
          
            Solution[i] += delta;
            float f1 = DistanceFromTarget(target, Solution);
          

            float gradient = (f1 - f2) / delta;

            return gradient;
        }






        //TODO: implement fabrik method to move legs 
        private void updateLegs()
        {
            for (int i = 0; i < _legs.Length; i++)
            {
                updateLeg(_legs[i], legTargets[i], legFutureBases[i], i);
                DrawFABRIKConstrins(_legs[i]);
                ApplyFABRIKConstrins(_legs[i]);
            }

            for (int i = 0; i < legTargets.Length; i++)
            {
                Debug.DrawLine(legTargets[i].position, legFutureBases[i].position);
            }
        }

        const float fabrikAngleConstrainDegree = 60f;

        void ApplyFABRIKConstrins(MyTentacleController leg)
        {
            var joints = leg.Bones;

            for (int i = 1; i < joints.Length - 1; i++)
            {
                var coneDir = (joints[i].position - joints[i - 1].position).normalized;
                var currentDir = (joints[i+1].position - joints[i].position).normalized;

                Vector3 cross = Vector3.Cross(coneDir, currentDir);
                float dot = Vector3.Dot(coneDir, currentDir);
                float angleDif = Mathf.Atan2(cross.magnitude, dot);
                angleDif *= Mathf.Rad2Deg;

                if(angleDif > fabrikAngleConstrainDegree)
                {
                    Vector3 correctionAxis = cross.normalized;
                    Quaternion correctionRotation = Quaternion.AngleAxis(fabrikAngleConstrainDegree - angleDif, correctionAxis);
                  

                    Vector3 correctedVecotr = correctionRotation * currentDir;
                    joints[i].rotation = correctionRotation * joints[i].rotation;
                }
                

            }
        }

        private void DrawFABRIKConstrins(MyTentacleController leg)
        {

            var joints = leg.Bones;

            for (int i = 1; i < joints.Length-1; i++)
            {
                const float coneDrawSize = 0.2f;
                var coneDir = (joints[i].position - joints[i - 1].position).normalized;
                var coneRight = new Vector3(coneDir.z, coneDir.y, -coneDir.x);
                var coneLeft = coneRight * -1f;

                // Draw cone Axis
                Debug.DrawLine(joints[i].position, joints[i].position + coneDir * coneDrawSize, Color.red);
                Debug.DrawLine(joints[i].position, joints[i].position + coneRight * coneDrawSize, Color.green);
                Debug.DrawLine(joints[i].position, joints[i].position + coneLeft * coneDrawSize, Color.blue);


                //CONE GENERATION
                var cross = Vector3.Cross(coneDir, coneRight);
                var axis = cross.normalized;


                Vector3 someConeEdge = Quaternion.AngleAxis(fabrikAngleConstrainDegree, coneRight) * coneDir;


                float increment = 1f;

                Quaternion rotation = Quaternion.AngleAxis(increment, coneDir);

                Color drawColor = new Color(Color.magenta.r, Color.magenta.g, Color.magenta.b, 0.3f);


                for (float dg = increment; dg < 360f; dg+= increment)
                {
                    Debug.DrawLine(joints[i].position, joints[i].position + someConeEdge.normalized * coneDrawSize, drawColor);
                    someConeEdge = rotation * someConeEdge;
                }
                

            }

        }


        Vector3[] positions;
        float[] desiredDistances;
        float totalDist;
        const float epsilon = 0.02f;

        void CalculatePositionsUnrechableTarget(Vector3 targetPos)
        {
            // The target is unreachable
            var toTarget = targetPos - positions[0];
            toTarget.Normalize();

            for (int i = 1; i < positions.Length - 1; i++)
            {
                positions[i] = positions[i - 1] + toTarget * desiredDistances[i];
            }
        }


        void CopyJointsPositions(Vector3 targetPos, Transform[] joints)
        {
            positions = new Vector3[joints.Length + 2];
            desiredDistances = new float[joints.Length + 2];

            // set origin
            positions[0] = joints[0].position;
            desiredDistances[0] = 0f;

            // set target
            desiredDistances[desiredDistances.Length - 1] = 0f;
            positions[positions.Length - 1] = targetPos;

            // set mid 
            for (int i = 1; i < positions.Length - 1; i++)
            {
                positions[i] = joints[i - 1].position;
                desiredDistances[i] = Vector3.Distance(positions[i], positions[i - 1]);

                totalDist += desiredDistances[i];
            }
        }

        void PositionIteration(ref Vector3 current, ref Vector3 previous, float desiredDistance)
        {
            previous = current + (previous - current).normalized * desiredDistance;
        }

        void FABRIKIteration(Vector3 targetPos)
        {
            // STAGE 1: FORWARD REACHING
            positions[positions.Length - 2] = positions[positions.Length - 1];
            for (int i = positions.Length - 2; i > 1; i--)
            {
                PositionIteration(ref positions[i], ref positions[i - 1], desiredDistances[i]);
            }

            // STAGE 2: BACKWARD REACHING
            positions[1] = positions[0];
            for (int i = 1; i < positions.Length - 1; i++)
            {
                PositionIteration(ref positions[i], ref positions[i + 1], desiredDistances[i + 1]);
            }
            positions[positions.Length - 1] = targetPos;
        }

        void FABRIKSetRotations(Transform[] joints)
        {
            // El joint ultimo es el endeffector!
            for (int i = 0; i < joints.Length - 1; i++)
            {
                // VECTORS
                Vector3 posDir = (positions[i + 2] - positions[i + 1]).normalized;
                Vector3 jointDir = (joints[i + 1].position - joints[i].position).normalized;

                // ANGLE
                Vector3 cross = Vector3.Cross(jointDir, posDir);
                float dot = Vector3.Dot(jointDir, posDir);
                float angle = Mathf.Atan2(cross.magnitude, dot);
                angle *= Mathf.Rad2Deg;

                // ROTATION
                Vector3 axis = cross.normalized;
                joints[i].Rotate(axis, angle, Space.World);

               
            }
        }

        IEnumerator LegBaseUpdate(Transform legBase, Transform futureLegBase, int legIndex)
        {

            interpolatingLeg[legIndex] = true;


            const float TOTAL_TIME = 0.05f;

            var endPos = futureLegBase.position;
            var startPos = legBase.position;

            float timeCounter = 0f;

            while (timeCounter < TOTAL_TIME)
            {
                timeCounter += Time.deltaTime;

                float t = timeCounter / TOTAL_TIME;
                Debug.DrawLine(startPos, endPos, Color.red);

                if (t <= 0.5f)
                {
                    legBase.position = Vector3.Lerp(startPos, endPos + Vector3.up * 0.5f, t);
                }
                else
                {
                    legBase.position = Vector3.Lerp(startPos + Vector3.up * 0.5f, endPos, t);
                }


                yield return null;
            }
            interpolatingLeg[legIndex] = false;
        }


        void updateLeg(MyTentacleController leg, Transform transformTarget, Transform futureBaseTransform, int legIndex)
        {
            // Update LegBase
            float distToFutureBase = Vector3.Distance(leg.Bones[0].position, futureBaseTransform.position);

            if (distToFutureBase > legDistanceToFutureBase && !interpolatingLeg[legIndex])
            {

                MonoBehaviour.FindObjectOfType<MonoBehaviour>().StartCoroutine(LegBaseUpdate(leg.Bones[0], futureBaseTransform, legIndex));
                
            }


            // Update FABRIK

            Vector3 targetPos = transformTarget.position;


            // Copy the joints positions to work with
            CopyJointsPositions(targetPos, leg.Bones);
            bool done = Vector3.Distance(targetPos, leg.Bones[leg.Bones.Length - 1].position) < epsilon;
            if (!done)
            {
                float targetRootDist = Vector3.Distance(leg.Bones[0].position, targetPos);

                // Update joint positions
                if (targetRootDist > totalDist)
                {
                    CalculatePositionsUnrechableTarget(targetPos);
                }
                else
                {
                    for (int iteration = 0; iteration < 10; iteration++)
                    {
                        FABRIKIteration(targetPos);
                    }
                }


                // Update original joint rotations
                FABRIKSetRotations(leg.Bones);
            }

        }
        #endregion
    }
}
