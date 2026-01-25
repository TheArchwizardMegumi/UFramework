using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFramework
{
    public abstract class BasePanel : MonoBehaviour
    {
        public UIPanelGroup group;
        public PanelID ID;
        public async virtual UniTask Show()
        {
            UEvent.Broadcast(EventCode.ShowPanel, this);
        }
        public async virtual UniTask Hide()
        {
            UEvent.Broadcast(EventCode.HidePanel, this);
        }
        public virtual void Destroy()
        {
            UEvent.Broadcast(EventCode.DestroyPanel, this);
        }
        public virtual void Pause()
        {
            UEvent.Broadcast(EventCode.PausePanel, this);
        }
    }

    /// <summary>
    /// 面板标识符
    /// </summary>
    public readonly struct PanelID : IEquatable<PanelID>
    {
        public UIPanelType Type { get; }
        public string InstanceID { get; }

        public PanelID(UIPanelType type, string instanceId)
        {
            Type = type;
            InstanceID = instanceId;
        }

        public bool Equals(PanelID other) => Type == other.Type && InstanceID == other.InstanceID;
        public override bool Equals(object obj) => obj is PanelID other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Type, InstanceID);
        public override string ToString() => $"{Type}_{InstanceID}";
    }
}
