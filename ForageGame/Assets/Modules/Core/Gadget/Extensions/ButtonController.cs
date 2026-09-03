using UnityEngine;
using UnityEngine.Events;

namespace TDK.Gadgets
{
    public class ButtonController : MonoBehaviour
    {
        public UnityEvent OnButtonPressed;
        public bool Locked = false;
        public void PressButton() { if (!Locked) OnButtonPressed.Invoke(); }
    }
}