using System;
using System.Linq;
using DG.Tweening;
using TDK.ItemSystem.Types;
using TDK.SaveSystem;
using UnityEngine;

namespace TDK.ItemSystem
{
    public class ItemServices : MonoBehaviour, ILoadable
    {
        public static ItemServices Instance { get; private set; }
        void Awake()
        {
            if (Instance != null && Instance != this)
                Destroy(this);
            Instance = this;
        }

        [SerializeField] private GameObject _itemPrefab;
        [SerializeField] public ItemDatabase Database;

        #region Item Spawning

        public ItemController SpawnItem(ItemData itemData, Vector3 position)
        {
            return SpawnItem(itemData, position, Vector3.up);
        }
        public ItemController SpawnItem(ItemSaveData data)
        {
            return SpawnItem(data.GetItemData(), data.Position, new(0, 0, 0));
        }
        public ItemController SpawnItem(ItemData itemData, Vector3 position, Vector3 velocity)
        {
            GameObject gameObject = Instantiate(_itemPrefab, position, Quaternion.identity, transform);
            ItemController itemController = gameObject.GetComponent<ItemController>();
            itemController.Initialize(itemData, position, velocity);
            return itemController;
        }

        #endregion

        public void LoadData(WorldSaveData data)
        {
            foreach (ItemSaveData dataEntry in data.Items)
                SpawnItem(dataEntry);
        }
    }
}