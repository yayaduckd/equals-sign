using UnityEngine;

namespace Project.Menus.Credits
{
    public class CreditsMenu : Menu
    {
        [Header("Connected Menus")]
        [SerializeField] private MenuManager _menuManager;
        [SerializeField] private Menu mainMenu;

        public override void Escape()
        {
            _ = _menuManager.ToMenu(mainMenu);
        }

        // ------------ Buttons ------------

        // ------------ Functions ------------
    }
}