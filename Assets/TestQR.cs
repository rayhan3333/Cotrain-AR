using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.OpenXR;
using UnityEngine.XR.ARSubsystems;

public class TestQR : MonoBehaviour
{
    [SerializeField] private ARMarkerManager markerManager;
    [SerializeField] private GameObject robotPrefab;

    [SerializeField] private bool destroyTrackablesAfterDisable = true;

    private bool robotSpawned;
    private bool pendingHandle;
    private Pose pendingPose;
    private ARMarker pendingMarker;

    void Awake()
    {
        if (markerManager == null)
            markerManager = FindObjectOfType<ARMarkerManager>(true);
    }

    void OnEnable()
    {
        if (markerManager == null)
        {
            enabled = false;
            return;
        }
        markerManager.markersChanged += OnMarkersChanged;
    }

    void OnDisable()
    {
        if (markerManager != null)
            markerManager.markersChanged -= OnMarkersChanged;
    }

    private void OnMarkersChanged(ARMarkersChangedEventArgs args)
    {
        if (!robotSpawned && args.added.Count > 0)
        {
            var marker = args.added[0];
            pendingPose = new Pose(marker.transform.position, marker.transform.rotation);
            pendingMarker = marker;
            pendingHandle = true;
        }
    }

    void Update()
    {
        if (!pendingHandle) return;
        pendingHandle = false;
        StartCoroutine(CoSpawnThenDisable());
    }

    private IEnumerator CoSpawnThenDisable()
    {
        if (robotPrefab != null)
        {
            robotPrefab.transform.SetPositionAndRotation(pendingPose.position, pendingPose.rotation);
            var e = robotPrefab.transform.eulerAngles;
            e.x = -90f;
            robotPrefab.transform.rotation = Quaternion.Euler(e);
        }
        Debug.Log("[TestQR_Safe] ROBOT SPAWNED");
        robotSpawned = true;

        yield return new WaitForEndOfFrame();

        if (markerManager != null)
            markerManager.enabled = false;

        if (destroyTrackablesAfterDisable && markerManager != null)
        {
            var toDestroy = new List<ARMarker>();
            foreach (var t in markerManager.trackables) if (t) toDestroy.Add(t);
            foreach (var t in toDestroy) if (t) Destroy(t.gameObject);
        }

        Debug.Log("[TestQR_Safe] Marker manager disabled and trackables cleared.");
    }
}
