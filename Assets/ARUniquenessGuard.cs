using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.OpenXR;                 // ARMarkerManager
using Microsoft.MixedReality.OpenXR.ARSubsystems;   // XRMarkerSubsystem

[DefaultExecutionOrder(-1000)]
public class ARUniquenessGuard : MonoBehaviour
{
    [Tooltip("If left empty, the script will FindObjectOfType<ARMarkerManager>().")]
    public ARMarkerManager markerManager;

    IEnumerator Start()
    {
        // Give the provider/managers one frame to initialize
        yield return null;

        if (markerManager == null)
            markerManager = FindObjectOfType<ARMarkerManager>(includeInactive: true);

        if (markerManager == null)
        {
            Debug.LogError("[ClearAllMarkersOnStart] No ARMarkerManager found in scene.");
            yield break;
        }

        // Disable manager to avoid producing a change list this frame
        markerManager.enabled = false;
        yield return new WaitForEndOfFrame();

        // Destroy any spawned trackable GameObjects (safety)
        // NOTE: trackables is an IEnumerable; copy to a temp list first.
        var toDestroy = new List<ARMarker>();
        foreach (var t in markerManager.trackables) toDestroy.Add(t);
        foreach (var t in toDestroy) if (t) Destroy(t.gameObject);
        yield return null;

        // Stop and restart all XRMarkerSubsystems to clear provider state/IDs
        var subs = new List<XRMarkerSubsystem>();
        UnityEngine.SubsystemManager.GetSubsystems(subs);
        foreach (var s in subs) { try { s.Stop(); } catch { } }
        yield return null;
        foreach (var s in subs) { try { s.Start(); } catch { } }

        // Re-enable the manager
        markerManager.enabled = true;

        Debug.Log("[ClearAllMarkersOnStart] Cleared marker trackables and restarted marker subsystem.");
    }
}
