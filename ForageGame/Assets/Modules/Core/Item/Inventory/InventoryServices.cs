namespace TDK.ItemSystem.Inventory
{
    public static class InventoryServices
    {
        public static void Next()
        {
            if (!RecipeBookController.Instance && !InventoryController.Instance) return;

            if (RecipeBookController.Instance.IsVisualized)
                RecipeBookController.Instance.NextPage();
            else
                InventoryController.Instance.SelectNext();
        }

        public static void Previous()
        {
            if (!RecipeBookController.Instance && !InventoryController.Instance) return;

            if (RecipeBookController.Instance.IsVisualized)
                RecipeBookController.Instance.PreviousPage();
            else
                InventoryController.Instance.SelectPrevious();
        }
    }
}