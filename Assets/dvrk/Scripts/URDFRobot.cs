/*
    Author(s):  Long Qian
    Created on: 2019-03-29
    (C) Copyright 2015-2018 Johns Hopkins University (JHU), All Rights Reserved.

    --- begin cisst license - do not edit ---
    This software is provided "as is" under an open source license, with
    no warranty.  The complete license can be found in license.txt and
    http://www.cisst.org/cisst/license.txt.
    --- end cisst license ---
*/
using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using UnityEngine;

namespace DVRK {

    public class URDFRobot : MonoBehaviour {

        public List<URDFJoint> independentJoints = new List<URDFJoint>();
        public URDFJoint jaw = null;

        public static List<URDFRobot> instances = new List<URDFRobot>();
        private int instanceID = -1;

        private UDPClient udpClient;
        private UdpJsonSender udpSender;

        public Transform gizmo;

        public Transform pivot;
        private Vector3 lastPos;
        public Transform RCM;

        public List<Transform> trochars;

        public Mode mode = Mode.write;
        private Transform RCMstart;

        public ArmControllerClassic sujs;

        public virtual void HandleMessage(string message)
        {
            Debug.Log("DVRK::URDFRobot base class not implementing HandleMessage");
        }
        public virtual void SendJointStates(UdpJsonSender udpsend, bool moving)
        {
            Debug.Log("DVRK::URDFRobot base class not implementing SendJointStates");

        }


        // Use this for initialization
        void Start()
        {
            foreach (var obj in FindObjectsOfType<GameObject>())
            {
                if (obj.name.Contains("trochar"))
                {
                    trochars.Add(obj.transform);
                }

            }
            if (!gameObject.name.Contains("ECM"))
            {
                foreach (MeshRenderer mr in pivot.GetComponentsInChildren<MeshRenderer>())
                {
                    mr.enabled = false;
                }
            }
            //RCMstart = Instantiate(RCM.gameObject, RCM.position, RCM.rotation, RCM.parent.parent.parent.parent.parent).transform;
                // all the joints, to setup linkage
                foreach (URDFJoint joint in GetComponentsInChildren<URDFJoint>())
                {
                    joint.SetupRobotJoint();
                }
            foreach (URDFJoint joint in independentJoints)
            {
                joint.SetJointValueDefault();
            }
            if (jaw != null)
            {
                jaw.SetJointValueDefault();
            }
            
            udpClient = GetComponent<UDPClient>();
            
            if (mode == Mode.write)
            {
                udpSender = gameObject.AddComponent<UdpJsonSender>();
                udpSender.port = GetComponent<UDPClient>().port + 10;
            }

            instances.Add(this);
            instanceID = instances.Count - 1;
            Debug.Log(name + ": Current URDFRobot instanceID: " + instanceID);
            lastPos = gizmo.position;
        }

        // LateUpdate is called once per frame

        bool mated = false;
        public bool beginSuture = false;
        bool gizmoDisabled = false;

        public void BeginSuture()
        {
            beginSuture = true;
            Debug.Log("beginning suture");
        }
        void LateUpdate()
        {
            if (mode == Mode.read)
            {
                string message = "";
                message = udpClient.GetLatestUDPPacket();
                Debug.Log("Message: " + message);
                if (message != "")
                {
                    HandleMessage(message);
                }
            }
            else
            {

                if (beginSuture && !gizmoDisabled) {
                    gizmo.gameObject.SetActive(false);
                    gizmoDisabled = true;

                    if (gameObject.name.Contains("ECM"))
                    {
                        transform.parent.parent.parent.parent.GetComponent<ArmControllerClassic>().target.gameObject.SetActive(false);
                    }
                    else
                    {
                        transform.parent.parent.parent.parent.parent.GetComponent<ArmControllerClassic>().target.gameObject.SetActive(false);

                    }
                }

                if (!mated)
                {
                    Transform trochar = trochars[0];
                    float minDistance = Mathf.Infinity;
                    foreach (Transform obj in trochars)
                    {
                        if (obj.parent.name.Contains("Marker"))
                        {
                            if (Vector3.Distance(pivot.position, obj.position) < minDistance)
                            {
                                trochar = obj;
                                minDistance = Vector3.Distance(pivot.position, obj.position);
                            }
                        }
                    }
                    //Debug.Log(minDistance);
                    if (minDistance < 0.03)
                    {
                        Debug.Log("trochar close");
                        //if (Quaternion.Angle(Quaternion.Euler(-90f, 0f, 0f) * pivot.rotation, trochar.rotation) < 5f)
                        if (true)
                        {
                            Debug.Log("Angle satisifed");
                            trochar.transform.SetParent(pivot);
                            foreach (MeshRenderer mr in pivot.GetComponentsInChildren<MeshRenderer>())
                            {
                                mr.enabled = true;
                            }
                            trochar.localPosition = new Vector3(0f, 0f, 0f);
                            trochar.localRotation = Quaternion.Euler(90f, 0, 0);
                            trochar.GetComponent<ObjectManipulator>().enabled = false;

                            if (gameObject.name.Contains("ECM"))
                            {
                                transform.parent.parent.parent.parent.GetComponent<ArmControllerClassic>().target.gameObject.SetActive(false);
                            }
                            else
                            {
                                transform.parent.parent.parent.parent.parent.GetComponent<ArmControllerClassic>().target.gameObject.SetActive(false);

                            }
                            mated = true;

                        }
                    }
                    
                }
                //Gizmo Solver
                if (!beginSuture)
                {
                    if (Vector3.Distance(gizmo.position, lastPos) > 0.0001f)
                    {
                        SendJointStates(udpSender, true);

                        if (!sujs.isMoving)
                        {
                            Vector3 localDir = RCM.InverseTransformDirection(gizmo.position - RCM.position);
                            float pitch = Mathf.Atan2(localDir.y, localDir.z) * Mathf.Rad2Deg;
                            float roll = -1f * Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;

                            independentJoints[0].SetJointValue(pitch);
                            independentJoints[1].SetJointValue(roll);

                            if (mated)
                            {
                                Vector3 proj = Vector3.Project(gizmo.position - RCM.position, RCM.forward);
                                float insertion = (1f - Mathf.Clamp01(proj.magnitude / .6f)) * independentJoints[2].jointLimit.y;
                                Debug.Log("Insertion: " + insertion);
                                independentJoints[2].SetJointValue(insertion);
                            }
                        }

                    }
                }

                else if (gameObject.name != "ECM" && !transform.parent.parent.parent.parent.parent.GetComponent<ArmControllerClassic>().isMoving)
                {
                    SendJointStates(udpSender, false);
                    string message = "";
                    message = udpClient.GetLatestUDPPacket();
                    //Debug.Log("PSM is overrided by AMBF: " + message);
                    if (message != "")
                    {
                        HandleMessage(message);
                    }
                }
                else if (gameObject.name == "ECM" && !transform.parent.parent.parent.parent.GetComponent<ArmControllerClassic>().isMoving)
                {
                    SendJointStates(udpSender, false);
                    string message = "";
                    message = udpClient.GetLatestUDPPacket();
                    //Debug.Log("PSM is overrided by AMBF: " + message);
                    if (message != "")
                    {
                        HandleMessage(message);
                    }
                }
            }
            lastPos = gizmo.position;

          
        }
        
        

// #if UNITY_EDITOR
//         void OnGUI() {
//             int width = 100;
//             int height = 20;
//             int currentHeight = height;
//             int setupHeight = 20;
//             foreach (URDFJoint joint in independentJoints) {
//                 GUI.Label(new Rect(10 + instanceID * width, currentHeight, width, height), joint.name);
//                 currentHeight += setupHeight;
//                 float val = joint.defaultJointValue;
//                 if (joint.jointType == URDFJoint.JointType.Revolute || joint.jointType == URDFJoint.JointType.Prismatic) {
//                     val = GUI.HorizontalSlider(new Rect(10 + instanceID * width, currentHeight, width, height), joint.currentJointValue,
//                         joint.jointLimit.x, joint.jointLimit.y);
//                 }
//                 else if (joint.jointType == URDFJoint.JointType.Continuous) {
//                     val = GUI.HorizontalSlider(new Rect(10 + instanceID * width, currentHeight, width, height), joint.currentJointValue,
//                         -180f, 180f);
//                 }
//                 joint.SetJointValue(val);
//                 currentHeight += setupHeight;
//             }
//             if (GUI.Button(new Rect(10 + instanceID * width, currentHeight, width, height), "Recenter")) {
//                 foreach (URDFJoint joint in independentJoints) {
//                     joint.SetJointValueDefault();
//                 }
//             }
//         }
//#endif

    }
}
