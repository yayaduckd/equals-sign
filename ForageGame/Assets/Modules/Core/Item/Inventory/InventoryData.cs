using System.Collections.Generic;

namespace TDK.ItemSystem.Inventory
{
    [System.Serializable]
    public class InventorySaveData
    {
        public List<string> CollectedRecipes = new();
        public List<string> UsedRecipes = new();
        public List<string> SeenItems = new();
        public List<ItemSlotSaveData> Items = new() { new(), new(), new() }; // Start with 3 slots
    }
}