using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Project.Menus
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Menu : MonoBehaviour
    {
        [Header("Menu Components & Settings")]
        [SerializeField] private Animator _animator;
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private GameObject _firstSelected;
        [SerializeField] private float _fadeInDuration = 0.5f;
        [SerializeField] private float _fadeOutDuration = 0.5f;

        public virtual void Escape()
        {
            throw new NotImplementedException();
        }

        public virtual void OnEnteringMenu() { }
        public virtual void OnEnteredMenu() { }
        public virtual void OnExitingMenu() { }
        public virtual void OnExitedMenu() { }

        public async Task EnterMenu()
        {
            Debug.Log("Entering menu " + this);
            gameObject.SetActive(true);
            SetCanvasGroup(false);

            OnEnteringMenu();

            _animator.SetBool("MenuActive", true);
            await Task.Delay(Mathf.CeilToInt(_fadeInDuration * 100));

            OnEnteredMenu();

            SetCanvasGroup(true);
            EventSystem.current.SetSelectedGameObject(_firstSelected);
            Debug.Log("Entered menu " + this);
        }

        public async Task ExitMenu()
        {
            Debug.Log("Exiting menu " + this);
            SetCanvasGroup(false);

            OnExitingMenu();

            _animator.SetBool("MenuActive", false);
            await Task.Delay(Mathf.CeilToInt(_fadeOutDuration * 1000));

            OnExitedMenu();

            gameObject.SetActive(false);
            Debug.Log("Exited menu " + this);
        }

        private void SetCanvasGroup(bool isActive)
        {
            _canvasGroup.blocksRaycasts = isActive;
            _canvasGroup.interactable = isActive;
        }
    }
}
