using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UFramework.KeySet;

namespace UFramework
{
    public static class KeyBinding
    {
        private static void SaveKeyBinding()
        {
            // 保存按键绑定到 PlayerPrefs
            foreach (KeyValuePair<OperationName, KeyCode> kv in AllKeys)
            {
                PlayerPrefs.SetString($"Key_{kv.Key}", kv.Value.ToString());
            }

            // 保存轴绑定到 PlayerPrefs
            foreach (KeyValuePair<AxisOperationName, string> kv in AllAxisBindings)
            {
                PlayerPrefs.SetString($"Axis_{kv.Key}", kv.Value);
            }

            // 保存轴到按键映射到 PlayerPrefs
            for (int i = 0; i < AxisToKeyMappings.Count; i++)
            {
                var mapping = AxisToKeyMappings[i];
                PlayerPrefs.SetString($"AxisMapping_{i}_axisName", mapping.axisName.ToString());
                PlayerPrefs.SetString($"AxisMapping_{i}_positiveKey", mapping.positiveKey.ToString());
                PlayerPrefs.SetString($"AxisMapping_{i}_negativeKey", mapping.negativeKey.ToString());
                PlayerPrefs.SetFloat($"AxisMapping_{i}_threshold", mapping.threshold);
                PlayerPrefs.SetFloat($"AxisMapping_{i}_resetDelay", mapping.resetDelay);
            }
            PlayerPrefs.SetInt("AxisMapping_Count", AxisToKeyMappings.Count);

            PlayerPrefs.Save();
        }
        public static void LoadKeyBindingFromPlayerPref()
        {
            // 从 PlayerPrefs 加载按键绑定
            foreach (OperationName operation in System.Enum.GetValues(typeof(OperationName)))
            {
                string value = PlayerPrefs.GetString($"Key_{operation}");
                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        KeyCode newKey = Enum.Parse<KeyCode>(value);
                        // 只设置存在的键，避免错误
                        if (AllKeys.ContainsKey(operation))
                        {
                            AllKeys[operation] = newKey;
                        }
                        else
                        {
                            Debug.LogWarning($"跳过不存在的操作 {operation}");
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"加载按键绑定失败 {operation}: {e.Message}");
                    }
                }
            }

            // 从 PlayerPrefs 加载轴绑定
            foreach (AxisOperationName axisName in System.Enum.GetValues(typeof(AxisOperationName)))
            {
                string value = PlayerPrefs.GetString($"Axis_{axisName}");
                if (!string.IsNullOrEmpty(value))
                {
                    // 只设置存在的轴，避免错误
                    if (AllAxisBindings.ContainsKey(axisName))
                    {
                        AllAxisBindings[axisName] = value;
                    }
                    else
                    {
                        Debug.LogWarning($"跳过不存在的轴 {axisName}");
                    }
                }
            }

            // 从 PlayerPrefs 加载轴到按键映射
            int mappingCount = PlayerPrefs.GetInt("AxisMapping_Count", 0);
            if (mappingCount > 0)
            {
                AxisToKeyMappings.Clear();
                for (int i = 0; i < mappingCount; i++)
                {
                    try
                    {
                        var mapping = new AxisToKeyMapping
                        {
                            axisName = Enum.Parse<AxisOperationName>(PlayerPrefs.GetString($"AxisMapping_{i}_axisName")),
                            positiveKey = Enum.Parse<OperationName>(PlayerPrefs.GetString($"AxisMapping_{i}_positiveKey")),
                            negativeKey = Enum.Parse<OperationName>(PlayerPrefs.GetString($"AxisMapping_{i}_negativeKey")),
                            threshold = PlayerPrefs.GetFloat($"AxisMapping_{i}_threshold", 0.1f),
                            resetDelay = PlayerPrefs.GetFloat($"AxisMapping_{i}_resetDelay", 0.1f)
                        };
                        AxisToKeyMappings.Add(mapping);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"加载轴映射失败 {i}: {e.Message}");
                    }
                }
            }
        }
        public static void SetKeyBinding(OperationName operationToChange, KeyCode newKey)
        {
            if (AllKeys.ContainsKey(operationToChange))
                AllKeys[operationToChange] = newKey;
            else
                Debug.LogError($"按键名称 {operationToChange} 不存在");
            SaveKeyBinding();
        }
        /// <summary>
        /// 设置指定轴操作对应的 Unity 轴名称
        /// </summary>
        /// <param name="axisOperationToChange">要更改的轴名称</param>
        /// <param name="unityAxisName">Unity 轴名称（如 "Mouse ScrollWheel"、"Horizontal" 等）</param>
        public static void SetAxisBinding(AxisOperationName axisOperationToChange, string unityAxisName)
        {
            if (AllAxisBindings.ContainsKey(axisOperationToChange))
                AllAxisBindings[axisOperationToChange] = unityAxisName;
            else
                Debug.LogError($"轴名称 {axisOperationToChange} 不存在");
            SaveKeyBinding();
        }
        /// <summary>
        /// 动态添加新的轴到按键映射
        /// </summary>
        /// <param name="axisOperationToBind">要监听的轴</param>
        /// <param name="positiveKey">正向触发的按键</param>
        /// <param name="negativeKey">负向触发的按键</param>
        /// <param name="threshold">触发阈值</param>
        /// <param name="resetDelay">重置延迟（秒），决定连续同向输入时有效输入的最小延迟</param>
        public static void SetAxisToKeyMapping(AxisOperationName axisOperationToBind, OperationName positiveKey, OperationName negativeKey, float threshold = 0.1f, float resetDelay = 0.1f)
        {
            var mapping = new AxisToKeyMapping
            {
                axisName = axisOperationToBind,
                positiveKey = positiveKey,
                negativeKey = negativeKey,
                threshold = threshold,
                resetDelay = resetDelay
            };
            RemoveAxisToKeyMapping(axisOperationToBind);
            AxisToKeyMappings.Add(mapping);
        }
        /// <summary>
        /// 根据轴名称移除轴到按键映射
        /// </summary>
        /// <param name="operationName">要移除的轴名称</param>
        public static void RemoveAxisToKeyMapping(AxisOperationName operationName)
        {
            AxisToKeyMappings.RemoveAll(m => m.axisName == operationName);
        }
        /// <summary>
        /// 重置所有绑定为默认值
        /// </summary>
        /// <remarks>
        /// 此方法会重新初始化按键绑定、轴绑定和轴到按键映射，
        /// 并清除所有已保存的 PlayerPrefs 数据
        /// </remarks>
        public static void ResetToDefaults()
        {
            InitializeKeyBindings();
            InitializeAxisBindings();
            InitializeAxisToKeyMappings();
        }
        
    }
}
