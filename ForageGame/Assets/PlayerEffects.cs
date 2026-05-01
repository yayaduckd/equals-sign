using System;
using TDK.PlayerSystem;
using UnityEngine;

public class PlayerEffects : MonoBehaviour
{
    public PlayerController pc;

    [SerializeField]private ParticleSystem attackParticles;

    private void Start()
    {
        pc = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        pc.onAttack.AddListener(AttackEffect);
    }

    private void OnDisable()
    {
        pc.onAttack.RemoveListener(AttackEffect);
    }

    private void AttackEffect()
    {
        print(pc.ViewDirection);
        print(Quaternion.LookRotation(pc.ViewDirection, Vector3.up).eulerAngles);
        attackParticles.transform.rotation = Quaternion.LookRotation(pc.ViewDirection, Vector3.up);
        attackParticles.Play();
    }
}
