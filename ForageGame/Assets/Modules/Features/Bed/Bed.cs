using System.Threading.Tasks;
using Assets.Modules.Interaction;
using TDK.PlayerSystem;
using UnityEngine;

public class Bed : DefaultInteractable
{
    [SerializeField] private float _animationLength;
    public override void AttemptInteract()
    {
        base.AttemptInteract();
        _ = Sleep(); // honk-shew-mi-mi-mi
    }

    private async Task Sleep()
    {
        AppController.Instance.InputsAllActive(false);
        Player.Instance.gameObject.SetActive(false);
        await Task.Delay(Mathf.CeilToInt(_animationLength * 1000));
        await GameplayController.Instance.Sleep();
    }
}
