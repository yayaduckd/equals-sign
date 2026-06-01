using UnityEngine;
using UnityEngine.Events;
 
/// <summary>
/// Used for the tutorial for the dialogue when approaching the wall around the island
/// </summary>
public class TriggerZoneCoordinator : MonoBehaviour
{
    [Tooltip("Fired once when the player enters any child zone (0 → 1 overlap).")]
    public UnityEvent OnFirstEntered;
 
    [Tooltip("Fired once when the player leaves all child zones (1 → 0 overlaps).")]
    public UnityEvent OnLastExited;
 
    [SerializeField] private int _overlapCount;
 
    /// <summary>Called by child TriggerZoneReporters when a tagged object enters them.</summary>
    public void ReportEnter()
    {
        _overlapCount++;
        if (_overlapCount == 1)
            OnFirstEntered.Invoke();
    }
 
    /// <summary>Called by child TriggerZoneReporters when a tagged object exits them.</summary>
    public void ReportExit()
    {
 
        // Guard against going below 0 (e.g. if a zone is disabled while player is inside)
        if (_overlapCount == 0) return;
        _overlapCount--;
        if (_overlapCount == 0)
            OnLastExited.Invoke();
    }
 
    /// <summary>
    /// Resets the overlap counter, e.g. when the tutorial ends and zones are disabled.
    /// Call this before disabling/destroying child zones to avoid a stuck state.
    /// </summary>
    public void ResetCounter()
    {
        bool wasInside = _overlapCount > 0;
        _overlapCount = 0;
        if (wasInside)
            OnLastExited.Invoke();
    }
}