using UnityEngine;

namespace UFramework
{
    /// <summary>
    /// 数据持久化管理器
    /// 提供统一的持久化数据访问接口，支持多种持久化提供者
    /// </summary>
    public class UPersistenceManager : Singleton<UPersistenceManager>
    {
        [Header("设置")]
        [SerializeField]
        private bool enableDebugLog = false;

        private IPersistenceProvider provider;
        /// <summary>
        /// 获取当前使用的持久化提供者
        /// </summary>
        public IPersistenceProvider Provider => provider;

        protected override void OnInit()
        {
            // 默认使用PlayerPrefs作为持久化提供者
            SetProvider(new PlayerPrefsPersistenceProvider(autoSave: false));

            if (enableDebugLog)
            {
                Debug.Log($"[UPersistence] 初始化完成，使用提供者: {provider.ProviderName}");
            }
        }

        /// <summary>
        /// 设置持久化提供者
        /// </summary>
        /// <param name="persistenceProvider">持久化提供者实例</param>
        public void SetProvider(IPersistenceProvider persistenceProvider)
        {
            if (persistenceProvider == null)
            {
                Debug.LogError("[UPersistence] 不能设置空的持久化提供者");
                return;
            }

            provider = persistenceProvider;

            if (enableDebugLog)
            {
                Debug.Log($"[UPersistence] 持久化提供者已切换为: {provider.ProviderName}");
            }
        }

        #region 字符串操作

        /// <summary>
        /// 保存字符串数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="value">字符串值</param>
        public void SetString(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[UPersistence] 键不能为空");
                return;
            }

            provider.SetString(key, value);

            if (enableDebugLog)
            {
                Debug.Log($"[UPersistence] 保存字符串 [{key}] = {value}");
            }
        }
        /// <summary>
        /// 获取字符串数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>字符串值</returns>
        public string GetString(string key, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[UPersistence] 键不能为空");
                return defaultValue;
            }

            return provider.GetString(key, defaultValue);
        }
        /// <summary>
        /// 尝试获取字符串数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="value">输出的值</param>
        /// <returns>是否成功获取</returns>
        public bool TryGetString(string key, out string value)
        {
            value = string.Empty;

            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!provider.HasKey(key))
            {
                return false;
            }

            value = provider.GetString(key);
            return true;
        }

        #endregion

        #region 浮点数操作

        /// <summary>
        /// 保存浮点数数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="value">浮点数值</param>
        public void SetFloat(string key, float value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[UPersistence] 键不能为空");
                return;
            }

            provider.SetFloat(key, value);

            if (enableDebugLog)
            {
                Debug.Log($"[UPersistence] 保存浮点数 [{key}] = {value}");
            }
        }
        /// <summary>
        /// 获取浮点数数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>浮点数值</returns>
        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[UPersistence] 键不能为空");
                return defaultValue;
            }

            return provider.GetFloat(key, defaultValue);
        }
        /// <summary>
        /// 尝试获取浮点数数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="value">输出的值</param>
        /// <returns>是否成功获取</returns>
        public bool TryGetFloat(string key, out float value)
        {
            value = 0f;

            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!provider.HasKey(key))
            {
                return false;
            }

            value = provider.GetFloat(key);
            return true;
        }

        #endregion

        #region 整数操作

        /// <summary>
        /// 保存整数数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="value">整数值</param>
        public void SetInt(string key, int value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[UPersistence] 键不能为空");
                return;
            }

            provider.SetInt(key, value);

            if (enableDebugLog)
            {
                Debug.Log($"[UPersistence] 保存整数 [{key}] = {value}");
            }
        }
        /// <summary>
        /// 获取整数数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>整数值</returns>
        public int GetInt(string key, int defaultValue = 0)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[UPersistence] 键不能为空");
                return defaultValue;
            }

            return provider.GetInt(key, defaultValue);
        }
        /// <summary>
        /// 尝试获取整数数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="value">输出的值</param>
        /// <returns>是否成功获取</returns>
        public bool TryGetInt(string key, out int value)
        {
            value = 0;

            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!provider.HasKey(key))
            {
                return false;
            }

            value = provider.GetInt(key);
            return true;
        }

        #endregion

        #region 布尔值操作

        /// <summary>
        /// 保存布尔值数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="value">布尔值</param>
        public void SetBool(string key, bool value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[UPersistence] 键不能为空");
                return;
            }

            provider.SetBool(key, value);

            if (enableDebugLog)
            {
                Debug.Log($"[UPersistence] 保存布尔值 [{key}] = {value}");
            }
        }
        /// <summary>
        /// 获取布尔值数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>布尔值</returns>
        public bool GetBool(string key, bool defaultValue = false)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[UPersistence] 键不能为空");
                return defaultValue;
            }

            return provider.GetBool(key, defaultValue);
        }
        /// <summary>
        /// 尝试获取布尔值数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="value">输出的值</param>
        /// <returns>是否成功获取</returns>
        public bool TryGetBool(string key, out bool value)
        {
            value = false;

            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            if (!provider.HasKey(key))
            {
                return false;
            }

            value = provider.GetBool(key);
            return true;
        }

        #endregion

        #region 通用操作

        /// <summary>
        /// 检查键是否存在
        /// </summary>
        /// <param name="key">数据键</param>
        /// <returns>是否存在</returns>
        public bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            return provider.HasKey(key);
        }
        /// <summary>
        /// 删除指定键的数据
        /// </summary>
        /// <param name="key">数据键</param>
        public void DeleteKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[UPersistence] 键不能为空");
                return;
            }

            provider.DeleteKey(key);

            if (enableDebugLog)
            {
                Debug.Log($"[UPersistence] 删除键 [{key}]");
            }
        }
        /// <summary>
        /// 删除所有数据
        /// </summary>
        public void DeleteAll()
        {
            provider.DeleteAll();

            if (enableDebugLog)
            {
                Debug.Log("[UPersistence] 删除所有数据");
            }
        }

        /// <summary>
        /// 保存所有数据到持久化存储
        /// </summary>
        public void Save()
        {
            provider.Save();

            if (enableDebugLog)
            {
                Debug.Log("[UPersistence] 保存数据到存储");
            }
        }
        /// <summary>
        /// 从持久化存储重新加载数据
        /// </summary>
        public void Load()
        {
            provider.Load();

            if (enableDebugLog)
            {
                Debug.Log("[UPersistence] 从存储加载数据");
            }
        }

        #endregion

        #region 批量操作

        /// <summary>
        /// 批量保存数据（最后统一保存）
        /// </summary>
        /// <param name="actions">保存操作委托</param>
        public void BatchSave(System.Action actions)
        {
            if (actions == null)
            {
                Debug.LogError("[UPersistence] 批量操作委托不能为空");
                return;
            }

            actions();
            Save();

            if (enableDebugLog)
            {
                Debug.Log("[UPersistence] 批量保存完成");
            }
        }
        /// <summary>
        /// 批量删除数据
        /// </summary>
        /// <param name="keys">要删除的键数组</param>
        public void DeleteKeys(params string[] keys)
        {
            if (keys == null || keys.Length == 0)
            {
                return;
            }

            foreach (string key in keys)
            {
                if (!string.IsNullOrEmpty(key))
                {
                    provider.DeleteKey(key);
                }
            }

            Save();

            if (enableDebugLog)
            {
                Debug.Log($"[UPersistence] 批量删除 {keys.Length} 个键");
            }
        }

        #endregion
    }
}
