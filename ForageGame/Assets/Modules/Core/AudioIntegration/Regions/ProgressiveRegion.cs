using UnityEngine;
using System.Collections.Generic;
using System.Linq;


//yes, the 'serializable dictionary' trick again
[System.Serializable]
public class StoryRegionEntry
{
    public StoryFlag flag;
    public Region region;
}

/// <summary>
/// Holds multiple regions, and only activates the one corresponding to the latest active story flag
/// </summary>
public class ProgressiveRegion : MonoBehaviour
{
    [Tooltip("Order matters, the latest active flag in this list has priority")]
    [SerializeField] private List<StoryRegionEntry> orderedRegionStages = new();
    // Start is called once before the first execution of Update after the MonoBehaviour is created


    void Awake()
    {
        StoryFlagManager.onFlagAdded += onStoryFlagAdded;
        RefreshActiveRegion(); //should not matter, but is a fallback
    }

    private void onStoryFlagAdded(StoryFlag newFlag)
    {
        if (orderedRegionStages.Any(e => e.flag == newFlag))
        {
            RefreshActiveRegion();
        }
    }

    private void RefreshActiveRegion()
    {
        Region target = orderedRegionStages[0].region;

        // Walk from the END of the list, since later index = later story progress.
        // First active flag we hit from the back IS the "latest active" one.
        for (int i = orderedRegionStages.Count - 1; i >= 0; i--)
        {
            var entry = orderedRegionStages[i];
            if (entry.flag != null && StoryFlagManager.Instance.FlagActive(entry.flag))
            {
                target = entry.region;
                break;
            }
        }

        foreach (var entry in orderedRegionStages)
        {
            if (entry.region == null) continue;
            entry.region.gameObject.SetActive(entry.region == target);
        }
    }
       
}
