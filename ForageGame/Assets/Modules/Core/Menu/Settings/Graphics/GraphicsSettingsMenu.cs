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
        [SerializeField] private SelectionUIElement _resolution;
        [SerializeField] private SelectionUIElement _quality;
        [SerializeField] private SelectionUIElement _vsync;
        [SerializeField] private Slider _framerate;

        public override void OnEnteringMenu()
        {
            // Resolution
            List<string> resolutionOptions = new();
            for (int i = 0; i < Screen.resolutions.Length; i++)
            {
                string resolutionOption = $"{Screen.resolutions[i].width} x {Screen.resolutions[i].height}";
                resolutionOptions.Add(resolutionOption);
            }
            _resolution.SetOptions(resolutionOptions.ToArray());

            // Quality
            _quality.SetOptions(QualitySettings.names);

            // Vsync
            _vsync.SetOptions(new string[] { "Off", "On" });

            _resolution.SetCurrentOption(GraphicsSettingsManager.Instance.Resolution);
            _quality.SetCurrentOption(GraphicsSettingsManager.Instance.Quality);
            _vsync.SetCurrentOption(GraphicsSettingsManager.Instance.Vsync);
            _framerate.value = GraphicsSettingsManager.Instance.Framerate;
        }

        // ------------ Buttons ------------

        public void OnResolutionChanged()
        {
            GraphicsSettingsManager.Instance.Resolution = _resolution._currentOption;
        }

        public void OnQualityChanged()
        {
            GraphicsSettingsManager.Instance.Quality = _quality._currentOption;
        }

        public void OnVsyncChanged()
        {
            GraphicsSettingsManager.Instance.Vsync = _vsync._currentOption;
        }

        public void OnFramerateChanged()
        {
            GraphicsSettingsManager.Instance.Framerate = Mathf.RoundToInt(_framerate.value);
        }

        // ------------ Functions ------------
    }
}