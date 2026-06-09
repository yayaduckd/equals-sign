using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using TDK.SaveSystem;
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

        public void RefreshVisuals()
        {
            if (IsEmpty())
            {
                itemImage.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    itemImage.enabled = false;
                    itemQuantity.enabled = false;
                });
            }
            else
            {
                if(itemImage.sprite == Item.GetSprite() && itemImage.enabled)
                {
                    // If the same item is being updated, just do a quick scale animation to indicate the change
                    itemImage.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f, 1);
                }
                else
                {
                    // If a different item is being set, update the sprite and do a scale animation from 0 to 1
                    itemImage.sprite = Item.GetSprite();
                    itemImage.enabled = true;
                    itemImage.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).From(0);

                }

                if (Quantity == 1)
                    itemQuantity.enabled = false;
                else
                {
                    itemQuantity.text = Quantity.ToString();
                    itemQuantity.enabled = true;
                }
            }
        }

        public void SetSelected(bool isSelected)
        {
            if (isSelected)
                slotImage.color = selectedColor;
            else
                slotImage.color = notSelectedColor;
        }

        #endregion
    }
}