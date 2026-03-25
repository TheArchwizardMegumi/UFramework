using UnityEngine;

namespace UFramework
{
    /// <summary>
    /// PlayerPrefs持久化提供者
    /// 使用Unity内置的PlayerPrefs作为默认的持久化方案
    /// </summary>
    public class PlayerPrefsPersistenceProvider : IPersistenceProvider
    {
        public string ProviderName => "PlayerPrefs";
        private readonly bool autoSave = true;

        public PlayerPrefsPersistenceProvider(bool autoSave = true)
        {
            this.autoSave = autoSave;
        }

        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
            if (autoSave) Save();
        }
        public string GetString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }
        public void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            if (autoSave) Save();
        }
        public float GetFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }
        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
            if (autoSave) Save();
        }
        public int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }
        public void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value ? 1 : 0);
            if (autoSave) Save();
        }
        public bool GetBool(string key, bool defaultValue = false)
        {
            return PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;
        }
        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }
        public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
            if (autoSave) Save();
        }
        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
            if (autoSave) Save();
        }
        public void Save()
        {
            PlayerPrefs.Save();
        }
        public void Load()
        {
            // PlayerPrefs的数据会在应用启动时自动加载，此处无需额外操作
        }
    }
}
