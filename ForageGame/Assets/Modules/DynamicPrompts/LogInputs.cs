using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class LogInputs : MonoBehaviour
{
    // We'll store our subscription here so we can unsubscribe later.
    private IDisposable m_OnAnyButtonPressSubscription;

    // This function is called when the object becomes enabled and active.
    private void OnEnable()
    {
        // We subscribe our custom function 'OnAnyButtonPress' to the 'onAnyButtonPress' event.
        // The Subscribe() method returns an IDisposable that represents the subscription.
        m_OnAnyButtonPressSubscription = InputSystem.onAnyButtonPress.Call(OnAnyButtonPress);
    }

    // This function is called when the object becomes disabled or inactive.
    private void OnDisable()
    {
        // It's crucial to unsubscribe from the event when the script is disabled
        // to prevent memory leaks and errors. We do this by calling Dispose() on our subscription.
        m_OnAnyButtonPressSubscription?.Dispose();
    }

    /// <summary>
    /// This method is called by the Input System whenever a button is pressed.
    /// </summary>
    /// <param name="control">The InputControl that was activated.</param>
    private void OnAnyButtonPress(InputControl control)
    {
        // Every control has a unique path that identifies it.
        // For example: "<Keyboard>/space", "<Gamepad>/buttonSouth", "<Mouse>/leftButton".
        // We log this path to the Unity console.
        Debug.Log("Input Pressed: " + control.path);
    }
}
