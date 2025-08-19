using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using RosSharp;
using RosSharp.Urdf;
using RosSharp.RosBridgeClient;
using Unity.VisualScripting;

public enum Version {Classic, SI};
public class IKSolverURDF : MonoBehaviour
{
    public List<Transform> joints = new List<Transform>(); // Ordered from base to end-effector
    public List<MeshCollider> colliders = new List<MeshCollider>();

    public List<float> initStates = new List<float>();
    public Transform endEffector;
    public Transform target;
    public int maxIterations = 5;
    public float threshold = 0.05f;
    public float rotationSpeed = 2f;       // radians/sec
    public float translationSpeed = 0.1f;   // meters/sec

    public int tries = 0;
    private Vector3 lastPos;
    public bool isMoving;
    //VelocityController controller;

    //public collisionDetection colScript;

    public bool collision = false;

    private Vector3 initPose;
    private bool isSUJ = false;
    public Transform PSMTarget;
    public Transform PSM_end;
    public Version version = Version.SI;

    void Start()
    {
        // Classic Version Get Joints
        if (version == Version.Classic)
        {
            // SUJ Get Joints
            if (gameObject.name.Contains("C_"))
            {
                joints.Add(transform); // link 0
                joints.Add(transform.GetChild(2)); // link 1
                joints.Add(transform.GetChild(2).GetChild(2)); // link 2
                joints.Add(transform.GetChild(2).GetChild(2).GetChild(2)); // link 3
                isSUJ = true;

            }

            // PSM Get Joints
            if (gameObject.name.Contains("yaw"))
            {
                joints.Add(transform); // link 0
                joints.Add(transform.GetChild(2)); // link 1
                joints.Add(transform.GetChild(2).GetChild(2)); // link 2
                joints.Add(transform.GetChild(2).GetChild(2).GetChild(2)); // link 3
                joints.Add(transform.GetChild(2).GetChild(2).GetChild(2).GetChild(2)); // link 4 | is there better way?

            }

        }
        // SI Version Get Joints
        if (version == Version.SI)
        {
            // SUJ Get Joints
            if (gameObject.name.Contains("SUJ"))
            {
                joints.Add(transform); // link 0
                joints.Add(transform.GetChild(2)); // link 1
                joints.Add(transform.GetChild(2).GetChild(2)); // link 2
                joints.Add(transform.GetChild(2).GetChild(2).GetChild(2)); // link 3
                isSUJ = true;

            }

            // PSM Get Joints
            if (gameObject.name.Contains("PSM"))
            {
                joints.Add(transform); // link 0
                joints.Add(transform.GetChild(2)); // link 1
                joints.Add(transform.GetChild(2).GetChild(2)); // link 2
                joints.Add(transform.GetChild(2).GetChild(2).GetChild(2)); // link 3
                joints.Add(transform.GetChild(2).GetChild(2).GetChild(2).GetChild(2)); // link 4 | is there better way?

            }
        }
        foreach (Transform joint in joints)
        {


            MeshCollider meshCol = joint.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<MeshCollider>();
            MeshCollider newCol = joint.gameObject.AddComponent<MeshCollider>();
            newCol.sharedMesh = meshCol.sharedMesh;
            newCol.convex = meshCol.convex;
            newCol.isTrigger = meshCol.isTrigger;
            newCol.cookingOptions = meshCol.cookingOptions;
            Destroy(meshCol);

            newCol.isTrigger = true;

            collisionDetection colScript = joint.transform.gameObject.AddComponent<collisionDetection>();
            colScript.ikScript = gameObject.GetComponent<IKSolverURDF>();

            if (joint.TryGetComponent<UrdfJointPrismatic>(out var prismatic))
            {
                initStates.Add(prismatic.GetPosition());
            }

            if (joint.TryGetComponent<UrdfJointPrismatic>(out var revolute))
            {
                initStates.Add(revolute.GetPosition());
            }

        }
        foreach (var num in initStates)
        {
            Debug.Log(num);
        }
        initPose = target.position;
    }

    void LateUpdate()
    {
        if (Vector3.Distance(target.position, lastPos) > 0.0001f)
        {
            Debug.Log("Moving");
            isMoving = true;
            tries = 0;
            if (isSUJ)
            {
                PSMTarget.position = PSM_end.position;
            }
        }
        else
        {
            isMoving = false;

        }
        lastPos = target.position;

        if (isMoving)
        {
            // if (Vector3.Distance(target.position, lastPos) > 0.01f)
            // {
            //     //Debug.Log("Moving");
            //     tries = 0;
            // }
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
                for (int i = joints.Count - 1; i >= 0; i--)
                {
                    Transform joint = joints[i];
                    Vector3 toEffector = endEffector.position - joint.position;
                    Vector3 toTarget = target.position - joint.position;

                    if (Vector3.Distance(endEffector.position, target.position) < threshold)
                        return;

                    if (joint.TryGetComponent<UrdfJointRevolute>(out var revolute))
                    {
                        var hinge = revolute.GetComponent<HingeJoint>();
                        Vector3 axis = joint.TransformDirection(hinge.axis);

                        Vector3 projEff = Vector3.ProjectOnPlane(toEffector, axis);
                        Vector3 projTgt = Vector3.ProjectOnPlane(toTarget, axis);

                        float angleDeg = -Vector3.SignedAngle(projEff, projTgt, axis);
                        float angleRad = Mathf.Deg2Rad * angleDeg;

                        float step = Mathf.Clamp(angleRad, -rotationSpeed * Time.deltaTime, rotationSpeed * Time.deltaTime);

                        float currentAngle = revolute.GetPosition(); // in radians
                        float newAngle = currentAngle + step;

                        var limits = joint.GetComponent<HingeJointLimitsManager>();
                        newAngle = Mathf.Clamp(newAngle, limits.LargeAngleLimitMin, limits.LargeAngleLimitMax);

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
                        if (collision)
                        {
                            revolute.GetComponent<JointStateWriter>().Write(currentAngle);
                            target.position = initPose;
                            collision = false;

                        }
                        //Velocity Update
                        //joint.GetComponent<VelocityController>().SetState(newAngle);

                        //Debug.Log(joint.gameObject.name + " (revolute): " + newAngle + " rad, Iteration: " + iteration);
                    }
                    else if (joint.TryGetComponent<UrdfJointPrismatic>(out var prismatic))
                    {
                        var cj = prismatic.GetComponent<ConfigurableJoint>();
                        Vector3 axis = joint.TransformDirection(cj.axis.normalized);

                        Vector3 projected = Vector3.Project(toTarget - toEffector, axis);
                        float moveAmount = projected.magnitude * Mathf.Sign(Vector3.Dot(projected, axis));

                        float step = Mathf.Clamp(moveAmount, -translationSpeed * Time.deltaTime, translationSpeed * Time.deltaTime);

                        float currentTranslation = prismatic.GetPosition(); // in meters
                        float newTranslation = currentTranslation + step;

                        var limits = joint.GetComponent<PrismaticJointLimitsManager>();
                        newTranslation = Mathf.Clamp(newTranslation, limits.PositionLimitMin, limits.PositionLimitMax);

                        prismatic.GetComponent<JointStateWriter>().Write(newTranslation);
                        //Debug.Log(joint.gameObject.name + " (prismatic): " + newTranslation + " m, Iteration: " + iteration);
                    }
                }
            }
            tries++;
        }

    }
}