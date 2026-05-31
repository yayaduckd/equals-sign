using UnityEngine;
using System.Collections.Generic;
using TDK.ItemSystem.Types;
using TDK.SaveSystem;
using System.Linq;
using System;
using UnityEngine.UI;

namespace TDK.ItemSystem.Inventory
{
    public class RecipeBookController : MonoBehaviour, ISaveable, ILoadable
    {
        public static RecipeBookController Instance;

        public List<RecipeItem> CollectedRecipes { get; private set; } = new();
        private List<RecipeItem> UsedRecipes = new();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public bool TryAddRecipe(RecipeItem recipeItem)
        {
            if (CollectedRecipes.Contains(recipeItem))
                return false;
            SetVisualization(false);
            CollectedRecipes.Add(recipeItem);
            return true;
        }

        public bool TryRemoveRecipe(RecipeItem recipeItem)
        {
            SetVisualization(false);
            if (CollectedRecipes.Remove(recipeItem))
            {
                UsedRecipes.Add(recipeItem);
                return true;
            }
            return false;
        }

        #region Triggers

        public void TriggerVisualization()
        {
            SetVisualization(!IsVisualized);
        }

        public void SetVisualization(bool isEnabled)
        {
            if (CollectedRecipes.Count == 0)
                IsVisualized = false;
            else
            {
                IsVisualized = isEnabled;
                currentPageIndex = (currentPageIndex % CollectedRecipes.Count + CollectedRecipes.Count) % CollectedRecipes.Count; // yes I have to do this (look up how the mod opperator works with negative numbers)
                _leftPage.sprite = CollectedRecipes[currentPageIndex].GetRecipeVisualizationSprite();
                _rightPage.sprite = CollectedRecipes[currentPageIndex].GetRecipeVisualizationSprite();
            }
            _bookAnimator.SetBool("OpenRecipeBook", IsVisualized);
            // DestroyStack();
            // if (IsVisualized) BuildStack();
        }

        public void NextPage()
        {
            if (IsVisualized)
            {
                if (CollectedRecipes.Count < 2) return;

                currentPageIndex = (currentPageIndex % CollectedRecipes.Count + CollectedRecipes.Count) % CollectedRecipes.Count; // yes I have to do this (look up how the mod opperator works with negative numbers)
                _leftPage.sprite = CollectedRecipes[currentPageIndex].GetRecipeVisualizationSprite();
                currentPageIndex = ((currentPageIndex + 1) % CollectedRecipes.Count + CollectedRecipes.Count) % CollectedRecipes.Count; // yes I have to do this (look up how the mod opperator works with negative numbers)
                _rightPage.sprite = CollectedRecipes[currentPageIndex].GetRecipeVisualizationSprite();

                print($"flipping page {currentPageIndex} left");
                _pageAnimator.SetTrigger("FlipLeft");
            }
        }

        public void PreviousPage()
        {
            if (IsVisualized)
            {
                if (CollectedRecipes.Count < 2) return;

                currentPageIndex = (currentPageIndex % CollectedRecipes.Count + CollectedRecipes.Count) % CollectedRecipes.Count; // yes I have to do this (look up how the mod opperator works with negative numbers)
                _rightPage.sprite = CollectedRecipes[currentPageIndex].GetRecipeVisualizationSprite();
                currentPageIndex = ((currentPageIndex - 1) % CollectedRecipes.Count + CollectedRecipes.Count) % CollectedRecipes.Count; // yes I have to do this (look up how the mod opperator works with negative numbers)
                _leftPage.sprite = CollectedRecipes[currentPageIndex].GetRecipeVisualizationSprite();

                print($"flipping page {currentPageIndex} right");
                _pageAnimator.SetTrigger("FlipRight");
            }
        }

        #endregion

        #region Visualization

        [Header("Visualization")]

        // private List<GameObject> pageObjects = new List<GameObject>();
        // [SerializeField] private GameObject pagePrefab;
        private int currentPageIndex = 0;
        [SerializeField] private Animator _bookAnimator;
        [SerializeField] private Animator _pageAnimator;
        [SerializeField] private Image _leftPage;
        [SerializeField] private Image _rightPage;
        // [SerializeField] private int xStackOffset = 1;
        public bool IsVisualized { get; private set; } = false;

        // private void BuildStack()
        // {
        //     for (int i = 0; i < CollectedRecipes.Count; i++)
        //     {
        //         GameObject obj = Instantiate(pagePrefab, transform, false);

        //         //set the image sprite (its in the children because of shitty ui reasons)
        //         var image = obj.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
        //         image.sprite = CollectedRecipes[i].GetRecipeVisualizationSprite();

        //         //position page UI (stacking offset)
        //         RectTransform imgRect = obj.transform.GetChild(0).GetComponent<RectTransform>();
        //         imgRect.anchoredPosition = new Vector2(xStackOffset * i, 0);

        //         pageObjects.Add(obj);
        //     }

        //     //Now set the draw order, because this is *of course* managed by hierarchy order
        //     //yes we must reverse it
        //     for (int i = 0; i < pageObjects.Count; i++)
        //     {
        //         pageObjects[i].transform.SetSiblingIndex(pageObjects.Count - 1 - i);
        //     }
        // }

        // private void DestroyStack()
        // {
        //     currentPageIndex = 0;
        //     foreach (var page in pageObjects)
        //     {
        //         Destroy(page.gameObject);
        //     }
        //     pageObjects.Clear();
        // }


        #endregion

        #region Save & Load

        private List<RecipeItem> ItemsToRecipes(IEnumerable<ItemData> items)
        {
            List<RecipeItem> recipes = new();
            foreach (ItemData item in items)
            {
                if (item is RecipeItem recipe)
                    recipes.Add(recipe);
                else
                    Debug.LogWarning("Items: Cannot extract recipe from item.");
            }
            return recipes;
        }

        public void LoadData(WorldSaveData data)
        {
            CollectedRecipes = ItemsToRecipes(ItemServices.Instance.Database.GetAssets(data.Inventory.CollectedRecipes));
            UsedRecipes = ItemsToRecipes(ItemServices.Instance.Database.GetAssets(data.Inventory.CollectedRecipes));
        }

        public void SaveData(ref WorldSaveData data)
        {
            data.Inventory.CollectedRecipes = ItemServices.Instance.Database.GetIds(CollectedRecipes).ToList();
            data.Inventory.UsedRecipes = ItemServices.Instance.Database.GetIds(UsedRecipes).ToList();
        }

        #endregion
    }
}