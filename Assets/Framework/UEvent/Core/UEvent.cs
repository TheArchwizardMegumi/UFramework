using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFramework
{
    public static class UEvent
    {
        private static readonly Dictionary<EventCode, Delegate> eventTable = new();
        public static void AddListener(EventCode eventCode, Callback callback)
        {
            if (OnListenerAdding(eventCode, callback))
            {
                eventTable[eventCode] = (Callback)eventTable[eventCode] + callback;
            }
        }
        public static void AddListener<T>(EventCode eventCode, Callback<T> callback)
        {
            if (OnListenerAdding(eventCode, callback))
            {
                eventTable[eventCode] = (Callback<T>)eventTable[eventCode] + callback;
            }
        }
        public static void AddListener<T1, T2>(EventCode eventCode, Callback<T1, T2> callback)
        {
            if (OnListenerAdding(eventCode, callback))
            {
                eventTable[eventCode] = (Callback<T1, T2>)eventTable[eventCode] + callback;
            }
        }
        public static void AddListener<T1, T2, T3>(EventCode eventCode, Callback<T1, T2, T3> callback)
        {
            if (OnListenerAdding(eventCode, callback))
            {
                eventTable[eventCode] = (Callback<T1, T2, T3>)eventTable[eventCode] + callback;
            }
        }
        public static void AddListener<T1, T2, T3, T4>(EventCode eventCode, Callback<T1, T2, T3, T4> callback)
        {
            if (OnListenerAdding(eventCode, callback))
            {
                eventTable[eventCode] = (Callback<T1, T2, T3, T4>)eventTable[eventCode] + callback;
            }
        }
        public static void AddListener<T1, T2, T3, T4, T5>(EventCode eventCode, Callback<T1, T2, T3, T4, T5> callback)
        {
            if (OnListenerAdding(eventCode, callback))
            {
                eventTable[eventCode] = (Callback<T1, T2, T3, T4, T5>)eventTable[eventCode] + callback;
            }
        }
        public static void AddListener<T1, T2, T3, T4, T5, T6>(EventCode eventCode, Callback<T1, T2, T3, T4, T5, T6> callback)
        {
            if (OnListenerAdding(eventCode, callback))
            {
                eventTable[eventCode] = (Callback<T1, T2, T3, T4, T5, T6>)eventTable[eventCode] + callback;
            }
        }
        public static void AddListener(EventCode eventCode, Callback_Return callback)
        {
            if (OnListenerAdding(eventCode, callback))
            {
                eventTable[eventCode] = (Callback_Return)eventTable[eventCode] + callback;
            }
        }
        public static void AddListener<T>(EventCode eventCode, Callback_Return<T> callback)
        {
            if (OnListenerAdding(eventCode, callback))
            {
                eventTable[eventCode] = (Callback_Return<T>)eventTable[eventCode] + callback;
            }
        }
        public static void AddListener<T1, T2>(EventCode eventCode, Callback_Return<T1, T2> callback)
        {
            if (OnListenerAdding(eventCode, callback))
            {
                eventTable[eventCode] = (Callback_Return<T1, T2>)eventTable[eventCode] + callback;
            }
        }
        public static void AddListener<T1, T2, T3>(EventCode eventCode, Callback_Return<T1, T2, T3> callback)
        {
            if (OnListenerAdding(eventCode, callback))
            {
                eventTable[eventCode] = (Callback_Return<T1, T2, T3>)eventTable[eventCode] + callback;
            }
        }
        public static void AddListener<T1, T2, T3, T4>(EventCode eventCode, Callback_Return<T1, T2, T3, T4> callback)
        {
            if (OnListenerAdding(eventCode, callback))
            {
                eventTable[eventCode] = (Callback_Return<T1, T2, T3, T4>)eventTable[eventCode] + callback;
            }
        }
        public static void AddListener<T1, T2, T3, T4, T5>(EventCode eventCode, Callback_Return<T1, T2, T3, T4, T5> callback)
        {
            if (OnListenerAdding(eventCode, callback))
            {
                eventTable[eventCode] = (Callback_Return<T1, T2, T3, T4, T5>)eventTable[eventCode] + callback;
            }
        }
        public static void AddListener<T1, T2, T3, T4, T5, T6>(EventCode eventCode, Callback_Return<T1, T2, T3, T4, T5, T6> callback)
        {
            if (OnListenerAdding(eventCode, callback))
            {
                eventTable[eventCode] = (Callback_Return<T1, T2, T3, T4, T5, T6>)eventTable[eventCode] + callback;
            }
        }
        public static void RemoveListener(EventCode eventCode, Callback callback)
        {
            if (OnListenerRemoving(eventCode, callback))
            {
                eventTable[eventCode] = (Callback)eventTable[eventCode] - callback;
                OnListenerRemoved(eventCode);
            }
        }
        public static void RemoveListener<T>(EventCode eventCode, Callback<T> callback)
        {
            if (OnListenerRemoving(eventCode, callback))
            {
                eventTable[eventCode] = (Callback<T>)eventTable[eventCode] - callback;
                OnListenerRemoved(eventCode);
            }
        }
        public static void RemoveListener<T1, T2>(EventCode eventCode, Callback<T1, T2> callback)
        {
            if (OnListenerRemoving(eventCode, callback))
            {
                eventTable[eventCode] = (Callback<T1, T2>)eventTable[eventCode] - callback;
                OnListenerRemoved(eventCode);
            }
        }
        public static void RemoveListener<T1, T2, T3>(EventCode eventCode, Callback<T1, T2, T3> callback)
        {
            if (OnListenerRemoving(eventCode, callback))
            {
                eventTable[eventCode] = (Callback<T1, T2, T3>)eventTable[eventCode] - callback;
                OnListenerRemoved(eventCode);
            }
        }
        public static void RemoveListener<T1, T2, T3, T4>(EventCode eventCode, Callback<T1, T2, T3, T4> callback)
        {
            if (OnListenerRemoving(eventCode, callback))
            {
                eventTable[eventCode] = (Callback<T1, T2, T3, T4>)eventTable[eventCode] - callback;
                OnListenerRemoved(eventCode);
            }
        }
        public static void RemoveListener<T1, T2, T3, T4, T5>(EventCode eventCode, Callback<T1, T2, T3, T4, T5> callback)
        {
            if (OnListenerRemoving(eventCode, callback))
            {
                eventTable[eventCode] = (Callback<T1, T2, T3, T4, T5>)eventTable[eventCode] - callback;
                OnListenerRemoved(eventCode);
            }
        }
        public static void RemoveListener<T1, T2, T3, T4, T5, T6>(EventCode eventCode, Callback<T1, T2, T3, T4, T5, T6> callback)
        {
            if (OnListenerRemoving(eventCode, callback))
            {
                eventTable[eventCode] = (Callback<T1, T2, T3, T4, T5, T6>)eventTable[eventCode] - callback;
                OnListenerRemoved(eventCode);
            }
        }
        public static void RemoveListener(EventCode eventCode, Callback_Return callback)
        {
            if (OnListenerRemoving(eventCode, callback))
            {
                eventTable[eventCode] = (Callback_Return)eventTable[eventCode] - callback;
                OnListenerRemoved(eventCode);
            }
        }
        public static void RemoveListener<T>(EventCode eventCode, Callback_Return<T> callback)
        {
            if (OnListenerRemoving(eventCode, callback))
            {
                eventTable[eventCode] = (Callback_Return<T>)eventTable[eventCode] - callback;
                OnListenerRemoved(eventCode);
            }
        }
        public static void RemoveListener<T1, T2>(EventCode eventCode, Callback_Return<T1, T2> callback)
        {
            if (OnListenerRemoving(eventCode, callback))
            {
                eventTable[eventCode] = (Callback_Return<T1, T2>)eventTable[eventCode] - callback;
                OnListenerRemoved(eventCode);
            }
        }
        public static void RemoveListener<T1, T2, T3>(EventCode eventCode, Callback_Return<T1, T2, T3> callback)
        {
            if (OnListenerRemoving(eventCode, callback))
            {
                eventTable[eventCode] = (Callback_Return<T1, T2, T3>)eventTable[eventCode] - callback;
                OnListenerRemoved(eventCode);
            }
        }
        public static void RemoveListener<T1, T2, T3, T4>(EventCode eventCode, Callback_Return<T1, T2, T3, T4> callback)
        {
            if (OnListenerRemoving(eventCode, callback))
            {
                eventTable[eventCode] = (Callback_Return<T1, T2, T3, T4>)eventTable[eventCode] - callback;
                OnListenerRemoved(eventCode);
            }
        }
        public static void RemoveListener<T1, T2, T3, T4, T5>(EventCode eventCode, Callback_Return<T1, T2, T3, T4, T5> callback)
        {
            if (OnListenerRemoving(eventCode, callback))
            {
                eventTable[eventCode] = (Callback_Return<T1, T2, T3, T4, T5>)eventTable[eventCode] - callback;
                OnListenerRemoved(eventCode);
            }
        }
        public static void RemoveListener<T1, T2, T3, T4, T5, T6>(EventCode eventCode, Callback_Return<T1, T2, T3, T4, T5, T6> callback)
        {
            if (OnListenerRemoving(eventCode, callback))
            {
                eventTable[eventCode] = (Callback_Return<T1, T2, T3, T4, T5, T6>)eventTable[eventCode] - callback;
                OnListenerRemoved(eventCode);
            }
        }
        public static void Broadcast(EventCode eventCode)
        {
            if (eventTable.TryGetValue(eventCode, out Delegate c))
            {
                if (c is Callback callback)
                    callback();
                else
                    Debug.LogError($"Can not broadcast event \"{eventCode}\" with the type of {eventTable[eventCode]}");
            }
        }
        public static void Broadcast<T>(EventCode eventCode, T arg)
        {
            if (eventTable.TryGetValue(eventCode, out Delegate c))
            {
                if (c is Callback<T> callback)
                    callback(arg);
                else
                    Debug.LogError($"Can not broadcast event \"{eventCode}\" with the type of {eventTable[eventCode]}");
            }
        }
        public static void Broadcast<T1, T2>(EventCode eventCode, T1 arg1, T2 arg2)
        {
            if (eventTable.TryGetValue(eventCode, out Delegate c))
            {
                if (c is Callback<T1, T2> callback)
                    callback(arg1, arg2);
                else
                    Debug.LogError($"Can not broadcast event \"{eventCode}\" with the type of {eventTable[eventCode]}");
            }
        }
        public static void Broadcast<T1, T2, T3>(EventCode eventCode, T1 arg1, T2 arg2, T3 arg3)
        {
            if (eventTable.TryGetValue(eventCode, out Delegate c))
            {
                if (c is Callback<T1, T2, T3> callback)
                    callback(arg1, arg2, arg3);
                else
                    Debug.LogError($"Can not broadcast event \"{eventCode}\" with the type of {eventTable[eventCode]}");
            }
        }
        public static void Broadcast<T1, T2, T3, T4>(EventCode eventCode, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (eventTable.TryGetValue(eventCode, out Delegate c))
            {
                if (c is Callback<T1, T2, T3, T4> callback)
                    callback(arg1, arg2, arg3, arg4);
                else
                    Debug.LogError($"Can not broadcast event \"{eventCode}\" with the type of {eventTable[eventCode]}");
            }
        }
        public static void Broadcast<T1, T2, T3, T4, T5>(EventCode eventCode, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if (eventTable.TryGetValue(eventCode, out Delegate c))
            {
                if (c is Callback<T1, T2, T3, T4, T5> callback)
                    callback(arg1, arg2, arg3, arg4, arg5);
                else
                    Debug.LogError($"Can not broadcast event \"{eventCode}\" with the type of {eventTable[eventCode]}");
            }
        }
        public static void Broadcast<T1, T2, T3, T4, T5, T6>(EventCode eventCode, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            if (eventTable.TryGetValue(eventCode, out Delegate c))
            {
                if (c is Callback<T1, T2, T3, T4, T5, T6> callback)
                    callback(arg1, arg2, arg3, arg4, arg5, arg6);
                else
                    Debug.LogError($"Can not broadcast event \"{eventCode}\" with the type of {eventTable[eventCode]}");
            }
        }
        public static object Broadcast_Return(EventCode eventCode)
        {
            if (eventTable.TryGetValue(eventCode, out Delegate c))
            {
                if (c is Callback_Return callback)
                    return callback();
                else
                    Debug.LogError($"Can not broadcast event \"{eventCode}\" with the type of {eventTable[eventCode]}");
            }
            return default;
        }
        public static object Broadcast_Return<T>(EventCode eventCode, T arg)
        {
            if (eventTable.TryGetValue(eventCode, out Delegate c))
            {
                if (c is Callback_Return<T> callback)
                    return callback(arg);
                else
                    Debug.LogError($"Can not broadcast event \"{eventCode}\" with the type of {eventTable[eventCode]}");
            }
            return default;
        }
        public static object Broadcast_Return<T1, T2>(EventCode eventCode, T1 arg1, T2 arg2)
        {
            if (eventTable.TryGetValue(eventCode, out Delegate c))
            {
                if (c is Callback_Return<T1, T2> callback)
                    return callback(arg1, arg2);
                else
                    Debug.LogError($"Can not broadcast event \"{eventCode}\" with the type of {eventTable[eventCode]}");
            }
            return default;
        }
        public static object Broadcast_Return<T1, T2, T3>(EventCode eventCode, T1 arg1, T2 arg2, T3 arg3)
        {
            if (eventTable.TryGetValue(eventCode, out Delegate c))
            {
                if (c is Callback_Return<T1, T2, T3> callback)
                    return callback(arg1, arg2, arg3);
                else
                    Debug.LogError($"Can not broadcast event \"{eventCode}\" with the type of {eventTable[eventCode]}");
            }
            return default;
        }
        public static object Broadcast_Return<T1, T2, T3, T4>(EventCode eventCode, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (eventTable.TryGetValue(eventCode, out Delegate c))
            {
                if (c is Callback_Return<T1, T2, T3, T4> callback)
                    return callback(arg1, arg2, arg3, arg4);
                else
                    Debug.LogError($"Can not broadcast event \"{eventCode}\" with the type of {eventTable[eventCode]}");
            }
            return default;
        }
        public static object Broadcast_Return<T1, T2, T3, T4, T5>(EventCode eventCode, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if (eventTable.TryGetValue(eventCode, out Delegate c))
            {
                if (c is Callback_Return<T1, T2, T3, T4, T5> callback)
                    return callback(arg1, arg2, arg3, arg4, arg5);
                else
                    Debug.LogError($"Can not broadcast event \"{eventCode}\" with the type of {eventTable[eventCode]}");
            }
            return default;
        }
        public static object Broadcast_Return<T1, T2, T3, T4, T5, T6>(EventCode eventCode, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            if (eventTable.TryGetValue(eventCode, out Delegate c))
            {
                if (c is Callback_Return<T1, T2, T3, T4, T5, T6> callback)
                    return callback(arg1, arg2, arg3, arg4, arg5, arg6);
                else
                    Debug.LogError($"Can not broadcast event \"{eventCode}\" with the type of {eventTable[eventCode]}");
            }
            return default;
        }
        public static void ClearAllEvents()
        {
            eventTable.Clear();
        }
        public static void ClearEvent(EventCode eventCode)
        {
            if (eventTable.ContainsKey(eventCode))
            {
                eventTable.Remove(eventCode);
            }
        }
        private static bool OnListenerAdding(EventCode ec, Delegate callback)
        {
            if (callback == null)
            {
                Debug.LogError($"Trying add null callback method to event \"{ec}\"");
                return false;
            }
            if (!eventTable.ContainsKey(ec))
                eventTable.Add(ec, null);
            else if (callback.GetType() != eventTable[ec].GetType())
            {
                Debug.LogError($"Callback type ({callback.GetType()}) inconsists with eventTable ({eventTable[ec].GetType()})");
                return false;
            }
            return true;
        }
        private static bool OnListenerRemoving(EventCode ec, Delegate callback)
        {
            if (callback == null)
            {
                Debug.LogError($"Trying remove null callback method from event \"{ec}\"");
                return false;
            }
            if (eventTable.ContainsKey(ec))
            {
                Delegate d = eventTable[ec];
                if (d != null)
                {
                    if (d.GetType() != callback.GetType())
                    {
                        Debug.LogError($"Event \"{ec}\"'s type inconsists with eventTable ({d.GetType()})");
                        return false;
                    }
                }
                else
                {
                    Debug.LogError($"Event \"{ec}\" doesn't have a listener");
                    return false;
                }
            }
            else
            {
                Debug.LogError($"Event \"{ec}\" doesn't exist");
                return false;
            }
            return true;
        }
        private static void OnListenerRemoved(EventCode eventType)
        {
            if (eventTable[eventType] == null)
                eventTable.Remove(eventType);
        }
    }
}