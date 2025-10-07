using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Modules.Interaction
{
    public class DefaultInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] GameObject PopupPrompt; // UI element to prompt the player to interact
        [SerializeField] AudioSource interactSound; // Sound to play when interacting

        public void Focus()
        {
            print("Focused on " + gameObject.name);

            //PopupPrompt?.SetActive(true);
        }

        public void Unfocus()
        {
            print("Unfocused from " + gameObject.name);

            //PopupPrompt?.SetActive(false);
        }

        public void Interact(UnityAction StopInteractionCallback)
        {
            print("Interacting with " + gameObject.name);

            //interactSound?.Play();
        }

        public void StopInteract()
        {
            print("Stopped interacting with " + gameObject.name);
        }

    }
}