using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace Project.Menus.Audio
{
    public class AudioSettingsMenu : Menu
    {
        [Header("UI References")]
        [SerializeField] private Slider _masterVolume;
        [SerializeField] private Slider _musicVolume;
        [SerializeField] private Slider _sfxVolume;
        [SerializeField] private Slider _ambienceVolume;

        void Start()
        {
        }

        public override void OnEnteringMenu()
        {
            _masterVolume.value = AudioSettingsManager.Instance.MasterVolume;
            _musicVolume.value = AudioSettingsManager.Instance.MusicVolume;
            _sfxVolume.value = AudioSettingsManager.Instance.SfxVolume;
            _ambienceVolume.value = AudioSettingsManager.Instance.AmbienceVolume;
        }

        // ------------ Buttons ------------

        public void OnMasterVolumeChanged()
        {
            AudioSettingsManager.Instance.MasterVolume = Mathf.RoundToInt(_masterVolume.value);
        }

        public void OnMusicVolumeChanged()
        {
            AudioSettingsManager.Instance.MusicVolume = Mathf.RoundToInt(_musicVolume.value);
        }

        public void OnSfxVolumeChanged()
        {
            AudioSettingsManager.Instance.SfxVolume = Mathf.RoundToInt(_sfxVolume.value);
        }

        public void OnAmbienceVolumeChanged()
        {
            AudioSettingsManager.Instance.AmbienceVolume = Mathf.RoundToInt(_ambienceVolume.value);
        }

        // ------------ Functions ------------
    }
}