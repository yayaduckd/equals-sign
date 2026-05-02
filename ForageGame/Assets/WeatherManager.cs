using System.Collections.Generic;
using UnityEngine;

public class WeatherManager : MonoBehaviour
{
    public enum Environments
    {
        None,
        DarkRain,
        AfternoonSun,
        LightRain,
        Flowers,
        DampAmbience,
        DarkCave
    }

    [SerializeField] private Environments environment = Environments.AfternoonSun;
    private Environments previousEnvironment = Environments.None;

    Dictionary<Environments, GameObject> _environmentObjects;

    Dictionary<Environments, GameObject> EnvironmentObjects
    {
        get {
            if (_environmentObjects == null)
            {
                _environmentObjects = new Dictionary<Environments, GameObject>
                {
                    { Environments.DarkRain, DarkRainObj },
                    { Environments.AfternoonSun, AfternoonSunObj },
                    { Environments.LightRain, LightRainObj },
                    { Environments.Flowers, FlowersObj },
                    { Environments.DampAmbience, DampAmbienceObj },
                    { Environments.DarkCave, DarkCaveObj },
                    { Environments.None, NoneObj }
                };
            }
            return _environmentObjects; }
    }

    [SerializeField] GameObject DarkRainObj;
    [SerializeField] GameObject AfternoonSunObj;
    [SerializeField] GameObject LightRainObj;
    [SerializeField] GameObject FlowersObj;
    [SerializeField] GameObject DampAmbienceObj;
    [SerializeField] GameObject DarkCaveObj;
    [SerializeField] GameObject NoneObj;

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;

        foreach (var env in EnvironmentObjects)
        {
            env.Value.SetActive(false);
        }

        SetActiveEnvironment();
    }
    

    void LateUpdate()
    {
        transform.position = cam.transform.position;
    }

    public void SetActiveEnvironment()
    {
        if (environment == previousEnvironment) return;

        GameObject oldObject = EnvironmentObjects[previousEnvironment];
        GameObject newObject = EnvironmentObjects[environment];

        previousEnvironment = environment;

        newObject.SetActive(true);
        oldObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SetActiveEnvironment();
    }
#endif
}
