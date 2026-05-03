using Assets.Modules.Interaction;
using UnityEngine;

public class Bed : DefaultInteractable
{
    public override void Interact()
    {
        base.Interact();
       GameplayController.Instance.Sleep(); // hawk-shew-mi-mi-mi
    }
}
