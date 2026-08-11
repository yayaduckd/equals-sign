using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

namespace TDK.ItemSystem.Inventory
{
    public class ItemSlot : MonoBehaviour
    {
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text itemQuantity;
        [SerializeField] private Image slotImage;
        [SerializeField] private Color selectedColor;
        [SerializeField] private Color notSelectedColor;

        public ItemData Item { get; private set; } = null;
        public int Quantity { get; private set; } = 0;

        public void Initialize()
        {
            SetItem(null, 0);
            SetSelected(false);
        }
        public void Initialize(ItemSlotSaveData data)
        {
            SetItem(data.GetItemData(), data.ItemQuantity);
            SetSelected(false);
        }

        private void SetItem(ItemData item, int quantity)
        {
            if (item == null || quantity <= 0)
            {
                Item = null;
                Quantity = 0;
            }
            else
            {
                Item = item;
                Quantity = Math.Max(0, quantity);
            }
            RefreshVisuals();
        }

        public bool IsEmpty() => Item == null || Quantity <= 0;
        public bool ContainsItem(ItemData item) => Item != null && Item == item;

        public bool TryAddQuantity(int quantity)
        {
            if (quantity < 0)
                return false;
            if (Item == null)
                return false;

            SetItem(Item, Quantity + quantity);
            return true;
        }

        public bool TryAddItem(ItemData item, int quantity)
        {
            if (item == null || quantity < 0) return false;

            if (IsEmpty())
            {
                SetItem(item, quantity);
                return true;
            }
            if (ContainsItem(item))
                return TryAddQuantity(quantity);

            return false;
        }

        public bool TryRemoveQuantity(int quantity)
        {
            if (quantity < 0)
                return false;
            if (Item == null)
                return false;
            if (Quantity - quantity < 0)
                return false;

            SetItem(Item, Quantity - quantity);
            return true;
        }

        public bool TryRemoveItem(ItemData item, int quantity)
        {
            if (item == null || quantity < 0) return false;

            if (IsEmpty())
                return false;
            if (ContainsItem(item))
                return TryRemoveQuantity(quantity);

            return false;
        }

        #region Visuals

        private Tween _tween;

        public void RefreshVisuals()
        {
            _tween?.Kill();
            if (IsEmpty())
                _tween = itemImage.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack);
            else
            {
                itemImage.sprite = Item.GetSprite();
                _tween = itemImage.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
            }

            if (Quantity <= 1)
                itemQuantity.enabled = false;
            else
            {
                itemQuantity.text = Quantity.ToString();
                itemQuantity.enabled = true;
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (isSelected)
                slotImage.color = selectedColor;
            else
                slotImage.color = notSelectedColor;
        }

        private void OnDestroy()
        {
            _tween?.Kill();
        }

        #endregion
    }
}