using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Story/Flag")]
public class StoryFlag : ScriptableObject
{
    // cons
    public StoryFlag(string id)
    {
        this.id = id;
    }
    //must string match with the event flags in Dialogue.txt
    public string id;
}
