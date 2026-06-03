using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

namespace Project.Menus.Keybind
{
    public class KeybindSettingsManager : MonoBehaviour
    {
        public static KeybindSettingsManager Instance { get; private set; }
        private string settingsPath = "Assets/Save Data/Settings";
        [SerializeField] private InputActionAsset actions;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            LoadSettings();
        }

        public void LoadSettings()
        {
            string keybindPath = Path.Combine(settingsPath, "keybind.json");

            if (File.Exists(keybindPath))
            {
                string settingsJson = File.ReadAllText(keybindPath);
                if (!string.IsNullOrEmpty(settingsJson))
                    actions.LoadBindingOverridesFromJson(settingsJson);
            }
        }

        public void SaveSettings()
        {
            if (!Directory.Exists(settingsPath))
                Directory.CreateDirectory(settingsPath);

            string settingsJson = actions.SaveBindingOverridesAsJson();

            File.WriteAllText(Path.Combine(settingsPath, "keybind.json"), settingsJson);
        }
    }
}