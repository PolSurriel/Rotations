using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace RobotController
{

    public struct MyQuat
    {

        public float w; // angulo rotaicon
        public float x;
        public float y; // eje de rotacion
        public float z;

        public MyVec vector { get { return new MyVec { x = this.x, y = this.y, z = this.z }; } }

        public static MyQuat identity
        {
            get { return new MyQuat { w = 1f, x = 0f, y = 0f, z = 0f }; }
        }

        // componente 0, 1, 2, 3
        public float c0 { get { return w; } set { w = value; } }
        public float c1 { get { return x; } set { x = value; } }
        public float c2 { get { return y; } set { y = value; } }
        public float c3 { get { return z; } set { z = value; } }


        public MyQuat conjugated
        {
            get
            {
                return new MyQuat
                {
                    w = this.w,
                    x = this.x * -1f,
                    y = this.y * -1f,
                    z = this.z * -1f,
                };
            }
        }



        public static MyQuat Lerp(MyQuat q, MyQuat p, float t)
        {
            return (q * (1f - t) + p * t).normalized;
        }

        public static MyQuat Slerp(MyQuat q, MyQuat p, float t)
        {

            float angle = (float)Math.Acos(MyVec.Dot(q.vector, p.vector) / (q.vector.modulus * p.vector.modulus));

            float sinAngle = (float)Math.Sin(angle);

            float coefq = (float)Math.Sin((1f - t) * angle) / sinAngle;
            float coefp = (float)Math.Sin(t * angle) / sinAngle;

            return (q * coefq + p * coefp).normalized;
        }

        public static MyQuat Sterp(MyQuat q, MyQuat p, float t)
        {
            var swingQ = MyQuat.GetSwing(q);
            var swingP = MyQuat.GetSwing(p);
            var twistQ = MyQuat.GetTwist(q);
            var twistP = MyQuat.GetTwist(p);

            var swing = Slerp(swingQ, swingP, t);
            var twist = Slerp(twistQ, twistP, t);

            return twist * swing;
        }

        public static MyQuat GetSwing(MyQuat rot3)
        {
            //return MyQuat.identity * rot3;
            return rot3 * GetTwist(rot3).inverse;

        }


        public static MyQuat GetTwist(MyQuat rot3)
        {

            return new MyQuat { w = rot3.w, x = 0f, y = rot3.y, z = 0f }.normalized;


        }



        public float modulus
        {
            get
            {
                return (float)Math.Sqrt(w * w + x * x + y * y + z * z);
            }
        }

        public MyQuat normalized
        {
            get
            {
                float mod = this.modulus;

                return new MyQuat
                {
                    w = this.w / mod,
                    x = this.x / mod,
                    y = this.y / mod,
                    z = this.z / mod
                };
            }
        }


        public string text => "w: " + w + ", x:" + x + ", y:" + y + ", z:" + z + " .";

        public static MyQuat AxisAngle(float angleInRadians, MyVec axis)
        {

            axis = axis.normalized;

            float sin = (float)Math.Sin(angleInRadians * 0.5f);
            float cos = (float)Math.Cos(angleInRadians * 0.5f);

            return new MyQuat
            {
                w = cos,
                x = axis.x * sin,
                y = axis.y * sin,
                z = axis.z * sin

            }.normalized;
        }

        public MyQuat inverse
        {
            get
            {
                float mod = this.modulus;

                if (mod == 1f)
                {
                    return conjugated;
                }

                return conjugated / (mod * mod);

            }
        }

        // Es como aplicar la distributiva a una multiplicacion de polinomios
        // (2 + 3a + 5b + 6c) * (2 + 3a + 5b + 6c)  = r + r2A + r3B + r4C
        public static MyQuat operator *(MyQuat p, MyQuat q)
        {
            return new MyQuat
            {
                w = p.c0 * q.c0 - p.c1 * q.c1 - p.c2 * q.c2 - p.c3 * q.c3,
                x = p.c0 * q.c1 + p.c1 * q.c0 - p.c2 * q.c3 + p.c3 * q.c2,
                y = p.c0 * q.c2 + p.c1 * q.c3 + p.c2 * q.c0 - p.c3 * q.c1,
                z = p.c0 * q.c3 - p.c1 * q.c2 + p.c2 * q.c1 + p.c3 * q.c0,
            };
        }

        public static MyQuat operator *(MyQuat p, float f)
        {
            return new MyQuat
            {
                w = p.w * f,
                x = p.x * f,
                y = p.y * f,
                z = p.z * f
            };
        }

        public static MyQuat operator /(MyQuat q, float n)
        {
            return new MyQuat
            {
                w = q.w / n,
                x = q.x / n,
                y = q.y / n,
                z = q.z / n,
            };
        }


        public static MyQuat operator +(MyQuat q, MyQuat p)
        {
            return new MyQuat
            {
                w = q.w + p.w,
                x = q.x + p.x,
                y = q.y + p.y,
                z = q.z + p.z,
            };
        }


        public static MyQuat Rotate(MyQuat point, MyQuat rotation)
        {

            return rotation * point * rotation.inverse;

        }

        public static MyQuat Rotate(MyVec point, MyQuat rotation)
        {

            MyQuat tmp = new MyQuat { w = 0, x = point.x, y = point.y, z = point.z };

            return rotation * tmp * rotation.inverse;

        }

        public static MyVec RotateVec(MyQuat point, MyQuat rotation)
        {

            return (rotation * point * rotation.inverse).vector;

        }

        public static MyVec RotateVec(MyVec point, MyQuat rotation)
        {

            MyQuat tmp = new MyQuat { w = 0, x = point.x, y = point.y, z = point.z };

            return (rotation * tmp * rotation.inverse).vector;

        }

        public static float WFromAngle(float angle)
        {
            return (float)Math.Cos(angle * 0.5f);
        }


    }

    public struct MyVec
    {

        public float x;
        public float y;
        public float z;


        public static MyVec Cross(MyVec a, MyVec b)
        {
            return new MyVec { };
        }

        public static float Dot(MyVec a, MyVec b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public float modulus
        {
            get
            {
                return (float)Math.Sqrt(x * x + y * y + z * z);
            }
        }


        public MyVec normalized
        {
            get
            {
                float mod = this.modulus;

                return new MyVec
                {
                    x = this.x / mod,
                    y = this.y / mod,
                    z = this.z / mod
                };
            }
        }


        public static MyVec Projection(MyVec a, MyVec b)
        {
            return b * (Dot(a, b) / Dot(b, b));
        }

        public static MyVec operator *(MyVec a, float b)
        {
            return new MyVec
            {
                x = a.x * b,
                y = a.y * b,
                z = a.z * b,
            };
        }

        public static MyVec operator /(MyVec a, float b)
        {
            return new MyVec
            {
                x = a.x / b,
                y = a.y / b,
                z = a.z / b,
            };
        }

    }

    public struct Angle
    {

        private float _degree;
        private float _radians;

        public float degree
        {
            get { return _degree; }
            set
            {
                _degree = value;
                _radians = value * (float)Math.PI / 180f;

            }
        }

        public float radians
        {
            get { return _radians; }
            set
            {
                _radians = value;
                _degree = value * 180f / (float)Math.PI;
            }

        }


    }




    public class MyRobotController
    {

        #region public methods



        public string Hi()
        {
            string s = "Names: Pol Surriel Muxiench / Eric Garcia Reverter (ID: 6)";
            return s;

        }


        public struct RobotArmPositionMaker
        {

            public Angle angle0;
            public Angle angle1;
            public Angle angle2;
            public Angle angle3;



            public void Make(out MyQuat rot0, out MyQuat rot1, out MyQuat rot2, out MyQuat rot3)
            {

                MyVec axis0 = new MyVec { x = 0f, y = 1f, z = 0f };
                MyVec axis1 = new MyVec { x = 1f, y = 0f, z = 0f };
                MyVec axis2 = new MyVec { x = 1f, y = 0f, z = 0f };
                MyVec axis3 = new MyVec { x = 1f, y = 0f, z = 0f };


                var rotation0 = MyQuat.AxisAngle(angle0.radians, axis0);

                //to local axis
                var cumulativeInverse = rotation0.inverse;
                axis1 = MyQuat.RotateVec(axis1, cumulativeInverse);

                var rotation1 = MyQuat.AxisAngle(angle1.radians, axis1);


                // to local axis
                cumulativeInverse = rotation1.inverse * cumulativeInverse;
                axis2 = MyQuat.RotateVec(axis2, cumulativeInverse);

                var rotation2 = MyQuat.AxisAngle(angle2.radians, axis2);

                // to local axis
                cumulativeInverse = rotation2.inverse * cumulativeInverse;
                axis3 = MyQuat.RotateVec(axis3, cumulativeInverse);

                var rotation3 = MyQuat.AxisAngle(angle3.radians, axis3);


                //todo: change this, use the function Rotate declared below
                rot0 = MyQuat.identity * rotation0;
                rot1 = rot0 * rotation1;
                rot2 = rot1 * rotation2;
                rot3 = rot2 * rotation3;
            }

        }

        //EX1: this function will place the robot in the initial position

        public void PutRobotStraight(out MyQuat rot0, out MyQuat rot1, out MyQuat rot2, out MyQuat rot3)
        {

            var solver = new RobotArmPositionMaker
            {
                angle0 = new Angle { degree = 73f },
                angle1 = new Angle { degree = -10f },
                angle2 = new Angle { degree = 120f },
                angle3 = new Angle { degree = -15f },

            };

            solver.Make(out rot0, out rot1, out rot2, out rot3);


            startPositionRot0 = rot0;
            startPositionRot1 = rot1;
            startPositionRot2 = rot2;
            startPositionRot3 = rot3;

            solver = new RobotArmPositionMaker
            {
                angle0 = new Angle { degree = 37f },
                angle1 = new Angle { degree = 5f },
                angle2 = new Angle { degree = 75f },
                angle3 = new Angle { degree = 0f },

            };

            solver.Make(out endPositionRot0, out endPositionRot1, out endPositionRot2, out endPositionRot3);

            exercise2SetupDone = true;

        }

        bool exercise2SetupDone = false;

        MyQuat startPositionRot0;
        MyQuat startPositionRot1;
        MyQuat startPositionRot2;
        MyQuat startPositionRot3;

        MyQuat endPositionRot0;
        MyQuat endPositionRot1;
        MyQuat endPositionRot2;
        MyQuat endPositionRot3;



        //EX2: this function will interpolate the rotations necessary to move the arm of the robot until its end effector collides with the target (called Stud_target)
        //it will return true until it has reached its destination. The main project is set up in such a way that when the function returns false, the object will be droped and fall following gravity.

        float t = 0f;

        public bool PickStudAnim(out MyQuat rot0, out MyQuat rot1, out MyQuat rot2, out MyQuat rot3)
        {

            if (!exercise2SetupDone)
            {
                PutRobotStraight(out rot0, out rot1, out rot2, out rot3);
            }


            t += 1f / 1200f;
            bool myCondition = t < 1f;


            if (myCondition)
            {

                //tmb hemos usado el lerp normal. El metodo es Lerp.
                rot0 = MyQuat.Slerp(startPositionRot0, endPositionRot0, t);
                rot1 = MyQuat.Slerp(startPositionRot1, endPositionRot1, t);
                rot2 = MyQuat.Slerp(startPositionRot2, endPositionRot2, t);
                rot3 = MyQuat.Slerp(startPositionRot3, endPositionRot3, t);

                return true;
            }

            //todo: remove this once your code works.
            rot0 = endPositionRot0;
            rot1 = endPositionRot1;
            rot2 = endPositionRot2;
            rot3 = endPositionRot3;

            return false;
        }


        //EX3: this function will calculate the rotations necessary to move the arm of the robot until its end effector collides with the target (called Stud_target)
        //it will return true until it has reached its destination. The main project is set up in such a way that when the function returns false, the object will be droped and fall following gravity.
        //the only difference wtih exercise 2 is that rot3 has a swing and a twist, where the swing will apply to joint3 and the twist to joint4


        public bool PickStudAnimVertical(out MyQuat rot0, out MyQuat rot1, out MyQuat rot2, out MyQuat rot3)
        {

            /*
             Nos hemos dado cuenta de que fuerzas el seteo del twist/swing 
             despues de la llamada a esta funcion asi que hemos puesto 
             exactamente el mismo codigo que el ejercicio 2. No tiene sentido
             usar un metodo diferente si los joints que tienen un trato especial (el 3 y el 4)
             se sobreescriben despues.
             */

            return PickStudAnim(out rot0, out rot1, out rot2, out rot3);
        }


        public static MyQuat GetSwing(MyQuat rot3)
        {
            return MyQuat.GetSwing(rot3);

        }


        public static MyQuat GetTwist(MyQuat rot3)
        {
            return MyQuat.GetTwist(rot3);

        }

        public static MyQuat GetTwistCutre(MyQuat rot3)
        {

            return new MyQuat { x = 0f, y = 0f, z = rot3.z, w = rot3.w }.normalized;

        }




        #endregion


        #region private and internal methods

        internal int TimeSinceMidnight { get { return (DateTime.Now.Hour * 3600000) + (DateTime.Now.Minute * 60000) + (DateTime.Now.Second * 1000) + DateTime.Now.Millisecond; } }


        private static MyQuat NullQ
        {
            get
            {
                MyQuat a;
                a.w = 1;
                a.x = 0;
                a.y = 0;
                a.z = 0;
                return a;

            }
        }

        internal MyQuat Multiply(MyQuat q1, MyQuat q2)
        {

            //todo: change this so it returns a multiplication:
            return q1 * q2;

        }

        internal MyQuat Rotate(MyQuat currentRotation, MyVec axis, float angle)
        {

            //todo: change this so it takes currentRotation, and calculate a new quaternion rotated by an angle "angle" radians along the normalized axis "axis"
            return currentRotation * MyQuat.AxisAngle(angle, axis);

        }





        //todo: add here all the functions needed

        #endregion






    }
}
