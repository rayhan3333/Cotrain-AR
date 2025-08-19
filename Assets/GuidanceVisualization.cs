using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuidanceVisualization : MonoBehaviour
{
    public float fovRadians = 1.2f;       // AMBF vertical FOV in radians
    public float far = 1.0f;             // Far clip plane
    public float aspect = 16f / 9f;       // Assume default, or link to screen

    public Transform endoCam;

    public Transform psm1;
    public Transform psm1Marker;
    public Transform psm2;
    public Transform psm2Marker;

    private LineRenderer lr;

    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 8;
        lr.widthMultiplier = 0.01f;
        lr.loop = false;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = Color.green;

        DrawFrustum();
    }

    void DrawFrustum()
    {
        float halfHeight = Mathf.Tan(fovRadians / 2f) * far;
        float halfWidth = halfHeight * aspect;

        Vector3 topLeft = new Vector3(-halfWidth, halfHeight, far);
        Vector3 topRight = new Vector3(halfWidth, halfHeight, far);
        Vector3 bottomLeft = new Vector3(-halfWidth, -halfHeight, far);
        Vector3 bottomRight = new Vector3(halfWidth, -halfHeight, far);
        Vector3 camOrigin = Vector3.zero;

        Vector3[] points = new Vector3[]
        {
            camOrigin, topLeft,
            camOrigin, topRight,
            camOrigin, bottomLeft,
            camOrigin, bottomRight,
            topLeft, topRight,
            topRight, bottomRight,
            bottomRight, bottomLeft,
            bottomLeft, topLeft
        };

        lr.positionCount = points.Length;
        lr.SetPositions(points);
    }

    public static Vector3 GetClampedFrustumPoint(Vector3 worldPoint, Transform camTransform, float fov, float aspect)
    {
        Vector3 local = camTransform.InverseTransformPoint(worldPoint);

        float halfHeight = Mathf.Tan(fov / 2f) * local.z;
        float halfWidth = halfHeight * aspect;

        bool inside = local.z > 0 &&
                    Mathf.Abs(local.x) <= halfWidth &&
                    Mathf.Abs(local.y) <= halfHeight;

        if (inside)
            return worldPoint;

        float clampedX = Mathf.Clamp(local.x, -halfWidth, halfWidth);
        float clampedY = Mathf.Clamp(local.y, -halfHeight, halfHeight);
        float clampedZ = Mathf.Max(local.z, 0.0001f);

        Vector3 localClamped = new Vector3(clampedX, clampedY, clampedZ);
        return camTransform.TransformPoint(localClamped);
    }

    void Update()
    {
        transform.SetPositionAndRotation(endoCam.position, endoCam.rotation);
        psm1Marker.position = GetClampedFrustumPoint(psm1.position, endoCam, fovRadians, aspect);  
        psm2Marker.position = GetClampedFrustumPoint(psm2.position, endoCam, fovRadians, aspect); 
    }
}
