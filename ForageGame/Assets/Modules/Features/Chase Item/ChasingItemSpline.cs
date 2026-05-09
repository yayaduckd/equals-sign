using Unity.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class ChasingItemSpline : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float playerForceStrength = 10;
    [SerializeField] private float playerForceRadius = 10;
    [SerializeField] private float playerForceDecay = 10;
    [SerializeField] private float initialSpeed = 5;
    [SerializeField] private float splineForce = 10;
    [SerializeField] private SplineContainer _spline;

    private float t = 0;
    private float v = 0;
    private float a = 0;

    void Awake()
    {
        // DISABLE STANDARD PHYSICS STUFF
        if (TryGetComponent(out Rigidbody rigidbody))
            rigidbody.isKinematic = true;

        v = initialSpeed;
    }

    private float r = 99;
    void FixedUpdate()
    {
        r = Vector3.Distance(player.position, transform.position);
        a = splineForce;
        if (r < playerForceRadius)
            // TLDR: an exponential-curve-based-force in the direction along the spline depending on the player position
            a += playerForceStrength * Mathf.Exp(-playerForceDecay * r) * Vector3.Dot(Vector3.Normalize(transform.position - player.position), Vector3.Normalize(_spline.EvaluateTangent(t)));

        v += a * Time.fixedDeltaTime;
        t += v * Time.fixedDeltaTime;
        transform.position = _spline.EvaluatePosition(t);
    }
}
