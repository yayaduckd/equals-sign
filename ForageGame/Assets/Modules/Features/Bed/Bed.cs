using System.Threading.Tasks;
using TDK.PlayerSystem;
using UnityEngine;

public class Bed : MonoBehaviour
{
    [SerializeField] private float _animationLength;
    public void Interact()
    {
        if (!AppController.Instance.IsInputsActive) return;
        _ = Sleep(); // honk-shew-mi-mi-mi
    }

    private async Task Sleep()
    {
        AppController.Instance.SetInputsActive(false);

        // healing process
        Player.Instance.energy.TakeDamage(-9999);
        Player.Instance.energy.AddEnergy(9999);

        // animation process
        Player.Instance.playerController.IsSleeping(true);
        await Task.Delay(Mathf.CeilToInt(_animationLength * 1000));
        await GameplayController.Instance.Sleep();
        Player.Instance.playerController.IsSleeping(false);
    }
}
