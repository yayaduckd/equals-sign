using System.Collections.Generic;
using TDK.ItemSystem.Types;
using UnityEngine;

namespace TDK.ItemSystem.Inventory
{
    public class Crafter : MonoBehaviour
    {
        [SerializeField] private ItemRackController _itemRack;
        [SerializeField] private SimpleItemSpawner _itemSpawner;
        [SerializeField] private Animator _animator;

        bool craftInProgress = false;

        public void TryCraftVoid() => TryCraft();

        public bool TryCraft()
        {
            if (craftInProgress) return false;
            foreach (RecipeItem recipeItem in RecipeBookController.Instance.CollectedRecipes)
            {
                if (TryCraftRecipe(recipeItem))
                    return true;
            }
            return false;
        }

        private bool TryCraftRecipe(RecipeItem recipeItem)
        {
            if (_itemRack.ContainsItemsExactly(recipeItem.GetCraftingIngredients()))
            {
                CraftRecipe(recipeItem);
                return true;
            }
            else return false;
        }

        private void CraftRecipe(RecipeItem recipeItem)
        {
            craftInProgress = true;
            if (!recipeItem.GetIsRepeatCraftable()) RecipeBookController.Instance?.TryRemoveRecipe(recipeItem);
            _itemSpawner.SetItem(recipeItem.GetCraftingResult());
            _animator.SetTrigger("Craft");
        }

        public void AnimatorOnLidClose()
        {
            List<ItemController> itemControllers = new(_itemRack.GetItemControllers()); // Make a copy
            foreach (ItemController item in itemControllers)
                Destroy(item.gameObject);
        }

        public void AnimatorOnLidOpen()
        {
            _itemSpawner.SpawnLocal();
            craftInProgress = false;
        }
    }
}