using UnityEngine;
using System.Collections.Generic;
using System;
using FMOD.Studio;
using FMODUnity;


namespace AudioIntegration
{
    public class AmbienceManager : MonoBehaviour, IManagedAudioSource
    {
        //This is to be a singleton, so we need to re-use this thing when we change regions!
        public static AmbienceManager Instance { get; private set; }

        public GameObject playerListener;


        [System.Serializable]
        public class AmbienceEvent
        {
            public bool active = false; //is this event playing
            public FMOD.Studio.EventInstance instance; 
            public Dictionary<string, FMOD.Studio.PARAMETER_ID> parameters = new Dictionary<string, FMOD.Studio.PARAMETER_ID>();
        }

        private Dictionary<EventReference, EventInstance> activeEvents = new Dictionary<EventReference, EventInstance>(); 

        [SerializeField] private List<Region> regions;
        [SerializeField] private AnimationCurve volumeCurve;

        //kinda lame to *also* have this, but I want to edit the list above in the editor, and ofc I can't serialize a dictionary
        private Dictionary<Region, AmbienceEvent> events = new Dictionary<Region, AmbienceEvent>();

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
        private void Start()
        {
            foreach(Region r in regions)
            {
                if(events.ContainsKey(r)) Debug.LogError($"Duplicate AmbienceRegion Id: {r}");

                AmbienceEvent e = new AmbienceEvent();
                e.instance = FMODUnity.RuntimeManager.CreateInstance(r.ambienceEvent);
                e.instance.getDescription(out FMOD.Studio.EventDescription desc);

                desc.getParameterDescriptionCount(out int paramcount);
                for (int i = 0; i < paramcount; i++)
                {
                    desc.getParameterDescriptionByIndex(i, out var param);
                    e.parameters.Add(param.name, param.id);
                    print("Dict entry added: " + param.name + " with Id: " + param.id + "for region: " + r);
                }
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(e.instance, playerListener); //Dear FMOD, sincerely: fuck you ~Lars
                events.Add(r, e);
            }
        }

 //OLD, but should be reworked
        public void SetParameter(string param, float value)
        {
            foreach(AmbienceEvent e in events.Values)
            {
                if(e.active) //only check active events
                {
                    if (e.parameters.TryGetValue(param, out var id))
                    {
                        e.instance.setParameterByID(id, value);
                        return;
                    }
                }
            }
            Debug.LogError("Recieved parameter name: " + name + " does not exist in an active ambience event");
        }

 //OLD
        public void StartEvent(Region r)
        {
            if (events.TryGetValue(r, out var e))
            {
                if(e.active) 
                {
                    Debug.LogError($"Event {e} already playing");
                    return;
                }
                e.instance.start(); //e is a class, so this is fine
                e.active = true; 
                activeEvents[r.ambienceEvent] = e.instance;
                Debug.Log($"Event {e} started");
            }
            else
            {
                Debug.LogError($"Region not found: {r}");
            }
        }

        //OLD
        public void StopEvent(Region r)
        {
            if (events.TryGetValue(r, out var e))
            {
                if(!e.active) 
                {
                    Debug.LogError($"Event {e} already stopped");
                    return;
                }
                e.instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); 
                activeEvents.Remove(r.ambienceEvent);
                e.active = false; 
                Debug.Log($"Event {e} stopped");
            }
            else
            {
                Debug.LogWarning($"Region not found: {r}, ignoring stop event");
            }
        }


        public void SetRegionInfluences(Dictionary<Region, float> influences)
        {
            //process regions to sum up the same ambiences
            var ambienceWeights = new Dictionary<FMODUnity.EventReference, float>();
            foreach (var (region, weight) in influences)
            {
                if(region.ambienceEvent.IsNull)
                {
                    Debug.Log($"[AmbienceManager] Region has no event assigned: {region}. Skipping!");
                }
                else if (ambienceWeights.TryGetValue(region.ambienceEvent, out float existing))
                    ambienceWeights[region.ambienceEvent] = existing + weight;
                else
                    ambienceWeights[region.ambienceEvent] = weight;
            }

            foreach (var (reference, weight) in ambienceWeights)
            {
                //Debug.Log($"[WeatherManager] setting influence for Region: {region} to {weight}");

                if (!activeEvents.TryGetValue(reference, out var instance))
                {
                    Debug.Log($"Event {reference} started");
                    instance = FMODUnity.RuntimeManager.CreateInstance(reference);
                    FMODUnity.RuntimeManager.AttachInstanceToGameObject(instance, playerListener); //Dear FMOD, sincerely: fuck you ~Lars
                    instance.start();
                    activeEvents[reference] = instance;
                }

                instance.setVolume(volumeCurve.Evaluate(weight));
            }

            // Stop anything that didn't appear this frame
            foreach (var (reference, instance) in activeEvents)
            {
                if(!ambienceWeights.ContainsKey(reference))
                {
                    activeEvents[reference].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    activeEvents[reference].release();
                    activeEvents.Remove(reference);
                }
            }
        }

        /// <summary>
        /// Inherited from IManagedAudioSource
        /// </summary>
        public void StopAndRelease()
        {
            StopAllEvents();
        }

        public void StopAllEvents()
        {
            foreach(Region r in regions)
            {
                StopEvent(r);
            }
        }
    }
}

