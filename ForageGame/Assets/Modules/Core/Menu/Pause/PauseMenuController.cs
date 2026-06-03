using System.Threading.Tasks;
using UnityEngine;

namespace Project.Menus
{
    public class PauseMenuController : MonoBehaviour
    {
        public static PauseMenuController Instance { get; private set; }
        [SerializeField] private MenuManager _menuManager;
        [SerializeField] private Menu _pauseMenu;
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Escape() => _menuManager.Escape();

        public async Task Load()
        {
            await _menuManager.ToMenu(_pauseMenu);
        }
    }
}