using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UFramework.KeyName;
using static UnityEngine.KeyCode;

namespace UFramework
{
    public static class KeySet
    {
        private static Dictionary<KeyName, KeyCode> set;
        /// <summary>
        /// A dictionary storing all key binding
        /// </summary>
        public static Dictionary<KeyName, KeyCode> AllKeys
        {
            get
            {
                if (set == null)
                    InitializeKeySet();
                return set;
            }
        }
        public static void InitializeKeySet()
        {
            set = new()
            {
                //Initial keys
                {up, W } ,
                {down, S },
                {left, A },
                {right, D },
                {interact, E },
            };
        }
    }
    public enum KeyName
    {
        up,
        down,
        left,
        right,
        interact,
    }
}