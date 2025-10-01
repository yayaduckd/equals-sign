using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Modules.Interaction
{
    public interface IInteractable
    {
        public void Interact(UnityAction StopInteractionCallback);
        public void StopInteract();
        public void Focus();
        public void Unfocus();
    }
}