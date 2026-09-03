using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TDK.SaveSystem;

namespace Project.Menus
{
    public class MainMenu : Menu
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text continueText;

        [Header("Connected Menus")]
        [SerializeField] private MenuManager _menuManager;
        [SerializeField] private Menu fileSelectMenu;
        [SerializeField] private Menu settingsMenu;
        [SerializeField] private Menu creditsMenu;

        public override void OnEnteringMenu()
        {
            RefreshVisuals();
        }

        public override void Escape()
        {
        }

        // ------------ Buttons ------------

        public void OnContinueClicked()
        {
            if (!AppController.Instance.IsInputsActive) return;
            AppController.Instance.SetInputsActive(false);
            _ = AppController.Instance.ToWorld();
        }

        public void OnFileSelectClicked()
        {
            _ = _menuManager.ToMenu(fileSelectMenu);
        }

        public void OnSettingsClicked()
        {
            _ = _menuManager.ToMenu(settingsMenu);
        }

        public void OnCreditsClicked()
        {
            _ = _menuManager.ToMenu(creditsMenu);
        }

        public void OnQuitClicked()
        {
            AppController.Instance.Quit();
        }

        // ------------ Functions ------------

        private void RefreshVisuals()
        {
            string worldId = PlayerPrefs.GetString("lastWorldUsed", null);

            if (worldId == null || !SaveServices.ExistsWorld(worldId))
                continueText.text = "New Game";
            else
                continueText.text = "Continue";
        }
    }
}