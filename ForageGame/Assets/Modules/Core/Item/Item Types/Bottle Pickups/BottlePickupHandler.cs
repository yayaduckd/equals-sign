using Assets.Modules.Interaction;
using TDK.ItemSystem;
using TDK.ItemSystem.Inventory;
using TDK.PlayerSystem;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BottlePickupHandler : MonoBehaviour
{
    [SerializeField] public ItemData returnItem;
}