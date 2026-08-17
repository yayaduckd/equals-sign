using System.IO;
using UnityEngine;
using UnityEngine.Audio;

namespace Project.Menus.Audio
{
    public class AudioSettingsManager : MonoBehaviour
    {
        public static AudioSettingsManager Instance { get; private set; }

        [Header("FMOD Studio Stuff")]

        [SerializeField] private string masterBusPath = "bus:/";
        private FMOD.Studio.Bus _masterBus;
        [SerializeField] private string musicBusPath;
        private FMOD.Studio.Bus _musicBus;
        [SerializeField] private string sfxBusPath;
        private FMOD.Studio.Bus _sfxBus;
        [SerializeField] private string ambienceBusPath;
        private FMOD.Studio.Bus _ambienceBus;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _masterBus = FMODUnity.RuntimeManager.GetBus(masterBusPath);
            _musicBus = FMODUnity.RuntimeManager.GetBus(musicBusPath);
            _ambienceBus = FMODUnity.RuntimeManager.GetBus(ambienceBusPath);
            _sfxBus = FMODUnity.RuntimeManager.GetBus(sfxBusPath);

            // Load & Apply All Settings
            MasterVolume = MasterVolume;
            MusicVolume = MusicVolume;
            SfxVolume = SfxVolume;
            AmbienceVolume = AmbienceVolume;
        }

        // ------------ Settings ------------

        public int MasterVolume
        {
            get => PlayerPrefs.GetInt("MasterVolume", 100);
            set
            {
                _masterBus.setVolume(value / 100);
                PlayerPrefs.SetInt("MasterVolume", value);
                PlayerPrefs.Save();
            }
        }

        public int MusicVolume
        {
            get => PlayerPrefs.GetInt("MusicVolume", 100);
            set
            {
                _musicBus.setVolume(value / 100);
                PlayerPrefs.SetInt("MusicVolume", value);
                PlayerPrefs.Save();
            }
        }

        public int SfxVolume
        {
            get => PlayerPrefs.GetInt("SfxVolume", 100);
            set
            {
                _sfxBus.setVolume(value / 100);
                PlayerPrefs.SetInt("SfxVolume", value);
                PlayerPrefs.Save();
            }
        }

        public int AmbienceVolume
        {
            get => PlayerPrefs.GetInt("AmbienceVolume", 100);
            set
            {
                _ambienceBus.setVolume(value / 100);
                PlayerPrefs.SetInt("AmbienceVolume", value);
                PlayerPrefs.Save();
            }
        }
    }
}