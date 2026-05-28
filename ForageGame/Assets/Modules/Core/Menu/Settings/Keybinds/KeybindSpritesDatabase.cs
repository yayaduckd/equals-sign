using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;

namespace Project.Menus.Keybind
{
    public class KeybindSpritesDatabase : MonoBehaviour
    {
        public static KeybindSpritesDatabase Instance { get; private set; }

        [Header("Sprites")]
        [SerializeField] public Sprite missingSprite;
        public GamepadSprites xbox;
        public GamepadSprites ps4;
        public KeyboardSprites keyboard;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            //DontDestroyOnLoad(gameObject);
        }

        public Sprite GetKeybindSprite(string bindingDisplayString, string deviceLayoutName, string controlPath)
        {
            if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Keyboard"))
                return keyboard.GetSprite(controlPath);
            else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "DualShockGamepad"))
                return ps4.GetSprite(controlPath);
            else if (InputSystem.IsFirstLayoutBasedOnSecond(deviceLayoutName, "Gamepad"))
                return xbox.GetSprite(controlPath);
            else
                return missingSprite;
        }

        [Serializable]
        public struct GamepadSprites
        {
            public Sprite missingSprite;
            public Sprite buttonSouth;
            public Sprite buttonNorth;
            public Sprite buttonEast;
            public Sprite buttonWest;
            public Sprite startButton;
            public Sprite selectButton;
            public Sprite leftTrigger;
            public Sprite rightTrigger;
            public Sprite leftShoulder;
            public Sprite rightShoulder;
            public Sprite dpad;
            public Sprite dpadUp;
            public Sprite dpadDown;
            public Sprite dpadLeft;
            public Sprite dpadRight;
            public Sprite leftStick;
            public Sprite rightStick;
            public Sprite leftStickPress;
            public Sprite rightStickPress;

            public Sprite GetSprite(string controlPath)
            {
                switch (controlPath)
                {
                    case "buttonSouth": return buttonSouth;
                    case "buttonNorth": return buttonNorth;
                    case "buttonEast": return buttonEast;
                    case "buttonWest": return buttonWest;
                    case "start": return startButton;
                    case "select": return selectButton;
                    case "leftTrigger": return leftTrigger;
                    case "rightTrigger": return rightTrigger;
                    case "leftShoulder": return leftShoulder;
                    case "rightShoulder": return rightShoulder;
                    case "dpad": return dpad;
                    case "dpad/up": return dpadUp;
                    case "dpad/down": return dpadDown;
                    case "dpad/left": return dpadLeft;
                    case "dpad/right": return dpadRight;
                    case "leftStick": return leftStick;
                    case "rightStick": return rightStick;
                    case "leftStickPress": return leftStickPress;
                    case "rightStickPress": return rightStickPress;
                }
                return missingSprite;
            }
        }

        [Serializable]
        public struct KeyboardSprites
        {
            public Sprite missingSprite;
            public Sprite f1;
            public Sprite f2;
            public Sprite f3;
            public Sprite f4;
            public Sprite f5;
            public Sprite f6;
            public Sprite f7;
            public Sprite f8;
            public Sprite f9;
            public Sprite f10;
            public Sprite f11;
            public Sprite f12;
            public Sprite _0;
            public Sprite _1;
            public Sprite _2;
            public Sprite _3;
            public Sprite _4;
            public Sprite _5;
            public Sprite _6;
            public Sprite _7;
            public Sprite _8;
            public Sprite _9;
            public Sprite a;
            public Sprite b;
            public Sprite c;
            public Sprite d;
            public Sprite e;
            public Sprite f;
            public Sprite g;
            public Sprite h;
            public Sprite i;
            public Sprite j;
            public Sprite k;
            public Sprite l;
            public Sprite m;
            public Sprite n;
            public Sprite o;
            public Sprite p;
            public Sprite q;
            public Sprite r;
            public Sprite s;
            public Sprite t;
            public Sprite u;
            public Sprite v;
            public Sprite w;
            public Sprite x;
            public Sprite y;
            public Sprite z;

            public Sprite upArrow;
            public Sprite downArrow;
            public Sprite leftArrow;
            public Sprite rightArrow;
            public Sprite escape;
            public Sprite del;
            public Sprite minus;
            public Sprite backspace;
            public Sprite tab;
            public Sprite leftBracket;
            public Sprite rightBracket;
            public Sprite backslash;
            public Sprite capsLock;
            public Sprite semicolon;
            public Sprite enter;
            public Sprite shift;
            public Sprite ctrl;
            public Sprite alt;
            public Sprite space;
            public Sprite home;
            public Sprite end;
            public Sprite pageDown;
            public Sprite pageUp;
            public Sprite numLock;

            public Sprite GetSprite(string controlPath)
            {
                switch (controlPath)
                {
                    case "f1": return f1;
                    case "f2": return f2;
                    case "f3": return f3;
                    case "f4": return f4;
                    case "f5": return f5;
                    case "f6": return f6;
                    case "f7": return f7;
                    case "f8": return f8;
                    case "f9": return f9;
                    case "f10": return f10;
                    case "f11": return f11;
                    case "f12": return f12;

                    case "1": return _1;
                    case "2": return _2;
                    case "3": return _3;
                    case "4": return _4;
                    case "5": return _5;
                    case "6": return _6;
                    case "7": return _7;
                    case "8": return _8;
                    case "9": return _9;
                    case "0": return _0;

                    case "a": return a;
                    case "b": return b;
                    case "c": return c;
                    case "d": return d;
                    case "e": return e;
                    case "f": return f;
                    case "g": return g;
                    case "h": return h;
                    case "i": return i;
                    case "j": return j;
                    case "k": return k;
                    case "l": return l;
                    case "m": return m;
                    case "n": return n;
                    case "o": return o;
                    case "p": return p;
                    case "q": return q;
                    case "r": return r;
                    case "s": return s;
                    case "t": return t;
                    case "u": return u;
                    case "v": return v;
                    case "w": return w;
                    case "x": return x;
                    case "y": return y;
                    case "z": return z;

                    case "upArrow": return upArrow;
                    case "downArrow": return downArrow;
                    case "leftArrow": return leftArrow;
                    case "rightArrow": return rightArrow;
                    case "escape": return escape;
                    case "del": return del;
                    case "minus": return minus;
                    case "backspace": return backspace;
                    case "tab": return tab;
                    case "leftBracket": return leftBracket;
                    case "rightBracket": return rightBracket;
                    case "backslash": return backslash;
                    case "capsLock": return capsLock;
                    case "semicolon": return semicolon;
                    case "enter": return enter;
                    case "shift": return shift;
                    case "ctrl": return ctrl;
                    case "alt": return alt;
                    case "space": return space;
                    case "home": return home;
                    case "end": return end;
                    case "pageDown": return pageDown;
                    case "pageUp": return pageUp;
                    case "numLock": return numLock;
                }
                return missingSprite;
            }
        }
    }
}