using UnityEngine;
using Weather;

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
        else
        {
            WeatherManager.Instance.SetWeatherType(WeatherType.Thunder);
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
    }

}
