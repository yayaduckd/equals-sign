using UnityEngine;
using UnityEngine.Events;

namespace TDK.Gadgets
{
    public class ButtonController : MonoBehaviour
    {
        public UnityEvent OnButtonPressed;
        public void PressButton() { OnButtonPressed.Invoke(); }
    }
}