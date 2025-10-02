using Assets.Modules.Interaction;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    LayerMask interactablesLayer;
    [SerializeField] private float interactionRadius = 3f;

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
        Dictionary<IInteractable, Transform> currentNearbyInteractables = new();

        foreach (Collider col in nearbyInteractables)
        {
            IInteractable interactable;
            if(col.TryGetComponent<IInteractable>(out interactable)) 
            {
                currentNearbyInteractables.Add(interactable, col.transform);
            }
        }   

        if (currentNearbyInteractables.Count == 0)
        {
            Defocus();
            return;
        }

        EvaluateInteractableRelevance(currentNearbyInteractables);
        
    }

    private void EvaluateInteractableRelevance(Dictionary<IInteractable, Transform> nearbyInteractables)
    {
        float smallestLossFunction = Mathf.Infinity;
        //The most relevant interactable is the one which best minimizes the loss function
        (IInteractable interactable, Transform transform) mostRelevantInteractable = ( null, null);

        foreach (var interactable_ in nearbyInteractables)
        {
            IInteractable interactable = interactable_.Key;
            Transform interactableTransform = interactable_.Value;

            float distance = Vector3.Distance(transform.position, interactableTransform.transform.position);
            float normDistance = distance / interactionRadius;
            float angle = Vector3.Angle(transform.forward, (interactableTransform.transform.position - transform.position).normalized);
            float normAngle = angle / 180f; // Normalize angle to [0, 1]

            float lossFunction = Mathf.Pow(normAngle, 2) + normDistance; // The closer and more directly in front of the player, the better

            if (lossFunction < smallestLossFunction)
            {
                smallestLossFunction = lossFunction;
                mostRelevantInteractable = (interactable, interactableTransform);
            }
        }

        if (mostRelevantInteractable.interactable == null || mostRelevantInteractable.interactable == focusedInteractable) { return; }
        Focus(mostRelevantInteractable.interactable);
    }

    public void Interact()
    {
        if(focusedInteractable == null || interacting) { return; }

        interacting = true;
        focusedInteractable.Interact(StopInteract);
    }

    public void StopInteract()
    {
        if(!interacting) { return; }
        focusedInteractable?.StopInteract();
        interacting = false;
        ScanInteractables();
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
        StopInteract();
        focusedInteractable?.Unfocus();
        focusedInteractable = null;
    }
}
