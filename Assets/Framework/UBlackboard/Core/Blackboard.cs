using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UFramework
{
    /// <summary>
    /// 黑板系统
    /// 提供跨模块的数据共享和事件通知机制
    /// </summary>
    public class Blackboard
    {
        private readonly Dictionary<string, BlackboardData> dataStorage;
        private readonly Dictionary<string, List<Action<BlackboardEventArgs>>> keyListeners;
        private readonly Dictionary<Type, List<Action<BlackboardEventArgs>>> typeListeners;
        private readonly List<Action<BlackboardEventArgs>> globalListeners;
        private readonly string name;
        private readonly object lockObject;

        /// <summary>
        /// 黑板名称
        /// </summary>
        public string Name => name;
        /// <summary>
        /// 数据数量
        /// </summary>
        public int Count => dataStorage.Count;

        /// <summary>
        /// 全局数据变更事件
        /// </summary>
        public event Action<BlackboardEventArgs> OnDataChanged;

        public Blackboard(string name = "Default")
        {
            this.name = name;
            dataStorage = new Dictionary<string, BlackboardData>();
            keyListeners = new Dictionary<string, List<Action<BlackboardEventArgs>>>();
            typeListeners = new Dictionary<Type, List<Action<BlackboardEventArgs>>>();
            globalListeners = new List<Action<BlackboardEventArgs>>();
            lockObject = new object();
        }

        #region 数据设置

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="isPersistent">是否持久化</param>
        /// <param name="validator">数据验证器</param>
        public void Set<T>(string key, T value, bool isPersistent = false, IBlackboardValidator validator = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }

            lock (lockObject)
            {
                object oldValue = null;
                BlackboardChangeType changeType;

                if (dataStorage.TryGetValue(key, out var data))
                {
                    oldValue = data.Value;
                    // 保留验证器，除非传入新的验证器
                    bool keepValidator = (validator == null);
                    data.UpdateValue(value, keepValidator);
                    if (validator != null)
                    {
                        data.Validator = validator;
                    }
                    changeType = BlackboardChangeType.Updated;
                }
                else
                {
                    data = new BlackboardData(key, value, isPersistent, validator);
                    dataStorage[key] = data;
                    changeType = BlackboardChangeType.Added;
                }

                var args = new BlackboardEventArgs(key, oldValue, value, changeType);
                NotifyListeners(args);
            }
        }
        /// <summary>
        /// 批量设置数据
        /// </summary>
        /// <param name="items">键值对集合</param>
        public void SetMany(params (string key, object value)[] items)
        {
            if (items == null || items.Length == 0)
                return;

            lock (lockObject)
            {
                foreach (var (key, value) in items)
                {
                    if (!string.IsNullOrEmpty(key))
                    {
                        Set(key, value);
                    }
                }
            }
        }

        #endregion

        #region 数据获取

        /// <summary>
        /// 获取数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>数据值</returns>
        public T Get<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            lock (lockObject)
            {
                if (dataStorage.TryGetValue(key, out var data))
                {
                    // 跳过无效数据
                    if (!data.IsValid())
                        return defaultValue;

                    return data.GetValue<T>();
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 尝试获取数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">输出值</param>
        /// <returns>是否成功获取</returns>
        public bool TryGet<T>(string key, out T value)
        {
            value = default;

            if (string.IsNullOrEmpty(key))
                return false;

            lock (lockObject)
            {
                if (dataStorage.TryGetValue(key, out var data))
                {
                    // 跳过无效数据
                    if (!data.IsValid())
                        return false;

                    if (data.IsType<T>())
                    {
                        value = data.GetValue<T>();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 获取数据（忽略验证）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>数据值</returns>
        public T GetIgnoreValidation<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(key))
                return defaultValue;

            lock (lockObject)
            {
                if (dataStorage.TryGetValue(key, out var data))
                {
                    return data.GetValue<T>();
                }
            }

            return defaultValue;
        }

        /// <summary>
        /// 尝试获取数据（忽略验证）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">输出值</param>
        /// <returns>是否成功获取</returns>
        public bool TryGetIgnoreValidation<T>(string key, out T value)
        {
            value = default;

            if (string.IsNullOrEmpty(key))
                return false;

            lock (lockObject)
            {
                if (dataStorage.TryGetValue(key, out var data))
                {
                    if (data.IsType<T>())
                    {
                        value = data.GetValue<T>();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 获取原始数据对象
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>黑板数据对象，不存在则返回null</returns>
        public BlackboardData GetData(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            lock (lockObject)
            {
                return dataStorage.TryGetValue(key, out var data) ? data : null;
            }
        }

        /// <summary>
        /// 获取有效数据（会跳过验证失败的数据）
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>有效的黑板数据对象，不存在或无效则返回null</returns>
        public BlackboardData GetValidData(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            lock (lockObject)
            {
                if (dataStorage.TryGetValue(key, out var data))
                {
                    return data.IsValid() ? data : null;
                }
            }
            return null;
        }

        #endregion

        #region 验证器管理

        /// <summary>
        /// 为指定键设置验证器
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="validator">验证器</param>
        public void SetValidator(string key, IBlackboardValidator validator)
        {
            if (string.IsNullOrEmpty(key))
                return;

            lock (lockObject)
            {
                if (dataStorage.TryGetValue(key, out var data))
                {
                    data.Validator = validator;
                }
            }
        }

        /// <summary>
        /// 移除指定键的验证器
        /// </summary>
        /// <param name="key">数据键</param>
        public void RemoveValidator(string key)
        {
            SetValidator(key, null);
        }

        /// <summary>
        /// 检查指定键的数据是否有效
        /// </summary>
        /// <param name="key">数据键</param>
        /// <returns>数据是否有效</returns>
        public bool IsValid(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            lock (lockObject)
            {
                if (dataStorage.TryGetValue(key, out var data))
                {
                    return data.IsValid();
                }
            }
            return false;
        }

        /// <summary>
        /// 获取指定键的验证错误描述
        /// </summary>
        /// <param name="key">数据键</param>
        /// <returns>错误描述，如果验证通过则返回null</returns>
        public string GetValidationError(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            lock (lockObject)
            {
                if (dataStorage.TryGetValue(key, out var data))
                {
                    return data.GetValidationError();
                }
            }
            return null;
        }

        /// <summary>
        /// 清理所有无效数据
        /// </summary>
        /// <returns>清理的数据数量</returns>
        public int CleanupInvalidData()
        {
            int count = 0;

            lock (lockObject)
            {
                var keysToRemove = dataStorage
                    .Where(kvp => !kvp.Value.IsValid())
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    dataStorage.Remove(key);
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// 获取所有无效数据的键
        /// </summary>
        /// <returns>无效数据的键集合</returns>
        public IEnumerable<string> GetInvalidKeys()
        {
            lock (lockObject)
            {
                return dataStorage
                    .Where(kvp => !kvp.Value.IsValid())
                    .Select(kvp => kvp.Key)
                    .ToList();
            }
        }

        #endregion

        #region 数据检查

        /// <summary>
        /// 检查键是否存在
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>是否存在</returns>
        public bool ContainsKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            lock (lockObject)
            {
                return dataStorage.ContainsKey(key);
            }
        }
        /// <summary>
        /// 检查是否存在指定类型的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>是否存在</returns>
        public bool ContainsType<T>()
        {
            lock (lockObject)
            {
                return dataStorage.Values.Any(d => d.IsType<T>());
            }
        }
        /// <summary>
        /// 检查指定键的数据是否为指定类型
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <returns>是否匹配</returns>
        public bool IsType<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            lock (lockObject)
            {
                return dataStorage.TryGetValue(key, out var data) && data.IsType<T>();
            }
        }

        #endregion

        #region 数据移除

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>是否成功移除</returns>
        public bool Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            lock (lockObject)
            {
                if (dataStorage.TryGetValue(key, out var data))
                {
                    object oldValue = data.Value;
                    dataStorage.Remove(key);
                    var args = new BlackboardEventArgs(key, oldValue, null, BlackboardChangeType.Removed);
                    NotifyListeners(args);
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 移除指定类型的所有数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>移除的数量</returns>
        public int RemoveType<T>()
        {
            lock (lockObject)
            {
                var keysToRemove = dataStorage
                    .Where(kvp => kvp.Value.IsType<T>())
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    Remove(key);
                }

                return keysToRemove.Count;
            }
        }
        /// <summary>
        /// 移除所有持久化标记为false的数据
        /// </summary>
        /// <returns>移除的数量</returns>
        public int RemoveNonPersistent()
        {
            lock (lockObject)
            {
                var keysToRemove = dataStorage
                    .Where(kvp => !kvp.Value.IsPersistent)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    Remove(key);
                }

                return keysToRemove.Count;
            }
        }
        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void Clear()
        {
            lock (lockObject)
            {
                var keys = dataStorage.Keys.ToList();
                foreach (var key in keys)
                {
                    Remove(key);
                }
            }
        }

        #endregion

        #region 数据查询

        /// <summary>
        /// 获取所有键
        /// </summary>
        /// <returns>键集合</returns>
        public IEnumerable<string> GetAllKeys()
        {
            lock (lockObject)
            {
                return dataStorage.Keys.ToList();
            }
        }
        /// <summary>
        /// 获取所有数据
        /// </summary>
        /// <returns>数据集合</returns>
        public IEnumerable<BlackboardData> GetAllData()
        {
            lock (lockObject)
            {
                return dataStorage.Values.ToList();
            }
        }
        /// <summary>
        /// 获取指定类型的所有数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns>数据集合</returns>
        public IEnumerable<KeyValuePair<string, T>> GetAll<T>()
        {
            lock (lockObject)
            {
                return dataStorage
                    .Where(kvp => kvp.Value.IsType<T>())
                    .Select(kvp => new KeyValuePair<string, T>(kvp.Key, kvp.Value.GetValue<T>()))
                    .ToList();
            }
        }
        /// <summary>
        /// 按前缀获取键
        /// </summary>
        /// <param name="prefix">前缀</param>
        /// <returns>键集合</returns>
        public IEnumerable<string> GetKeysByPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
                return Enumerable.Empty<string>();

            lock (lockObject)
            {
                return dataStorage.Keys.Where(k => k.StartsWith(prefix)).ToList();
            }
        }
        /// <summary>
        /// 获取持久化数据
        /// </summary>
        /// <returns>数据集合</returns>
        public IEnumerable<BlackboardData> GetPersistentData()
        {
            lock (lockObject)
            {
                return dataStorage.Values.Where(d => d.IsPersistent).ToList();
            }
        }

        #endregion

        #region 事件监听

        /// <summary>
        /// 添加全局监听器
        /// </summary>
        /// <param name="listener">监听器</param>
        public void AddListener(Action<BlackboardEventArgs> listener)
        {
            if (listener == null) return;

            lock (lockObject)
            {
                if (!globalListeners.Contains(listener))
                {
                    globalListeners.Add(listener);
                }
            }
        }
        /// <summary>
        /// 添加键级监听器
        /// </summary>
        /// <param name="key">要监听的键</param>
        /// <param name="listener">监听器</param>
        public void AddKeyListener(string key, Action<BlackboardEventArgs> listener)
        {
            if (string.IsNullOrEmpty(key) || listener == null)
                return;

            lock (lockObject)
            {
                if (!keyListeners.ContainsKey(key))
                {
                    keyListeners[key] = new List<Action<BlackboardEventArgs>>();
                }

                if (!keyListeners[key].Contains(listener))
                {
                    keyListeners[key].Add(listener);
                }
            }
        }
        /// <summary>
        /// 添加类型级监听器
        /// </summary>
        /// <typeparam name="T">要监听的类型</typeparam>
        /// <param name="listener">监听器</param>
        public void AddTypeListener<T>(Action<BlackboardEventArgs> listener)
        {
            if (listener == null) return;

            var type = typeof(T);
            lock (lockObject)
            {
                if (!typeListeners.ContainsKey(type))
                {
                    typeListeners[type] = new List<Action<BlackboardEventArgs>>();
                }

                if (!typeListeners[type].Contains(listener))
                {
                    typeListeners[type].Add(listener);
                }
            }
        }
        /// <summary>
        /// 移除全局监听器
        /// </summary>
        /// <param name="listener">监听器</param>
        public void RemoveListener(Action<BlackboardEventArgs> listener)
        {
            if (listener == null) return;

            lock (lockObject)
            {
                globalListeners.Remove(listener);
            }
        }
        /// <summary>
        /// 移除键级监听器
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="listener">监听器</param>
        public void RemoveKeyListener(string key, Action<BlackboardEventArgs> listener)
        {
            if (string.IsNullOrEmpty(key) || listener == null)
                return;

            lock (lockObject)
            {
                if (keyListeners.ContainsKey(key))
                {
                    keyListeners[key].Remove(listener);
                    if (keyListeners[key].Count == 0)
                    {
                        keyListeners.Remove(key);
                    }
                }
            }
        }
        /// <summary>
        /// 移除类型级监听器
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="listener">监听器</param>
        public void RemoveTypeListener<T>(Action<BlackboardEventArgs> listener)
        {
            if (listener == null) return;

            var type = typeof(T);
            lock (lockObject)
            {
                if (typeListeners.ContainsKey(type))
                {
                    typeListeners[type].Remove(listener);
                    if (typeListeners[type].Count == 0)
                    {
                        typeListeners.Remove(type);
                    }
                }
            }
        }
        /// <summary>
        /// 清除所有监听器
        /// </summary>
        public void ClearListeners()
        {
            lock (lockObject)
            {
                globalListeners.Clear();
                keyListeners.Clear();
                typeListeners.Clear();
            }
        }
        private void NotifyListeners(BlackboardEventArgs args)
        {
            OnDataChanged?.Invoke(args);

            foreach (var listener in globalListeners)
            {
                listener?.Invoke(args);
            }

            if (keyListeners.TryGetValue(args.Key, out var keyListenerList))
            {
                foreach (var listener in keyListenerList)
                {
                    listener?.Invoke(args);
                }
            }

            if (args.NewValue != null)
            {
                var valueType = args.NewValue.GetType();
                if (typeListeners.TryGetValue(valueType, out var typeListenerList))
                {
                    foreach (var listener in typeListenerList)
                    {
                        listener?.Invoke(args);
                    }
                }
            }
        }

        #endregion

        #region 持久化支持

        /// <summary>
        /// 保存数据到持久化存储
        /// </summary>
        public void SavePersistentData()
        {
            if (!UPersistenceManager.HasInstance)
                return;

            var persistentData = GetPersistentData();
            foreach (var data in persistentData)
            {
                string prefix = $"{name}_";
                string key = prefix + data.Key;

                if (data.Value is string strValue)
                {
                    UPersistenceManager.Instance.SetString(key, strValue);
                }
                else if (data.Value is int intValue)
                {
                    UPersistenceManager.Instance.SetInt(key, intValue);
                }
                else if (data.Value is float floatValue)
                {
                    UPersistenceManager.Instance.SetFloat(key, floatValue);
                }
                else if (data.Value is bool boolValue)
                {
                    UPersistenceManager.Instance.SetBool(key, boolValue);
                }
                else
                {
                    // 复杂类型转为JSON字符串存储
                    string json = JsonUtility.ToJson(data.Value);
                    UPersistenceManager.Instance.SetString(key, json);
                }
            }

            UPersistenceManager.Instance.Save();
        }
        /// <summary>
        /// 从持久化存储加载数据
        /// </summary>
        public void LoadPersistentData()
        {
            if (!UPersistenceManager.HasInstance)
                return;

            var persistentData = GetPersistentData();
            foreach (var data in persistentData)
            {
                string prefix = $"{name}_";
                string key = prefix + data.Key;

                if (data.Value is string)
                {
                    if (UPersistenceManager.Instance.TryGetString(key, out var value))
                    {
                        Set(data.Key, value, true);
                    }
                }
                else if (data.Value is int)
                {
                    if (UPersistenceManager.Instance.TryGetInt(key, out var value))
                    {
                        Set(data.Key, value, true);
                    }
                }
                else if (data.Value is float)
                {
                    if (UPersistenceManager.Instance.TryGetFloat(key, out var value))
                    {
                        Set(data.Key, value, true);
                    }
                }
                else if (data.Value is bool)
                {
                    if (UPersistenceManager.Instance.TryGetBool(key, out var value))
                    {
                        Set(data.Key, value, true);
                    }
                }
                else if (data.Value != null)
                {
                    // 尝试加载复杂类型
                    if (UPersistenceManager.Instance.TryGetString(key, out var json))
                    {
                        try
                        {
                            var loadedValue = JsonUtility.FromJson(json, data.Type);
                            Set(data.Key, loadedValue, true);
                        }
                        catch
                        {
                            // 加载失败，跳过
                        }
                    }
                }
            }
        }

        #endregion

        public override string ToString()
        {
            return $"Blackboard '{name}' ({Count} items)";
        }
    }
}
