using UnityEngine;
using DG.Tweening;
using TDK.SaveSystem;
using System;
using TDK.PlayerSystem;

namespace TDK.ItemSystem
{
    [RequireComponent(typeof(Rigidbody))]
    public class ItemController : MonoBehaviour, ISaveable
    {
        public ItemData ItemData;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private Rigidbody _rigidbody;
        [SerializeField] private bool _saveItem = true;
        [SerializeField] private GameObject _dropShadow;

        public event Action<ItemController> OnDestroyEvent;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
        }

        void OnValidate()
        {
            UpdateVisuals();
        }

        public void Initialize(ItemSaveData data) => Initialize(data.GetItemData(), data.Position, new());
        public void Initialize(ItemData item, Vector3 position, Vector3 velocity)
        {
            ItemData = item;
            transform.position = position;
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.linearVelocity = velocity;
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            if (ItemData != null && _spriteRenderer != null)
                _spriteRenderer.sprite = ItemData.GetSprite();
        }


        private Sequence _seq;

        public void SetPhysics(bool usePhysics)
        {
            _rigidbody.isKinematic = !usePhysics;
        }
        public void SetShadow(bool useShadow)
        {
            _dropShadow.SetActive(useShadow);
        }

        public void MoveTo(Vector3 target, float duration)
        {
            _seq?.Kill();
            _seq = DOTween.Sequence()
            .Append(transform.DOBlendableMoveBy(target - transform.position, duration));
        }

        #region  Interactable Interface

        public void Interact()
        {
            if (ItemData.TryWorldItemInteract())
            {
                _seq?.Kill();
                _seq = DOTween.Sequence()
                .Append(transform.DOMove(Player.Instance.transform.position, 0.1f).SetEase(Ease.InBack))
                .Insert(0, transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InBack)
                .OnComplete(() => Destroy(gameObject)));
            }
        }

        #endregion

        private void OnDestroy()
        {
            _seq?.Kill();
            OnDestroyEvent?.Invoke(this);
        }

        public void SaveData(ref WorldSaveData data)
        {
            if (!_saveItem) return;
            if (ItemData == null) return;
            data.Items.Add(new()
            {
                ItemId = ItemData.GetId(),
                Position = transform.position,
            });
        }
    }
}