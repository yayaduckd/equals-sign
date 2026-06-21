using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class PlayerHurtEffect : MonoBehaviour
{

    [SerializeField] private Energy en;

    [Header("Volume")]
    [SerializeField] private Volume hurtVolume;

    [Header("Response Curve")]
    [Tooltip("X = health 0-1 (0=dead,1=full). Y = severity 0-1. Should stay near 0 until health drops below ~0.4-0.5")]
    [SerializeField] private AnimationCurve healthToSeverity = AnimationCurve.EaseInOut(0f, 1f, 0.5f, 0f);
    // Recommend shaping this curve so severity is 0 above ~50% health,
    // then ramps up toward 1 as health approaches 0.

    [Header("Oscillation")]
    [SerializeField] private float oscillationSpeed = 0.6f;       // cycles per second, slow = dazed not seizure
    [SerializeField] private float oscillationSpeedVariance = 0.15f;

    [Header("Depth of Field")]
    [SerializeField] private float dofFocusDistanceBase = 8f;     // normal/no-hurt focus distance
    [SerializeField] private float dofFocusDistanceMin = 1.5f;    // closest focus swings to at max severity
    [SerializeField] private float dofApertureMax = 12f;          // narrower at high severity (lower = blurrier in URP)

    [Header("Chromatic Aberration")]
    [SerializeField] private float chromAberrationMaxIntensity = 0.35f;

    [Header("Lens Distortion")]
    [SerializeField] private float lensDistortionMaxIntensity = 15f;

    [Header("Film Grain")]
    [SerializeField] private float filmGrainMaxIntensity = 0.4f;

    private DepthOfField _dof;
    private ChromaticAberration _chromAberration;
    private LensDistortion _lensDistortion;
    private FilmGrain _filmGrain;

    private float _currentSeverity;
    private float _oscPhase;
    private bool _isActive;


    private void Awake()
    {
        en = GetComponentInParent<Energy>();
        if (hurtVolume == null) hurtVolume = GetComponent<Volume>();
        var profile = hurtVolume.profile;

        profile.TryGet(out _dof);
        profile.TryGet(out _chromAberration);
        profile.TryGet(out _lensDistortion);
        profile.TryGet(out _filmGrain);

        hurtVolume.weight = 0f;
    }

    private void OnEnable()
    {
        Debug.Log("subbed!");
        //enabled = false; // don't tick until something calls SetSeverity above 0
        en.onMaxEnergyChanged.AddListener(SetMaxHealthNormalized);
    }
    private void OnDisable()
    {
        en.onMaxEnergyChanged.RemoveListener(SetMaxHealthNormalized);
    }

    // Call this from your health/energy script whenever health changes.
    // healthNormalized: 0 = dead/critical, 1 = full health
    public void SetMaxHealthNormalized(float healthNormalized)
    {
        Debug.Log($"[PlayerHurtEffect]: new normalized max health value: {healthNormalized}");

        float severity = healthToSeverity.Evaluate(Mathf.Clamp01(healthNormalized));
        _currentSeverity = severity;

        bool shouldBeActive = severity > 0.001f;
        if (shouldBeActive != _isActive)
        {
            _isActive = shouldBeActive;
            //enabled = shouldBeActive; // stop/start Update entirely
            if (!shouldBeActive)
            {
                hurtVolume.weight = 0f; // fully off, zero cost
            }
        }
    }

    private void Update()
    {
        // Only runs when severity > 0, since we disable the component otherwise.
        hurtVolume.weight = _currentSeverity;

        _oscPhase += Time.deltaTime * oscillationSpeed *
                     (1f + Random.Range(-oscillationSpeedVariance, oscillationSpeedVariance) * Time.deltaTime);

        // Smooth, organic oscillation: sum two sine waves at different rates
        // rather than a single sine, so it doesn't feel like a metronome.
        float wave = Mathf.Sin(_oscPhase) * 0.7f + Mathf.Sin(_oscPhase * 1.7f + 1.3f) * 0.3f;
        float wave01 = (wave + 1f) * 0.5f; // remap -1..1 to 0..1

        float severity = _currentSeverity;

        if (_dof != null)
        {
            _dof.focusDistance.value = Mathf.Lerp(
                dofFocusDistanceBase,
                dofFocusDistanceMin,
                severity * wave01
            );
            _dof.aperture.value = Mathf.Lerp(32f, dofApertureMax, severity);
        }

        if (_chromAberration != null)
        {
            _chromAberration.intensity.value = severity * chromAberrationMaxIntensity * wave01;
        }

        if (_lensDistortion != null)
        {
            _lensDistortion.intensity.value = severity * lensDistortionMaxIntensity * wave01 / 100f;
        }

        if (_filmGrain != null)
        {
            // Grain doesn't need to oscillate with the wave - keep it steadier,
            // just scaling with severity, so it reads as constant texture not pulsing.
            _filmGrain.intensity.value = severity * filmGrainMaxIntensity;
        }
    }
}