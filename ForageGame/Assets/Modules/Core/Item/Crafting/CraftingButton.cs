using Assets.Modules.Interaction;
using UnityEngine;
using UnityEngine.Events;

namespace TDK.ItemSystem.Inventory
{
    public class CraftingButton : DefaultInteractable
    {
        [SerializeField] Crafter crafter;

        public override void Interact()
        {
            base.Interact();
            crafter.TryCraft();
        }
    }
}