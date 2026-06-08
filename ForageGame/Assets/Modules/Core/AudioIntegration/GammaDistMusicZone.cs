using UnityEngine;
using System.Collections;

namespace AudioIntegration
{
    [RequireComponent(typeof(BoxCollider))]
    public class GammaDistMusicZone : MusicTrigger
    {

        //rememba the FMOD event path, last played time and box collider are inherited ~Lars
        [SerializeField] private float meanDelay;
        [SerializeField] private int shape = 2;          //gamma shape (more than 1 equals less variance, ask Tim he can prob explain this in depth lol)
        [SerializeField] private float cooldown;


        private void Awake()
        {
            col = GetComponent<BoxCollider>();
        }
        
        //I purposely leave these for the child class, so we can decide when each trigger zone is active, for example we can have one that is story flag-locked ~Lars
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                MusicManager.Instance.ZoneActivated(this);
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                MusicManager.Instance.ZoneDeactivated(this);
            }
        }

        //fancy Gamma distribution
        float SampleGamma(int k, float theta)
        {
            float sum = 0f;
            for (int i = 0; i < k; i++)
            {
                sum += -Mathf.Log(1f - Random.value) * theta;
            }
            return sum;
        }

        public override float GetScheduledTime()
        {
            float theta = meanDelay / shape;
            float schedTime = Mathf.Max(Time.time, lastPlayedTime + cooldown) + SampleGamma(shape, theta);
            print("Theme scheduled for time: " + schedTime);
            return Time.time + schedTime; //TODO: change to have the distro
        }

    }
}