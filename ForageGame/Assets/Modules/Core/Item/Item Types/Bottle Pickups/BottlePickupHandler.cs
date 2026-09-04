using TDK.ItemSystem;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BottlePickupHandler : MonoBehaviour
{
    [SerializeField] public ItemData returnItem;
}