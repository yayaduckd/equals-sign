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

        void Start()
        {
            _ = _menuManager.ToMenu(_pauseMenu);
        }

        public void Escape() => _menuManager.Escape();
    }
}