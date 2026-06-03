using UnityEngine;
using TDK.ItemSystem.Inventory;
using TDK.PlayerSystem;

namespace TDK.ItemSystem.Types
{
    [CreateAssetMenu(fileName = "New Bottle", menuName = "Items/Bottle")]
    public class Bottle : ItemData
    {
        [SerializeField] protected ItemData waterBottle;
        [SerializeField] protected ItemData pollenBottle;
        [SerializeField] protected ItemData sporeBottle;
        [SerializeField] protected ItemData fireflyBottle;

        public override bool TryWorldItemInteract()
        {
            if (!InventoryController.Instance.TryAddItemAtAny(this))
                return false;

            InventoryController.Instance.TryAddUnseenItem(this);
            return true;
        }

        public override bool TryUse()
        {
            Collider[] colliders = Physics.OverlapSphere(Player.Instance.transform.position, 3);

            foreach (Collider collider in colliders)
            {
                if (collider.TryGetComponent(out BottlePickupHandler handler))
                {
                    if (!InventoryController.Instance.TryRemoveItemAtCurrent(this))
                        return false;

                    if (!InventoryController.Instance.TryAddItemAtAny(handler.returnItem))
                        ItemServices.Instance?.SpawnItem(handler.returnItem, Player.Instance.transform.position);

                    return true;
                }
            }
            return false;

        }

        public bool TryUseBottleToGetItem(ItemData item)
        {
            if (!InventoryController.Instance.TryRemoveItemAtCurrent(this))
                return false;

            if (!InventoryController.Instance.TryAddItemAtAny(item))
                ItemServices.Instance?.SpawnItem(item, Player.Instance.transform.position);
            return true;
        }
    }
}