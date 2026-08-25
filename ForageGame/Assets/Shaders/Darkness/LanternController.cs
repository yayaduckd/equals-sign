using UnityEngine;
using Weather;
using TDK.PlayerSystem;

public class PlayerLanternController : MonoBehaviour
{
    [System.Serializable]
    private struct LanternPose
    {
        public Vector3 localPosition;   // offset from player, world-axis-aligned
        public Quaternion localRotation;
        public float stickLength;
    }

    [Header("References")]
    [SerializeField] private Transform stickTransform;
    [SerializeField] private Light _light;
    [SerializeField] private Material playerMat; //TODO: materialpropertyblock instead
    private Transform player;
    private Rigidbody rb;


    [Header("Positioning")]

    [SerializeField] private LanternPose[] facingPoses = new LanternPose[4];
    [SerializeField] private LanternPose retractedPose;   // stick upright, lantern at player position
    [SerializeField] private float poseLerpRate = 12f;    // higher = snappier follow of the target pose
    [SerializeField] private float deployDuration = 0.4f; // seconds to fully deploy/retract

    private int currentFacingIndex = 0; //front left, front right, back left, back right
    [SerializeField] private float deployProgress = 0f; // 0 = retracted, 1 = deployed
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


    [SerializeField] private float weight = 1f; //TODO: pushed instead of pulled

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        player.GetComponentInChildren<PlayerVisuals>().onFacingDirectionChanged.AddListener(OnFacingDirectionChanged);
        rb = GetComponent<Rigidbody>();
    }

    public void SetWeight(float newWeight)
    {
        weight = newWeight;
    }

    //visual-only stuff
    private void Update()
    {
        weight = WeatherManager.Instance.lanternIntensity; //weathermanager decides relative lantern strength TODO: get rid of
        if (weight > 0f)
        {
            lanternStrength = fbm.Eval01(Time.time * flickerSpeed);
            _light.color = Color.Lerp(mutedColor, BrightColor, lanternStrength);
            _light.intensity = Mathf.Lerp(minIntensity * weight, maxIntensity * weight, lanternStrength);
        }
        else _light.intensity = 0f;

        //needs to happen in update, since the player might just walk out of a dark region
        //TODO: don't do when retracted
        playerMat.SetVector("_Emission", currentFacingIndex <= 1? playerMatNegativeEmission * weight : Vector3.zero);
    }

    private void OnFacingDirectionChanged(bool isFacingLeft, bool isFacingFront)
    {
        // Debug.Log($"[PlayerLanternController]: recieved facing direction change!: L:{isFacingLeft}, F: {isFacingFront}"); 
        currentFacingIndex = (isFacingLeft ? 0 : 1) + (isFacingFront ? 0 : 2);


        // transform.localScale = new Vector3(isFacingLeft ? -1f : 1f, 1f, isFacingFront ? 1f : -1f); //to flip stick position

        // if (isFacingFront)
        // {
        //     playerMat.SetVector("_Emission", playerMatNegativeEmission * weight);
        // }
        // else
        // {
        //     playerMat.SetVector("_Emission", Vector3.zero);
        // }

        // lerpTimer = directionChangeLerpTime;
        // var newSettings = facingPoses[currentFacingIndex];
        // rb.MoveRotation(newSettings.localRotation);
        // stickTransform.localScale = new Vector3(stickTransform.localScale.x, newSettings.stickLength, stickTransform.localScale.z);
    }

    void FixedUpdate()
    {
        // Advance deploy/retract progress toward its target.
        float deployTarget = weight > 0f ? 1f : 0f;
        deployProgress = Mathf.MoveTowards(deployProgress, deployTarget, Time.fixedDeltaTime / deployDuration);

        LanternPose facingPose = facingPoses[currentFacingIndex];

        Vector3 blendedLocalPos = Vector3.Lerp(retractedPose.localPosition, facingPose.localPosition, deployProgress);
        Quaternion blendedLocalRot = Quaternion.Slerp(retractedPose.localRotation, facingPose.localRotation, deployProgress);
        float blendedStickLength = Mathf.Lerp(retractedPose.stickLength, facingPose.stickLength, deployProgress);

        Vector3 worldTargetPos = player.position + blendedLocalPos;

        // Fixed-timestep-consistent smoothing; since FixedUpdate runs at a constant dt,
        // a flat lerp factor is deterministic here (no need for exp-decay smoothing).
        float t = 1f - Mathf.Pow(1f - 0.5f, Time.fixedDeltaTime * poseLerpRate); // ~critically damped feel

        rb.MovePosition(Vector3.Lerp(rb.position, worldTargetPos, t));
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, blendedLocalRot, t));

        bool isFacingLeft = currentFacingIndex == 0 || currentFacingIndex == 2;
        bool isFacingFront = currentFacingIndex <= 1;
        //no scale flipping required anymore
    
        // Vector3 targetScale = new Vector3(isFacingLeft ? -1f : 1f, 1f, isFacingFront ? 1f : -1f);
        Vector3 targetStickScale = new Vector3(stickTransform.localScale.x, blendedStickLength, stickTransform.localScale.z);;
        // transform.localScale = Vector3.Lerp(transform.localScale, targetScale, t);
        stickTransform.localScale = Vector3.Lerp(stickTransform.localScale, targetStickScale, t);





        // Vector3 targetPos;
        // if (lerpTimer > 0f)
        // {
        //     lerpTimer -= Time.fixedDeltaTime;
        //     targetPos = Vector3.Lerp(transform.position, player.position + facingPoses[currentFacingIndex].localPosition, .6f);
        // }
        // else
        // {
        //     targetPos = player.position + facingPoses[currentFacingIndex].localPosition;
        // }
        // rb.MovePosition(targetPos);
    }
}
