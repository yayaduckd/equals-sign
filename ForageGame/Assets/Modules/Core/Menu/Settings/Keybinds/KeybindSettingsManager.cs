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

        public void LoadSettings() => actions.LoadBindingOverridesFromJson(PlayerPrefs.GetString("Keybinds"));
        public void SaveSettings() => PlayerPrefs.SetString("Keybinds", actions.SaveBindingOverridesAsJson());
    }
}