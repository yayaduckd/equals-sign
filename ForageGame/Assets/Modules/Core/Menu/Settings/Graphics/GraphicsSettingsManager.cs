using UnityEngine;
using System.IO;

namespace Project.Menus.Graphics
{
    public class GraphicsSettingsManager : MonoBehaviour
    {
        public static GraphicsSettingsManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Load & Apply All Settings
            Resolution = Resolution;
            Quality = Quality;
            Vsync = Vsync;
            Framerate = Framerate;
        }

        // ------------ Settings ------------

        public int Resolution
        {
            get => PlayerPrefs.GetInt("Resolution", 0); // TODO
            set
            {
                if (-1 < value && value < Screen.resolutions.Length)
                    Screen.SetResolution(Screen.resolutions[value].width, Screen.resolutions[value].height, Screen.fullScreen);

                PlayerPrefs.SetInt("Resolution", value);
                PlayerPrefs.Save();
                Screen.SetResolution(1920, 1080, true);
            }
        }

        public int Quality
        {
            get => PlayerPrefs.GetInt("Quality", 0); // TODO
            set
            {
                QualitySettings.SetQualityLevel(value);
                PlayerPrefs.SetInt("Quality", value);
                PlayerPrefs.Save();
            }
        }

        public int Vsync
        {
            get => PlayerPrefs.GetInt("Vsync", 0);
            set
            {
                QualitySettings.vSyncCount = value;
                PlayerPrefs.SetInt("Vsync", value);
                PlayerPrefs.Save();
            }
        }

        public int Framerate
        {
            get => PlayerPrefs.GetInt("Framerate", 60);
            set
            {
                Application.targetFrameRate = value;
                PlayerPrefs.SetInt("Framerate", value);
                PlayerPrefs.Save();
            }
        }
    }
}