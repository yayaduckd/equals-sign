using DG.Tweening;
using DG.Tweening.Core;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Weather;
using TDK.PlayerSystem;

public class PlayerLanternController : MonoBehaviour
{
    [Header("Positioning")]
    [SerializeField] private Vector3[] relativePosition = new Vector3[4];
    private int currentFacingIndex = 0; //front left, front right, back left, back right
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody rb;
    [Header("Lantern Settings")]
    [SerializeField] private Light _light;

    [SerializeField] private Color mutedColor = Color.red;
    [SerializeField] private Color BrightColor = Color.yellow;

    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 5f;

    [SerializeField][Range(0f, 1f)] private float lanternStrength = 0.5f;

    [SerializeField][Range(0f, 10f)] private float flickerSpeed = 1f;

    [SerializeField] FBM1D fbm = new FBM1D(FBM1D.NoiseFunctionType.Sin, 4, 1.97f, 0.43f);


    private float weight = 0f;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        player.GetComponentInChildren<PlayerVisuals>().onFacingDirectionChanged.AddListener(OnFacingDirectionChanged);
        //relativePosition = transform.localPosition;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        weight = WeatherManager.Instance.lanternIntensity; //weathermanager decides relative lantern strength
        if(weight > 0f)
        {
            lanternStrength = fbm.Eval01(Time.time * flickerSpeed);
            SetLanternVisuals();
        }
        else _light.intensity = 0f;
    }

    private void OnFacingDirectionChanged(bool isFacingLeft, bool isFacingFront)
    {
       Debug.Log($"[PlayerLanternController]: recieved facing direction change!"); 
       currentFacingIndex = (isFacingLeft ? 0 : 1) + (isFacingFront ? 0 : 2);
       transform.localScale = new Vector3(isFacingLeft ? -1f : 1f, 1f, isFacingFront ? 1f : -1f); //to flip stick position
    }

    void FixedUpdate()
    {
        Vector3 targetPos = Vector3.Lerp(transform.position, player.position + relativePosition[currentFacingIndex], .7f);
        rb.MovePosition(targetPos);
    }

    [ContextMenu("Set Lantern Visuals")]
    private void SetLanternVisuals()
    {
        _light.color = Color.Lerp(mutedColor, BrightColor, lanternStrength);
        _light.intensity = Mathf.Lerp(minIntensity*weight, maxIntensity*weight, lanternStrength);
    }

}
