using Assets.Modules.Interaction;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    LayerMask interactablesLayer;
    [SerializeField] private float interactionRadius = 3f;

    private HashSet<Transform> interactablesInRange = new HashSet<Transform>();

    private IInteractable focusedInteractable;
    private Transform focusedInteractableTransform;

    bool interacting;
    float lastScanTime;
    float scanInterval = 0.2f; // Scan for interactables every 0.2 seconds

    private void Awake()
    {
        interactablesLayer = LayerMask.GetMask("Interactable");
    }

    private void Update()
    {
        if (!interacting && Time.time > lastScanTime + scanInterval)
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
        Collider[] nearbyInteractables = Physics.OverlapSphere(transform.position, interactionRadius, interactablesLayer);
        HashSet<Transform> currentNearbyInteractables = new();

        foreach (Collider col in nearbyInteractables)
        {
            if(col.GetComponent<IInteractable>() == null) { continue; }
            currentNearbyInteractables.Add(col.transform);
        }   

        HashSet<Transform> lostInteractables = new();
        lostInteractables = interactablesInRange.ExceptWith(currentNearbyInteractables);

        currentNearbyInteractables.UnionWith(interactablesInRange);

    }

    private void EvaluateInteractableRelevance()
    {
        float smallestLossFunction = Mathf.Infinity;
        //The most relevant interactable is the one which best minimizes the loss function
        Transform mostRelevantInteractableTransform = null;

        foreach (Collider col in nearbyInteractables)
        {
            IInteractable interactable = col.GetComponent<IInteractable>();
            if (interactable != null)
            {
                float distance = Vector3.Distance(transform.position, col.transform.position);
                float normDistance = distance / interactionRadius;
                float angle = Vector3.Angle(transform.forward, (col.transform.position - transform.position).normalized);
                float normAngle = angle / 180f; // Normalize angle to [0, 1]

                float lossFunction = Mathf.Pow(normAngle, 2) + normDistance; // The closer and more directly in front of the player, the better

                if (lossFunction < smallestLossFunction)
                {
                    smallestLossFunction = lossFunction;
                    mostRelevantInteractableTransform = col.transform;
                }
            }
        }

        if (mostRelevantInteractableTransform == null || mostRelevantInteractableTransform == focusedInteractableTransform) { return; }
        Focus(mostRelevantInteractable);
    }

    private void CheckOutOfRange()
    {
        if(Vector3.Distance(focusedInteractable.))
    }

    public void Interact()
    {
        if(focusedInteractable == null) { return; }

        interacting = true;
        focusedInteractable.Interact(StopInteract);
    }

    public void StopInteract()
    {
        focusedInteractable?.StopInteract();
        interacting = false;
    }

    /// <summary>
    /// Focuses on the given interactable, defocusing any previously focused interactable.
    /// </summary>
    /// <param name="interactable"></param>
    private void Focus(IInteractable interactable)
    {
        Defocus();
        focusedInteractable = interactable;
        focusedInteractable.Focus();
    }

    /// <summary>
    /// Defocuses the currently focused interactable, if any.
    /// </summary>
    private void Defocus()
    {
        focusedInteractable?.Unfocus();
        focusedInteractable = null;
    }
}
