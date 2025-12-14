using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UFramework.KeySet;

namespace UFramework
{
    public static class InputManager
    {
        private static readonly Dictionary<KeyName, bool> allowInput;
        static InputManager()
        {
            allowInput = new Dictionary<KeyName, bool>();
            foreach (var key in AllKeys)
            {
                allowInput.Add(key.Key, true);
            }
        }
        /// <summary>
        /// Get the key binding to the given operation
        /// </summary>
        public static KeyCode GetKeyCode(KeyName operation)
        {
            if (AllKeys.ContainsKey(operation))
                return AllKeys[operation];
            else
                Debug.LogError($"Key name {operation} doesn't exist.");
            return KeyCode.None;
        }
        /// <summary>
        /// Returns true while the user holds down the key identified by the key KeyName enum parameter.
        /// </summary>
        public static bool GetKey(KeyName operation)
        {
            if (!allowInput[operation])
                return false;
            if (AllKeys.ContainsKey(operation))
                return Input.GetKey(AllKeys[operation]);
            else
                Debug.LogError($"Key name {operation} doesn't exist.");
            return false;
        }
        /// <summary>
        /// Returns true during the frame the user releases the key identified by the key KeyName enum parameter.
        /// </summary>
        public static bool GetKeyUp(KeyName operation)
        {
            if (!allowInput[operation])
                return false;
            if (AllKeys.ContainsKey(operation))
                return Input.GetKeyUp(AllKeys[operation]);
            else
                Debug.LogError($"Key name {operation} doesn't exist.");
            return false;
        }
        /// <summary>
        /// Returns true during the frame the user starts pressing down the key identified by the key KeyName enum parameter.
        /// </summary>
        public static bool GetKeyDown(KeyName operation)
        {
            if (!allowInput[operation])
                return false;
            if (AllKeys.ContainsKey(operation))
                return Input.GetKeyDown(AllKeys[operation]);
            else
                Debug.LogError($"Key name {operation} doesn't exist.");
            return false;
        }
        public static void DisableKeyInput(KeyName operation)
        {
            allowInput[operation] = false;
        }
        public static void DisableKeyInputs(List<KeyName> operations)
        {
            foreach (KeyName operation in operations)
            {
                DisableKeyInput(operation);
            }
        }
        public static void EnableKeyInput(KeyName operation)
        {
            allowInput[operation] = true;
        }
        public static void EnableKeyInputs(List<KeyName> operations)
        {
            foreach (KeyName operation in operations)
            {
                EnableKeyInput(operation);
            }
        }
    }
}