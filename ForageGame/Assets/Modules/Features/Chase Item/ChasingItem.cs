using TDK.PlayerSystem;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ChasingItem : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float playerEffectRadius = 4;
    [SerializeField] private float playerForceStrength = 10;
    [SerializeField] private float coreForceStrength = 5;
    private Vector3 corePos;
    private float playerForceOffset;

    void Awake()
    {
        corePos = transform.position;
        playerForceOffset = playerForceStrength * playerEffectRadius;
    }

    void FixedUpdate()
    {
        Vector3 playerForce = (transform.position - Player.Instance.transform.position).normalized;
        playerForce.y = 0;
        playerForce *= (-playerForceStrength * Vector3.Distance(transform.position, Player.Instance.transform.position) + playerForceOffset);
        Vector3 coreForce = (corePos - transform.position).normalized;
        coreForce.y = 0;
        coreForce *= coreForceStrength * Vector3.Distance(transform.position, corePos);
        rb.AddForce(playerForce, ForceMode.Acceleration);
        rb.AddForce(coreForce, ForceMode.Acceleration);
    }
}
