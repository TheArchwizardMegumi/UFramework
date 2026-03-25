using System;
using System.Linq;
using UnityEngine;

namespace UFramework
{
    /// <summary>
    /// 黑板数据验证器
    /// 提供灵活的数据有效性验证机制
    /// </summary>
    public interface IBlackboardValidator
    {
        /// <summary>
        /// 验证数据是否有效
        /// </summary>
        /// <param name="value">要验证的数据值</param>
        /// <returns>数据是否有效</returns>
        bool IsValid(object value);

        /// <summary>
        /// 获取验证失败的描述信息
        /// </summary>
        /// <returns>错误描述，如果验证通过则返回 null</returns>
        string GetErrorDescription();
    }

    /// <summary>
    /// 基于委托的自定义验证器
    /// </summary>
    public class DelegateValidator : IBlackboardValidator
    {
        private readonly Func<object, bool> validateFunc;
        private readonly Func<string> errorFunc;

        /// <summary>
        /// 创建委托验证器
        /// </summary>
        /// <param name="validateFunc">验证函数，返回 true 表示有效</param>
        /// <param name="errorFunc">错误描述函数</param>
        public DelegateValidator(Func<object, bool> validateFunc, Func<string> errorFunc = null)
        {
            this.validateFunc = validateFunc ?? throw new ArgumentNullException(nameof(validateFunc));
            this.errorFunc = errorFunc ?? (() => "数据验证失败");
        }

        public bool IsValid(object value)
        {
            try
            {
                return validateFunc(value);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DelegateValidator] 验证时发生异常: {ex.Message}");
                return false;
            }
        }

        public string GetErrorDescription()
        {
            return errorFunc?.Invoke();
        }
    }

    /// <summary>
    /// 泛型委托验证器
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public class DelegateValidator<T> : IBlackboardValidator
    {
        private readonly Func<T, bool> validateFunc;
        private readonly Func<string> errorFunc;

        /// <summary>
        /// 创建泛型委托验证器
        /// </summary>
        /// <param name="validateFunc">验证函数，返回 true 表示有效</param>
        /// <param name="errorFunc">错误描述函数</param>
        public DelegateValidator(Func<T, bool> validateFunc, Func<string> errorFunc = null)
        {
            this.validateFunc = validateFunc ?? throw new ArgumentNullException(nameof(validateFunc));
            this.errorFunc = errorFunc ?? (() => "数据验证失败");
        }

        public bool IsValid(object value)
        {
            if (value == null)
                return false;

            if (!(value is T typedValue))
            {
                Debug.LogWarning($"[DelegateValidator<T>] 类型不匹配，期望 {typeof(T)}, 实际 {value.GetType()}");
                return false;
            }

            try
            {
                return validateFunc(typedValue);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[DelegateValidator<T>] 验证时发生异常: {ex.Message}");
                return false;
            }
        }

        public string GetErrorDescription()
        {
            return errorFunc?.Invoke();
        }
    }

    /// <summary>
    /// 基于过期时间的验证器
    /// </summary>
    public class ExpirationValidator : IBlackboardValidator
    {
        private readonly DateTime expireTime;
        private readonly string key;

        /// <summary>
        /// 创建过期时间验证器
        /// </summary>
        /// <param name="expireTime">过期时间点</param>
        /// <param name="key">数据键，用于错误描述</param>
        public ExpirationValidator(DateTime expireTime, string key = null)
        {
            this.expireTime = expireTime;
            this.key = key;
        }

        /// <summary>
        /// 创建过期时间验证器
        /// </summary>
        /// <param name="timeToLive">存活时间（从创建开始）</param>
        /// <param name="key">数据键，用于错误描述</param>
        public ExpirationValidator(TimeSpan timeToLive, string key = null)
        {
            this.expireTime = DateTime.Now + timeToLive;
            this.key = key;
        }

        public bool IsValid(object value)
        {
            return DateTime.Now < expireTime;
        }

        public string GetErrorDescription()
        {
            return $"数据 [{key}] 已过期，过期时间: {expireTime:yyyy-MM-dd HH:mm:ss}";
        }

        /// <summary>
        /// 获取剩余存活时间
        /// </summary>
        public TimeSpan GetRemainingTime()
        {
            var remaining = expireTime - DateTime.Now;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
    }

    /// <summary>
    /// 组合验证器（AND 逻辑）
    /// 所有验证器都通过才认为数据有效
    /// </summary>
    public class AndValidator : IBlackboardValidator
    {
        private readonly IBlackboardValidator[] validators;

        public AndValidator(params IBlackboardValidator[] validators)
        {
            this.validators = validators ?? throw new ArgumentNullException(nameof(validators));
            if (validators.Length == 0)
            {
                throw new ArgumentException("至少需要一个验证器", nameof(validators));
            }
        }

        public bool IsValid(object value)
        {
            foreach (var validator in validators)
            {
                if (validator == null)
                    continue;

                if (!validator.IsValid(value))
                    return false;
            }
            return true;
        }

        public string GetErrorDescription()
        {
            foreach (var validator in validators)
            {
                if (validator != null && !validator.IsValid(null))
                {
                    return validator.GetErrorDescription();
                }
            }
            return "所有验证器都通过";
        }
    }

    /// <summary>
    /// 组合验证器（OR 逻辑）
    /// 任意一个验证器通过就认为数据有效
    /// </summary>
    public class OrValidator : IBlackboardValidator
    {
        private readonly IBlackboardValidator[] validators;

        public OrValidator(params IBlackboardValidator[] validators)
        {
            this.validators = validators ?? throw new ArgumentNullException(nameof(validators));
            if (validators.Length == 0)
            {
                throw new ArgumentException("至少需要一个验证器", nameof(validators));
            }
        }

        public bool IsValid(object value)
        {
            foreach (var validator in validators)
            {
                if (validator == null)
                    continue;

                if (validator.IsValid(value))
                    return true;
            }
            return false;
        }

        public string GetErrorDescription()
        {
            var errors = new System.Text.StringBuilder();
            for (int i = 0; i < validators.Length; i++)
            {
                if (validators[i] != null)
                {
                    errors.AppendLine($"验证器 {i + 1}: {validators[i].GetErrorDescription()}");
                }
            }
            return $"所有验证器均失败:\n{errors}";
        }
    }

    /// <summary>
    /// 黑板数据验证器扩展方法
    /// 提供便捷的验证器创建方式
    /// </summary>
    public static class ValidatorExtensions
    {
        /// <summary>
        /// 创建基于委托的验证器
        /// </summary>
        public static IBlackboardValidator Validate(this object _, Func<object, bool> validateFunc, Func<string> errorFunc = null)
        {
            return new DelegateValidator(validateFunc, errorFunc);
        }

        /// <summary>
        /// 创建泛型委托验证器
        /// </summary>
        public static IBlackboardValidator Validate<T>(this object _, Func<T, bool> validateFunc, Func<string> errorFunc = null)
        {
            return new DelegateValidator<T>(validateFunc, errorFunc);
        }

        /// <summary>
        /// 创建过期时间验证器
        /// </summary>
        public static IBlackboardValidator ExpireAfter(this object _, TimeSpan timeToLive, string key = null)
        {
            return new ExpirationValidator(timeToLive, key);
        }

        /// <summary>
        /// 创建在指定时间点过期的验证器
        /// </summary>
        public static IBlackboardValidator ExpireAt(this object _, DateTime expireTime, string key = null)
        {
            return new ExpirationValidator(expireTime, key);
        }

        /// <summary>
        /// 组合多个验证器（AND 逻辑）
        /// </summary>
        public static IBlackboardValidator And(this IBlackboardValidator validator, params IBlackboardValidator[] others)
        {
            return new AndValidator(new[] { validator }.Concat(others).ToArray());
        }

        /// <summary>
        /// 组合多个验证器（OR 逻辑）
        /// </summary>
        public static IBlackboardValidator Or(this IBlackboardValidator validator, params IBlackboardValidator[] others)
        {
            return new OrValidator(new[] { validator }.Concat(others).ToArray());
        }
    }
}
