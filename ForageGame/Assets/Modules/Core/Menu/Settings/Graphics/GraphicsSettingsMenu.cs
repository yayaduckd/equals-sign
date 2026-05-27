using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using System;

namespace Project.Menus.Graphics
{
    public class GraphicsSettingsMenu : Menu
    {
        [Header("UI References")]
        [SerializeField] private TMP_Dropdown resolutionDropdown;
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Toggle vsyncToggle;
        [SerializeField] private Slider framerateSlider;
        [SerializeField] private TMP_Text framerateText;

        void Start()
        {
            SetupResolutions();
            SetupQualityLevels();
        }

        public override void OnEnteringMenu()
        {
            RefreshVisuals();
        }

        public override void OnExitingMenu()
        {
            GraphicsSettingsManager.Instance.SaveSettings();
        }

        // ------------ Buttons ------------

        public void OnResolutionChanged()
        {
            int index = resolutionDropdown.value;
            GraphicsSettingsManager.Instance.Resolution = index;
        }

        public void OnQualityChanged()
        {
            int index = qualityDropdown.value;
            GraphicsSettingsManager.Instance.Quality = index;
        }

        public void OnVsyncChanged()
        {
            bool isEnabled = vsyncToggle.isOn;
            GraphicsSettingsManager.Instance.Vsync = isEnabled;
        }

        public void OnFramerateChanged()
        {
            int value = Mathf.RoundToInt(framerateSlider.value);
            GraphicsSettingsManager.Instance.Framerate = value;
            RefreshVisuals();
        }

        // ------------ Functions ------------

        private void SetupResolutions()
        {
            resolutionDropdown.ClearOptions();

            List<string> resolutionOptions = new List<string>();

            for (int i = 0; i < GraphicsSettingsManager.Instance._resolutions.Length; i++)
            {
                string resolutionOption = $"{GraphicsSettingsManager.Instance._resolutions[i].width} x {GraphicsSettingsManager.Instance._resolutions[i].height}";
                resolutionOptions.Add(resolutionOption);
            }
            resolutionDropdown.AddOptions(resolutionOptions);
        }

        private void SetupQualityLevels()
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        }

        private void RefreshVisuals()
        {
            GraphicsSettings settings = GraphicsSettingsManager.Instance._settings;

            resolutionDropdown.value = settings.resolutionIndex;

            qualityDropdown.value = settings.qualityLevel;

            vsyncToggle.isOn = settings.vsyncEnabled;

            framerateSlider.value = settings.targetFramerate;
            framerateText.text = settings.targetFramerate.ToString();
        }
    }
}