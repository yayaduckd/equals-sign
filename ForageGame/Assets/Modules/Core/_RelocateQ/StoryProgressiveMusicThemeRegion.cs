using System.Collections.Generic;
using UnityEngine;

namespace AudioIntegration
{
    [RequireComponent(typeof(BoxCollider))]
    public class StoryProgressiveMusicThemeRegion : MusicTrigger
    {

        //rememba the FMOD event path, last played time and box collider are inherited ~Lars
        [SerializeField] private float meanDelay;
        [SerializeField] private int shape = 2;          //gamma shape (more than 1 equals less variance, ask Tim he can prob explain this in depth lol)
        [SerializeField] private float cooldown;

        [SerializeField] private List<ThemeStage> themeStages;

        [System.Serializable]
        public class ThemeStage
        {
            public List<StoryFlag> requiredFlags;
            public FMODUnity.EventReference theme;
        }


        private void Awake()
        {
            col = GetComponent<BoxCollider>();
        }
        void OnEnable()
        {
            StoryFlagManager.onFlagAdded += OnFlagChanged;
        }
        void OnDisable()
        {
            StoryFlagManager.onFlagAdded -= OnFlagChanged;
        }


        private void Start()
        {
            UpdateCurrentStage();
        }

        private void OnFlagChanged(StoryFlag flag)
        {
            UpdateCurrentStage();
        }

        private void UpdateCurrentStage()
        {
            //update the current stage to the LAST one we have all the flags for
            //yeah yeah, isn't that neat per se. Might rework later to something more clean but we don't have much time left ~Lars
            foreach (var stage in themeStages)
            {
                if (StoryFlagManager.Instance.FlagListActive(stage.requiredFlags))
                {
                    //TODO/IMPORTANT: this does not interrupt/restart scheduling. I should make sure I mask the areas in which flags can be obtained with a different, forced theme ~Lars
                    theme = stage.theme;
                    Debug.Log($"Progressive Theme Region active theme updated to: {theme}");
                }
            }
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
