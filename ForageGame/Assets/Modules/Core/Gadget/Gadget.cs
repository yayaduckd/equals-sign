using System;
using Assets.Modules.Interaction;
using TDK.SaveSystem;
using UnityEngine;
using UnityEngine.Events;

namespace TDK.Gadgets
{
    [RequireComponent(typeof(Animator))]
    public class Gadget : MonoBehaviour, ISaveable, ILoadable
    {
        [Header("Save Options")]
        [SerializeField] private string _guid;
        [ContextMenu("Generate GUID")]
        public void GenerateGuid()
        {
            _guid = Guid.NewGuid().ToString();
        }

        [Header("Gadget Options")]
        [SerializeField] private Animator _animator;
        [SerializeField] private bool _initialState = false;
        [SerializeField] private bool _singleUse = false;
        public UnityEvent OnActivate;
        public UnityEvent OnDeactivate;

        void OnValidate()
        {
            _animator = GetComponent<Animator>();
        }

        private bool _locked = false;
        public bool Locked
        {
            get => _locked;
            private set => _locked = value;
        }
        private bool _state = false;
        public bool State
        {
            get => _state;
            private set
            {
                if (_locked) return;
                if (_singleUse) _locked = true;
                _state = value;
                if (value) OnActivate.Invoke();
                else OnDeactivate.Invoke();
                UpdateVisuals();
            }
        }

        void Awake()
        {
            _state = _initialState;
            UpdateVisuals();
        }

        private void Initialize(bool state, bool locked)
        {
            _state = state;
            _locked = locked;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (!_animator) return;
            if (!_animator.runtimeAnimatorController) return;
            _animator.SetBool("State", _state);
        }

        public void ToggleState() => State = !State;
        public void SetState(bool state) => State = state;
        public void ToggleLocked() => Locked = !Locked;
        public void SetLocked(bool locked) => Locked = locked;

        public void SaveData(ref WorldSaveData data)
        {
            data.Gadgets[_guid] = new()
            {
                State = _state,
                Locked = _locked
            };
        }

        public void LoadData(WorldSaveData data)
        {
            if (data.Gadgets.ContainsKey(_guid))
                Initialize(data.Gadgets[_guid].State, data.Gadgets[_guid].Locked);
            else
                Initialize(_state, _locked);
        }
    }

    [System.Serializable]
    public class GadgetSaveData
    {
        public bool State = new();
        public bool Locked = new();
    }
}
