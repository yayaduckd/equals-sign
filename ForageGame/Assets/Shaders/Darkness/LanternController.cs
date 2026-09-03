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
    }

    [Header("References")]
    [SerializeField] private Transform _playerHand;
    [SerializeField] private Light _light;
    [SerializeField] private Material playerMat; //TODO: materialpropertyblock instead

    [Header("Positioning")]
    [SerializeField] private LanternPose[] facingPoses = new LanternPose[5]; //front left, front right, back left, back right
    [SerializeField] private LanternPose _retractedPose = new() { localPosition = Vector3.zero, localRotation = Quaternion.identity };
    [SerializeField] private float deployDuration = 0.4f; // seconds to fully deploy/retract
    [SerializeField] private float _lerpPosSpeed = 1;
    [SerializeField] private float _lerpRotSpeed = 1;

    [SerializeField] private int _currentFacingIndex = 0; //front left, front right, back left, back right
    [SerializeField] private LanternPose _currentLanternPose = new() { localPosition = Vector3.zero, localRotation = Quaternion.identity };
    [SerializeField] private bool _isDeployed = false;
    [SerializeField] private float _deployProgress = 0f; // 0 = retracted, 1 = deployed

    /// <summary>
    /// For Tim:
    /// Set player material using MaterialPropertyBlock
    /// </summary>

    void Start()
    {
        Player.Instance.visuals.onFacingDirectionChanged.AddListener(OnFacingDirectionChanged);
        SetDeployment(false);
    }

    public void SetDeployment(bool isDeployed)
    {
        _isDeployed = isDeployed;
        RefreshLanternPose();
    }

    private void OnFacingDirectionChanged(bool isFacingLeft, bool isFacingFront)
    {
        _currentFacingIndex = (isFacingLeft ? 0 : 1) + (isFacingFront ? 0 : 2);
        RefreshLanternPose();
    }

    private void RefreshLanternPose()
    {
        if (_isDeployed)
            _currentLanternPose = facingPoses[_currentFacingIndex];
        else
            _currentLanternPose = _retractedPose;
    }

    void FixedUpdate()
    {
        // Advance deploy/retract progress.
        _deployProgress = Mathf.MoveTowards(_deployProgress, _isDeployed ? 1 : 0, Time.fixedDeltaTime / deployDuration);
        transform.localScale = Mathf.SmoothStep(0, 1, _deployProgress) * Vector3.one;

        // Move Lantern Base
        _playerHand.SetLocalPositionAndRotation(
            Vector3.MoveTowards(_playerHand.localPosition, _currentLanternPose.localPosition, _lerpPosSpeed * Time.fixedDeltaTime),
            Quaternion.RotateTowards(_playerHand.localRotation, _currentLanternPose.localRotation, _lerpRotSpeed * Time.fixedDeltaTime)
            );
    }

    #region Visuals

    [Header("Lantern Settings")]
    [SerializeField] private Color mutedColor = Color.red;
    [SerializeField] private Color BrightColor = Color.yellow;
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 5f;
    [SerializeField][Range(0f, 10f)] private float flickerSpeed = 1f;
    [SerializeField] private Vector3 playerMatNegativeEmission;
    [SerializeField] FBM1D fbm = new FBM1D(FBM1D.NoiseFunctionType.Sin, 4, 1.97f, 0.43f);
    private float _lanternStrength = 0.5f;

    private void Update()
    {
        if (_deployProgress > 0.01f)
        {
            _lanternStrength = fbm.Eval01(Time.time * flickerSpeed);
            _light.color = Color.Lerp(mutedColor, BrightColor, _lanternStrength);
            _light.intensity = Mathf.Lerp(minIntensity * _deployProgress, maxIntensity * _deployProgress, _lanternStrength);
        }
        else _light.intensity = 0f;

        //needs to happen in update, since the player might just walk out of a dark region
        // playerMat.SetVector("_Emission", _currentFacingIndex <= 1 ? playerMatNegativeEmission * _deployProgress : Vector3.zero);
    }

    #endregion
}
