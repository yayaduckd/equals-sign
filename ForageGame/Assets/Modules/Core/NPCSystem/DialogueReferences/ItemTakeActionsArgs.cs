using TDK.ItemSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemTakeActionsArgs", menuName = "NpcSystem/ItemTakeActionsArgs")]
public class ItemTakeActionsArgs : ScriptableObject
{
    public ItemData item;
    public StoryFlag OnSuccess;

    [Tooltip("If true, the DialogueBox will remain open after the item is taken (and StoryStage progresses)")]
    public bool ContinuousDialogue = true;
    
}
