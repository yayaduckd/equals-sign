using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SpriteAssetRenamer : MonoBehaviour
{
    [Header("Assign your TMP Sprite Asset")]
    public TMP_SpriteAsset spriteAsset;

    [Header("Assign an Image or SpriteRenderer to preview")]
    public Image uiImage;
    public SpriteRenderer spriteRenderer;

    [Header("Assign a TextMeshProUGUI to display the current name")]
    public TextMeshProUGUI nameDisplay;

    private int currentIndex = 0;

    void Start()
    {
        if (spriteAsset == null || spriteAsset.spriteCharacterTable.Count == 0)
        {
            Debug.LogError("Sprite Asset is empty or not assigned.");
            enabled = false;
            return;
        }

        ShowCurrentSprite();
    }

    void Update()
    {
        if (!Input.anyKeyDown) return;

        // --- Navigation (Ctrl + Arrows) ---
        if (IsCtrlHeld())
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                GoToPreviousSprite();
                return;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                GoToNextSprite(); // Now updates UI
                return;
            }
        }

        // --- Skip with Tab ---
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            RenameCurrentSprite("");
            Debug.Log($"Skipped sprite {currentIndex} (blank name).");
            GoToNextSprite();
            return;
        }

        // --- Rename with valid key ---
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key) && IsValidRenameKey(key))
            {
                string newName = key.ToString();
                RenameCurrentSprite(newName);
                Debug.Log($"Renamed sprite {currentIndex} to \"{newName}\"");
                GoToNextSprite();
                break;
            }
        }
    }

    bool IsCtrlHeld()
    {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    bool IsValidRenameKey(KeyCode key)
    {
        return key != KeyCode.LeftControl &&
               key != KeyCode.RightControl &&
               key != KeyCode.LeftShift &&
               key != KeyCode.RightShift &&
               key != KeyCode.LeftAlt &&
               key != KeyCode.RightAlt &&
               key != KeyCode.Tab &&
               key != KeyCode.LeftArrow &&
               key != KeyCode.RightArrow &&
               key != KeyCode.UpArrow &&
               key != KeyCode.DownArrow &&
               key != KeyCode.None;
    }

    void RenameCurrentSprite(string newName)
    {
        var spriteChar = spriteAsset.spriteCharacterTable[currentIndex];
        spriteChar.name = newName;
        UpdateNameDisplay(newName);
    }

    void GoToNextSprite()
    {
        currentIndex++;
        if (currentIndex >= spriteAsset.spriteCharacterTable.Count)
        {
            Debug.Log("All sprites processed.");
            currentIndex = spriteAsset.spriteCharacterTable.Count - 1;
            enabled = false;
            return;
        }
        ShowCurrentSprite();
    }

    void GoToPreviousSprite()
    {
        currentIndex--;
        if (currentIndex < 0)
        {
            currentIndex = 0;
            Debug.Log("Already at first sprite.");
            return;
        }
        ShowCurrentSprite();
    }

    void ShowCurrentSprite()
    {
        var spriteChar = spriteAsset.spriteCharacterTable[currentIndex];
        int glyphIndex = (int)spriteChar.glyphIndex;

        if (glyphIndex < 0 || glyphIndex >= spriteAsset.spriteGlyphTable.Count)
        {
            Debug.LogWarning($"Sprite {currentIndex} has invalid glyph index.");
            return;
        }

        var sprite = spriteAsset.spriteGlyphTable[glyphIndex].sprite;

        if (uiImage != null) uiImage.sprite = sprite;
        if (spriteRenderer != null) spriteRenderer.sprite = sprite;

        UpdateNameDisplay(spriteChar.name);
        Debug.Log($"Showing sprite {currentIndex}/{spriteAsset.spriteCharacterTable.Count - 1}: {sprite.name} (Current name: \"{spriteChar.name}\")");
    }

    void UpdateNameDisplay(string name)
    {
        if (nameDisplay != null)
            nameDisplay.text = $"Sprite {currentIndex + 1}/{spriteAsset.spriteCharacterTable.Count}\nName: \"{name}\"";
    }
}
