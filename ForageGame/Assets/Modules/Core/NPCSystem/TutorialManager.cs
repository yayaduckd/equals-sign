using UnityEngine;
using Weather;
using AudioIntegration;

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

    [SerializeField] private Region homeBaseRegion; //TODO: on startup we should remember in general where we are... not just from the tutorial
    [SerializeField] private string thunderParamName = "ThunderWeight";
    [SerializeField] private string morningParamName = "MorningWeight";

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
            WeatherManager.Instance.SetWeatherType(WeatherType.Thunder);
            AmbienceManager.Instance.StartEvent(homeBaseRegion);
            AmbienceManager.Instance.SetParameter(thunderParamName, 1f);
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

        RegionManager.Instance.gameObject.SetActive(true); //just turn it off

        //TODO: this only assumes yes tutorial - no tutorial, remove once the player can load in at different spots
        //WeatherManager.Instance.SetWeatherType(WeatherType.Blossom);
        //AmbienceManager.Instance.StartEvent(homeBaseRegion);
        AmbienceManager.Instance.SetParameter(thunderParamName, 0f);
        AmbienceManager.Instance.SetParameter(morningParamName, 1f);
    }

}
