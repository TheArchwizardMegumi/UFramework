using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFramework
{
    [CreateAssetMenu(fileName = "newPanelConfig", menuName = "PanelConfigData")]
    public class PanelConfig : ScriptableObject
    {
        public SerializableDictionary<UIPanelType, PanelData> config;
    }
    [Serializable]
    public class PanelData
    {
        public GameObject panelPrefab;
        public UIPanelMode panelMode;
    }
    public enum UIPanelMode
    {
        Single,
        Multiple
    }
}
