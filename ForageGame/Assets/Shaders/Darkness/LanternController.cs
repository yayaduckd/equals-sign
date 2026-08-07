using DG.Tweening;
using DG.Tweening.Core;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using Weather;
using TDK.PlayerSystem;

public class PlayerLanternController : MonoBehaviour
{
    [System.Serializable]
    struct LanternPosition
    {
        public Vector3 position;
        public Quaternion rotation;
        public float stickLength;
    }

    [Header("References")]
    [SerializeField] private Transform stickTransform;
    [SerializeField] private Light _light;
    [SerializeField] private Material playerMat;
    private Transform player;
    private Rigidbody rb;


    [Header("Positioning")]

    [SerializeField] private LanternPosition[] lanternPositions = new LanternPosition[4];
    private int currentFacingIndex = 0; //front left, front right, back left, back right
    [SerializeField] private float directionChangeLerpTime = .2f;
    private float lerpTimer = 0f;
    [Header("Lantern Settings")]

    [SerializeField] private Color mutedColor = Color.red;
    [SerializeField] private Color BrightColor = Color.yellow;

    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 5f;

    [SerializeField][Range(0f, 1f)] private float lanternStrength = 0.5f;

    [SerializeField][Range(0f, 10f)] private float flickerSpeed = 1f;

    [SerializeField] private Vector3 playerMatNegativeEmission;

    [SerializeField] FBM1D fbm = new FBM1D(FBM1D.NoiseFunctionType.Sin, 4, 1.97f, 0.43f);


    private float weight = 0f;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        player.GetComponentInChildren<PlayerVisuals>().onFacingDirectionChanged.AddListener(OnFacingDirectionChanged);
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

       if(isFacingFront)
        {
            playerMat.SetVector("_Emission", playerMatNegativeEmission);
        }
        else
        {
            playerMat.SetVector("_Emission", Vector3.zero);
        }

       lerpTimer = directionChangeLerpTime;
       var newSettings = lanternPositions[currentFacingIndex];
       rb.MoveRotation(newSettings.rotation);
       stickTransform.localScale = new Vector3(stickTransform.localScale.x, newSettings.stickLength, stickTransform.localScale.z);
    }

    void FixedUpdate()
    {
        Vector3 targetPos;
        if (lerpTimer > 0f)
        {
            lerpTimer -= Time.fixedDeltaTime;
            targetPos = Vector3.Lerp(transform.position, player.position + lanternPositions[currentFacingIndex].position, .5f);
        }
        else
        {
            targetPos = player.position + lanternPositions[currentFacingIndex].position;
        }
        rb.MovePosition(targetPos);
    }

    [ContextMenu("Set Lantern Visuals")]
    private void SetLanternVisuals()
    {
        _light.color = Color.Lerp(mutedColor, BrightColor, lanternStrength);
        _light.intensity = Mathf.Lerp(minIntensity*weight, maxIntensity*weight, lanternStrength);
    }

}
