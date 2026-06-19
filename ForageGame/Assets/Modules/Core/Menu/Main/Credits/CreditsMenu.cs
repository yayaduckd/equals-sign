using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.Events;

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