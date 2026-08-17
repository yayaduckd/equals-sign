using UnityEngine;
using UnityEngine.UI;
using Project.Menus.Keybind;
using Project.Menus.Graphics;
using Project.Menus.Audio;
using System.Threading.Tasks;

namespace Project.Menus
{
    public class SettingsMenu : Menu
    {
        [Header("Buttons")]
        [SerializeField] private Button graphicsButton;
        [SerializeField] private Button audioButton;
        [SerializeField] private Button keybindsButton;

        [Header("Connected Menus")]
        [SerializeField] private MenuManager _menuManager;
        [SerializeField] private Menu returnMenu;
        [SerializeField] private MenuManager _submenuManager;
        [SerializeField] private Menu graphicsSettingsMenu;
        [SerializeField] private Menu audioSettingsMenu;
        [SerializeField] private Menu keybindSettingsMenu;

        [Header("Page Flutter")]
        [SerializeField] private Animator _pageFlutter;

        public override void OnEnteringMenu()
        {
            OnGraphicsClicked();
        }

        public override void OnExitingMenu()
        {
            KeybindSettingsManager.Instance.SaveSettings();
        }

        public override void Escape()
        {
            graphicsButton.interactable = false;
            audioButton.interactable = false;
            keybindsButton.interactable = false;
            _ = _submenuManager.ToMenu(null);
            _ = _menuManager.ToMenu(returnMenu);
        }

        // ------------ Buttons ------------

        public void OnGraphicsClicked()
        {
            _ = GoToSubMenu(graphicsSettingsMenu);
        }

        public void OnAudioClicked()
        {
            _ = GoToSubMenu(audioSettingsMenu);
        }

        public void OnKeybindsClicked()
        {
            _ = GoToSubMenu(keybindSettingsMenu);
        }

        private async Task GoToSubMenu(Menu menu)
        {
            if (_submenuManager.currentMenu == menu) return;
            _pageFlutter.SetTrigger("Trigger");
            graphicsButton.interactable = false;
            audioButton.interactable = false;
            keybindsButton.interactable = false;
            await _submenuManager.ToMenu(menu);
            graphicsButton.interactable = menu != graphicsSettingsMenu;
            audioButton.interactable = menu != audioSettingsMenu;
            keybindsButton.interactable = menu != keybindSettingsMenu;
        }
    }
}