using TDK.ItemSystem;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemTakeActionsArgs", menuName = "NpcSystem/ItemTakeActionsArgs")]
public class ItemTakeActionsArgs : ScriptableObject
{
    public ItemData item;
    public StoryFlag OnSuccess;
    
}
