using UnityEngine;

namespace UFramework
{
    /// <summary>
    /// 黑板扩展方法
    /// 提供便捷的数据访问方法
    /// </summary>
    public static class BlackboardExtensions
    {
        #region 常用类型快捷方法

        /// <summary>
        /// 获取 Vector3 数据
        /// </summary>
        public static Vector3 GetVector3(this Blackboard blackboard, string key, Vector3 defaultValue = default)
        {
            if (blackboard.TryGet(key, out string json))
            {
                return JsonUtility.FromJson<Vector3>(json);
            }
            return defaultValue;
        }
        /// <summary>
        /// 设置 Vector3 数据
        /// </summary>
        public static void SetVector3(this Blackboard blackboard, string key, Vector3 value, bool isPersistent = false)
        {
            blackboard.Set(key, JsonUtility.ToJson(value), isPersistent);
        }
        /// <summary>
        /// 获取 Vector2 数据
        /// </summary>
        public static Vector2 GetVector2(this Blackboard blackboard, string key, Vector2 defaultValue = default)
        {
            if (blackboard.TryGet(key, out string json))
            {
                return JsonUtility.FromJson<Vector2>(json);
            }
            return defaultValue;
        }
        /// <summary>
        /// 设置 Vector2 数据
        /// </summary>
        public static void SetVector2(this Blackboard blackboard, string key, Vector2 value, bool isPersistent = false)
        {
            blackboard.Set(key, JsonUtility.ToJson(value), isPersistent);
        }
        /// <summary>
        /// 获取 Color 数据
        /// </summary>
        public static Color GetColor(this Blackboard blackboard, string key, Color defaultValue = default)
        {
            if (blackboard.TryGet(key, out string json))
            {
                return JsonUtility.FromJson<Color>(json);
            }
            return defaultValue;
        }
        /// <summary>
        /// 设置 Color 数据
        /// </summary>
        public static void SetColor(this Blackboard blackboard, string key, Color value, bool isPersistent = false)
        {
            blackboard.Set(key, JsonUtility.ToJson(value), isPersistent);
        }
        /// <summary>
        /// 获取 Quaternion 数据
        /// </summary>
        public static Quaternion GetQuaternion(this Blackboard blackboard, string key, Quaternion defaultValue = default)
        {
            if (blackboard.TryGet(key, out string json))
            {
                return JsonUtility.FromJson<Quaternion>(json);
            }
            return defaultValue;
        }
        /// <summary>
        /// 设置 Quaternion 数据
        /// </summary>
        public static void SetQuaternion(this Blackboard blackboard, string key, Quaternion value, bool isPersistent = false)
        {
            blackboard.Set(key, JsonUtility.ToJson(value), isPersistent);
        }
        /// <summary>
        /// 获取数组数据
        /// </summary>
        public static T[] GetArray<T>(this Blackboard blackboard, string key, T[] defaultValue = null)
        {
            if (blackboard.TryGet(key, out string json))
            {
                var wrapper = JsonUtility.FromJson<ArrayWrapper<T>>(json);
                return wrapper?.items ?? defaultValue;
            }
            return defaultValue;
        }
        /// <summary>
        /// 设置数组数据
        /// </summary>
        public static void SetArray<T>(this Blackboard blackboard, string key, T[] value, bool isPersistent = false)
        {
            var wrapper = new ArrayWrapper<T> { items = value };
            blackboard.Set(key, JsonUtility.ToJson(wrapper), isPersistent);
        }
        /// <summary>
        /// 获取列表数据
        /// </summary>
        public static System.Collections.Generic.List<T> GetList<T>(this Blackboard blackboard, string key)
        {
            var array = blackboard.GetArray<T>(key);
            return array != null ? new System.Collections.Generic.List<T>(array) : null;
        }
        /// <summary>
        /// 设置列表数据
        /// </summary>
        public static void SetList<T>(this Blackboard blackboard, string key, System.Collections.Generic.List<T> value, bool isPersistent = false)
        {
            blackboard.SetArray(key, value?.ToArray(), isPersistent);
        }

        #endregion

        #region 数学操作

        /// <summary>
        /// 增加数值
        /// </summary>
        public static void Add(this Blackboard blackboard, string key, float delta)
        {
            float current = blackboard.Get(key, 0f);
            blackboard.Set(key, current + delta);
        }
        /// <summary>
        /// 增加数值
        /// </summary>
        public static void Add(this Blackboard blackboard, string key, int delta)
        {
            int current = blackboard.Get(key, 0);
            blackboard.Set(key, current + delta);
        }
        /// <summary>
        /// 增加数值(使用默认黑板)
        /// </summary>
        public static void Add(this UBlackboardManager manager, string key, float delta)
        {
            float current = manager.Get(key, 0f);
            manager.Set(key, current + delta);
        }
        /// <summary>
        /// 增加数值(使用默认黑板)
        /// </summary>
        public static void Add(this UBlackboardManager manager, string key, int delta)
        {
            int current = manager.Get(key, 0);
            manager.Set(key, current + delta);
        }
        /// <summary>
        /// 获取或设置默认值
        /// </summary>
        public static T GetOrSet<T>(this Blackboard blackboard, string key, T defaultValue)
        {
            if (!blackboard.ContainsKey(key))
            {
                blackboard.Set(key, defaultValue);
            }
            return blackboard.Get(key, defaultValue);
        }

        #endregion

        #region 持久化快捷方法

        /// <summary>
        /// 设置持久化数据
        /// </summary>
        public static void SetPersistent<T>(this Blackboard blackboard, string key, T value)
        {
            blackboard.Set(key, value, isPersistent: true);
        }
        /// <summary>
        /// 设置持久化 Vector3
        /// </summary>
        public static void SetPersistentVector3(this Blackboard blackboard, string key, Vector3 value)
        {
            blackboard.SetVector3(key, value, isPersistent: true);
        }
        /// <summary>
        /// 设置持久化 Vector2
        /// </summary>
        public static void SetPersistentVector2(this Blackboard blackboard, string key, Vector2 value)
        {
            blackboard.SetVector2(key, value, isPersistent: true);
        }
        /// <summary>
        /// 设置持久化 Color
        /// </summary>
        public static void SetPersistentColor(this Blackboard blackboard, string key, Color value)
        {
            blackboard.SetColor(key, value, isPersistent: true);
        }
        /// <summary>
        /// 设置持久化数据(使用默认黑板)
        /// </summary>
        public static void SetPersistent<T>(this UBlackboardManager manager, string key, T value)
        {
            manager.Set(key, value, isPersistent: true);
        }
        /// <summary>
        /// 设置持久化 Vector3(使用默认黑板)
        /// </summary>
        public static void SetPersistentVector3(this UBlackboardManager manager, string key, Vector3 value)
        {
            manager.Default.SetVector3(key, value, isPersistent: true);
        }
        /// <summary>
        /// 设置持久化 Vector2(使用默认黑板)
        /// </summary>
        public static void SetPersistentVector2(this UBlackboardManager manager, string key, Vector2 value)
        {
            manager.Default.SetVector2(key, value, isPersistent: true);
        }
        /// <summary>
        /// 设置持久化 Color(使用默认黑板)
        /// </summary>
        public static void SetPersistentColor(this UBlackboardManager manager, string key, Color value)
        {
            manager.Default.SetColor(key, value, isPersistent: true);
        }

        #endregion

        #region 辅助类

        [System.Serializable]
        private class ArrayWrapper<T>
        {
            public T[] items;
        }

        #endregion
    }
}
