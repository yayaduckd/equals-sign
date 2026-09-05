using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Very simple component that reacts to a flag being added
/// Invokes a UnityEvent to be as generic as possible
/// </summary>
public class StoryFlagListener : MonoBehaviour
{
    [SerializeField] private StoryFlag flag;

    public UnityEvent onFlagAdded;

    void OnEnable()
    {
        StoryFlagManager.onFlagAdded += onStoryFlagAdded;
    }
    void OnDisable()
    {
        StoryFlagManager.onFlagAdded -= onStoryFlagAdded;
    }

    private void onStoryFlagAdded(StoryFlag newFlag)
    {
        if (newFlag == flag)
        {
            onFlagAdded?.Invoke();
        }
    }
}
