using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
    public enum TentacleMode { LEG, TAIL, TENTACLE };

    public class MyOctopusController
    {

        MyTentacleController[] _tentacles = new MyTentacleController[4];

        Transform _currentRegion;
        Transform _target;

        Transform[] _randomTargets;// = new Transform[4];


        float _twistMin, _twistMax;
        float _swingMin, _swingMax;

        int _tries, _mtries;

        bool _done;

        #region public methods
        //DO NOT CHANGE THE PUBLIC METHODS!!

        public float TwistMin { set => _twistMin = value; }
        public float TwistMax { set => _twistMax = value; }
        public float SwingMin { set => _swingMin = value; }
        public float SwingMax { set => _swingMax = value; }


        public void TestLogging(string objectName)
        {


            Debug.Log("hello, I am initializing my Octopus Controller in object " + objectName);


        }

        public void Init(Transform[] tentacleRoots, Transform[] randomTargets)
        {
            _tentacles = new MyTentacleController[tentacleRoots.Length];

            // foreach (Transform t in tentacleRoots)
            for (int i = 0; i < tentacleRoots.Length; i++)
            {

                _tentacles[i] = new MyTentacleController();
                _tentacles[i].LoadTentacleJoints(tentacleRoots[i], TentacleMode.TENTACLE);
                //TODO: initialize any variables needed in ccd



            }

            _randomTargets = randomTargets;
            //TODO: use the regions however you need to make sure each tentacle stays in its region

        }


        public void NotifyTarget(Transform target, Transform region)
        {
            MonoBehaviour.FindObjectOfType<MonoBehaviour>().StartCoroutine(RegionTransition());

            _currentRegion = region;
            _target = target;
        }

        bool useRegion = false;

        public void NotifyShoot()
        {
            //TODO. what happens here?
            useRegion = true;
        }


        public void UpdateTentacles()
        {
            //TODO: implement logic for the correct tentacle arm to stop the ball and implement CCD method
            update_ccd();
        }




        #endregion


        #region private and internal methods
        //todo: add here anything that you need

        void update_ccd()
        {
            for (int i = 0; i < _tentacles.Length; i++)
            {
                for (int iterations = 0; iterations < 1; iterations++)
                {
                    //ccdTwistConstrain(_tentacles[i]);
                    tentacle_ccd(_tentacles[i], _randomTargets[i]);
                    //for (int j = 0; j < _tentacles[i].Bones.Length; j++)
                    //{
                    //    CameraConstrain(_tentacles[i], j);
                    //}
                }
            }

        }

     
        
        void ccdTwistConstrain(MyTentacleController currentTentacle, int i)
        {

            var twistMax = _twistMax;
            var twistMin = _twistMin;


            Quaternion originalRotation = currentTentacle.Bones[i].transform.localRotation;

            // Variable de control para no aplicar cambios si los angulos son correctos
            bool needToApplyConstrain = false;

            //EXTRAEMOS ROTACIONES LOCALES
            // Twist
            Quaternion localtwist = new Quaternion(
                0f,
                currentTentacle.Bones[i].transform.localRotation.y,
                0f,
                currentTentacle.Bones[i].transform.localRotation.w).normalized;

            // Local rotation
            Quaternion localRotation = new Quaternion(
               currentTentacle.Bones[i].transform.localRotation.x,
               currentTentacle.Bones[i].transform.localRotation.y,
               currentTentacle.Bones[i].transform.localRotation.z,
               currentTentacle.Bones[i].transform.localRotation.w).normalized;


            // Extraemos del twist la afectacion del swing (decomposicion)
            Quaternion twist = localtwist;

            // Extraemos en angulo y el eje de rotacion.
            float twistAngle;
            Vector3 twistAxis;
            twist.ToAngleAxis(out twistAngle, out twistAxis);

            // Comprobamos si el angulo NO se encuentra dentro de los margenes
            // para reajustarlo
            if (twistAngle < twistMin || twistAngle > twistMax)
            {
                //Reajustamos el angulo dentro de los limites que queremos
                twistAngle = Mathf.Clamp(twistAngle, twistMin, twistMax);

                //Obtenemos la rotacion twist final
                twist = Quaternion.AngleAxis(twistAngle, twistAxis);
                needToApplyConstrain = true;

            }


            // Extraemos del swing la afectacion del twist (decomposicion del eje local y)
            // usamos localtwist porque debemos extraer el swing del twist no modificado!! --> qs = q * qt^-1
            Quaternion swing = Quaternion.Inverse(localtwist) * localRotation;

            // Extraemos el angulo de rotacion y el eje
            float swingAngle;
            Vector3 swingAxis;
            swing.ToAngleAxis(out swingAngle, out swingAxis);


            // Comprobamos si el angulo NO se encuentra dentro de los margenes
            // para reajustarlo
            if (swingAngle < _swingMin || swingAngle > _swingMax)
            {
                // Reajustamos el angulo si es necesario
                swingAngle = Mathf.Clamp(swingAngle, _swingMin, _swingMax);

                //Obtenemos la rotacion twist final
                swing = Quaternion.AngleAxis(swingAngle, swingAxis);

                needToApplyConstrain = true;
            }


            if (needToApplyConstrain)
            {
                // Combinamos en una rotacion
                var finalRotation = twist * swing;

                // Aplicamos
                currentTentacle.Bones[i].transform.localRotation = finalRotation;

            }


        }



        private void LookToCamera(Transform transform)
        {
            //tengo el plano formado en punto bone position y normal twist axis.
            //tmb tengo el vector punto camara
            // So:
            //     1 proyecto el punto en el plano
            //     2 miro la diferencia angular entre el localForward y el proyectado
            //     3 roto usando el twist como eje esa cantidad de grados.

            // Let's go:


            // info:            
            var planeNormal = transform.up;
            var boneForward = transform.right;
            var toCamera = (Camera.main.transform.position - transform.position).normalized;

            // 1
            Vector3 projection = Vector3.ProjectOnPlane(toCamera, planeNormal).normalized;

            // 2
            Vector3 cross = Vector3.Cross(projection, boneForward);
            float dot = Vector3.Dot(projection, boneForward);
            float angle = Mathf.Atan2(cross.magnitude, dot) * Mathf.Rad2Deg;


            if (cross.normalized == planeNormal.normalized)
            {
                angle *= -1f;
            }

            //3
            var rotation = Quaternion.AngleAxis(angle, planeNormal.normalized);


            var newRot = rotation * transform.rotation;
            transform.rotation = newRot;
            if (transform.childCount != 0)
            {
                transform.GetChild(0).rotation = Quaternion.Inverse(rotation) * transform.GetChild(0).rotation;

            }




        }

        IEnumerator RegionTransition()
        {
            blendingRegion = true;
            blendRegionState = 0f;

            float speed = 0.05f;
            while (blendRegionState < 1f)
            {
                Debug.Log("doing" + blendRegionState);

                blendRegionState = Mathf.Min(1f, blendRegionState + Time.deltaTime * speed);
                yield return null;
            }
            blendingRegion = false;
        }

        bool blendingRegion;
        float blendRegionState;
        Vector3 GetRegionTarget(Vector3 lastTarget)
        {

            if (blendingRegion)
            {
                return Vector3.Lerp(lastTarget, _target.position, blendRegionState);
            }
            
            return _target.position;
        }

        void tentacle_ccd(MyTentacleController currentTentacle, Transform target)
        {

            for (int i = currentTentacle.Bones.Length - 2; i >= 0; i--)
            {

                Vector3 targetPos = useRegion ? GetRegionTarget(target.position) : target.position;

                Vector3 endEffectorPos = currentTentacle.EndEffectorSphere.position;

                // The vector from the ith joint to the end effector
                Vector3 r1 = (endEffectorPos - currentTentacle.Bones[i].position).normalized;

                // The vector from the ith joint to the target
                Vector3 r2 = (targetPos - currentTentacle.Bones[i].position).normalized;

                // The axis of rotation 
                Vector3 cross = Vector3.Cross(r1, r2);
                Vector3 axis = cross.normalized;

                // find the angle between r1 and r2
                float dot = Vector3.Dot(r1, r2);
                float angle = Mathf.Atan2(cross.magnitude, dot) * Mathf.Rad2Deg;


                if (Mathf.Abs(angle) <= 0.1f)
                {
                    continue;
                }

                Quaternion rotationToApply = Quaternion.AngleAxis(angle, axis);
                Quaternion finalRotation = rotationToApply * currentTentacle.Bones[i].rotation;

                currentTentacle.Bones[i].rotation = finalRotation;

                ccdTwistConstrain(currentTentacle, i);

            }


            

        }




        #endregion






    }
}
