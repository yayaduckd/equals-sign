using UnityEngine;
using UnityEngine.Splines;

public class SplineFollower : MonoBehaviour
{
    [SerializeField] private SplineContainer _spline;
    [SerializeField] private float _speed = 10f;
    [SerializeField] private float _initialPosition = 0f;
    private float t = 0;
    private float splineSpeed = 0;


    void Awake()
    {
        splineSpeed = _speed / _spline.CalculateLength();
        t = _initialPosition;
        transform.position = _spline.EvaluatePosition(t);
    }

    void Update()
    {
        t += Time.deltaTime * splineSpeed;
        t %= 1;
        transform.position = _spline.EvaluatePosition(t);
    }
}
