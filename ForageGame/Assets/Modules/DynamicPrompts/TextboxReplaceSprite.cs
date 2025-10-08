using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextboxReplaceSprite : MonoBehaviour
{
    [TextArea(2, 3)]
    [SerializeField] private string message = "Press {Action} to interact";

    [Header("Sprite Assets by Control Scheme")]
    [SerializeField] private ListOfTmpSpriteAssets listOfTmpSpriteAssets;
    [SerializeField] private PlayerInput playerInput;

    [Header("Optional TMP Text Display")]
    private TextMeshProUGUI textBox;
    private string activeScheme;

    private void Start()
    {
        textBox = GetComponent<TextMeshProUGUI>();

        // Track control scheme changes safely
        InputSystem.onActionChange += (_, _) => TrackActiveScheme();

        // Initialize display
        TrackActiveScheme();
    }

    private void TrackActiveScheme()
    {
        string scheme = playerInput?.currentControlScheme;
        if (!string.IsNullOrEmpty(scheme) && scheme != activeScheme)
        {
            activeScheme = scheme;
            SetText(activeScheme);
        }
    }

    [ContextMenu("Set Text")]
    public void SetText(string scheme = "")
    {
        if (string.IsNullOrEmpty(scheme))
            scheme = playerInput?.currentControlScheme;

        if (string.IsNullOrEmpty(scheme))
        {
            textBox.text = message;
            return;
        }

        TMP_SpriteAsset spriteAsset = listOfTmpSpriteAssets?.SpriteAssets
            .FirstOrDefault(w => w.inputControlScheme == scheme)?.spriteAsset
            ?? listOfTmpSpriteAssets?.SpriteAssets.FirstOrDefault()?.spriteAsset;

        if (spriteAsset == null)
        {
            textBox.text = message;
            return;
        }

        textBox.text = ReplacePlaceholders(message, scheme, spriteAsset);
    }

    private string ReplacePlaceholders(string template, string scheme, TMP_SpriteAsset spriteAsset)
    {
        string result = template;

        int start;
        while ((start = result.IndexOf('{')) != -1)
        {
            int end = result.IndexOf('}', start + 1);
            if (end == -1) break;

            string placeholder = result.Substring(start, end - start + 1);
            string actionName = result.Substring(start + 1, end - start - 1);

            string replacement = BuildBindingString(actionName, scheme, spriteAsset);
            result = result.Replace(placeholder, replacement);
        }

        return result;
    }

    private string BuildBindingString(string actionName, string scheme, TMP_SpriteAsset spriteAsset)
    {
        var action = playerInput?.actions?[actionName];
        if (action == null)
            return $"<b>{actionName}</b>";

        var bindings = GetBindingsForScheme(action, scheme);
        if (bindings.Count == 0)
            return $"<b>{actionName}</b>";

        var spriteTable = spriteAsset?.spriteCharacterTable;
        bool hasSprites = spriteTable != null && spriteTable.Count > 0;

        return string.Join(" / ", bindings.Select(binding =>
            hasSprites && spriteTable.Any(s => s != null && s.name == binding)
                ? $"<sprite=\"{spriteAsset.name}\" name=\"{binding}\">"
                : $"<b>{binding}</b>"
        ));
    }

    private List<string> GetBindingsForScheme(InputAction action, string scheme)
    {
        var list = new List<string>();
        if (action == null) return list;

        foreach (var binding in action.bindings)
        {
            if (!string.IsNullOrEmpty(binding.groups) && binding.groups.Contains(scheme))
                list.Add(binding.ToDisplayString());
        }

        return list;
    }
}
