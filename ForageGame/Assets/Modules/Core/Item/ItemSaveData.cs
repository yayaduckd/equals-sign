using UnityEngine;

namespace TDK.ItemSystem
{
    [System.Serializable]
    public class ItemSaveData
    {
        public string ItemId = null;
        public Vector3 Position = new();

        public ItemData GetItemData() => ItemServices.Instance.Database.GetAsset(ItemId);
    }
}