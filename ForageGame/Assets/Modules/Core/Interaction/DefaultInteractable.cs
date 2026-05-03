using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Modules.Interaction
{
    public class DefaultInteractable : MonoBehaviour, IInteractable
    {
        [Header("Interaction Callbacks")]
        public UnityEvent onInteract; // Event to invoke when interacting
        public UnityEvent onFocus;
        public UnityEvent OnUnfocus;

        [Header("Debug")]
        [SerializeField] protected bool printInteractions = false;

        [Header("Prompt popup")]
        InteractablePrompt popupPrompt; // UI element to prompt the player to interact
        [SerializeField] protected bool doPopup;

        [Header("Outline")]
        OutlineObject outlineObject;
        [SerializeField] protected bool doOutline = true;
        [SerializeField] protected float outlineWidth = 5f;
        [SerializeField] protected Color outlineColor = Color.white;

        private void Start()
        {
            if (doPopup)
            {
                popupPrompt = GetComponentInChildren<InteractablePrompt>(true);
                if (popupPrompt == null) Debug.LogWarning("No InteractablePrompt found in children of " + gameObject.name + ". Please add one to use popup prompts.");
            }

            if (doOutline)
            {
                if (!TryGetComponent<OutlineObject>(out outlineObject))
                {
                    outlineObject = gameObject.AddComponent<OutlineObject>();
                }

                outlineObject.enabled = false;
                outlineObject.outlineInfo.outlineColor = outlineColor;
            }
        }

        public virtual void Interact()
        {
            onInteract?.Invoke();
            
            if(printInteractions) print("Interacting with " + gameObject.name);

            if(doOutline) outlineObject.AnimateBounce();
        }

        public virtual void Focus()
        {
            if (printInteractions) print("Focused on " + gameObject.name);

            onFocus?.Invoke();

            if (doPopup) popupPrompt?.Activate();

            if (doOutline) outlineObject.AnimateIn(outlineWidth);

        }

        public virtual void Unfocus()
        {
            if (printInteractions) print("Unfocused from " + gameObject.name);

            OnUnfocus?.Invoke();

            if (doPopup) popupPrompt?.Deactivate();

            if (doOutline) outlineObject.AnimateOut();
        }
    }
}