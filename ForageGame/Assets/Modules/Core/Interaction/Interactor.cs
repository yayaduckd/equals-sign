using System.Collections.Generic;
using UnityEngine;
using TDK.PlayerSystem;

namespace TDK.InteractionSystem
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private LayerMask interactableLayers;
        [SerializeField] private float interactionRadius = 3f;
        public Interactable _currentFocus { get; private set; } = null;

        private float lastScanTime = 0;
        private readonly float scanInterval = 0.2f; // Scan for interactables every 0.2 seconds

        private void Update()
        {
            if (Time.time > lastScanTime + scanInterval)
            {
                lastScanTime = Time.time;
                ScanInteractables();
            }
        }

        /// <summary>
        /// Finds all interactables within range and focuses on the most relevant one as determined by a loss function.
        /// </summary>
        private void ScanInteractables()
        {
            if (!AppController.Instance.IsInputsActive)
            {
                Defocus();
                return;
            }

            List<Interactable> nearbyInteractables = new();
            Collider[] colliders = Physics.OverlapSphere(Player.Instance.transform.position, interactionRadius, interactableLayers);

            foreach (Collider col in colliders)
            {
                if (col.TryGetComponent(out Interactable interactable))
                    if (interactable.isActiveAndEnabled)
                        nearbyInteractables.Add(interactable);
            }

            if (nearbyInteractables.Count == 0)
                Defocus();
            else
                EvaluateInteractableRelevance(nearbyInteractables);
        }

        /// <summary>
        /// Runs through the list of nearby interactables and evaluates their relevance using a loss function based on distance and angle to the player, then focuses on the most relevant one if it is not already focused.
        /// </summary>
        /// <param name="nearbyInteractables">The list of interactables to consider when choosing the most relevant interactable to focus on. Keys should be IInteractables, values should be the Transforms they are attached to.</param>
        private void EvaluateInteractableRelevance(List<Interactable> interactables)
        {
            float minLossFunction = Mathf.Infinity;
            // The most relevant interactable is the one which best minimizes the loss function
            Interactable mostRelevantInteractable = null;
            Vector3 playerPos = Player.Instance.transform.position;

            foreach (Interactable interactable in interactables)
            {
                // float distance = Vector3.Distance(playerPos, interactableTransform.position);
                // float normDistance = distance / interactionRadius;
                // float angle = Vector3.Angle(Player.Instance.playerController.ViewDirection, (interactableTransform.position - playerPos).normalized);
                // float normAngle = angle / 180f; // Normalize angle to [0, 1]
                // float lossFunction = Mathf.Pow(normAngle, 2) + normDistance; // The closer and more directly in front of the player, the better
                float lossFunction = Mathf.Pow(Vector3.Angle(Player.Instance.playerController.ViewDirection, (interactable.transform.position - playerPos).normalized) / 180f, 2) + Vector3.Distance(playerPos, interactable.transform.position) / interactionRadius;
                if (minLossFunction > lossFunction)
                {
                    minLossFunction = lossFunction;
                    mostRelevantInteractable = interactable;
                }
            }
            if (mostRelevantInteractable == _currentFocus) return;
            Focus(mostRelevantInteractable);
        }

        /// <summary>
        /// Used to interact with the currently focused interactable, if any.
        /// </summary>
        public void Interact()
        {
            if (_currentFocus == null || !_currentFocus._isInteractable || !AppController.Instance.IsInputsActive) return;
            Player.Instance.playerController.ViewDirection = _currentFocus.transform.position - Player.Instance.transform.position; //make duck face the target ~Lars
            _currentFocus.Interact();
        }

        /// <summary>
        /// Focuses on the given interactable, defocusing any previously focused interactable.
        /// </summary>
        /// <param name="interactable"></param>
        private void Focus(Interactable interactable)
        {
            if (interactable == null || interactable == _currentFocus || !interactable._isInteractable) return;
            Defocus();
            _currentFocus = interactable;
            _currentFocus.Focus();
        }

        /// <summary>
        /// Defocuses the currently focused interactable, if any.
        /// </summary>
        private void Defocus()
        {
            if (_currentFocus == null) return;
            _currentFocus?.Unfocus();
            _currentFocus = null;
        }
    }
}