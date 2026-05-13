using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

namespace TDK.ItemSystem.Inventory
{
    public class ItemPickupUI : MonoBehaviour
    {
        [Header("UI References")]
        public Image itemIcon;
        public TextMeshProUGUI itemName;
        public TextMeshProUGUI itemDescription;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        public void TriggerNewItemPopup(ItemData item)
        {
            gameObject.SetActive(true);

            transform.DOScale(Vector3.one, 0.4f).From(Vector3.zero).SetEase(Ease.OutBack);

            //Time.timeScale = 0f;
            itemIcon.sprite = item.GetSprite();
            itemName.text = item.GetName();
            itemDescription.text = item.GetDescription();
            StartCoroutine(ShowPopup(
            item.GetSprite(),
            item.GetName(),
            item.GetDescription()
            ));
        }

        public IEnumerator ShowPopup(Sprite icon, string name, string description)
        {
            // Optional small delay so player can't instantly skip
            yield return new WaitForSecondsRealtime(0.3f);

            // Wait for any key
            while (!Input.anyKeyDown)
                yield return null;

            // Resume game
            Time.timeScale = 1f;
            transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).onComplete = () => gameObject.SetActive(false);
        }
    }
}