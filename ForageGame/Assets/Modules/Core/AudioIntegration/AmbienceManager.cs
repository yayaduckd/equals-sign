using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using FMOD.Studio;
using FMODUnity;


namespace AudioIntegration
{
    public class AmbienceManager : MonoBehaviour, IManagedAudioSource
    {
        //This is to be a singleton, so we need to re-use this thing when we change regions!
        public static AmbienceManager Instance { get; private set; }

        public GameObject playerListener;

        private Dictionary<EventReference, EventInstance> activeEvents = new Dictionary<EventReference, EventInstance>(); 

        [SerializeField] private AnimationCurve volumeCurve;

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

        public void SetGlobalParameter(string param, float value)
        {
            FMOD.RESULT result = FMODUnity.RuntimeManager.StudioSystem.setParameterByName(param, value);
            if (result != FMOD.RESULT.OK) Debug.LogError($"[AmbienceManager]: global parameter {param} does not exist or its value {value} is out of bounds");
        }

        public void SetLocalParameter(EventReference e, string param, float value)
        {
            if(activeEvents.TryGetValue(e, out var instance))
            {
                FMOD.RESULT result = instance.setParameterByName(param, value);
                if (result != FMOD.RESULT.OK) Debug.LogError($"[AmbienceManager]: event {e} does not have parameter {param} or its value {value} is out of bounds");
            }
            else
            {
                Debug.LogError($"[AmbienceManager]: event {e} does not exist or is not active, can't set parameter");
            }
        }

        public void StartEvent(FMODUnity.EventReference e)
        {
            //if it isn't started already
            if (!activeEvents.TryGetValue(e, out var instance))
            {
                instance = FMODUnity.RuntimeManager.CreateInstance(e);
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(instance, playerListener); //Dear FMOD, sincerely: fuck you ~Lars
                instance.start(); //e is a class, so this is fine
                activeEvents[e] = instance;
                Debug.Log($"Event {e} started");
            }
            else
            {
                Debug.LogError($"[AmbienceManager]: event {e} already started.");
            }
        }

        public void StopEvent(FMODUnity.EventReference e)
        {
            if (activeEvents.TryGetValue(e, out var instance))
            {
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT); 
                activeEvents[e].release();
                activeEvents.Remove(e);
                Debug.Log($"Event {e} stopped");
            }
            else
            {
                Debug.LogError($"[AmbienceManager]: event {e} already stopped.");
            }
        }


        public void SetRegionInfluences(Dictionary<FMODUnity.EventReference, float> influences)
        {
            foreach (var (reference, weight) in influences)
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

            // Stop anything that didn't appear this frame, done like this to not modify the collection being iterated over
            foreach (var reference in activeEvents.Keys.Where(r => !influences.ContainsKey(r)).ToList())
            {
                activeEvents[reference].stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                activeEvents[reference].release();
                activeEvents.Remove(reference);
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
            foreach(var e in activeEvents.Keys)
            {
                StopEvent(e);
            }
        }
    }
}

