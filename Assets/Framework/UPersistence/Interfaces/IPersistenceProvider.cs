using System;

namespace UFramework
{
    /// <summary>
    /// 数据持久化提供者接口
    /// 定义数据持久化的基本操作，便于扩展不同的持久化工具
    /// </summary>
    public interface IPersistenceProvider
    {
        /// <summary>
        /// 保存字符串数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="value">字符串值</param>
        void SetString(string key, string value);
        /// <summary>
        /// 获取字符串数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>字符串值</returns>
        string GetString(string key, string defaultValue = "");
        /// <summary>
        /// 保存浮点数数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="value">浮点数值</param>
        void SetFloat(string key, float value);
        /// <summary>
        /// 获取浮点数数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>浮点数值</returns>
        float GetFloat(string key, float defaultValue = 0f);
        /// <summary>
        /// 保存整数数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="value">整数值</param>
        void SetInt(string key, int value);
        /// <summary>
        /// 获取整数数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>整数值</returns>
        int GetInt(string key, int defaultValue = 0);
        /// <summary>
        /// 保存布尔值数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="value">布尔值</param>
        void SetBool(string key, bool value);
        /// <summary>
        /// 获取布尔值数据
        /// </summary>
        /// <param name="key">数据键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>布尔值</returns>
        bool GetBool(string key, bool defaultValue = false);
        /// <summary>
        /// 检查键是否存在
        /// </summary>
        /// <param name="key">数据键</param>
        /// <returns>是否存在</returns>
        bool HasKey(string key);
        /// <summary>
        /// 删除指定键的数据
        /// </summary>
        /// <param name="key">数据键</param>
        void DeleteKey(string key);
        /// <summary>
        /// 删除所有数据
        /// </summary>
        void DeleteAll();
        /// <summary>
        /// 保存所有数据到持久化存储
        /// </summary>
        void Save();
        /// <summary>
        /// 从持久化存储重新加载数据
        /// </summary>
        void Load();
        /// <summary>
        /// 获取提供者名称
        /// </summary>
        string ProviderName { get; }
    }
}
