using System.Collections.Generic;
using System.Collections;
using UnityEngine;

namespace AudioIntegration
{
    /// <summary>
    /// Handles the random trigger-ing of general music themes, also with events in mind
    /// </summary>
    public class MusicManager : MonoBehaviour, IManagedAudioSource
    {
        //yes this is a singleton, fight me ~Lars
        public static MusicManager Instance { get; private set; }

        //list of themes that can be played rn, depending on what trigger regions we are in
        private Dictionary<MusicTrigger, float> zoneSchedule = new Dictionary<MusicTrigger, float>();
        [SerializeField] private FMOD.Studio.EventInstance currentTheme; 
        [SerializeField] private MusicTrigger currentZone;

        public enum MusicManagerState
        {
            Scheduling,
            Playing,
            Suspended
        }

        [SerializeField] private MusicManagerState state = MusicManagerState.Scheduling;

        void OnEnable()  => AudioManager.Instance.Register(this);
        void OnDisable() => AudioManager.Instance.Unregister(this);

        private void Awake() 
        { 
            //May only be one instance ofc
            if (Instance != null && Instance != this) 
            { 
                Destroy(this); 
            } 
            else 
            { 
                Instance = this; 
            } 
        }

        public void Update()
        {
            //I used to use fancy IEnumerator coroutines and stuff but it introduces so much unstability so we just rockin the state machine now ~Lars
            switch(state)
            {
                case MusicManagerState.Scheduling:
                {
                    float now = Time.time;
                    foreach(var (zone, schedTime) in zoneSchedule)
                    {
                        if (schedTime <= now)
                        {
                            PlayTheme(zone);
                            break;
                        }
                    }
                    break;
                }
                case MusicManagerState.Playing:
                {
                    if(!IsPlaying())
                    {
                        print("Theme done! Restarting scheduling...");
                        currentTheme.release();
                        state = MusicManagerState.Scheduling;
                        RestartScheduling();
                    }
                    break;
                }
                default: break; //no logic to be done while suspended
            }
        }

        public void ZoneActivated(MusicTrigger zone)
        {
            zoneSchedule.Add(zone, zone.GetScheduledTime());
            print("Theme added to active list: " + zone);
        }

        public void ZoneDeactivated(MusicTrigger zone)
        {
            zoneSchedule.Remove(zone);
            //If currently playing this theme
            if(state == MusicManagerState.Playing && currentZone == zone)
            {
                print("Player exited zone for current theme, fading out and restarting scheduling...");
                state = MusicManagerState.Scheduling;
                currentTheme.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                RestartScheduling();
            }
        }

        public void SuspendScheduling()
        {
            //if a theme is playing, fade it out
            if (state == MusicManagerState.Playing)
            {
                currentTheme.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
            state = MusicManagerState.Suspended;
            print("MusicManager suspended");
        }

        public void RestartScheduling()
        {
            //Get a new schedule time for each active trigger zone
            //yeah uhh I'm not allowed to modify the collection I'm iterating over, so I gotta deep copy it
            var keys = new List<MusicTrigger>(zoneSchedule.Keys);

            foreach (var zone in keys)
            {
                zoneSchedule[zone] = zone.GetScheduledTime();
            }
            state = MusicManagerState.Scheduling;
        }

        // Is public, so we can override anything if need be, i.e. make instant playback for a given room or anything ~Lars
        public bool PlayTheme(MusicTrigger zone)
        {
            if (state != MusicManagerState.Scheduling || !zoneSchedule.ContainsKey(zone))
            {
                print("Theme play attempt ignored for zone: " + zone);
                return false; //something went wrong, or race condition!
            }
            //we acquired the lock, stop all other scheduling
            state = MusicManagerState.Playing;
            print("Play " + zone.theme);
            
            currentZone = zone;
            currentTheme = FMODUnity.RuntimeManager.CreateInstance(zone.theme);
            currentTheme.start();

            zone.lastPlayedTime = Time.time; //for cooldown
            return true;
        }

        //FMOD is restarted and requires this tomfoolery
        bool IsPlaying() 
        {
            FMOD.Studio.PLAYBACK_STATE eventState;   
            currentTheme.getPlaybackState(out eventState);
            return eventState != FMOD.Studio.PLAYBACK_STATE.STOPPED;
        }

        /// <summary>
        /// Inherited from IManagedAudioSource
        /// </summary>
        public void StopAndRelease()
        {
            SuspendScheduling();
        }
    }
}
