using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

namespace Project.Menus
{
    public class MenuManager : MonoBehaviour
    {
        public Menu currentMenu { get; private set; } = null;

        private async Task MenuTransition(Menu fromMenu, Menu toMenu)
        {
            if (fromMenu) await fromMenu.ExitMenu();
            currentMenu = toMenu;
            if (toMenu) await toMenu.EnterMenu();
        }

        public async Task ToMenu(Menu toMenu) => await MenuTransition(currentMenu, toMenu);

        public void Escape()
        {
            if (currentMenu != null)
                currentMenu.Escape();
        }
    }
}