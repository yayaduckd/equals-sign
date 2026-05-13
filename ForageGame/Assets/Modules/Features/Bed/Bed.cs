using Assets.Modules.Interaction;
using UnityEngine;

public class Bed : DefaultInteractable
{
    public override void AttemptInteract()
    {
        base.AttemptInteract();
       GameplayController.Instance.Sleep(); // hawk-shew-mi-mi-mi
    }
}
