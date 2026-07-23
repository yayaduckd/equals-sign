using System;
using TDK.PlayerSystem;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    PlayerController pc;
    Rigidbody rb;
    SurfaceTypeDetector surfaceTypeDetector;
    Energy en;

    [SerializeField] private ParticleSystem attackParticles;
    [SerializeField] private ParticleSystem jumpParticles;

    [Header("Footstep Dust Particles")]
    [SerializeField] private ParticleSystem dustParticles;

    [Header("Water Step Particles")]
    [SerializeField] private ParticleSystem waterStepParticles;

    [Header("Land Particles")]
    [SerializeField] private ParticleSystem landParticles;

    [SerializeField] private AnimationCurve landParticleSpeedVSParticlecountCurve;
    [SerializeField] private float landParticlesSaturationSpeed;
    [SerializeField] private float landParticlesSaturationCount;

    [Header("Hit Particles")]
    [SerializeField] private ParticleSystem hitParticles;
    [SerializeField] private int hitParticlesSaturationCount = 10;
    private float hitParticlesDamageSaturation;

    private void Awake()
    {
        pc = GetComponentInParent<PlayerController>();
        rb = GetComponentInParent<Rigidbody>();
        surfaceTypeDetector = GetComponentInParent<SurfaceTypeDetector>();
        en = GetComponentInParent<Energy>();
        hitParticlesDamageSaturation = en.currentMaxEnergy;
    }

    private void OnEnable()
    {
        pc.onAttack.AddListener(AttackEffect);
        pc.onJump.AddListener(JumpEffect);
        pc.onLand.AddListener(LandEffect);
        en.onHit.AddListener(HitEffect);
    }

    private void OnDisable()
    {
        pc.onAttack.RemoveListener(AttackEffect);
        pc.onJump.RemoveListener(JumpEffect);
        pc.onLand.RemoveListener(LandEffect);
        en.onHit.RemoveListener(HitEffect);
    }

    //Called from the player's walking animation directly
    public void FootstepEffects()
    {
        SurfaceType surfaceType = surfaceTypeDetector.GetSurfaceType(); //yes IK this sucks, I can't pass a label for a labeled parameter, FMOD is great :)
        if(surfaceType == SurfaceType.Water)
        {
            //TODO: play water splash
            //waterStepParticles.transform.position = 
            waterStepParticles.Play();
        }
        else //obstacles or terrain, TODO: check dustyness
        {
            
        }
        //always play footstep audio regardless
        PlayerSounds.Instance.PlayFootstep(surfaceType);
    }

    private void AttackEffect()
    {
        attackParticles.transform.rotation = Quaternion.LookRotation(pc.ViewDirection, Vector3.up);
        attackParticles.Play();
    }

    private void JumpEffect()
    {
        jumpParticles.Play();
    }

    private void HitEffect(float damage)
    {
        float intensity = Mathf.Clamp01(damage / hitParticlesDamageSaturation);
        var burst = hitParticles.emission.GetBurst(0);
        burst.cycleCount = Mathf.CeilToInt(hitParticlesSaturationCount * intensity); // scale particles by damage
        hitParticles.emission.SetBurst(0, burst);
        hitParticles.time = 0f;
        hitParticles.Play();
    }

    private void LandEffect()
    {
        float speed = rb.linearVelocity.magnitude;
        //print(speed);
        SurfaceType surfaceType = surfaceTypeDetector.GetSurfaceType();

        Color color;
        switch (surfaceType)
        {
            case SurfaceType.Grass:
                color = new Color(0.1f, 0.41f, 0.11f);
                break;
            case SurfaceType.Gravel:
                color = new Color(0.545f, 0.271f, 0.075f);
                break;
            case SurfaceType.Wood:
                color = new Color(0.627f, 0.322f, 0.176f);
                break;
            case SurfaceType.Rock:
                color = new Color(0.5f, 0.5f, 0.5f);
                break;
            case SurfaceType.Water:
                color = new Color(0.93f, 0.93f, 0.93f);
                break;
            case SurfaceType.Sand:
                color = new Color(0.941f, 0.902f, 0.549f);
                break;
            default:
                color = Color.white;
                break;
        }
        var mainParticles = landParticles.main;
        mainParticles.startColor = color;
        landParticles.emission.SetBurst(0, new ParticleSystem.Burst(0f, landParticlesSaturationCount * landParticleSpeedVSParticlecountCurve.Evaluate(speed / landParticlesSaturationSpeed)));
        landParticles.Play();
        PlayerSounds.Instance.PlayFootstep(surfaceType); //play a footstep sound for landing as well
    }
}
