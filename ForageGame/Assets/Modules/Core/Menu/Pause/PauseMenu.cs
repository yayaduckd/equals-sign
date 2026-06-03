using System.Threading.Tasks;
using UnityEngine;

namespace Project.Menus
{
    public class PauseMenu : Menu
    {
        [Header("Connected Menus")]
        [SerializeField] private MenuManager _menuManager;
        [SerializeField] private Menu settingsMenu;

        public override void Escape()
        {
            _ = GameplayController.Instance.ResumeGame();
        }

        private async Task EscapeSequence()
        {
            await _menuManager.ToMenu(null);
            await GameplayController.Instance.ResumeGame();
        }

        // ------------ Buttons ------------

        public void OnSettingsClicked()
        {
            _ = _menuManager.ToMenu(settingsMenu);
        }

        public void OnMainMenuClicked()
        {
            _ = GameplayController.Instance.QuitToMainMenu();
        }

        public void OnQuitClicked()
        {
            GameplayController.Instance.QuitToDesktop();
        }

        // ------------ Functions ------------
    }
}