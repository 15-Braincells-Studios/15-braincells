using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Project.Scripts.Core
{
    // this code keeps track of which input device the player used last
    public static class InputPrompts
    {
        public static InputDevice LastDevice { get; private set; }
        public static event Action OnDeviceChanged;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        
        private static void Init()
        {
            // start with the keyboard, then listen for any button press on any device
            LastDevice = Keyboard.current;
            InputSystem.onAnyButtonPress.Call(OnAnyButton);
        }

        // called on every button press
        private static void OnAnyButton(InputControl control)
        {
            var device = control.device;
            
            if (device is Mouse) device = Keyboard.current;
            
            if (device == null || device == LastDevice) return;

            LastDevice = device;
            OnDeviceChanged?.Invoke();
        }

        // Returns the button name for an action on the last used device "E" on keyboard, "Square" on PlayStation, "X" on Xbox
        public static string GetDisplayName(string actionName)
        {
            var action = InputSystem.actions.FindAction(actionName);
            if (action == null) return actionName;

            // find the interact control of the last used device
            foreach (var control in action.controls)
                if (control.device == LastDevice)
                    return control.displayName;

            // no match for that device: fall back to the action's default binding name
            return action.GetBindingDisplayString();
        }
    }
}