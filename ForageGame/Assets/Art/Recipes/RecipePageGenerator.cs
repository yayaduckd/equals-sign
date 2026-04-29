using System;
using System.Collections;
using TDK.ItemSystem;
using TDK.ItemSystem.Types;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecipePageGenerator : MonoBehaviour
{
    [SerializeField] private TMP_Text _title;
    [SerializeField] private RectTransform _ingredientPanel;
    [SerializeField] private RecipeItem _recipeItem;

    void OnValidate() => Create();

    public void Create()
    {
        gameObject.name = _recipeItem.GetCraftingResult().GetName();
        _title.text = _recipeItem.GetCraftingResult().GetName();

        int requiredCount = _recipeItem.GetCraftingIngredients().Count;
        int currentCount = _ingredientPanel.childCount;

        if (requiredCount > currentCount)
            Debug.Log($"Need more ingredient UIs. Total needed = {requiredCount}");

        if (requiredCount < currentCount)
            Debug.Log($"Too many ingredient UIs. Total needed = {requiredCount}");

        int totalCount = Math.Min(requiredCount, currentCount);

        for (int i = 0; i < totalCount; i++)
        {
            ItemData item = _recipeItem.GetCraftingIngredients()[i];
            GameObject ingredientUI = _ingredientPanel.GetChild(i).gameObject;

            Image image = ingredientUI.GetComponentInChildren<Image>();
            TMP_Text text = ingredientUI.GetComponentInChildren<TMP_Text>();
            image.sprite = item.GetSketchSprite();
            text.text = item.GetName();
        }
    }

    public void Twist(Transform transform, float amount)
    {
        System.Random rnd = new();
        transform.rotation = new();
        transform.Rotate(new(0, 0, 1), ((float)rnd.NextDouble() - 0.5f) * 2 * amount);
    }

}
