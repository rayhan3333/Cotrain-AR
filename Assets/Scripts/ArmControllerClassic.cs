using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using Microsoft.MixedReality.Toolkit.Utilities;
using RosSharp;
using RosSharp.Urdf;
using RosSharp.RosBridgeClient;
using UnityEngine;
using Unity.VisualScripting;
//using System.Numerics;
namespace DVRK
{
    
    [System.Serializable]
    public class SUJJointState
    {
        public bool AutomaticTimestamp;
        public string[] Name;
        public float[] Position;
        public float Timestamp;

    }
    public class SUJState {
        public SUJJointState GetStateJoint;
    }
   

    public enum Mode
    {
        read,
        write
    }

    public class ArmControllerClassic : MonoBehaviour
    {
        public Mode mode;
        public bool isECM;
        UDPClient sujUDP;
        public int SUJport;
        UdpJsonSender sujWrite;
        public string ipAddress;
        private List<MeshCollider> colliders = new List<MeshCollider>();

        public Transform endEffector;
        public Transform target;
        public int maxIterations = 5;
        public float threshold = 0.05f;
        public float rotationSpeed = 2f;     
        public float translationSpeed = 0.1f;  

        private int tries = 0;
        private Vector3 lastPos;
        private Vector3 lastPosPSM;

        public bool isMoving;
        //VelocityController controller;

        //public collisionDetection colScript;

        private bool collision = false;

        private Vector3 initPose;
        private bool isSUJ = false;
        public Transform PSMTarget;
        public Transform PSM_end;
        public Transform RCM;
        private Vector3 psmLast;
        Version version;
        private bool messageFirstParsed = false;
        public List<UrdfJoint> sujJoints = new List<UrdfJoint>();
        private Vector3 jointOrigin;
        private URDFRobot psmScript; 
        public float[] initPoses;

        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("START IS RUNNING-----------------------------------");
            jointOrigin = transform.position;
            lastPosPSM = PSM_end.position;
            if (!isECM)
            {
                sujJoints.Add(transform.GetComponent<UrdfJointPrismatic>()); // link 0
                sujJoints.Add(transform.GetChild(2).GetComponent<UrdfJointContinuous>()); // link 1
                sujJoints.Add(sujJoints[1].transform.GetChild(2).GetComponent<UrdfJointContinuous>()); // link 2
                sujJoints.Add(sujJoints[2].transform.GetChild(2).GetComponent<UrdfJointContinuous>()); // link 3
                //sujJoints.Add(sujJoints[4].transform.GetChild(2).GetComponent<UrdfJointContinuous>()); // link 5
            }
            else
            {
                sujJoints.Add(transform.GetComponent<UrdfJointPrismatic>()); // link 0
                sujJoints.Add(transform.GetChild(2).GetComponent<UrdfJointContinuous>()); // link 1
                sujJoints.Add(sujJoints[1].transform.GetChild(2).GetComponent<UrdfJointContinuous>()); // link 2
                                                                                                       //sujJoints.Add(sujJoints[2].transform.GetChild(2).GetComponent<UrdfJointContinuous>()); // link 3

            }

            int currentJoint = 0;
            foreach (UrdfJoint joint in sujJoints)
            {
                JointStateWriter initJoint = joint.AddComponent<JointStateWriter>();
                if (!isECM)
                {
                    initJoint.Write(initPoses[currentJoint]);
                }
                currentJoint++;
            }
            if (!isECM)
            {
                sujJoints.RemoveAt(3);
            }
            target.position = endEffector.position;
            PSMTarget.position = PSM_end.position;


            if (mode == Mode.read)
            {
                sujUDP = gameObject.AddComponent<UDPClient>();
                sujUDP.port = SUJport;
            }
            else
            {
                sujWrite = gameObject.AddComponent<UdpJsonSender>();
                sujWrite.port = SUJport;
            }


            if (mode == Mode.read)
            {
                //SyncSUJ();
            }
            if (!isECM)
            {
                psmScript = RCM.parent.GetComponent<PSM>();
                Debug.Log("Got PSM Reference");
                foreach (URDFJoint joint in psmScript.independentJoints)
                {
                    Debug.Log(joint.gameObject.name);
                }
            }
            else
            {
                psmScript = RCM.parent.GetComponent<ECM>();
            }
        }

        // Update is called once per frame
        bool initTrue = false;
        float prevYaw = 0;
        float prevPitch = 0;
        void LateUpdate()
        {
            prevYaw = psmScript.independentJoints[0].currentJointValue;
            prevPitch = psmScript.independentJoints[1].currentJointValue;

            if (!initTrue)
            {
                int currentJoint = 0;

                foreach (UrdfJoint joint in sujJoints)
                {
                    if (!isECM)
                    {
                        JointStateWriter initJoint = joint.GetComponent<JointStateWriter>();
                        initJoint.Write(initPoses[currentJoint]);
                    }
                    currentJoint++;
                }
                target.position = endEffector.position;
                PSMTarget.position = PSM_end.position;
                initTrue = true;
            }
            if (Vector3.Distance(target.position, endEffector.position) > 20.0f)
            {

               // target.position = endEffector.position + new Vector3(0.0f, 3.0f, 0.0f);
            }
            if (Vector3.Distance(PSM_end.position, PSMTarget.position) > 1.2f)
            {
                Debug.Log("resetting target position");
                PSMTarget.position = PSM_end.position;
            }
            if (mode == Mode.read)
            {
                SyncSUJ();
            }

            else if (initTrue)
            {
                Vector3 normalizedPos = RCM.position - transform.parent.parent.parent.parent.GetChild(0).position; //get shell position
                Debug.Log("PSM: " + normalizedPos + " RCM Position: " + RCM.position + "Shell position: " + transform.parent.parent.parent.parent.GetChild(0).position);
                sujWrite.states = new float[] { normalizedPos.x, normalizedPos.y, normalizedPos.z,
                RCM.rotation.eulerAngles.x, RCM.rotation.eulerAngles.y, RCM.rotation.eulerAngles.z};
                if (Vector3.Distance(target.position, lastPos) > 0.0001f)
                {
                    //Debug.Log("Moving");
                    isMoving = true;
                    tries = 0;

                    //PSMTarget.position = PSMTarget.position + PSM_end.position - lastPosPSM;
                    //lastPosPSM = PSM_end.position;
                    PSMTarget.position = PSM_end.position;

                    psmScript.independentJoints[0].SetJointValue(prevYaw);
                    psmScript.independentJoints[1].SetJointValue(prevPitch);


                }
                else
                {
                    isMoving = false;

                }
                lastPos = target.position;

                if (isMoving)
                {

                    if (Vector3.Distance(endEffector.position, target.position) < threshold)
                    {
                        tries = 0;
                        return;
                    }
                    if (tries > maxIterations * 10)
                    {
                        return;
                    }



                    for (int iteration = 0; iteration < maxIterations; iteration++)
                    {
                        for (int i = sujJoints.Count - 1; i >= 0; i--)
                        {
                            Transform joint = sujJoints[i].transform;
                            Vector3 toEffector = endEffector.position - joint.position;
                            Vector3 toTarget = target.position - joint.position;

                            if (Vector3.Distance(endEffector.position, target.position) < threshold)
                                return;

                            if (joint.TryGetComponent<UrdfJointContinuous>(out var revolute))
                            {
                                var hinge = revolute.GetComponent<HingeJoint>();
                                Vector3 axis = joint.TransformDirection(hinge.axis);

                                Vector3 projEff = Vector3.ProjectOnPlane(toEffector, axis);
                                Vector3 projTgt = Vector3.ProjectOnPlane(toTarget, axis);
                                
                                float angleDeg = -Vector3.SignedAngle(projEff, projTgt, axis);
                                // if (isECM)
                                // {
                                //     angleDeg = Vector3.SignedAngle(projEff, projTgt, axis);

                                // }
                                float angleRad = Mathf.Deg2Rad * angleDeg;

                                float step = Mathf.Clamp(angleRad, -rotationSpeed * Time.deltaTime, rotationSpeed * Time.deltaTime);

                                float currentAngle = revolute.GetPosition(); // in radians
                                float newAngle = currentAngle + step;

                                //var limits = joint.GetComponent<HingeJointLimitsManager>();
                                //newAngle = Mathf.Clamp(newAngle, limits.LargeAngleLimitMin, limits.LargeAngleLimitMax);

                                //Direct State (position update)
                                revolute.GetComponent<JointStateWriter>().Write(newAngle);
                                // Bounds bounds = colliders[i].bounds;
                                // Vector3 center = bounds.center;
                                // Vector3 halfExtents = bounds.extents;
                                // Collider[] overlaps = Physics.OverlapBox(center, halfExtents, transform.rotation);
                                // foreach (var hit in overlaps)
                                //     {
                                //         if (hit != colliders[i])  // avoid self
                                //         {
                                //             Debug.Log("Overlap with: " + hit.name);
                                //         }
                                //     }

                                //UNCOMMENT FOR COLLISION DETECTION
                                // if (collision)
                                // {
                                //     revolute.GetComponent<JointStateWriter>().Write(currentAngle);
                                //     target.position = initPose;
                                //     collision = false;
                                // }

                                //Velocity Update
                                //joint.GetComponent<VelocityController>().SetState(newAngle);

                                //Debug.Log(joint.gameObject.name + " (revolute): " + newAngle + " rad, Iteration: " + iteration);
                            }
                            else if (joint.TryGetComponent<UrdfJointPrismatic>(out var prismatic))
                            {
                                //prismatic.GetComponent<JointStateWriter>().Write(-(target.position.y - 1.115f));
                                prismatic.GetComponent<JointStateWriter>().Write(-(target.position.y - transform.parent.position.y)+0.8f);

                                // var cj = prismatic.GetComponent<ConfigurableJoint>();
                                // Vector3 axis = joint.TransformDirection(cj.axis.normalized);

                                // Vector3 projected = Vector3.Project(toTarget - toEffector, axis);
                                // float moveAmount = projected.magnitude * Mathf.Sign(Vector3.Dot(projected, axis));

                                // float step = Mathf.Clamp(moveAmount, -translationSpeed * Time.deltaTime, translationSpeed * Time.deltaTime);
                                // if (step < 0)
                                // {
                                //     step *= -1f;
                                // }
                                // //Debug.Log(step);
                                // float currentTranslation = prismatic.GetPosition(); // in meters
                                // float newTranslation = currentTranslation + step;
                                // if (moveAmount > 0)
                                // {
                                //     Debug.Log("positive");
                                // } else {
                                //     Debug.Log(moveAmount);
                                //     prismatic.GetComponent<JointStateWriter>().Write(moveAmount);

                                // }


                            }
                        }
                    }
                    tries++;
                }
            }
           
        }

        public void SyncSUJ()
        {
            string message = sujUDP.GetLatestUDPPacket();
            SUJState state = JsonUtility.FromJson<SUJState>(message);
           // Debug.Log(state);
           
            int currentIndex = 0;
            // Assuming correct order
            if (state != null)
            {
                //Debug.Log(state);
                foreach (UrdfJoint joint in sujJoints)
                {
                    joint.GetComponent<JointStateWriter>().Write(-1*state.GetStateJoint.Position[currentIndex]);

                    currentIndex++;
                }
            }

        }
       
    }
}
