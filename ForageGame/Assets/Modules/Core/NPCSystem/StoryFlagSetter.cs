using UnityEngine;

public class StoryFlagSetter : MonoBehaviour
{
    [SerializeField] private StoryFlag flag;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetFlag()
    {
        StoryFlagManager.Instance.AddFlag(flag);
    }

    public void SetFlag(StoryFlag flag)
    {
        StoryFlagManager.Instance.AddFlag(flag);
    }
}
