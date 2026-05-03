using System;
using UnityEngine;
using UnityEngine.UI;

public class Energy : MonoBehaviour, IHitHandler
{
    [Header("Energy Settings")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float energyRegenRate = 10f; // energy regenerated per second
    [SerializeField] private float energyRegenDelay = 3f; // seconds delay before regen starts

    [Header("UI Settings")]
    [Tooltip("RectTransform of the energy bar GameObject")]
    [SerializeField] private RectTransform energyFill;

    [Tooltip("RectTransform of the damage bar GameObject")]
    [SerializeField] private RectTransform damageFill;

    // Current values
    public float energy { get; private set; }
    public float damage { get; set; }

    // Derived max energy after damage is considered
    public float currentMaxEnergy { get; private set; }

    // Time tracker for energy regeneration delay
    private float timeSinceEnergyUsed;

    private void Awake()
    {
        _energyBarWidth = energyFill.rect.width;
        damage = 0f;
        UpdateMaxEnergy();
        energy = currentMaxEnergy;
        timeSinceEnergyUsed = energyRegenDelay; // Start ready to regenerate

        if (hitParticles) hitParticles.Stop();
    }

    private void Update()
    {
        RegenerateEnergy();
        UpdateEnergyBar();

        // update invincibility timer (only when needed)
        if (iFramesTimer > 0f)
        {
            iFramesTimer -= Time.deltaTime;
            if (iFramesTimer < 0f)
                iFramesTimer = 0f;
        }
    }

    private void RegenerateEnergy()
    {
        if (energy < currentMaxEnergy)
        {
            timeSinceEnergyUsed += Time.deltaTime;
            if (timeSinceEnergyUsed >= energyRegenDelay)
            {
                energy += energyRegenRate * Time.deltaTime;
                energy = Mathf.Min(energy, currentMaxEnergy);
            }
        }
    }

    // Attempts to use energy. Returns true if successful.
    // Resets the regen timer if energy is used.
    public bool UseEnergy(float amount)
    {
        if (amount <= 0)
            return true;

        if (energy < amount)
        {
            energy = 0f;
            return false;
        }

        energy -= amount;
        if (energy < 0f)
            energy = 0f;

        timeSinceEnergyUsed = 0f;
        return true;
    }

    // Adds energy up to the current max.
    public void AddEnergy(float amount)
    {
        if (amount <= 0) return;
        energy = Mathf.Min(energy + amount, currentMaxEnergy);
    }

    // Takes damage and updates max energy accordingly.
    public void TakeDamage(float amount)
    {
        damage += amount;
        damage = Mathf.Clamp(damage, 0f, maxEnergy);

        UpdateMaxEnergy();

        // Clamp current energy to the new max
        energy = Mathf.Clamp(energy, 0f, currentMaxEnergy);
    }

    // Updates the cached max energy after considering damage.
    private void UpdateMaxEnergy()
    {
        currentMaxEnergy = maxEnergy - damage;
    }

    // Updates the UI bars positions based on current energy and damage.

    private float _energyBarWidth;
    private void UpdateEnergyBar()
    {
        float energyGap = 0.005f;
        if (energyFill != null)
        {
            // Move energy bar left as energy decreases (anchored at right)
            float energyX = energy / maxEnergy;
            energyX -= (damage < 0.1f) ? 0 : energyGap;
            energyX = Mathf.Clamp01(energyX);
            energyFill.anchorMax = new(energyX, 1f);
        }

        if (damageFill != null)
        {
            // Move damage bar right as damage increases (anchored at left)
            float damageX = damage / maxEnergy;
            damageX -= (currentMaxEnergy < 0.1f) ? 0 : energyGap;
            damageX = Mathf.Max(0, damageX);
            damageFill.anchorMin = new(1 - damageX, 0f);
        }
    }

    // ------------ TAKE DAMAGE ------------

    public ParticleSystem hitParticles;
    public float iFramesDuration = 0.5f; // Invincibility frames duration in seconds
    private float iFramesTimer = 0f;

    public void Hit(float damage)
    {
        if (iFramesTimer > 0f) return; // currently in invincibility frames

        iFramesTimer = iFramesDuration; // reset invincibility timer

        TakeDamage(damage);
        Debug.Log(gameObject.name + " took " + damage + " damage.");

        // assume hit particles are burst at time 0
        if (hitParticles)
        {
            hitParticles.time = 0f;
            hitParticles.Play();
        }
    }
}