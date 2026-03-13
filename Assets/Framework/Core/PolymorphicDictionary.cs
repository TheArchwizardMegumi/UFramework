using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace UFramework
{
    /// <summary>
    /// 支持多态序列化的字典（Value 类型必须是 class），使用 SerializeReference 保留派生类的完整类型信息
    /// </summary>
    [Serializable]
    public class PolymorphicDictionary<TKey, TValue> :
        ISerializationCallbackReceiver,
        IDictionary<TKey, TValue>
        where TValue : class
    {
        [Serializable]
        private struct PolymorphicPair
        {
            public TKey Key;
            [SerializeReference]
            public TValue Value;

            public PolymorphicPair(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }

        [SerializeField]
        private List<PolymorphicPair> serializeList = new();
        [NonSerialized]
        private readonly Dictionary<TKey, TValue> runtimeDict = new();

        #region IDictionary<TKey, TValue>

        public TValue this[TKey key]
        {
            get => runtimeDict[key];
            set => runtimeDict[key] = value;
        }

        public ICollection<TKey> Keys => runtimeDict.Keys;
        public ICollection<TValue> Values => runtimeDict.Values;

        public void Add(TKey key, TValue value) => runtimeDict.Add(key, value);
        public bool ContainsKey(TKey key) => runtimeDict.ContainsKey(key);
        public bool Remove(TKey key) => runtimeDict.Remove(key);
        public bool TryGetValue(TKey key, out TValue value) => runtimeDict.TryGetValue(key, out value);

        #endregion

        #region ICollection<KeyValuePair<TKey, TValue>>

        public int Count => runtimeDict.Count;
        public bool IsReadOnly => false;

        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public void Clear() => runtimeDict.Clear();
        public bool Contains(KeyValuePair<TKey, TValue> item) => runtimeDict.Contains(item);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)runtimeDict).CopyTo(array, arrayIndex);
        }
        public bool Remove(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)runtimeDict).Remove(item);

        #endregion

        #region IEnumerable<KeyValuePair<TKey, TValue>>

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => runtimeDict.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => runtimeDict.GetEnumerator();

        #endregion

        #region ISerializationCallbackReceiver

        public void OnBeforeSerialize()
        {
            serializeList.Clear();
            foreach (var kvp in runtimeDict)
            {
                serializeList.Add(new PolymorphicPair(kvp.Key, kvp.Value));
            }
        }

        public void OnAfterDeserialize()
        {
            runtimeDict.Clear();
            foreach (var kvp in serializeList)
            {
                if (kvp.Value != null)
                {
                    runtimeDict[kvp.Key] = kvp.Value;
                }
            }
        }

        #endregion

        #region Operator

        public static implicit operator PolymorphicDictionary<TKey, TValue>(Dictionary<TKey, TValue> dict)
        {
            if (dict == null)
                return null;

            var result = new PolymorphicDictionary<TKey, TValue>();
            foreach (var kvp in dict)
            {
                result.Add(kvp.Key, kvp.Value);
            }
            return result;
        }

        public static implicit operator Dictionary<TKey, TValue>(PolymorphicDictionary<TKey, TValue> polyDict)
        {
            if (polyDict == null)
                return null;

            var result = new Dictionary<TKey, TValue>();
            foreach (var kvp in polyDict)
            {
                result[kvp.Key] = kvp.Value;
            }
            return result;
        }

        #endregion
    }

#if UNITY_EDITOR

    [CustomPropertyDrawer(typeof(PolymorphicDictionary<,>), true)]
    public class PolymorphicDictionaryDrawer : PropertyDrawer
    {
        private SerializedProperty listProperty;

        private SerializedProperty GetListProperty(SerializedProperty property) =>
            listProperty ??= property.FindPropertyRelative("serializeList");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var list = GetListProperty(property);
            EditorGUI.PropertyField(position, list, label, true);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(GetListProperty(property), label, true);
        }
    }
#endif
}
