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

        private Menu currentSubMenu;

        public override void OnEnteringMenu()
        {
            OnGraphicsClicked();
        }

        public override void OnExitingMenu()
        {
            GraphicsSettingsManager.Instance.SaveSettings();
            AudioSettingsManager.Instance.SaveSettings();
            KeybindSettingsManager.Instance.SaveSettings();
        }

        public override void Escape()
        {
            _ = _menuManager.ToMenu(returnMenu);
        }

        // ------------ Buttons ------------

        public void OnGraphicsClicked()
        {
            _ = GoToSubMenu(graphicsSettingsMenu, 1);
        }

        public void OnAudioClicked()
        {
            _ = GoToSubMenu(audioSettingsMenu, 2);
        }

        public void OnKeybindsClicked()
        {
            _ = GoToSubMenu(keybindSettingsMenu, 3);
        }

        private async Task GoToSubMenu(Menu menu, int janckySolIndex)
        {
            graphicsButton.interactable = false;
            audioButton.interactable = false;
            keybindsButton.interactable = false;
            await _submenuManager.ToMenu(keybindSettingsMenu);
            graphicsButton.interactable = true;
            audioButton.interactable = true;
            keybindsButton.interactable = true;
            if (janckySolIndex == 1) graphicsButton.interactable = false;
            if (janckySolIndex == 2) audioButton.interactable = false;
            if (janckySolIndex == 3) keybindsButton.interactable = false;
        }
    }
}