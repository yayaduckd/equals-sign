using UnityEngine;

namespace Project.Menus.Credits
{
    public class CreditsRoller : MonoBehaviour
    {
        [SerializeField] private CreditsMenu _creditsMenu;
        [SerializeField] private Animator _creditsAnimator;
        [SerializeField] private float _fastSpeed = 3f;

        void Update()
        {
            if (_creditsAnimator.isActiveAndEnabled)
            {
                if (Input.anyKey)
                    _creditsAnimator.SetFloat("speed", _fastSpeed);
                else
                    _creditsAnimator.SetFloat("speed", 1);
            }
        }

        public void EndCredits()
        {
            _creditsMenu.Escape();
        }
    }
}
