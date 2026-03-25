using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UFramework
{
    public class UIManager : Singleton<UIManager>
    {
        /// <summary>
        /// 面板配置SO文件的引用
        /// </summary>
        [SerializeField]
        private PanelConfig panelConfig;
        /// <summary>
        /// 管理单例面板，储存面板类型到已加载单例面板对象的映射
        /// </summary>
        private readonly Dictionary<UIPanelType, BasePanel> singletonPanelDict = new();
        /// <summary>
        /// 管理多例面板，储存面板ID到已加载多例面板的映射
        /// </summary>
        private readonly Dictionary<PanelID, BasePanel> multiplePanelDict = new();

        #region 面板管理方法
        private BasePanel CreatePanel(UIPanelType panelType, PanelID panelID)
        {
            //根据面板类型读取配置文件
            if (panelConfig.config.TryGetValue(panelType, out PanelData config))
            {
                GameObject panelInstance = Instantiate(config.panelPrefab);
                panelInstance.SetActive(false);
                if (panelInstance.TryGetComponent(out BasePanel panelComp))
                {
                    panelComp.ID = panelID;
                    return panelComp;
                }
                else
                {
                    Debug.LogError($"{panelType} 类型的面板实例 {panelInstance.name} 没有BasePanel类型的组件，请检查面板预制体组件是否正确");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"找不到 {panelType} 类型的配置文件，请在panelConfig字段中配置");
                return null;
            }
        }
        private async UniTask HideSingletonPanel(UIPanelType panelType)
        {
            if (singletonPanelDict.TryGetValue(panelType, out BasePanel panel))
            {
                await panel.group.HidePanel(panel);
            }
            else
            {
                Debug.LogError($"{panelType} 类型的面板未加载");
            }
        }
        private async UniTask HideMultiplePanel(PanelID panelID)
        {
            if (multiplePanelDict.TryGetValue(panelID, out BasePanel panel))
            {
                await panel.group.HidePanel(panel);
            }
            else
            {
                Debug.LogError($"未找到类型为 {panelID.Type}，ID为 {panelID} 的多例面板");
            }
        }
        private async UniTask DestroySingletonPanel(UIPanelType panelType, bool waitUntilHidden)
        {
            if (singletonPanelDict.TryGetValue(panelType, out BasePanel panel))
            {
                singletonPanelDict.Remove(panelType);
                await panel.group.DestroyPanel(panel, waitUntilHidden);
            }
            else
            {
                Debug.LogError($"{panelType} 类型的面板未加载");
            }
        }
        private async UniTask DestroyMultiplePanel(PanelID panelID, bool waitUntilHidden)
        {
            if (multiplePanelDict.TryGetValue(panelID, out BasePanel panel))
            {
                multiplePanelDict.Remove(panelID);
                await panel.group.DestroyPanel(panel, waitUntilHidden);
            }
            else
            {
                Debug.LogError($"未找到类型为 {panelID.Type}，ID为 {panelID} 的多例面板");
            }
        }
        /// <summary>
        /// 获取指定类型的单例面板
        /// </summary>
        public BasePanel Get(UIPanelType panelType)
        {
            if (!panelConfig.config.TryGetValue(panelType, out PanelData data))
            {
                Debug.LogError($"找不到 {panelType} 类型的配置文件，请在panelConfig字段中配置");
                return null;
            }

            if (data.panelMode == UIPanelMode.Single)
            {
                if (singletonPanelDict.TryGetValue(panelType, out BasePanel panel))
                    return panel;
                else
                {
                    Debug.LogError($"{panelType} 类型的面板未加载");
                    return null;
                }
            }
            else if (data.panelMode == UIPanelMode.Multiple)
            {
                Debug.LogError($"{panelType} 类型为多例面板，请用ID获取");
                return null;
            }
            else
            {
                Debug.LogError($"面板模式 {data.panelMode} 未知");
                return null;
            }
        }
        /// <summary>
        /// 获取指定ID的面板（单例或多例）
        /// </summary>
        public BasePanel Get(PanelID panelID)
        {
            if (!panelConfig.config.TryGetValue(panelID.Type, out PanelData data))
            {
                Debug.LogError($"找不到 {panelID.Type} 类型的配置文件，请在panelConfig字段中配置");
                return null;
            }

            if (data.panelMode == UIPanelMode.Single)
            {
                return Get(panelID.Type);
            }
            else if (data.panelMode == UIPanelMode.Multiple)
            {
                if (multiplePanelDict.TryGetValue(panelID, out BasePanel panel))
                {
                    return panel;
                }
                else
                {
                    Debug.LogError($"找不到类型为 {panelID.Type}，ID为 {panelID} 的多例面板");
                    return null;
                }
            }
            else
            {
                Debug.LogError($"面板模式 {data.panelMode} 未知");
                return null;
            }
        }
        /// <summary>
        /// 尝试获取指定类型的单例面板
        /// </summary>
        public bool TryGet(UIPanelType panelType, out BasePanel panel)
        {
            panel = null;
            if (!panelConfig.config.TryGetValue(panelType, out PanelData data))
            {
                Debug.LogError($"找不到 {panelType} 类型的配置文件，请在panelConfig字段中配置");
                return false;
            }

            if (data.panelMode == UIPanelMode.Single)
            {
                return singletonPanelDict.TryGetValue(panelType, out panel);
            }
            else if (data.panelMode == UIPanelMode.Multiple)
            {
                Debug.LogError($"{panelType} 类型为多例面板，请用ID获取");
                return false;
            }
            else
            {
                Debug.LogError($"面板模式 {data.panelMode} 未知");
                return false;
            }
        }
        /// <summary>
        /// 尝试获取指定ID的面板（单例或多例）
        /// </summary>
        public bool TryGet(PanelID panelID, out BasePanel panel)
        {
            panel = null;
            if (!panelConfig.config.TryGetValue(panelID.Type, out PanelData data))
            {
                Debug.LogError($"找不到 {panelID.Type} 类型的配置文件，请在panelConfig字段中配置");
                return false;
            }

            if (data.panelMode == UIPanelMode.Single)
            {
                return TryGet(panelID.Type, out panel);
            }
            else if (data.panelMode == UIPanelMode.Multiple)
            {
                return multiplePanelDict.TryGetValue(panelID, out panel);
            }
            else
            {
                Debug.LogError($"面板模式 {data.panelMode} 未知");
                return false;
            }
        }
        /// <summary>
        /// 获取指定面板组中顶层的可见面板
        /// </summary>
        public BasePanel GetTop(UIPanelGroup panelGroup) => panelGroup.GetTopPanel();
        /// <summary>
        /// 手动指定面板ID并将面板推入指定面板组的顶层，并可选择是否显示
        /// </summary>
        /// <param name="isShow">是否在推入后调用面板的Show()方法</param>
        /// <param name="isActiveOnPush">是否在创建面板时立刻为active状态</param>
        /// <returns></returns>
        public async UniTask<BasePanel> Push(UIPanelType panelType, UIPanelGroup panelGroup, PanelID panelID, bool isShow = true)
        {
            if (!panelConfig.config.TryGetValue(panelType, out PanelData data))
            {
                Debug.LogError($"找不到 {panelType} 类型的配置文件，请在panelConfig字段中配置");
                return null;
            }

            if (data.panelMode == UIPanelMode.Single)
            {
                if (!singletonPanelDict.TryGetValue(panelType, out BasePanel panel))
                {
                    if (string.IsNullOrEmpty(panelID.InstanceID))
                    {
                        panel = CreatePanel(panelType, panelID);
                    }
                    else
                    {
                        panel = CreatePanel(panelType, new(panelType, ""));
                        Debug.LogWarning($"不应为单例面板指定ID: {panelID.InstanceID}");
                    }
                    if (panel == null)
                        return null;
                    singletonPanelDict.Add(panelType, panel);
                }
                await panelGroup.PushPanel(panel, isShow);
                return panel;
            }
            else if (data.panelMode == UIPanelMode.Multiple)
            {
                if (!multiplePanelDict.TryGetValue(panelID, out BasePanel panel))
                {
                    panel = CreatePanel(panelType, panelID);
                    if (panel == null)
                        return null;
                    multiplePanelDict.Add(panelID, panel);
                }
                await panelGroup.PushPanel(panel, isShow);
                return panel;
            }
            else
            {
                Debug.LogError($"面板模式 {data.panelMode} 未知");
                return null;
            }
        }
        /// <summary>
        /// 将面板推入指定面板组的顶层，并可选择是否显示（自动生成ID）
        /// </summary>
        public async UniTask<BasePanel> Push(UIPanelType panelType, UIPanelGroup panelGroup, bool isShow = true)
        {
            if (!panelConfig.config.TryGetValue(panelType, out PanelData data))
            {
                Debug.LogError($"找不到 {panelType} 类型的配置文件，请在panelConfig字段中配置");
                return null;
            }

            if (data.panelMode == UIPanelMode.Single)
            {
                return await Push(panelType, panelGroup, new(panelType, ""), isShow);
            }
            else if (data.panelMode == UIPanelMode.Multiple)
            {
                return await Push(panelType, panelGroup, new(panelType, Guid.NewGuid().ToString("N")[..8]), isShow);
            }
            else
            {
                Debug.LogError($"面板模式 {data.panelMode} 未知");
                return null;
            }
        }
        /// <summary>
        /// 隐藏指定类型的单例面板
        /// </summary>
        public async UniTask Hide(UIPanelType panelType)
        {
            if (!panelConfig.config.TryGetValue(panelType, out PanelData data))
            {
                Debug.LogError($"找不到 {panelType} 类型的配置文件，请在panelConfig字段中配置");
            }

            if (data.panelMode == UIPanelMode.Single)
            {
                await HideSingletonPanel(panelType);
            }
            else if (data.panelMode == UIPanelMode.Multiple)
            {
                Debug.LogError($"{panelType} 类型为多例面板，请用ID指定面板实例");
            }
            else
            {
                Debug.LogError($"面板模式 {data.panelMode} 未知");
            }
        }
        /// <summary>
        /// 隐藏指定ID的面板
        /// </summary>
        public async UniTask Hide(PanelID panelID)
        {
            if (!panelConfig.config.TryGetValue(panelID.Type, out PanelData data))
            {
                Debug.LogError($"找不到 {panelID.Type} 类型的配置文件，请在panelConfig字段中配置");
            }

            if (data.panelMode == UIPanelMode.Single)
            {
                await Hide(panelID.Type);
            }
            else if (data.panelMode == UIPanelMode.Multiple)
            {
                await HideMultiplePanel(panelID);
            }
            else
            {
                Debug.LogError($"面板模式 {data.panelMode} 未知");
            }
        }
        /// <summary>
        /// 隐藏指定面板组的顶层可见面板
        /// </summary>
        public async UniTask HideTop(UIPanelGroup panelGroup) => await panelGroup.HideTopPanel();
        /// <summary>
        /// 销毁指定类型的单例面板
        /// </summary>
        /// <param name="waitUntilHidden">是否等待面板隐藏后再销毁</param>
        public async UniTask Destroy(UIPanelType panelType, bool waitUntilHidden)
        {
            if (!panelConfig.config.TryGetValue(panelType, out PanelData data))
            {
                Debug.LogError($"找不到 {panelType} 类型的配置文件，请在panelConfig字段中配置");
            }

            if (data.panelMode == UIPanelMode.Single)
            {
                await DestroySingletonPanel(panelType, waitUntilHidden);
            }
            else if (data.panelMode == UIPanelMode.Multiple)
            {
                Debug.LogError($"{panelType} 类型为多例面板，请用ID指定面板实例");
            }
            else
            {
                Debug.LogError($"面板模式 {data.panelMode} 未知");
            }
        }
        /// <summary>
        /// 销毁指定ID的面板
        /// </summary>
        /// <param name="waitUntilHidden">是否等待面板隐藏后再销毁</param>
        public async UniTask Destroy(PanelID panelID, bool waitUntilHidden)
        {
            if (!panelConfig.config.TryGetValue(panelID.Type, out PanelData data))
            {
                Debug.LogError($"找不到 {panelID.Type} 类型的配置文件，请在panelConfig字段中配置");
            }

            if (data.panelMode == UIPanelMode.Single)
            {
                await DestroySingletonPanel(panelID.Type, waitUntilHidden);
            }
            else if (data.panelMode == UIPanelMode.Multiple)
            {
                await DestroyMultiplePanel(panelID, waitUntilHidden);
            }
            else
            {
                Debug.LogError($"面板模式 {data.panelMode} 未知");
            }
        }
        /// <summary>
        /// 销毁指定面板组的顶层可见面板
        /// </summary>
        /// <param name="waitUntilHidden">是否等待面板隐藏后再销毁</param>
        public async UniTask DestroyTop(UIPanelGroup panelGroup, bool waitUntilHidden) => await panelGroup.DestroyTopPanel(waitUntilHidden);
        #endregion
    }
}
