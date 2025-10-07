using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Modules.Interaction
{
    public class DefaultInteractable : MonoBehaviour, IInteractable
    {
        InteractablePrompt PopupPrompt; // UI element to prompt the player to interact
        AudioSource interactSound; // Sound to play when interacting

        private void Start()
        {
            PopupPrompt = GetComponentInChildren<InteractablePrompt>(true);
            interactSound = GetComponentInChildren<AudioSource>(true);
        }

        public void Focus()
        {
            print("Focused on " + gameObject.name);

            PopupPrompt?.Activate();
        }

        public void Unfocus()
        {
            print("Unfocused from " + gameObject.name);

            PopupPrompt?.Deactivate();
        }

        public void Interact(UnityAction StopInteractionCallback)
        {
            print("Interacting with " + gameObject.name);

            interactSound?.Play();
            GetComponentInChildren<Renderer>().material.color = Color.cyan;
        }

        public void StopInteract()
        {
            print("Stopped interacting with " + gameObject.name);

            interactSound?.Stop();
            GetComponentInChildren<Renderer>().material.color = Color.gray;
        }

    }
}