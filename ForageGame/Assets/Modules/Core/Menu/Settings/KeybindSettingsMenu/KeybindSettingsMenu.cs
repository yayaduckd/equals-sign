using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Project.Menus.Keybind
{
    public class KeybindSettingsMenu : Menu
    {
        private KeybindElement[] keybindElements;
        void OnValidate()
        {
            keybindElements = GetComponentsInChildren<KeybindElement>();
        }

        public override void EnteringMenu()
        {
            base.EnteringMenu();
            RefreshVisuals();
        }

        public override void ExitingMenu()
        {
            base.ExitingMenu();
            KeybindSettingsManager.Instance.SaveSettings();
        }

        // ------------ Buttons ------------

        public void OnResetButtonClicked()
        {
            foreach (KeybindElement keybindElement in keybindElements)
                keybindElement.ResetToDefault();
            RefreshVisuals();
        }

        // ------------ Functions ------------

        private void RefreshVisuals()
        {
            foreach (KeybindElement keybindElement in keybindElements)
                keybindElement.UpdateBindingDisplay();
        }
    }
}