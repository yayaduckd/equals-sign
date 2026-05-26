using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

// IMPORTANT: ItemRacks cannot overlap; this will result in breaking possibly everything!
namespace TDK.ItemSystem.Inventory
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(SplineContainer))]
    public class ItemRackController : MonoBehaviour
    {
        public enum Alignment { Left, Right, Center, Justified }
        [SerializeField] private Alignment _alignment = Alignment.Left;
        [SerializeField] private float suckDuration = 1f;

        [SerializeField] private List<ItemController> _itemControllers = new();
        private SplineContainer _splineContainer;

        void OnValidate()
        {
            _splineContainer = GetComponent<SplineContainer>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ItemController controller))
                AddItem(controller);
        }

        #region Set Spline Position

        public void RefreshVisuals()
        {
            float dt = 1f / Mathf.Max(1, _itemControllers.Count);

            for (int i = 0; i < _itemControllers.Count; i++)
            {
                Vector3 target = Vector3.zero;
                switch (_alignment)
                {
                    case Alignment.Left:
                        target = _splineContainer.EvaluatePosition(dt * i);
                        break;
                    case Alignment.Right:
                        target = _splineContainer.EvaluatePosition(dt * (i + 1));
                        break;
                    case Alignment.Center:
                        target = _splineContainer.EvaluatePosition(dt * i + dt / 2);
                        break;
                    case Alignment.Justified:
                        target = _splineContainer.EvaluatePosition(1 / (1 / dt - 1) * i);
                        break;
                }
                _itemControllers[i]?.MoveTo(target, suckDuration);
            }
        }

        #endregion

        #region Getting & Setting

        public List<ItemController> GetItemControllers() => new(_itemControllers);  // RETURNS A COPY

        public List<ItemData> GetItems() // RETURNS A COPY
        {
            List<ItemData> items = new();
            foreach (ItemController controller in GetItemControllers())
                items?.Add(controller.ItemData);
            return items;
        }

        public bool ContainsItem(ItemData item) => GetItems().Contains(item);

        public bool ContainsItems(List<ItemData> items)
        {
            List<ItemData> itemsCopy = GetItems();
            foreach (ItemData item in items)
            {
                if (!itemsCopy.Remove(item))
                    return false;
            }
            return true;
        }

        public bool ContainsItemsExactly(List<ItemData> items)
        {
            if (items.Count != GetItems().Count)
                return false;
            return ContainsItems(items);
        }

        public void AddItem(ItemController controller)
        {
            if (_itemControllers.Contains(controller))
                return;
            controller.SetPhysics(false);
            controller.SetShadow(false);
            _itemControllers.Add(controller);
            controller.OnDestroyEvent += RemoveItemVoid;
            RefreshVisuals();
        }

        public void RemoveItemVoid(ItemController controller) => RemoveItem(controller);

        public bool RemoveItem(ItemController controller)
        {
            if (!_itemControllers.Remove(controller))
                return false;
            controller.OnDestroyEvent -= RemoveItemVoid;
            RefreshVisuals();
            return true;
        }

        public bool RemoveItem(ItemData data)
        {
            ItemController controller = _itemControllers.Find(c => c.ItemData == data);
            return RemoveItem(controller);
        }

        // public bool RemoveAllItem(ItemData data)
        // {
        //     List<ItemController> controllers = _itemControllers.FindAll(c => c.ItemData == data);
        //     return RemoveItem(controller);
        // }

        public void RemoveItems(List<ItemData> items)
        {
            foreach (ItemData item in items)
                RemoveItem(item);
        }

        public void RemoveAll()
        {
            foreach (ItemController controller in _itemControllers)
                controller.OnDestroyEvent -= RemoveItemVoid;
            _itemControllers.Clear();
            RefreshVisuals();
        }

        void OnDestroy()
        {
            foreach (ItemController controller in _itemControllers)
                controller.OnDestroyEvent -= RemoveItemVoid;
        }

        #endregion
    }
}
