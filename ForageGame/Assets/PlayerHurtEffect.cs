using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
[RequireComponent(typeof(ParticleSystem))]
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

    [Header("Depth of Field")]
    [SerializeField] private float dofFocusDistanceVariance= 2f;     // normal/no-hurt focus distance
    private float dofFocusDistance;     // normal/no-hurt focus distance
    // [SerializeField] private float dofApertureMax = 12f;          // narrower at high severity (lower = blurrier in URP)

    [Header("Chromatic Aberration")]
    [SerializeField] private float chromAberrationVariance = 0.1f;
    private float chromAberrationIntensity;

    [Header("Film Grain")]
    [SerializeField] private float filmGrainVariance = 0.1f;
    private float filmGrainIntensity;

    [Header("Feathers")]
    [SerializeField] private ParticleSystem feathers;
    [SerializeField] private float featherRateOverDistanceVariance = 0.02f;
    private float featherRateOverDistance;

    private DepthOfField _dof;
    private ChromaticAberration _chromAberration;
    private FilmGrain _filmGrain;

    private float _currentSeverity;
    private float _oscPhase;
    private bool _isActive;

    [SerializeField] private float _sinePhase = 0f; //Radians


    private void Awake()
    {
        en = GetComponentInParent<Energy>();
        feathers = GetComponent<ParticleSystem>();
        if (hurtVolume == null) hurtVolume = GetComponent<Volume>();
        var profile = hurtVolume.profile;

        profile.TryGet(out _dof);
        dofFocusDistance = (float)_dof.focusDistance;
        
        profile.TryGet(out _chromAberration);
        chromAberrationIntensity = (float)_chromAberration.intensity;
        
        profile.TryGet(out _filmGrain);
        filmGrainIntensity = (float)_filmGrain.intensity;

        featherRateOverDistance = feathers.emission.rateOverDistance.constant;

        hurtVolume.weight = 0f;
    }

    private void OnEnable()
    {
        //Debug.Log("subbed!");
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
                var emission = feathers.emission;
                emission.rateOverDistance = 0f;
            }
        }
    }

    private void Update()
    {
        // Only runs when severity > 0, since we disable the component otherwise.
        hurtVolume.weight = _currentSeverity;

        //sine wave, makes it look like it's heavy breathing
        _sinePhase += Time.deltaTime * oscillationSpeed * Mathf.PI * 2;
        _sinePhase %= (Mathf.PI * 2);
        float wave01 = Mathf.Sin(_sinePhase);

        float severity = _currentSeverity;

        if (_dof != null)
        {
            _dof.focusDistance.value = dofFocusDistance + wave01*dofFocusDistanceVariance;
        }

        if (_chromAberration != null)
        {
            _chromAberration.intensity.value = chromAberrationIntensity + wave01 * chromAberrationVariance;
        }

        if (_filmGrain != null)
        {
            // Grain doesn't need to oscillate with the wave - keep it steadier,
            // just scaling with severity, so it reads as constant texture not pulsing.
            _filmGrain.intensity.value =  filmGrainIntensity + wave01 * filmGrainVariance;
        }

        if (feathers != null)
        {
            var emission = feathers.emission;
            emission.rateOverDistance = _currentSeverity * (featherRateOverDistance + wave01 * featherRateOverDistanceVariance);;
        }
    }
}