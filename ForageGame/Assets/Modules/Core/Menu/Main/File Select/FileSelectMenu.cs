using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;

namespace Project.Menus.FileSelect
{
    public class FileSelectMenu : Menu
    {
        [Header("UI References")]
        [SerializeField] private SaveSlotUI[] saveSlots = new SaveSlotUI[3];

        [Header("Connected Menus")]
        [SerializeField] private MenuManager _menuManager;
        [SerializeField] private Menu mainMenu;

        public override void OnEnteringMenu()
        {
            RefreshVisuals();
        }

        public override void Escape()
        {
            _ = _menuManager.ToMenu(mainMenu);
        }

        // ------------ Buttons ------------

        // ------------ Functions ------------

        private void RefreshVisuals()
        {
            foreach (SaveSlotUI saveSlot in saveSlots)
                saveSlot.Refresh();
        }
    }
}