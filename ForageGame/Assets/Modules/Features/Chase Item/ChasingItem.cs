using TDK.PlayerSystem;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ChasingItem : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float playerEffectRadius = 4;
    [SerializeField] private float playerForceStrength = 10;

    private Vector3 r;

    void FixedUpdate()
    {
        r = transform.position - Player.Instance.transform.position;
        if (r.magnitude < playerEffectRadius)
            rb.AddForce(r.normalized * (playerForceStrength * (1 - (r.magnitude / playerEffectRadius))), ForceMode.Acceleration);
    }
}
