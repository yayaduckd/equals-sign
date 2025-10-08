using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Modules.DynamicPrompts
{
    public class TextboxInputSprites : MonoBehaviour
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

            // Hook our method into input system action change
            InputSystem.onActionChange += (a, b) => TrackActiveScheme();
            TrackActiveScheme();
        }

        private void OnDestroy()
        {
            // Unhook our method from input system action change
            InputSystem.onActionChange -= (a, b) => TrackActiveScheme();
        }

        /// <summary>
        /// Checks if the active control scheme has changed and updates the text display accordingly.
        /// </summary>
        private void TrackActiveScheme()
        {
            string scheme = playerInput?.currentControlScheme;

            // Only update if the scheme has changed and is valid
            if (!string.IsNullOrEmpty(scheme) && scheme != activeScheme)
            {
                activeScheme = scheme;
                SetText();
            }
        }

        [ContextMenu("Set Text")]
        /// <summary>
        /// Sets the text display by replacing placeholders with corresponding input sprites based on the active control scheme.
        /// </summary>
        private void SetText()
        {
            if (activeScheme == null)
            {
                // No active scheme, revert to default message
                textBox.text = message;
                return;
            }

            // Get the corresponding sprite asset for the active control scheme
            TMP_SpriteAsset spriteAsset = GetCorrespondingSpriteAsset(activeScheme);
            if (spriteAsset == null)
            {
                textBox.text = message; // Fallback to default message if no sprite asset found
                return;
            }

            string updatedMessage = ReplacePlaceholders(message, activeScheme, spriteAsset);

            textBox.text = updatedMessage;
        }

        /// <summary>
        /// Gets the TMP_SpriteAsset corresponding to the given control scheme from the <see cref="listOfTmpSpriteAssets"/>. Returns null if none is found."/>
        /// </summary>
        /// <param name="scheme">The name of the scheme for which to retrieve the sprite asset.</param>
        /// <returns>The sprite asset containing the sprites for the inputs of the given control scheme.</returns>
        private TMP_SpriteAsset GetCorrespondingSpriteAsset(string scheme)
        {
            // Try to find a sprite asset matching the active control scheme

            // Safety checks
            if (listOfTmpSpriteAssets == null || listOfTmpSpriteAssets.SpriteAssets == null)
            {
                Debug.LogWarning("List of TMP Sprite Assets is not assigned or empty.");
                return null;
            }

            // Search for a matching control scheme
            foreach (var wrapper in listOfTmpSpriteAssets.SpriteAssets)
            {
                if (wrapper.inputControlScheme == activeScheme)
                {
                    if(wrapper.spriteAsset != null)
                    {
                        return wrapper.spriteAsset;
                    }
                    else
                    {
                        Debug.LogWarning($"No spriteAsset defined for {wrapper.inputControlScheme}");
                    }
                }
            }

            // Fallback if no match was found
            Debug.LogWarning($"No sprite asset found for control scheme '{activeScheme}'. Reverting to default message.");
            return null;
        }

        /// <summary>
        /// Replaces placeholders in the template string (e.g., {Action}) with
        /// their corresponding bindings from the TMP sprite asset.
        /// </summary>
        /// <param name="template">The input string containing placeholders</param>
        /// <param name="scheme">The current control scheme</param>
        /// <param name="spriteAsset">The TMP sprite asset to use for replacements</param>
        /// <returns>The processed string with placeholders replaced</returns>
        private string ReplacePlaceholders(string template, string scheme, TMP_SpriteAsset spriteAsset)
        {
            if (string.IsNullOrEmpty(template)) return string.Empty;

            var result = new StringBuilder();
            int lastIndex = 0;

            while (true)
            {
                // Find the next opening brace
                int start = template.IndexOf('{', lastIndex);
                if (start == -1) // If no opening brace is found
                {
                    // No more placeholders; append the rest of the string
                    result.Append(template.Substring(lastIndex));
                    break;
                }

                // Find the corresponding closing brace
                int end = template.IndexOf('}', start + 1);
                if (end == -1) // If no closing brace is found
                {
                    // Malformed placeholder; append the rest as-is
                    result.Append(template.Substring(lastIndex));
                    break;
                }

                // Append text before the placeholder
                result.Append(template.Substring(lastIndex, start - lastIndex));

                // Extract the placeholder content (e.g., "Action")
                string actionName = template.Substring(start + 1, end - start - 1);

                // Generate the replacement text for this placeholder
                string replacement = BuildBindingString(actionName, scheme, spriteAsset);

                // Append the replacement
                result.Append(replacement);

                // Move past the current placeholder
                lastIndex = end + 1;
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns a string with the sprite tag for the given action name and control scheme. If no sprite is found, returns the action name in bold. If no action is found, returns the action name in bold with a warning.
        /// </summary>
        /// <param name="actionName">The name of the action. This should correspond to an action in the given InputScheme.</param>
        /// <param name="scheme">The name of the input scheme for which to get the bindings</param>
        /// <param name="spriteAsset">The sprite asset corresponding to the given scheme</param>
        /// <returns></returns>
        private string BuildBindingString(string actionName, string scheme, TMP_SpriteAsset spriteAsset)
        {
            InputAction action = playerInput.actions[actionName];
            if (action == null)
            {
                Debug.LogWarning($"Action '{actionName}' not found in PlayerInput actions.");
                return $"<b>{actionName}</b>";
            }

            List<string> bindings = GetBindingsForScheme(action, scheme);

            if (bindings.Count == 0)
            {
                Debug.LogWarning($"No bindings found for action '{actionName}' in control scheme '{scheme}'.");
                return $"<b>{actionName}</b>";
            }

            List<TMP_SpriteCharacter> spriteTable = spriteAsset?.spriteCharacterTable;

            // Prepare a list to hold the formatted strings
            List<string> formattedBindings = new List<string>();

            foreach (string binding in bindings)
            {
                bool hasSprite = spriteTable.Any(s => s != null && s.name == binding);

                if (hasSprite)
                {
                    // Use the sprite from the TMP_SpriteAsset
                    formattedBindings.Add($"<sprite=\"{spriteAsset.name}\" name=\"{binding}\">");
                }
                else
                {
                    // Fallback: display the binding as bold text
                    formattedBindings.Add($"<b>{binding}</b>");
                    Debug.LogWarning($"No sprite found for binding '{binding}' in sprite asset '{spriteAsset.name}'.");
                }
            }

            // Join all formatted bindings with " / " separator (e.g., "A / B / X" if there are multiple bindings associated with the action)
            string result = string.Join(" / ", formattedBindings);

            return result;

        }

        /// <summary>
        /// Returns a list of binding names associated with the given action and control scheme.
        /// </summary>
        /// <param name="action">The input action for which to give the associated bindings</param>
        /// <param name="scheme">The scheme to get the bindings for</param>
        /// <returns></returns>
        private List<string> GetBindingsForScheme(InputAction action, string scheme)
        {
            var list = new List<string>();
            if (action == null)
            {
                Debug.LogWarning("InputAction is null. Cannot retrieve bindings.");
                return list;
            }

            foreach (InputBinding binding in action.bindings)
            {
                if (!string.IsNullOrEmpty(binding.groups) && binding.groups.Contains(scheme))
                    list.Add(binding.ToDisplayString());
            }

            return list;
        }
    }
}