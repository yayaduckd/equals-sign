using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private SplineContainer _splineContainer;

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ItemController controller))
                AddItem(controller);
        }

        void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out ItemController controller))
                RemoveItem(controller);
        }

        #region Set Spline Position

        private void Refresh()
        {
            // Remove null
            _itemControllers.RemoveAll(controller => controller == null);

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
                _itemControllers[i].MoveTo(target, suckDuration);
            }
        }

        #endregion

        #region Getting & Setting

        public List<ItemController> GetItemControllers()
        {
            Refresh();
            return new(_itemControllers);  // RETURNS A COPY
        }

        public List<ItemData> GetItems() // RETURNS A COPY
        {
            List<ItemData> items = new();
            foreach (ItemController controller in GetItemControllers())
                items.Add(controller.ItemData);
            return items;
        }

        public bool ContainsItem(ItemData item) => GetItems().Contains(item);

        public bool ContainsItems(List<ItemData> items)
        {
            List<ItemData> itemsCopy = GetItems();
            foreach (ItemData item in items)
            {
                if (!itemsCopy.Remove(item)) // this is so that if we ask if you contain 2 of an item, we can check for 2 of those :)
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
            Refresh();
        }

        public void RemoveItemVoid(ItemController controller) => RemoveItem(controller);

        public bool RemoveItem(ItemController controller)
        {
            if (controller == null || !_itemControllers.Remove(controller))
            {
                Refresh();
                return false;
            }
            controller.OnDestroyEvent -= RemoveItemVoid;
            Refresh();
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
                RemoveItem(controller);
        }

        void OnDestroy()
        {
            foreach (ItemController controller in _itemControllers)
                if (controller != null)
                    controller.OnDestroyEvent -= RemoveItemVoid;
        }

        #endregion
    }
}
