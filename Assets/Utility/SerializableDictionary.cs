using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class SerializableDictionary { }

[Serializable]
public class SerializableDictionary<TKey, TValue> :
    SerializableDictionary,
    ISerializationCallbackReceiver,
    IDictionary<TKey, TValue>
{
    [Serializable]
    private struct SerializableKeyValuePair
    {
        public TKey Key;
        public TValue Value;

        public SerializableKeyValuePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    [SerializeField] 
    private List<SerializableKeyValuePair> kvpList = new();
    private Lazy<Dictionary<TKey, int>> keyPositions;
    private Dictionary<TKey, int> KeyPositions => keyPositions.Value;

    public SerializableDictionary()
    {
        keyPositions = new Lazy<Dictionary<TKey, int>>(MakeKeyPositions);
    }

    private Dictionary<TKey, int> MakeKeyPositions()
    {
        var dictionary = new Dictionary<TKey, int>(kvpList.Count);
        for (int i = 0; i < kvpList.Count; i++)
        {
            dictionary[kvpList[i].Key] = i;
        }
        return dictionary;
    }

    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        keyPositions = new Lazy<Dictionary<TKey, int>>(MakeKeyPositions);
    }

    #region IDictionary<TKey, TValue>

    public TValue this[TKey key]
    {
        get => kvpList[KeyPositions[key]].Value;
        set
        {
            var pair = new SerializableKeyValuePair(key, value);
            if (KeyPositions.ContainsKey(key))
            {
                kvpList[KeyPositions[key]] = pair;
            }
            else
            {
                KeyPositions[key] = kvpList.Count;
                kvpList.Add(pair);
            }
        }
    }

    public ICollection<TKey> Keys => kvpList.Select(tuple => tuple.Key).ToArray();
    public ICollection<TValue> Values => kvpList.Select(tuple => tuple.Value).ToArray();

    public void Add(TKey key, TValue value)
    {
        if (KeyPositions.ContainsKey(key))
            throw new ArgumentException("An element with the same key already exists in the dictionary.");
        else
        {
            KeyPositions[key] = kvpList.Count;
            kvpList.Add(new SerializableKeyValuePair(key, value));
        }
    }

    public bool ContainsKey(TKey key) => KeyPositions.ContainsKey(key);

    public bool Remove(TKey key)
    {
        if (KeyPositions.TryGetValue(key, out var index))
        {
            KeyPositions.Remove(key);

            kvpList.RemoveAt(index);
            for (var i = index; i < kvpList.Count; i++)
                KeyPositions[kvpList[i].Key] = i;

            return true;
        }
        else
            return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (KeyPositions.TryGetValue(key, out var index))
        {
            value = kvpList[index].Value;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    #endregion

    #region ICollection <KeyValuePair<TKey, TValue>>

    public int Count => kvpList.Count;
    public bool IsReadOnly => false;

    public void Add(KeyValuePair<TKey, TValue> kvp) => Add(kvp.Key, kvp.Value);
    public void Clear() => kvpList.Clear();
    public bool Contains(KeyValuePair<TKey, TValue> kvp) => KeyPositions.ContainsKey(kvp.Key);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        var numKeys = kvpList.Count;
        if (array.Length - arrayIndex < numKeys)
            throw new ArgumentException("arrayIndex");
        for (var i = 0; i < numKeys; i++, arrayIndex++)
        {
            var entry = kvpList[i];
            array[arrayIndex] = new KeyValuePair<TKey, TValue>(entry.Key, entry.Value);
        }
    }
    public bool Remove(KeyValuePair<TKey, TValue> kvp) => Remove(kvp.Key);

    #endregion

    #region IEnumerable <KeyValuePair<TKey, TValue>>

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return kvpList.Select(ToKeyValuePair).GetEnumerator();

        static KeyValuePair<TKey, TValue> ToKeyValuePair(SerializableKeyValuePair skvp)
        {
            return new KeyValuePair<TKey, TValue>(skvp.Key, skvp.Value);
        }
    }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}

[CustomPropertyDrawer(typeof(SerializableDictionary), true)]
public class SerializableDictionaryDrawer : PropertyDrawer
{
    private SerializedProperty listProperty;

    private SerializedProperty GetListProperty(SerializedProperty property) =>
        listProperty ??= property.FindPropertyRelative("kvpList");

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.PropertyField(position, GetListProperty(property), label, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(GetListProperty(property), true);
    }
}