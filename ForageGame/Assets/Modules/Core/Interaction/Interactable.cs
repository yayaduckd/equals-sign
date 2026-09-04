using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace TDK.InteractionSystem
{
    public class Interactable : MonoBehaviour
    {
        [Header("Interaction Callbacks")]
        public UnityEvent onInteract; // Event to invoke when interacting
        public UnityEvent onFocus;
        public UnityEvent OnUnfocus;

        [Header("Debug")]
        [SerializeField] private bool printInteractions = false;
        public bool _isInteractable { get; private set; } = true;
        private bool _isFocused = false;

        public void Interact()
        {
            if (!_isInteractable) return;
            if (printInteractions) print("Interacting with " + gameObject.name);
            onInteract?.Invoke();
        }

        public void Focus()
        {
            if (!_isInteractable) return;
            if (_isFocused) return;
            _isFocused = true;
            if (printInteractions) print("Focused on " + gameObject.name);
            onFocus?.Invoke();
        }

        public void Unfocus()
        {
            if (!_isInteractable) return;
            if (!_isFocused) return;
            _isFocused = false;
            if (printInteractions) print("Unfocused from " + gameObject.name);
            OnUnfocus?.Invoke();
        }

        public void SetInteractibility(bool isInteractable)
        {
            if (_isFocused && !isInteractable) Unfocus();
            _isInteractable = isInteractable;
            this.enabled = isInteractable;
        }

        // for unity events
        public void EnableInteraction() => SetInteractibility(true);
        public void DisableInteraction() => SetInteractibility(false);
    }
}

// ------------------------------------------------------------------------------ OLD:

// using System.Collections;
// using UnityEngine;
// using UnityEngine.Events;

// namespace Assets.Modules.Interaction
// {
//     public class DefaultInteractable : MonoBehaviour, IInteractable
//     {
//         [Header("Interaction Callbacks")]
//         public UnityEvent onInteract; // Event to invoke when interacting
//         public UnityEvent onFocus;
//         public UnityEvent OnUnfocus;

//         [Header("Debug")]
//         [SerializeField] protected bool printInteractions = false;

//         [Header("Prompt popup")]
//         InteractablePrompt popupPrompt; // UI element to prompt the player to interact
//         [SerializeField] protected bool doPopup = false;

//         [Header("Outline")]
//         protected OutlineObject outlineObject;
//         [Tooltip("The object containing the renderers as children. Default to self.")][SerializeField] GameObject visualsObject;
//         [SerializeField] protected bool doOutline = true;
//         [SerializeField] protected float outlineWidth = 10f;
//         [SerializeField] protected Color outlineColor = Color.white;

//         protected virtual void Start()
//         {
//             if (doPopup)
//             {
//                 popupPrompt = GetComponentInChildren<InteractablePrompt>(true);
//                 if (popupPrompt == null) Debug.LogWarning("No InteractablePrompt found in children of " + gameObject.name + ". Please add one to use popup prompts.");
//             }

//             if (doOutline)
//             {
//                 if (visualsObject == null) visualsObject = gameObject;

//                 if (!visualsObject.TryGetComponent<OutlineObject>(out outlineObject))
//                 {
//                     print("Adding outline component");
//                     outlineObject = visualsObject.AddComponent<OutlineObject>();
//                 }

//                 outlineObject.enabled = false;
//                 outlineObject.outlineInfo.outlineColor = outlineColor;
//             }
//         }

//         public virtual void AttemptInteract()
//         {
//             // Default is always successful. Override this method to add conditions for interaction success.
//             SuccessfulInteract();
//         }

//         protected virtual void SuccessfulInteract()
//         {
//             if (printInteractions) print("Interacting with " + gameObject.name);
//             if (doOutline) outlineObject.AnimateSuccess();
//             onInteract?.Invoke();
//         }

//         protected virtual void FailedInteract()
//         {
//             if (printInteractions) print("Failed to interact with " + gameObject.name);
//             if (doOutline) outlineObject.AnimateFailure();
//         }

//         public virtual void Focus()
//         {
//             if (printInteractions) print("Focused on " + gameObject.name);
//             if (doOutline) outlineObject.AnimateIn(outlineWidth);
//             if (doPopup) popupPrompt?.Activate();
//             onFocus?.Invoke();
//         }

//         public virtual void Unfocus()
//         {
//             if (printInteractions) print("Unfocused from " + gameObject.name);
//             if (doOutline) outlineObject.AnimateOut();
//             if (doPopup) popupPrompt?.Deactivate();
//             OnUnfocus?.Invoke();
//         }


//         /// <summary>
//         /// Additions to disable readables. Does not actually disable the interactable component, just turns the outline off
//         /// </summary>

//         public virtual void DisableOutline()
//         {
//             if (printInteractions) print("Outline disabled for " + gameObject.name);

//             // I am no longer asking
//             //This is because these stupid ass outlines only enable themselves after the first frame
//             //for some reason, so this nullreferences in those cases
//             //~Lars
//             try
//             {
//                 if (doOutline) outlineObject.AnimateOut();
//                 if (doPopup) popupPrompt?.Deactivate();
//             }
//             catch (System.Exception e)
//             {
//                 Debug.LogError($"Error while disabling outline or popup for {gameObject.name}: {e}");
//             }
//             doOutline = false;
//             doPopup = false;
//         }

//         public virtual void EnableOutline()
//         {
//             if (printInteractions) print("Outline enabled for " + gameObject.name);

//             doOutline = true;
//             doPopup = true;
//         }
//     }
// }