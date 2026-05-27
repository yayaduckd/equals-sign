using UnityEngine;
using UnityEngine.Events;

namespace TDK.Gadgets
{
    public class BreakableWallController : MonoBehaviour
    {
        public UnityEvent OnBreak;
        private bool _isBroken = false;

        public void Break()
        {
            if (_isBroken) return;
            _isBroken = true;
            OnBreak.Invoke();
        }
    }
}
