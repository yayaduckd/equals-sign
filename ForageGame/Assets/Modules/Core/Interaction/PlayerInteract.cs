using Assets.Modules.Interaction;
using System.Collections.Generic;
using UnityEngine;
using TDK.PlayerSystem;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayers;
    [SerializeField] private float interactionRadius = 3f;

    private IInteractable currentFocus;
    private Transform currentFocusTransform;

    public IInteractable GetCurrentFocus() => currentFocus;

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
        Dictionary<IInteractable, Transform> nearbyInteractables = new();
        Collider[] colliders = Physics.OverlapSphere(Player.Instance.transform.position, interactionRadius, interactableLayers);

        foreach (Collider col in colliders)
        {
            if (col.TryGetComponent(out IInteractable interactable))
                nearbyInteractables.Add(interactable, col.transform);
        }

        if (nearbyInteractables.Count == 0)
        {
            Defocus();
            return;
        }

        EvaluateInteractableRelevance(nearbyInteractables);
    }

    /// <summary>
    /// Runs through the list of nearby interactables and evaluates their relevance using a loss function based on distance and angle to the player, then focuses on the most relevant one if it is not already focused.
    /// </summary>
    /// <param name="nearbyInteractables">The list of interactables to consider when choosing the most relevant interactable to focus on. Keys should be IInteractables, values should be the Transforms they are attached to.</param>
    private void EvaluateInteractableRelevance(Dictionary<IInteractable, Transform> nearbyInteractables)
    {
        float smallestLossFunction = Mathf.Infinity;
        // The most relevant interactable is the one which best minimizes the loss function
        IInteractable mostRelevantInteractable = null;
        Transform mostRelevantInteractableTransform = null;

        foreach (var interactable_ in nearbyInteractables)
        {
            IInteractable interactable = interactable_.Key;
            Transform interactableTransform = interactable_.Value;

            Vector3 playerPos = Player.Instance.transform.position;
            float distance = Vector3.Distance(playerPos, interactableTransform.position);
            float normDistance = distance / interactionRadius;
            float angle = Vector3.Angle(Player.Instance.playerController.ViewDirection, (interactableTransform.position - playerPos).normalized);
            float normAngle = angle / 180f; // Normalize angle to [0, 1]

            float lossFunction = Mathf.Pow(normAngle, 2) + normDistance; // The closer and more directly in front of the player, the better

            if (lossFunction < smallestLossFunction)
            {
                smallestLossFunction = lossFunction;
                mostRelevantInteractable = interactable;
                mostRelevantInteractableTransform = interactableTransform;
            }
        }

        if (mostRelevantInteractable == null || mostRelevantInteractable == currentFocus) return;
        Focus(mostRelevantInteractable, mostRelevantInteractableTransform);
    }

    /// <summary>
    /// Used to interact with the currently focused interactable, if any.
    /// </summary>
    public void Interact()
    {
        if (currentFocus == null || currentFocusTransform == null) return;
        Player.Instance.playerController.ViewDirection = currentFocusTransform.position - Player.Instance.transform.position; //make duck face the target ~Lars
        currentFocus.AttemptInteract();
    }

    /// <summary>
    /// Focuses on the given interactable, defocusing any previously focused interactable.
    /// </summary>
    /// <param name="interactable"></param>
    private void Focus(IInteractable interactable, Transform transform)
    {
        Defocus();
        currentFocus = interactable;
        currentFocusTransform = transform;
        currentFocus.Focus();
    }

    /// <summary>
    /// Defocuses the currently focused interactable, if any.
    /// </summary>
    private void Defocus()
    {
        currentFocus?.Unfocus();
        currentFocus = null;
        currentFocusTransform = null;
    }
}
