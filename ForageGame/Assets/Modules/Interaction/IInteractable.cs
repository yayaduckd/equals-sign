using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Modules.Interaction
{
    public interface IInteractable
    {
        /// <summary>
        /// Called by <see cref="PlayerInteract"/> when an interaction is started.
        /// </summary>
        /// <param name="StopInteractionCallback">This callback should be invoked when the interaction is done.</param>
        public void Interact(UnityAction StopInteractionCallback);

        /// <summary>
        /// This is called by <see cref="PlayerInteract"/> when an interaction is stopped, and should halt any ongoing interaction.
        /// </summary>
        public void StopInteract();

        /// <summary>
        /// This is called by <see cref="PlayerInteract"/> when this interactable is focused, and should handle any visual or audio feedback to indicate this interactable is now focused.
        /// </summary>
        public void Focus();

        /// <summary>
        /// This is called by <see cref="PlayerInteract"/> when this interactable is defocused, and should handle any visual or audio feedback to indicate this interactable is no longer focused.
        /// </summary>
        public void Unfocus();
    }
}