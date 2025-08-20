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
using Unity.VisualScripting;
using UnityEngine;

namespace DVRK {

    [System.Serializable]
    public class PSMState {
        //public JointState GetStateJaw;
        public JointState GetStateJoint;
    }
    public class SimplePSMState {
        //public JointState GetStateJaw;
        public SimpleJointState GetStateJoint;
    }

    public class PSM : URDFRobot
    {

        private bool messageFirstParsed = false;

        // private bool CheckConsistency(PSMState state) {
        //     int currentIndex = 0;
        //     foreach (URDFJoint joint in independentJoints) {
        //         if (joint.name.StartsWith(state.GetStateJoint.Name[currentIndex])) {
        //             currentIndex++;
        //             continue;
        //         }
        //         else {
        //             Debug.Log("PSM error: " + joint.name + " does not start with " + state.GetStateJoint.Name[currentIndex]);
        //             return false;
        //         }
        //     }
        //     if (jaw == null) {
        //         Debug.Log("Jaw joint does not exist");
        //         return false;
        //     }
        //     if (!jaw.name.StartsWith(state.GetStateJaw.Name[0])) {
        //         Debug.Log("PSM error: " + jaw.name + " does not start with " + state.GetStateJaw.Name[0]);
        //         return false;
        //     }
        //     Debug.Log("PSM consistency check passed");
        //     return true;
        // }

        public override void HandleMessage(string message)
        {
            SimplePSMState state = JsonUtility.FromJson<SimplePSMState>(message);
            // if (!messageFirstParsed) {
            //     if (!CheckConsistency(state)) {
            //         messageFirstParsed = false;
            //         return;
            //     }
            //     else {
            //         messageFirstParsed = true;
            //     }
            // }
            int currentIndex = 0;

            Debug.Log("Recieved from AMBF: " + state.GetStateJoint.Position);
            // Assuming correct order
             foreach (URDFJoint joint in independentJoints)
             {
                 if (joint.jointType == URDFJoint.JointType.Prismatic)
                 {
                     joint.SetJointValue(state.GetStateJoint.Position[currentIndex]);
                 }
                 else
                 {
                     joint.SetJointValue(state.GetStateJoint.Position[currentIndex] / (float)(Math.PI) * 180f);
                 }
                 currentIndex++;
             }
            // gizmo.position = transform.parent.parent.parent.parent.parent.GetComponent<ArmControllerClassic>().PSM_end.position;
            //jaw.SetJointValue(state.GetStateJaw.Position[0] / (float)(Math.PI) * 180f);
        }

        public override void SendJointStates(UdpJsonSender udpsend, bool moving)
        {
            if (moving)
            {
                List<float> js = new List<float>();
                foreach (URDFJoint joint in independentJoints)
                {
                    js.Add(joint.currentJointValue);
                }
                udpsend.states = js.ToArray();
            }
            else
            {
                udpsend.states = new float[] { };
            }
        }
    }

}
