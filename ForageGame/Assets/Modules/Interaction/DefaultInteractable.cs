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

        public void Interact()
        {
            print("Interacted with " + gameObject.name);
        }

        public void Interact(UnityAction StopInteractionCallback)
        {
            throw new System.NotImplementedException();
        }

        public void StopInteract()
        {
            throw new System.NotImplementedException();
        }

        public void Unfocus()
        {
            print("Unfocused from " + gameObject.name);
        }
    }
}