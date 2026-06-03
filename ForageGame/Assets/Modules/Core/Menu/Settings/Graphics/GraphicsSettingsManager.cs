using UnityEngine;
using System.IO;

namespace Project.Menus.Graphics
{
    public class GraphicsSettingsManager : MonoBehaviour
    {
        public static GraphicsSettingsManager Instance { get; private set; }
        private string settingsPath = "Assets/Save Data/Settings";
        public Resolution[] _resolutions { get; private set; }
        public GraphicsSettings _settings { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _resolutions = Screen.resolutions;

            LoadSettings();
        }

        // ------------ Settings ------------

        public int Resolution
        {
            get => _settings.resolutionIndex;
            set
            {
                if (-1 < value && value < _resolutions.Length)
                {
                    _settings.resolutionIndex = value;
                    Screen.SetResolution(_resolutions[value].width, _resolutions[value].height, Screen.fullScreen);
                }
                SaveSettings();
                Screen.SetResolution(1920, 1080, true);
            }
        }

        public int Quality
        {
            get => _settings.qualityLevel;
            set
            {
                _settings.qualityLevel = value;
                QualitySettings.SetQualityLevel(value);
                SaveSettings();
            }
        }

        public bool Vsync
        {
            get => _settings.vsyncEnabled;
            set
            {
                _settings.vsyncEnabled = value;
                QualitySettings.vSyncCount = value ? 1 : 0;
                SaveSettings();
            }
        }

        public int Framerate
        {
            get => _settings.targetFramerate;
            set
            {
                _settings.targetFramerate = value;
                Application.targetFrameRate = value;
                SaveSettings();
            }
        }

        private void ApplySettings()
        {
            Resolution = Resolution;
            Quality = Quality;
            Vsync = Vsync;
            Framerate = Framerate;
        }

        // ------------ Save & Load ------------

        public void LoadSettings()
        {
            string graphicsPath = Path.Combine(settingsPath, "graphics.json");

            if (File.Exists(graphicsPath))
            {
                string json = File.ReadAllText(graphicsPath);
                _settings = JsonUtility.FromJson<GraphicsSettings>(json);
            }
            else
                _settings = new GraphicsSettings();
            ApplySettings();
        }

        public void SaveSettings()
        {
            if (!Directory.Exists(settingsPath))
                Directory.CreateDirectory(settingsPath);

            string settingsJson = JsonUtility.ToJson(_settings, true);

            File.WriteAllText(Path.Combine(settingsPath, "graphics.json"), settingsJson);
        }
    }
}