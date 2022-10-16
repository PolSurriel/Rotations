using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;




namespace OctopusController
{

    
    internal class MyTentacleController

    //MAINTAIN THIS CLASS AS INTERNAL
    {

        TentacleMode tentacleMode;
        Transform[] _bones;
        Transform _endEffectorSphere;

        public Transform EndEffectorSphere { get => _endEffectorSphere; }

        public Transform[] Bones { get => _bones; }

        //Exercise 1.
        public Transform[] LoadTentacleJoints(Transform root, TentacleMode mode)
        {

            //TODO: add here whatever is needed to find the bones forming the tentacle for all modes
            //you may want to use a list, and then convert it to an array and save it into _bones
            tentacleMode = mode;

            var auxBones = new List<Transform>();

            switch (tentacleMode){
                case TentacleMode.LEG:
                    //TODO: in _endEffectorsphere you keep a reference to the base of the leg

                    int i = 0;
                    while (root != null)
                    {
                        auxBones.Add(root);
                        
                        
                        var next = root.GetChild(i++==0?0:1);


                        try { next.GetChild(1); } catch (UnityException e)
                        {
                            auxBones.Add(next);
                            _endEffectorSphere = next;
                            break;
                        }
                        

                        root = next;
                    }

                    _bones = auxBones.ToArray();

                    break;
                case TentacleMode.TAIL:
                    //TODO: in _endEffectorsphere you keep a reference to the red sphere 

                    while (root != null)
                    {
                        auxBones.Add(root);
                        var next = root.GetChild(1);

                        try { next.GetChild(1); } catch (UnityException e)
                        {
                            auxBones.Add(next);
                            _endEffectorSphere = next;
                            break;
                        }

                        root = next;
                    }

                    _bones = auxBones.ToArray();

                    break;

                case TentacleMode.TENTACLE:
                    //TODO: in _endEffectorphere you  keep a reference to the sphere with a collider attached to the endEffector


                    root = root.GetChild(0).GetChild(0);

                    while (root != null)
                    {
                        auxBones.Add(root);
                        var next = root.GetChild(0);

                        try { next.GetChild(0); } catch (UnityException e)
                        {
                            _endEffectorSphere = next;
                            break;
                        }

                        root = next;
                    }

                    _bones = auxBones.ToArray();

                    break;
            }
            return Bones;
        }
    }
}
