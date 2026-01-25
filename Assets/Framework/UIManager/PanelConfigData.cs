using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFramework
{
    [CreateAssetMenu(fileName = "newPanelConfig", menuName = "PanelConfigData")]
    public class PanelConfigData : ScriptableObject
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
