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

    //Set flag only if 'branchCondition' is an active flag, used for small branching paths
    public void SetBranchFlag(StoryFlag branchCondition)
    {
        if(!StoryFlagManager.Instance.FlagActive(branchCondition))
        {
            StoryFlagManager.Instance.AddFlag(flag);
        }
    }
}
