using System;
using Assets.Modules.Interaction;
using TDK.SaveSystem;
using UnityEngine;
using UnityEngine.Events;

namespace TDK.Gadgets
{
    public class StartEvent : MonoBehaviour
    {
        public UnityEvent OnStart;

        void Start()
        {
            OnStart.Invoke();
        }
    }
}
