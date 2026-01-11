using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UFramework.KeySet;

namespace UFramework
{
    public static class KeyBinding
    {
        public static void SaveKeyBinding()
        {
            //Convert key set to string pairs
            foreach (KeyValuePair<KeyName, KeyCode> kv in AllKeys)
            {
                PlayerPrefs.SetString(kv.Key.ToString(), kv.Value.ToString());
            }
            PlayerPrefs.Save();
        }
        public static void LoadKeyBindingFromPlayerPref()
        {
            var keys = AllKeys.Keys.ToArray();
            foreach (KeyName key in keys)
            {
                SetKeyBinding(key, Enum.Parse<KeyCode>(PlayerPrefs.GetString(key.ToString())));
            }
        }
        public static void SetKeyBinding(KeyName keyToChange, KeyCode newKey)
        {
            if (AllKeys.ContainsKey(keyToChange))
                AllKeys[keyToChange] = newKey;
            else
                Debug.LogError($"keyName {keyToChange} doesn't exist");
        }
    }
}