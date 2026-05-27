using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

namespace Project.Menus.Credits
{
    public class CreditsMenu : Menu
    {
        [Header("UI References")]
        [SerializeField] private RectTransform contentRect;

        [Header("Connected Menus")]
        [SerializeField] private MenuManager _menuManager;
        [SerializeField] private Menu mainMenu;

        [Header("Settings")]
        [SerializeField] private float scrollSpeed = 50f;
        [SerializeField] private float fastScrollSpeed = 100f;
        [SerializeField] private float autoReturnDelay = 5f;

        private Sequence creditsSeq;

        public override void OnEnteredMenu()
        {
            // Ensure anchors are centered horizontally and vertically
            contentRect.anchorMin = new Vector2(0.5f, 0);
            contentRect.anchorMax = new Vector2(0.5f, 0);
            contentRect.pivot = new Vector2(0.5f, 0);

            float parentHeight = ((RectTransform)contentRect.parent).rect.height;

            Vector2 startPos = Vector2.up * (-contentRect.rect.height);
            Vector2 endPos = Vector2.up * (parentHeight);

            contentRect.anchoredPosition = startPos;

            float duration = Vector2.Distance(startPos, endPos) / scrollSpeed;

            creditsSeq?.Kill();
            creditsSeq = DOTween.Sequence()
            .SetUpdate(true)
            .Append(contentRect.DOAnchorPos(endPos, duration))
            .AppendInterval(autoReturnDelay)
            .AppendCallback(() => { Escape(); });
        }

        public override void Escape()
        {
            creditsSeq?.Kill();
            _ = _menuManager.ToMenu(mainMenu);
        }

        // ------------ Buttons ------------

        // ------------ Functions ------------

        void Update()
        {
            if (creditsSeq != null && creditsSeq.IsActive())
            {
                if (Input.anyKey)
                    creditsSeq.timeScale = 1;
                else
                    creditsSeq.timeScale = scrollSpeed / fastScrollSpeed;
            }
        }
    }
}