using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

namespace Project.Menus.Keybind
{
    public class KeybindSettingsManager : MonoBehaviour
    {
        public static KeybindSettingsManager Instance { get; private set; }
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
            string keybinds = PlayerPrefs.GetString("Keybinds");
            if (!string.IsNullOrEmpty(keybinds))
                actions.LoadBindingOverridesFromJson(keybinds);
        }
        public void SaveSettings()
        {
            string keybinds = actions.SaveBindingOverridesAsJson();
            PlayerPrefs.SetString("Keybinds", keybinds);
        }
    }
}