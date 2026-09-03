// using System.Collections;
// using UnityEngine;
// using UnityEngine.Events;

// namespace Assets.Modules.Interaction
// {
//     public interface IInteractable
//     {
//         /// <summary>
//         /// Called by <see cref="PlayerInteract"/> when an interaction is started.
//         /// </summary>
//         public void AttemptInteract();

//         /// <summary>
//         /// This is called by <see cref="PlayerInteract"/> when this interactable is focused, and should handle any visual or audio feedback to indicate this interactable is now focused.
//         /// </summary>
//         public void Focus();

//         /// <summary>
//         /// This is called by <see cref="PlayerInteract"/> when this interactable is defocused, and should handle any visual or audio feedback to indicate this interactable is no longer focused.
//         /// </summary>
//         public void Unfocus();
//     }
// }