using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Project.Menus.Keybind
{
    public class KeybindSettingsMenu : Menu
    {
        [SerializeField] private RebindActionUI[] keybindElements;
        void OnValidate()
        {
            keybindElements = GetComponentsInChildren<RebindActionUI>();
        }

        public override void OnEnteringMenu()
        {
            RefreshVisuals();
        }

        public override void OnExitingMenu()
        {
            KeybindSettingsManager.Instance.SaveSettings();
        }

        // ------------ Buttons ------------

        public void OnResetButtonClicked()
        {
            foreach (RebindActionUI keybindElement in keybindElements)
                keybindElement.ResetToDefault();
            RefreshVisuals();
        }

        // ------------ Functions ------------

        private void RefreshVisuals()
        {
            foreach (RebindActionUI keybindElement in keybindElements)
                keybindElement.UpdateBindingDisplay();
        }
    }
}