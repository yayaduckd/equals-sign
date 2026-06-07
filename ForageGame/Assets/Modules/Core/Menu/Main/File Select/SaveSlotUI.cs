using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using TMPro;
using TDK.SaveSystem;
using UnityEngine.UIElements.Experimental;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace Project.Menus.FileSelect
{
    public class SaveSlotUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button slotButton;
        [SerializeField] private TMP_Text slotText;
        [SerializeField] private TMP_Text infoText;
        [SerializeField] private Button deleteButton;
        [SerializeField] private TMP_Text deleteText;

        [Header("Settings")]
        [SerializeField] private string _worldId;


        public void Refresh()
        {
            if (SaveServices.ExistsWorld(_worldId))
            {
                WorldSaveData data = SaveServices.GetWorld(_worldId);
                slotText.text = "Continue"; // TODO: add duck progress image?
                string playtimeText = FormatPlaytime(data.playtimeSeconds);
                string completionText = FormatCompletion(data.StoryFlagSaveData);
                infoText.text = $"{playtimeText}\n{completionText}";
                deleteButton.interactable = true;
                deleteText.text = "Delete";
            }
            else
            {
                slotText.text = "New Game";
                infoText.text = "";
                deleteButton.interactable = false;
                deleteText.text = "";
            }
        }

        // ------------ Buttons ------------

        public void OnSlotSelected()
        {
            if (SaveServices.ExistsWorld(_worldId))
                _ = AppController.Instance.ToWorld(_worldId);   // Load game with this save file
            else
                _ = AppController.Instance.ToNewWorld(_worldId);    // Create new game in this slot
        }

        public void OnDeleteSlot()
        {
            if (SaveServices.ExistsWorld(_worldId))
            {
                SaveServices.DeleteWorld(_worldId);
                EventSystem.current.SetSelectedGameObject(slotButton.gameObject);
                Refresh();
            }
        }

        // ------------ Functions ------------

        private string FormatPlaytime(float seconds)
        {
            int hours = (int)(seconds / 3600);
            int minutes = (int)((seconds % 3600) / 60);
            return $"{hours}h {minutes}m";
        }

        private string FormatCompletion(List<string> flags)
        {
            // Hardcode the number of story flags?
            int value = Mathf.RoundToInt(flags.Count / 100);
            return $"{value}% Complete";
        }
    }
}