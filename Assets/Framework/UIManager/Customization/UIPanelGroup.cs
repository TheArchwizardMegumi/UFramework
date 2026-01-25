using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace UFramework
{
    public class UIPanelGroup : MonoBehaviour
    {
        private bool needsUpdateOrder = false;
        private readonly List<BasePanel> panels = new();

        private void UpdatePanelOrder()
        {
            if (!needsUpdateOrder)
                return;

            for (int i = 0; i < panels.Count; i++)
            {
                panels[i].transform.SetSiblingIndex(i);
            }
            needsUpdateOrder = false;
        }
        /// <summary>
        /// 获取顶层可见面板
        /// </summary>
        public BasePanel GetTopPanel()
        {
            BasePanel panel;
            for (int i = panels.Count - 1; i >= 0 ; i--)
            {
                panel = panels[i];
                if (panel.gameObject.activeSelf)
                {
                    return panel;
                }
            }
            return null;
        }
        /// <summary>
        /// 将面板推入此面板组的顶层
        /// </summary>
        public async UniTask PushPanel(BasePanel panel, bool isShow)
        {
            if (panels.Contains(panel))
                panels.Remove(panel);
            panels.Add(panel);

            panel.group = this;
            needsUpdateOrder = true;
            UpdatePanelOrder();

            if (isShow)
                await panel.Show();
            if (!panel.gameObject.activeSelf)
            {
                Debug.LogWarning($"面板 {panel} 显示后仍为inactive状态");
            }
        }
        /// <summary>
        /// 隐藏指定面板
        /// </summary>
        public async UniTask HidePanel(BasePanel panel)
        {
            if (panels.Contains(panel))
            {
                await panel.Hide();
                if (panel.gameObject.activeSelf)
                {
                    Debug.LogWarning($"面板 {panel} 隐藏后仍为active状态");
                    panel.gameObject.SetActive(false);
                }
            }
            else
            {
                Debug.LogError($"面板 {panel.ID} 不在此面板组中");
            }
        }
        /// <summary>
        /// 隐藏顶层可见面板
        /// </summary>
        public async UniTask HideTopPanel()
        {
            BasePanel topPanel = GetTopPanel();
            if (topPanel == null)
            {
                return;
            }
            await HidePanel(topPanel);
        }
        /// <summary>
        /// 销毁指定面板
        /// </summary>
        public async UniTask DestroyPanel(BasePanel panel, bool waitUntilHidden)
        {
            if (panels.Contains(panel))
            {
                if (waitUntilHidden)
                {
                    await HidePanel(panel);
                }
                panels.Remove(panel);

                panel.group = null;
                panel.transform.SetParent(null);
                panel.Destroy();
                needsUpdateOrder = true;
                UpdatePanelOrder();
            }
            else
            {
                Debug.LogError($"面板 {panel.ID} 不在此面板组中");
            }
        }
        /// <summary>
        /// 销毁顶层可见面板
        /// </summary>
        public async UniTask DestroyTopPanel(bool waitUntilHidden)
        {
            BasePanel topPanel = GetTopPanel();
            if (topPanel == null)
            {
                return;
            }
            await DestroyPanel(topPanel, waitUntilHidden);
        }
    }
}
