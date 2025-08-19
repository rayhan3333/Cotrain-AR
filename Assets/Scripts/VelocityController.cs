using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities.Gltf.Schema;
using UnityEngine;

public class VelocityController : MonoBehaviour
{
    private float target;
    private Joint joint;
    private float maxVelocity = 50f;
    private float power = 10f;

    // Start is called before the first frame update
    void Start()
    {
        if (gameObject.TryGetComponent<HingeJoint>(out HingeJoint hingeTry1))
        {
            joint = hingeTry1;
        }
        if (joint is HingeJoint hingeTry)
        {
            hingeTry.useMotor = true;


        }

        if (gameObject.TryGetComponent<HingeJoint>(out HingeJoint cfgTry1))
        {
            joint = cfgTry1;
        }
        if (joint is HingeJoint cfgTry)
        {
            cfgTry.useMotor = true;
            target = cfgTry.angle;

        }

        

    }

    // Update is called once per frame
    void Update()
    {
        if (joint is HingeJoint hinge)
        {
            float error = target - hinge.angle;
            float velocity = Mathf.Clamp(error * power, -maxVelocity, maxVelocity);

            JointMotor motor = hinge.motor;
            motor.targetVelocity = velocity;
            motor.force = 1000f;
            hinge.motor = motor;

        }
        if (joint is ConfigurableJoint cfg)
        {
            Vector3 anchorWorld = joint.transform.TransformPoint(joint.anchor);
            Vector3 connectedAnchorWorld = joint.connectedBody.transform.TransformPoint(joint.connectedAnchor);

            float position = Vector3.Dot(anchorWorld - connectedAnchorWorld, joint.axis.normalized);
            float velocity = Mathf.Clamp((target - position) * power, -maxVelocity, maxVelocity);
            /*JointMotor motor = cfg.;
            motor.targetVelocity = velocity;
            motor.force = 1000f;
            cfg.motor = motor;*/
        }

       
    }

    public void SetState(float pos)
    {
        target = pos;
    }
}
