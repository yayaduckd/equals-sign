using System;
using TDK.PlayerSystem;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    PlayerController pc;
    Rigidbody rb;
    TerrainTextureDetector terrainTextureDetector;

    [SerializeField]private ParticleSystem attackParticles;
    [SerializeField]private ParticleSystem jumpParticles;
    [SerializeField]private ParticleSystem landParticles;

    [SerializeField] private AnimationCurve landParticleSpeedVSParticlecountCurve;
    [SerializeField] private float landParticlesSaturationSpeed;
    [SerializeField] private float landParticlesSaturationCount;
    
    private void Start()
    {
        pc = GetComponent<PlayerController>();
        rb =  GetComponent<Rigidbody>();
        terrainTextureDetector = GetComponent<TerrainTextureDetector>();
    }

    private void OnEnable()
    {
        pc.onAttack.AddListener(AttackEffect);
        pc.onJump.AddListener(JumpEffect);
        pc.onLand.AddListener(LandEffect);
    }

    private void OnDisable()
    {
        pc.onAttack.RemoveListener(AttackEffect);
        pc.onJump.RemoveListener(JumpEffect);
        pc.onLand.RemoveListener(LandEffect);
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

    private void LandEffect()
    {
        float speed = rb.linearVelocity.magnitude;
        print(speed);
        TerrainType terrainType = terrainTextureDetector.GetTerrainType();
        
        Color color;
        switch (terrainType)
        {
            case TerrainType.Grass:
                color = new Color(0.1f, 0.41f, 0.11f);
                break;
            case TerrainType.Dirt:
                color = new Color(0.545f, 0.271f, 0.075f);
                break;
            case TerrainType.Wood:
                color = new Color(0.627f, 0.322f, 0.176f); 
                break;
            case TerrainType.Rock:
                color = new Color(0.5f, 0.5f, 0.5f);
                break;
            case TerrainType.Snow:
                color = new Color(0.93f, 0.93f, 0.93f);
                break;
            case TerrainType.Sand:
                color = new Color(0.941f, 0.902f, 0.549f);
                break;
            default:
                color = Color.white;
                break;
        }
        var mainParticles = landParticles.main;
        mainParticles.startColor = color;
        landParticles.emission.SetBurst(0, new ParticleSystem.Burst(0f, landParticlesSaturationCount * landParticleSpeedVSParticlecountCurve.Evaluate(speed/landParticlesSaturationSpeed)));
        landParticles.Play();
    }
}
