using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TDK.SaveSystem;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Project.Menus
{
    public class MainMenuController : MonoBehaviour
    {
        public static MainMenuController Instance { get; private set; }
        [SerializeField] private MenuManager _menuManager;

        [SerializeField] private Menu _mainMenu;
        [SerializeField] private Menu _creditsMenu;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public async Task Load(bool loadCredits = false)
        {
            if (loadCredits)
                await _menuManager.ToMenu(_creditsMenu);
            else
                await _menuManager.ToMenu(_mainMenu);
        }

        public void Escape() => _menuManager.Escape();
    }
}