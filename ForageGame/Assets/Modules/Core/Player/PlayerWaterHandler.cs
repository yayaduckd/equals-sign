using TDK.PlayerSystem;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerWaterHandler : MonoBehaviour
{
    [SerializeField] private LayerMask waterLayer;


    private int _waterColliderCounter = 0;

    void OnTriggerEnter(Collider other)
    {
        if ((waterLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            _waterColliderCounter++;
            if (_waterColliderCounter > 1) return;
            Player.Instance.playerController.OnWaterEnter();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if ((waterLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            _waterColliderCounter--;
            if (_waterColliderCounter != 0) return;
            Player.Instance.playerController.OnWaterExit();
        }
    }
}
