using System;

namespace UFramework
{
    /// <summary>
    /// 黑板数据变更事件参数
    /// </summary>
    public class BlackboardEventArgs : EventArgs
    {
        /// <summary>
        /// 数据键
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// 旧值
        /// </summary>
        public object OldValue { get; }

        /// <summary>
        /// 新值
        /// </summary>
        public object NewValue { get; }

        /// <summary>
        /// 变更类型
        /// </summary>
        public BlackboardChangeType ChangeType { get; }

        public BlackboardEventArgs(string key, object oldValue, object newValue, BlackboardChangeType changeType)
        {
            Key = key;
            OldValue = oldValue;
            NewValue = newValue;
            ChangeType = changeType;
        }
    }

    /// <summary>
    /// 黑板数据变更类型
    /// </summary>
    public enum BlackboardChangeType
    {
        /// <summary>
        /// 数据添加
        /// </summary>
        Added,

        /// <summary>
        /// 数据更新
        /// </summary>
        Updated,

        /// <summary>
        /// 数据移除
        /// </summary>
        Removed
    }
}
