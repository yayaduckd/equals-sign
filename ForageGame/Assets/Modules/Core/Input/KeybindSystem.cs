using System.Linq;
using Project.Menus.Keybind;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D;

public static class KeybindSystem
{
    public enum DeviceType { Keyboard, Gamepad, LastUsed, LastConnected }

    public static InputControl GetInputControlByDeviceType(DeviceType deviceType)
    {
        InputControl result;
        switch (deviceType)
        {
            case DeviceType.Keyboard:
                break; // TODO

            case DeviceType.Gamepad:
                break; // TODO

            case DeviceType.LastUsed:
                result = InputSystem.devices
                .Where(d => d.lastUpdateTime > 0)
                .OrderByDescending(d => d.lastUpdateTime)
                .FirstOrDefault();
                if (result != null) return result;
                break;

            case DeviceType.LastConnected:
                result = InputSystem.devices
                .OrderByDescending(d => d.lastUpdateTime)
                .FirstOrDefault();
                if (result != null) return result;
                break;
        }
        // Default to first available device
        result = InputSystem.devices[0];
        if (result != null) return result;
        Debug.Log("This is a major error - you have royally fucked up ngl...");
        return null;
    }

    public static Sprite GetKeybindVisual(InputAction action, DeviceType device)
    {
        InputControl inputControl = GetInputControlByDeviceType(device);

        if (action == null) return null;

        foreach (var binding in action.bindings)
        {
            var control = InputControlPath.TryFindControl(inputControl, binding.effectivePath);
            if (control != null)
            {
                int bindingIndex = action.GetBindingIndexForControl(control);
                if (bindingIndex >= 0)
                {
                    string displayString = action.GetBindingDisplayString(bindingIndex, out string deviceLayoutName, out string controlPath);
                    return KeybindSpritesDatabase.Instance?.GetKeybindSprite(displayString, deviceLayoutName, controlPath);
                }
            }
        }
        return null;
    }

    // public SpriteAtlas GetDeviceInputAtlas(DeviceType device)
    // {

    // }

    // public SpriteAtlas GetInputSprite(InputControl controlScheme, InputAction action)
    // {

    // }
}
