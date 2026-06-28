using UnityEngine;
using Weather;
using AudioIntegration;
using TDK.PlayerSystem;
using FMODUnity;

/// <summary>
/// Manages the Tutorial sequence,
/// So turns on the thunderstorm weather + audio and turns on the wall around the island
/// Gets turned off automatically after the player obtains the tutorial_complete flag
/// </summary>
public class TutorialManager : MonoBehaviour
{

    [SerializeField] private GameObject wallAroundIsland;
    [SerializeField] private GameObject wallThinkingZones;

    [SerializeField] private StoryFlag tutorialCompleteFlag;

    [SerializeField] private WeatherTypeProfile thunderWeather;

    [SerializeField] private EventReference thunderAmbience;

    void Awake()
    {
        StoryFlagManager.onFlagAdded += onStoryFlagAdded;
    }

    void Start()
    {
        if(StoryFlagManager.Instance.FlagActive(tutorialCompleteFlag))
        {
            DisableTutorial();
        }
        else //start tutorial
        {
            RegionManager.Instance.gameObject.SetActive(false); //just turn it off
            WeatherManager.Instance.SetWeatherTypeInstant(thunderWeather);
            AmbienceManager.Instance.StartEvent(thunderAmbience);

            //make player take damage
            var en = Player.Instance.GetComponent<Energy>();
            en.Hit(en.currentMaxEnergy - 10f);
        }
    }

    private void onStoryFlagAdded(StoryFlag newFlag)
    {
        if (newFlag == tutorialCompleteFlag)
        {
            DisableTutorial();
        }
    }
    public void DisableTutorial()
    {
        Debug.Log("[TutorialManager] Disabling tutorial elements");
        wallAroundIsland.SetActive(false);  
        wallThinkingZones.SetActive(false);

        AmbienceManager.Instance.StopEvent(thunderAmbience);
        RegionManager.Instance.gameObject.SetActive(true);

        var en = Player.Instance.GetComponent<Energy>();
        en.TakeDamage(-90f); //nasty hack, but Tim's saving comes in too late...

    }

}
