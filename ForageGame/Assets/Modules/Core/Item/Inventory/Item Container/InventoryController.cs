using UnityEngine;
using System.Collections.Generic;
using TDK.SaveSystem;
using TDK.PlayerSystem;
using System;
using DG.Tweening;
using System.Linq;

namespace TDK.ItemSystem.Inventory
{
    public class InventoryController : ItemContainer, ILoadable, ISaveable
    {
        public static InventoryController Instance;
        [SerializeField] private int initialSlotCount = 3;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private RectTransform _belt;
        [SerializeField] private float _beltOffsetWidth;
        [SerializeField] private float _beltExtensionWidth;
        [SerializeField] private Transform _itemSlotsParent;
        [SerializeField] public ItemPickupUI itemPickupUI;

        public static event Action<ItemData> onNewItemSeen;

        public HashSet<ItemData> seenItems = new();

        // void OnValidate()
        // {
        //     Vector2 sizeDelta = _belt.sizeDelta;
        //     sizeDelta.x = _beltOffsetWidth + initialSlotCount * _beltExtensionWidth;
        //     _belt.sizeDelta = sizeDelta;
        // }

        public void TryAddUnseenItem(ItemData item)
        {
            if (!seenItems.Contains(item))
            {
                Debug.Log($"[InventoryController] New item seen: {item.name}");
                seenItems.Add(item);
                itemPickupUI.TriggerNewItemPopup(item);
                onNewItemSeen?.Invoke(item);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;

            Initialize(initialSlotCount);
        }

        public void Initialize(int slotCount)
        {
            Slots.Clear();
            foreach (Transform child in _itemSlotsParent) Destroy(child.gameObject);
            AddSlots(slotCount, true, false);
            currentSlotIndex = 0;
            SelectSlot(currentSlotIndex);
        }
        public void Initialize(List<ItemSlotSaveData> data)
        {
            Slots.Clear();
            foreach (Transform child in _itemSlotsParent) Destroy(child.gameObject);

            foreach (ItemSlotSaveData dataEntry in data)
            {
                ItemSlot slot = AddSlot(false);
                slot.Initialize(dataEntry);
            }
            AdjustBeltVisual(false);
            currentSlotIndex = 0;
            SelectSlot(currentSlotIndex);
        }

        public ItemSlot AddSlot(bool refreshVisual = true, bool animateVisual = true)
        {
            GameObject slotObject = Instantiate(slotPrefab, _itemSlotsParent);
            ItemSlot slot = slotObject.GetComponent<ItemSlot>();
            slot.Initialize();
            Slots.Add(slot);
            if (refreshVisual) AdjustBeltVisual(animateVisual);
            return slot;
        }
        public void AddSlots(int count, bool refreshVisual = true, bool animateVisual = true)
        {
            for (int i = 0; i < count; i++)
                AddSlot(false);
            if (refreshVisual) AdjustBeltVisual(animateVisual);
        }

        private void AdjustBeltVisual(bool animate = true)
        {
            Vector2 sizeDelta = _belt.sizeDelta;
            sizeDelta.x = _beltOffsetWidth + Slots.Count * _beltExtensionWidth;
            if (animate) _belt.DOSizeDelta(sizeDelta, 1f);
            else _belt.sizeDelta = sizeDelta;
        }

        #region Triggers

        public bool TryUseItem()
        {
            if (!IsSlotValid(currentSlotIndex))
                return false;
            if (Slots[currentSlotIndex].IsEmpty())
                return false;
            return Slots[currentSlotIndex].Item.TryUse();
        }

        public bool TryDropItem()
        {
            if (!IsSlotValid(currentSlotIndex))
                return false;
            ItemData item = Slots[currentSlotIndex].Item; // do this because the item will be removed in the next step

            if (!TryRemoveAnyAtCurrent())
                return false;

            ItemServices.Instance.SpawnItem(item, Player.Instance.transform.position);
            return true;
        }

        #endregion

        #region Save & Load

        public void LoadData(WorldSaveData data)
        {
            Initialize(data.Inventory.Items);
            seenItems = ItemServices.Instance.Database.GetAssets(data.Inventory.SeenItems).ToHashSet();
        }

        public void SaveData(ref WorldSaveData data)
        {
            List<ItemSlotSaveData> saveItems = new();
            foreach (ItemSlot itemslot in Slots)
            {
                saveItems.Add(new()
                {
                    ItemId = itemslot.IsEmpty() ? null : itemslot.Item.GetId(),
                    ItemQuantity = itemslot.IsEmpty() ? 0 : itemslot.Quantity
                });
            }
            data.Inventory.Items = saveItems;
            data.Inventory.SeenItems = ItemServices.Instance.Database.GetIds(seenItems).ToList();
        }

        #endregion
    }
}