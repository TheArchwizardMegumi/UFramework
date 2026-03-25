using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFramework
{
    /// <summary>
    /// 黑板管理器
    /// 管理多个黑板实例，提供全局访问接口
    /// </summary>
    public class UBlackboardManager : Singleton<UBlackboardManager>
    {
        [Header("设置")]
        [SerializeField]
        private bool enableDebugLog = false;

        [SerializeField]
        private bool autoSavePersistent = false;

        [SerializeField]
        private float autoSaveInterval = 60f;

        private readonly Dictionary<string, Blackboard> blackboards = new();
        private Blackboard defaultBlackboard;
        private float autoSaveTimer;

        /// <summary>
        /// 获取默认黑板
        /// </summary>
        public Blackboard Default => defaultBlackboard;

        protected override void OnInit()
        {
            defaultBlackboard = new Blackboard("Default");
            blackboards["Default"] = defaultBlackboard;

            if (enableDebugLog)
            {
                Debug.Log("[UBlackboard] 初始化完成");
            }
        }

        private void Update()
        {
            if (autoSavePersistent)
            {
                autoSaveTimer += Time.deltaTime;
                if (autoSaveTimer >= autoSaveInterval)
                {
                    SaveAllPersistent();
                    autoSaveTimer = 0f;
                }
            }
        }

        #region 黑板管理

        /// <summary>
        /// 获取或创建黑板
        /// </summary>
        /// <param name="name">黑板名称</param>
        /// <returns>黑板实例</returns>
        public Blackboard GetBlackboard(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("[UBlackboard] 黑板名称不能为空，返回默认黑板");
                return defaultBlackboard;
            }

            if (!blackboards.TryGetValue(name, out var blackboard))
            {
                blackboard = new Blackboard(name);
                blackboards[name] = blackboard;

                if (enableDebugLog)
                {
                    Debug.Log($"[UBlackboard] 创建新黑板: {name}");
                }
            }

            return blackboard;
        }

        /// <summary>
        /// 检查黑板是否存在
        /// </summary>
        /// <param name="name">黑板名称</param>
        /// <returns>是否存在</returns>
        public bool HasBlackboard(string name)
        {
            return !string.IsNullOrEmpty(name) && blackboards.ContainsKey(name);
        }

        /// <summary>
        /// 移除黑板
        /// </summary>
        /// <param name="name">黑板名称</param>
        /// <returns>是否成功移除</returns>
        public bool RemoveBlackboard(string name)
        {
            if (string.IsNullOrEmpty(name) || name == "Default")
            {
                Debug.LogWarning("[UBlackboard] 不能移除默认黑板");
                return false;
            }

            if (blackboards.TryGetValue(name, out var blackboard))
            {
                blackboard.Clear();
                blackboard.ClearListeners();
                blackboards.Remove(name);

                if (enableDebugLog)
                {
                    Debug.Log($"[UBlackboard] 移除黑板: {name}");
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// 获取所有黑板名称
        /// </summary>
        /// <returns>黑板名称集合</returns>
        public string[] GetAllBlackboardNames()
        {
            var names = new string[blackboards.Count];
            blackboards.Keys.CopyTo(names, 0);
            return names;
        }

        /// <summary>
        /// 清空所有黑板
        /// </summary>
        public void ClearAll()
        {
            foreach (var blackboard in blackboards.Values)
            {
                if (blackboard != defaultBlackboard)
                {
                    blackboard.Clear();
                    blackboard.ClearListeners();
                }
                else
                {
                    blackboard.RemoveNonPersistent();
                }
            }

            if (enableDebugLog)
            {
                Debug.Log("[UBlackboard] 清空所有黑板");
            }
        }

        #endregion

        #region 便捷方法 - 默认黑板

        /// <summary>
        /// 设置数据（默认黑板）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="isPersistent">是否持久化</param>
        /// <param name="validator">验证器</param>
        public void Set<T>(string key, T value, bool isPersistent = false, IBlackboardValidator validator = null)
        {
            defaultBlackboard.Set(key, value, isPersistent, validator);
        }

        /// <summary>
        /// 获取数据（默认黑板）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>数据值</returns>
        public T Get<T>(string key, T defaultValue = default)
        {
            return defaultBlackboard.Get(key, defaultValue);
        }

        /// <summary>
        /// 尝试获取数据（默认黑板）
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">键</param>
        /// <param name="value">输出值</param>
        /// <returns>是否成功获取</returns>
        public bool TryGet<T>(string key, out T value)
        {
            return defaultBlackboard.TryGet(key, out value);
        }

        /// <summary>
        /// 检查键是否存在（默认黑板）
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>是否存在</returns>
        public bool ContainsKey(string key)
        {
            return defaultBlackboard.ContainsKey(key);
        }

        /// <summary>
        /// 检查数据是否有效（默认黑板）
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>是否有效</returns>
        public bool IsValid(string key)
        {
            return defaultBlackboard.IsValid(key);
        }

        /// <summary>
        /// 移除数据（默认黑板）
        /// </summary>
        /// <param name="key">键</param>
        /// <returns>是否成功移除</returns>
        public bool Remove(string key)
        {
            return defaultBlackboard.Remove(key);
        }

        /// <summary>
        /// 设置验证器（默认黑板）
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="validator">验证器</param>
        public void SetValidator(string key, IBlackboardValidator validator)
        {
            defaultBlackboard.SetValidator(key, validator);
        }

        /// <summary>
        /// 移除验证器（默认黑板）
        /// </summary>
        /// <param name="key">键</param>
        public void RemoveValidator(string key)
        {
            defaultBlackboard.RemoveValidator(key);
        }

        /// <summary>
        /// 清理无效数据（默认黑板）
        /// </summary>
        /// <returns>清理的数据数量</returns>
        public int CleanupInvalidData()
        {
            return defaultBlackboard.CleanupInvalidData();
        }

        /// <summary>
        /// 添加全局监听器（默认黑板）
        /// </summary>
        /// <param name="listener">监听器</param>
        public void AddListener(Action<BlackboardEventArgs> listener)
        {
            defaultBlackboard.AddListener(listener);
        }

        /// <summary>
        /// 添加键级监听器（默认黑板）
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="listener">监听器</param>
        public void AddKeyListener(string key, Action<BlackboardEventArgs> listener)
        {
            defaultBlackboard.AddKeyListener(key, listener);
        }

        /// <summary>
        /// 添加类型级监听器（默认黑板）
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="listener">监听器</param>
        public void AddTypeListener<T>(Action<BlackboardEventArgs> listener)
        {
            defaultBlackboard.AddTypeListener<T>(listener);
        }

        /// <summary>
        /// 移除全局监听器（默认黑板）
        /// </summary>
        /// <param name="listener">监听器</param>
        public void RemoveListener(Action<BlackboardEventArgs> listener)
        {
            defaultBlackboard.RemoveListener(listener);
        }

        #endregion

        #region 持久化操作

        /// <summary>
        /// 保存所有黑板的持久化数据
        /// </summary>
        public void SaveAllPersistent()
        {
            foreach (var blackboard in blackboards.Values)
            {
                blackboard.SavePersistentData();
            }

            if (enableDebugLog)
            {
                Debug.Log("[UBlackboard] 保存所有持久化数据");
            }
        }

        /// <summary>
        /// 加载所有黑板的持久化数据
        /// </summary>
        public void LoadAllPersistent()
        {
            foreach (var blackboard in blackboards.Values)
            {
                blackboard.LoadPersistentData();
            }

            if (enableDebugLog)
            {
                Debug.Log("[UBlackboard] 加载所有持久化数据");
            }
        }

        /// <summary>
        /// 清空所有黑板中的非持久化数据
        /// </summary>
        public void ClearAllNonPersistent()
        {
            foreach (var blackboard in blackboards.Values)
            {
                blackboard.RemoveNonPersistent();
            }

            if (enableDebugLog)
            {
                Debug.Log("[UBlackboard] 清空所有非持久化数据");
            }
        }

        /// <summary>
        /// 清理所有黑板中的无效数据
        /// </summary>
        /// <returns>清理的数据总数</returns>
        public int CleanupAllInvalidData()
        {
            int total = 0;
            foreach (var blackboard in blackboards.Values)
            {
                total += blackboard.CleanupInvalidData();
            }

            if (enableDebugLog)
            {
                Debug.Log($"[UBlackboard] 清理了 {total} 条无效数据");
            }

            return total;
        }

        /// <summary>
        /// 启用自动保存
        /// </summary>
        /// <param name="interval">保存间隔（秒）</param>
        public void EnableAutoSave(float interval = 60f)
        {
            autoSavePersistent = true;
            autoSaveInterval = interval;
            autoSaveTimer = 0f;

            if (enableDebugLog)
            {
                Debug.Log($"[UBlackboard] 启用自动保存，间隔: {interval}秒");
            }
        }

        /// <summary>
        /// 禁用自动保存
        /// </summary>
        public void DisableAutoSave()
        {
            autoSavePersistent = false;

            if (enableDebugLog)
            {
                Debug.Log("[UBlackboard] 禁用自动保存");
            }
        }

        #endregion

        #region 调试

        /// <summary>
        /// 打印所有黑板信息
        /// </summary>
        public void PrintAllInfo()
        {
            Debug.Log("=== UBlackboard Info ===");

            foreach (var kvp in blackboards)
            {
                var blackboard = kvp.Value;
                Debug.Log($"Blackboard: {blackboard.Name}, Items: {blackboard.Count}");

                foreach (var data in blackboard.GetAllData())
                {
                    Debug.Log($"  [{data.Key}] = {data.Value} ({data.Type?.Name}) [Persistent: {data.IsPersistent}]");
                }
            }

            Debug.Log("=== End ===");
        }

        /// <summary>
        /// 打印指定黑板信息
        /// </summary>
        /// <param name="name">黑板名称</param>
        public void PrintBlackboardInfo(string name)
        {
            if (!blackboards.TryGetValue(name, out var blackboard))
            {
                Debug.LogWarning($"[UBlackboard] 黑板不存在: {name}");
                return;
            }

            Debug.Log($"=== Blackboard: {name} ===");
            Debug.Log($"Total Items: {blackboard.Count}");

            foreach (var data in blackboard.GetAllData())
            {
                Debug.Log($"  [{data.Key}] = {data.Value} ({data.Type?.Name}) [Persistent: {data.IsPersistent}]");
            }

            Debug.Log("=== End ===");
        }

        #endregion

        #region 应用生命周期

        protected override void OnApplicationQuit()
        {
            // 应用退出时自动保存持久化数据
            if (autoSavePersistent)
            {
                SaveAllPersistent();
            }
        }

        #endregion
    }
}
