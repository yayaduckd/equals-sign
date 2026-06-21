using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace TDK.Actuators
{
    [RequireComponent(typeof(Collider))]
    public class HitActuator : MonoBehaviour, IHitHandler
    {
        public UnityEvent<float> OnHit;

        HitEffect hurtEffect;

        private void Start()
        {
            hurtEffect = GetComponentInChildren<HitEffect>();
            if (hurtEffect)
            {
                hurtEffect.Initialize(OnHit, 50f);
            }    
        }

        public void Hit(float damage) => OnHit.Invoke(damage);
    }
}
