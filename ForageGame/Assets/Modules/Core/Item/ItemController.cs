using UnityEngine;
using Assets.Modules.Interaction;
using UnityEngine.Events;
using DG.Tweening;
using TDK.SaveSystem;
using System;
using TDK.PlayerSystem;

namespace TDK.ItemSystem
{
    [RequireComponent(typeof(Rigidbody))]
    public class ItemController : DefaultInteractable, ISaveable
    {
        public ItemData ItemData;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private Rigidbody _rigidbody;
        [SerializeField] private bool _saveItem = true;

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


        private Tweener doMove;

        public void SetPhysics(bool usePhysics)
        {
            _rigidbody.isKinematic = !usePhysics;
        }
        public void SetShadow(bool useShadow)
        {
            DropShadowCaster shadow = GetComponentInChildren<DropShadowCaster>();
            if (shadow) shadow.enabled = useShadow;
        }

        public void MoveTo(Vector3 target, float duration)
        {
            doMove?.Kill();
            doMove = transform.DOBlendableMoveBy(target - transform.position, duration);
        }

        #region  Interactable Interface

        override public void AttemptInteract()
        {
            if (ItemData.TryWorldItemInteract())
            {
                SuccessfulInteract();
            }
            else
            {
                FailedInteract();
            }
        }

        protected override void SuccessfulInteract()
        {
            base.SuccessfulInteract();
            RemoveItem();
        }

        #endregion

        private void RemoveItem()
        {
            Sequence anim = DOTween.Sequence()
            .Append(transform.DOMove(Player.Instance.transform.position, 0.1f).SetEase(Ease.InBack))
            .Insert(0, transform.DOScale(Vector3.zero, 0.1f).SetEase(Ease.InBack)
            .OnComplete(() => Destroy(gameObject)));
        }

        private void OnDestroy()
        {
            OnDestroyEvent?.Invoke(this);
            doMove?.Kill();
        }

        public void SaveData(ref WorldSaveData data)
        {
            if (!_saveItem) return;
            if (ItemData == null) return;
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            data.Items.Add(new()
            {
                ItemId = ItemData.GetId(),
                Position = transform.position,
            });
        }
    }
}