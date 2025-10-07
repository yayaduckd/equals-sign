using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

public class TextboxReplaceSprite : MonoBehaviour
{

    [TextArea(2, 3)]
    [SerializeField] private string message = "Press {Action} to interact";

    [Header("Setup for sprites")]
    [SerializeField] private ListOfTmpSpriteAssets listOfTmpSpriteAssets;
    [SerializeField] private PlayerInput playerInput;
    private TMP_Text _textBox;

    string activeScheme;

    private void Start()
    {
        _textBox = GetComponent<TMP_Text>();
        InputSystem.onActionChange += (a, b) => TrackActiveScheme();
        TrackActiveScheme();
    }

    private void TrackActiveScheme()
    {

        if(activeScheme != playerInput.currentControlScheme && playerInput.currentControlScheme != null) //null check to prevent error when closing the game
        {
            activeScheme = playerInput.currentControlScheme;
            SetText(activeScheme);
        }
    }

    [ContextMenu("Set Text")]
    public void SetText(string scheme = "")
    {

        if(scheme == "") { scheme = playerInput.currentControlScheme; } //If a specific scheme is specified, we use that one. Otherwise, use the one that's active.

        //BAD IMPLEMENTATION! This is due to Unity not serializing dictionaries. This is very much a workaround approach, and would be better if replaced by a custom serializable dictionary
        TMP_SpriteAsset spriteAsset = listOfTmpSpriteAssets.SpriteAssets[0].spriteAsset;
        foreach (ListOfTmpSpriteAssets.SpriteAssetWrapper wrapper in listOfTmpSpriteAssets.SpriteAssets)
        {
            if(wrapper.inputControlScheme == scheme) { spriteAsset = wrapper.spriteAsset; }
        }
        

        string pattern = "{([^{}]*)}";
        string newMessage = Regex.Replace(message, pattern, m => {
            string input = m.Groups[1].Value;
            List<string> bindings = GetAssociatedBindings(playerInput.actions[input], scheme);

            string textToDisplay = "";
            for (int i = 0; i < bindings.Count; i++)
            {
                if(i != 0) { textToDisplay += " / "; }
                if(spriteAsset.spriteCharacterTable.Any(sprite => sprite.name == bindings[i]))//Check if the sprite asset has the correct device type
                {
                    textToDisplay += $"<sprite=\"{spriteAsset.name}\" name=\"{bindings[i]}\">";
                }
                else
                {
                    textToDisplay += "<b>" + bindings[i] + "</b>"; //If the binding does not have a sprite, just display the text
                }
            }
            return textToDisplay;
            }
        );

        _textBox.text = newMessage;
    }


    private List<string> GetAssociatedBindings(InputAction action, string scheme)
    {
        List<string> Bindings = new List<string>();
        foreach (InputBinding binding in action.bindings)
        {
            string s = binding.ToString();
            Regex regex = new Regex("\\[(.*)\\]");
            MatchCollection matches = regex.Matches(s);
            foreach (Match match in matches)
            {
                if(match.Groups[1].Value == scheme)
                {
                    Bindings.Add(binding.ToDisplayString());
                }
            }
        }

        return Bindings;
    }
}
