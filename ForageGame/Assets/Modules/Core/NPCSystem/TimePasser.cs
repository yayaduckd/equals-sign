using UnityEngine;

public class TimePasser : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PassTime()
    {
        StoryFlagManager.Instance.OnTimePassing();
    }
}
