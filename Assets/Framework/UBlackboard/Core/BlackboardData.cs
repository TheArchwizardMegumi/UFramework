using System;
using UnityEngine;

namespace UFramework
{
    /// <summary>
    /// 黑板数据项
    /// 封装存储在黑板中的数据及其元信息
    /// </summary>
    public class BlackboardData
    {
        /// <summary>
        /// 数据键
        /// </summary>
        public string Key { get; private set; }

        /// <summary>
        /// 数据值
        /// </summary>
        public object Value { get; private set; }

        /// <summary>
        /// 数据类型
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; private set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; private set; }

        /// <summary>
        /// 是否持久化
        /// </summary>
        public bool IsPersistent { get; set; }

        /// <summary>
        /// 数据验证器
        /// </summary>
        public IBlackboardValidator Validator { get; set; }

        /// <summary>
        /// 是否启用验证
        /// </summary>
        public bool ValidateEnabled => Validator != null;

        public BlackboardData(string key, object value, bool isPersistent = false, IBlackboardValidator validator = null)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key cannot be null or empty", nameof(key));
            }

            Key = key;
            Value = value;
            Type = value?.GetType();
            CreateTime = DateTime.Now;
            LastUpdateTime = DateTime.Now;
            IsPersistent = isPersistent;
            Validator = validator;
        }

        /// <summary>
        /// 检查数据是否有效
        /// </summary>
        /// <returns>数据是否有效</returns>
        public bool IsValid()
        {
            return !ValidateEnabled || Validator.IsValid(Value);
        }

        /// <summary>
        /// 获取验证失败描述
        /// </summary>
        /// <returns>错误描述，如果验证通过则返回 null</returns>
        public string GetValidationError()
        {
            if (!ValidateEnabled)
                return null;

            return Validator.IsValid(Value) ? null : Validator.GetErrorDescription();
        }

        /// <summary>
        /// 更新数据值
        /// </summary>
        /// <param name="value">新值</param>
        /// <param name="keepValidator">是否保留验证器</param>
        public void UpdateValue(object value, bool keepValidator = true)
        {
            Value = value;
            Type = value?.GetType();
            LastUpdateTime = DateTime.Now;
            if (!keepValidator)
            {
                Validator = null;
            }
        }

        /// <summary>
        /// 获取指定类型的值
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <returns>转换后的值</returns>
        public T GetValue<T>()
        {
            if (Value is T typedValue)
            {
                return typedValue;
            }

            try
            {
                return (T)Convert.ChangeType(Value, typeof(T));
            }
            catch (Exception)
            {
                Debug.LogWarning($"[BlackboardData] 无法将 {Type?.Name} 转换为 {typeof(T).Name}");
                return default;
            }
        }

        /// <summary>
        /// 检查值是否为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <returns>是否匹配</returns>
        public bool IsType<T>()
        {
            return Value is T;
        }

        public override string ToString()
        {
            var validatorStatus = ValidateEnabled ? (IsValid() ? "[✓有效]" : "[✗无效]") : "[无验证]";
            return $"[{Key}] = {Value} ({Type?.Name}) {validatorStatus}";
        }
    }
}
