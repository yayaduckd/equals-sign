using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(ParticleSystem))]
public class HurtEffect : MonoBehaviour
{
    private UnityEvent<float> onHit;
    private float damageSaturation;

    [SerializeField] private ParticleSystem hurtParticles;
    [SerializeField] private int saturationParticleCycleCount = 10;
    public void Initialize(UnityEvent<float> onHit, float damageSaturation)
    {
        this.onHit = onHit;
        this.damageSaturation = damageSaturation;
        onHit.AddListener(ApplyHurtEffect);
    }

    private void ApplyHurtEffect(float damage)
    {
        float intensity = Mathf.Clamp01(damage / damageSaturation);
        var burst = hurtParticles.emission.GetBurst(0);
        burst.cycleCount = Mathf.CeilToInt(saturationParticleCycleCount * intensity); // scale particles by damage
        hurtParticles.emission.SetBurst(0, burst);
        hurtParticles.time = 0f;
        hurtParticles.Play();
    }
}
