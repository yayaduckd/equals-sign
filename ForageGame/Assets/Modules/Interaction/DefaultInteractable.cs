using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Modules.Interaction
{
    public class DefaultInteractable : MonoBehaviour, IInteractable
    {
        public void Focus()
        {
            print("Focused on " + gameObject.name);
        }

        public void Interact(UnityAction StopInteractionCallback)
        {
            print("Interacting with " + gameObject.name);
        }

        public void StopInteract()
        {
            print("Stopped interacting with " + gameObject.name);
        }

        public void Unfocus()
        {
            print("Unfocused from " + gameObject.name);
        }
    }
}