using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 布尔条件组，继承Dictionary<T, bool>，只有当条件组中所有条件均为真时，GetState()才为真
/// </summary>
/// <typeparam name="T">条件的类型</typeparam>
public class BoolState<T> : Dictionary<T, bool>
{
    public bool GetState(bool defaultValue)
    {
        if (Count == 0)
        {
            return defaultValue;
        }

        foreach (bool value in Values)
        {
            if (!value)
            {
                return false;
            }
        }
        return true;
    }
    public static implicit operator bool(BoolState<T> state) => state.GetState(true);
}
