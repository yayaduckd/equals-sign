using System;
using TDK.PlayerSystem;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    public PlayerController pc;

    [SerializeField]private ParticleSystem attackParticles;
    [SerializeField]private ParticleSystem jumpParticles;

    private void Start()
    {
        pc = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        pc.onAttack.AddListener(AttackEffect);
        pc.onJump.AddListener(JumpEffect);
    }

    private void OnDisable()
    {
        pc.onAttack.RemoveListener(AttackEffect);
        pc.onJump.RemoveListener(JumpEffect);
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
}
