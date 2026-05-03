using UnityEngine;
using Assets.Modules.Interaction;
using UnityEngine.Events;
using DG.Tweening;
using TDK.SaveSystem;
using System;

namespace TDK.ItemSystem
{
    [RequireComponent(typeof(Rigidbody))]
    public class ItemController : MonoBehaviour, IInteractable, ISaveable
    {
        public ItemData ItemData;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        private Rigidbody _rigidbody;

        public event Action<ItemController> OnDestroyEvent;

        private OutlineObject _outlineObject;
        
        static float OutlineWidth = 10f;
        static Color OutlineColor = Color.white;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if(!TryGetComponent<OutlineObject>(out _outlineObject)){
                _outlineObject = gameObject.AddComponent<OutlineObject>();
            }
            _outlineObject.enabled = false;
            _outlineObject.outlineInfo.outlineColor = OutlineColor;
        }

        void OnValidate()
        {
            UpdateVisuals();
        }

        public void Initialize(ItemSaveData data) => Initialize(data.GetItemData(), data.Position, data.Velocity);
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

        public void MoveTo(Vector3 target, float duration)
        {
            doMove?.Kill();
            doMove = transform.DOBlendableMoveBy(target - transform.position, duration);
        }

        #region  Interactable Interface

        virtual public void Interact()
        {
            if (ItemData.TryWorldItemInteract())
                Destroy(gameObject);
        }

        public void Focus()
        {
            _outlineObject.AnimateIn(OutlineWidth);
        }

        public void Unfocus()
        {
            _outlineObject.AnimateOut();
        }

        #endregion

        private void OnDestroy()
        {
            OnDestroyEvent?.Invoke(this);
            doMove?.Kill();
        }

        public void SaveData(ref WorldSaveData data)
        {
            if (ItemData == null) return;
            Rigidbody rigidbody = GetComponent<Rigidbody>();
            data.Items.Add(new()
            {
                ItemId = ItemData.GetId(),
                Position = transform.position,
                Velocity = rigidbody.linearVelocity
            });
        }
    }
}