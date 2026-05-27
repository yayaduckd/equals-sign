using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

namespace Project.Menus.Audio
{
    public class AudioSettingsMenu : Menu
    {
        [Header("UI References")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TMP_Text masterVolumeText;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private TMP_Text musicVolumeText;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TMP_Text sfxVolumeText;
        [SerializeField] private Slider ambienceVolumeSlider;
        [SerializeField] private TMP_Text ambienceVolumeText;

        public override void OnEnteringMenu()
        {
            RefreshVisuals();
        }

        public override void OnExitingMenu()
        {
            AudioSettingsManager.Instance.SaveSettings();
        }

        // ------------ Buttons ------------

        public void OnMasterVolumeChanged()
        {
            float value = masterVolumeSlider.value;
            AudioSettingsManager.Instance.MasterVolume = value;
            RefreshVisuals();
        }

        public void OnMusicVolumeChanged()
        {
            float value = musicVolumeSlider.value;
            AudioSettingsManager.Instance.MusicVolume = value;
            RefreshVisuals();
        }

        public void OnSfxVolumeChanged()
        {
            float value = sfxVolumeSlider.value;
            AudioSettingsManager.Instance.SfxVolume = value;
            RefreshVisuals();
        }

        public void OnAmbienceVolumeChanged()
        {
            float value = ambienceVolumeSlider.value;
            AudioSettingsManager.Instance.AmbienceVolume = value;
            RefreshVisuals();
        }

        // ------------ Functions ------------

        private void RefreshVisuals()
        {
            AudioSettings settings = AudioSettingsManager.Instance._settings;
            masterVolumeSlider.value = settings.masterVolume;
            masterVolumeText.text = Mathf.RoundToInt(settings.masterVolume * 100) + "%";
            musicVolumeSlider.value = settings.musicVolume;
            musicVolumeText.text = Mathf.RoundToInt(settings.musicVolume * 100) + "%";
            sfxVolumeSlider.value = settings.sfxVolume;
            sfxVolumeText.text = Mathf.RoundToInt(settings.sfxVolume * 100) + "%";
            ambienceVolumeSlider.value = settings.ambienceVolume;
            ambienceVolumeText.text = Mathf.RoundToInt(settings.ambienceVolume * 100) + "%";
        }
    }
}