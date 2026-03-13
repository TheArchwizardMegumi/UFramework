using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UFramework
{
    /// <summary>
    /// HexGrid 编辑器，提供 Inspector 按钮功能
    /// </summary>
    [CustomEditor(typeof(HexGrid))]
    public class HexGridEditor : Editor
    {
        // 静态字典，存储所有活跃的 HexGridEditor 实例
        private static readonly Dictionary<HexGrid, HexGridEditor> activeEditors = new();

        private HexGrid grid;
        private int hotControlID;

        #region 调试属性
        private HexCoord debugSelectedCoord;
        private EditorWindow inspectorWindow;
        private UnityEngine.Object selectedGridObject;  // 缓存场景中选中的对象

        /// <summary>
        /// 调试模式是否开启
        /// </summary>
        public bool DebugMode { get; set; }
        /// <summary>
        /// 显示网格线
        /// </summary>
        public bool ShowGridLines { get; set; }

        public event Action<HexCoord> OnCellSelected;

        //网格绘制属性
        private Color gridLineColor = Color.red;
        private readonly float gridLineWidth = 0.05f;
        private Color originCellColor = Color.yellow;
        private Color qAxisPositiveColor = Color.blue;
        private Color rAxisPositiveColor = Color.green;
        #endregion
                                                                                                        
        private void OnEnable()
        {
            grid = (HexGrid)target;
            
            // 将当前实例添加到静态字典
            if (grid != null)
            {
                activeEditors[grid] = this;
            }
        }
        private void OnDisable()
        {
            OnCellSelected = null;
            
            // 从静态字典中移除当前实例
            if (grid != null && activeEditors.ContainsKey(grid))
            {
                activeEditors.Remove(grid);
            }
        }
        public override void OnInspectorGUI()
        {
            // 绘制默认 Inspector
            DrawDefaultInspector();

            GUILayout.Space(10);

            // 绘制调试控制按钮
            EditorGUILayout.LabelField("调试控制", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();

            // 网格显示开关按钮
            if (GUILayout.Button(ShowGridLines ? "隐藏网格" : "显示网格", GUILayout.Height(25)))
            {
                ToggleGridLines();
            }

            // 调试模式开关按钮
            if (GUILayout.Button(DebugMode ? "关闭调试模式" : "开启调试模式", GUILayout.Height(25)))
            {
                ToggleDebugMode();
            }

            //重置到默认网格
            if (GUILayout.Button("重置网格", GUILayout.Height(25)))
            {
                grid.Clear();
                grid.CreateDefaultGrid(grid.GridRadius);
                EditorUtility.SetDirty(grid);
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            // 调试模式提示
            if (DebugMode)
            {
                EditorGUILayout.HelpBox(
                    "调试模式已开启！\n\n" +
                    "点击场景中的任意单元格将：\n" +
                    "• 打印 Hex 坐标\n" +
                    "• 打印世界坐标\n" +
                    "• 显示绿色射线和紫色中心点",
                    MessageType.Info
                );
            }
        }
        public void OnSceneGUI()
        {
            // 获取热控件 ID
            if (Event.current != null)
            {
                hotControlID = GUIUtility.GetControlID(FocusType.Passive);
            }

            if (grid.Cells.Count == 0)
            {
                // 如果网格未初始化，尝试初始化
                if (!Application.isPlaying)
                {
                    grid.Clear();
                    grid.CreateDefaultGrid(grid.GridRadius);
                }
            }

            // 如果处于调试模式则拦截鼠标事件，获取点击的位置等信息
            if (DebugMode && Event.current != null)
            {
                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        if (Event.current.button == 0)
                        {
                            // 阻止默认选择行为
                            GUIUtility.hotControl = hotControlID;
                            HandleDebugClick();
                        }
                        break;

                    case EventType.MouseUp:
                        if (Event.current.button == 0 && GUIUtility.hotControl != 0)
                        {
                            // 释放热控件
                            GUIUtility.hotControl = 0;
                        }
                        break;
                }
            }

            DrawGrid();

            // 调试模式：绘制选中的单元格
            if (DebugMode && grid.Cells.ContainsKey(debugSelectedCoord))
            {
                DrawDebugSelection();
            }
        }

        /// <summary>
        /// 处理调试模式下的点击
        /// </summary>
        private void HandleDebugClick()
        {
            // 使用 HandleUtility 获取 Scene 视图的射线
            Vector2 mousePos = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);

            if (grid.RayPlaneIntersection(ray, out Vector3 intersectionPoint))
            {
                Vector3 clickPosition = intersectionPoint;
                HexCoord clickedCoord = grid.WorldToHex(clickPosition);

                if (grid.Cells.ContainsKey(clickedCoord))
                {
                    debugSelectedCoord = clickedCoord;
                    OnCellSelected?.Invoke(clickedCoord);

                    //消费该点击事件，阻止其它脚本接收
                    Event.current.Use();
                    // 清除选择，阻止 Unity 选中物体
                    Selection.activeObject = null;
                }
            }
        }

        #region 静态方法
        /// <summary>
        /// 获取指定 HexGrid 对应的 HexGridEditor 实例
        /// </summary>
        /// <param name="hexGrid">HexGrid 对象</param>
        /// <returns>对应的 HexGridEditor 实例，如果不存在则返回 null</returns>
        public static HexGridEditor GetActiveEditor(HexGrid hexGrid)
        {
            if (hexGrid == null) return null;
            
            return activeEditors.TryGetValue(hexGrid, out var editor) ? editor : null;
        }

        /// <summary>
        /// 获取所有活跃的 HexGridEditor 实例
        /// </summary>
        /// <returns>所有活跃的 HexGridEditor 实例的只读集合</returns>
        public static IReadOnlyCollection<HexGridEditor> GetAllActiveEditors()
        {
            return activeEditors.Values;
        }
        #endregion

        #region 调试绘制
        public void ToggleGridLines()
        {
            Undo.RecordObject(grid, "Toggle Grid Lines");
            ShowGridLines = !ShowGridLines;
            EditorUtility.SetDirty(grid);
        }
        public void ToggleDebugMode()
        {
            Undo.RecordObject(grid, "Toggle Debug Mode");
            DebugMode = !DebugMode;
            EditorUtility.SetDirty(grid);

            if (DebugMode)
            {
                // 开启调试模式时，缓存当前选中的对象
                selectedGridObject = Selection.activeObject;
            }
            else
            {
                // 关闭调试模式时，重新选中原对象
                if (selectedGridObject != null)
                {
                    Selection.activeObject = selectedGridObject;
                    selectedGridObject = null;
                }
            }

            // 切换 Inspector 窗口锁定状态
            ToggleInspectorLock(DebugMode);
        }
        /// <summary>
        /// 切换 Inspector 窗口锁定状态
        /// </summary>
        private void ToggleInspectorLock(bool shouldLock)
        {
            try
            {
                // 优先使用 mouseOverWindow（因为按钮点击时鼠标一定在窗口上）
                // 如果 mouseOverWindow 为空，再尝试 focusedWindow
                EditorWindow targetWindow = EditorWindow.mouseOverWindow != null ? EditorWindow.mouseOverWindow : EditorWindow.focusedWindow;

                // 检查是否是 Inspector 窗口
                if (targetWindow == null || targetWindow.GetType().Name != "InspectorWindow")
                {
                    Debug.Log("未找到目标Inspector窗口，请手动锁定窗口");
                    return;
                }

                inspectorWindow = targetWindow;

                // 通过反射访问 isLocked 属性
                var isLockedProperty = inspectorWindow.GetType().GetProperty("isLocked");
                if (isLockedProperty != null)
                {
                    if (shouldLock)
                    {
                        isLockedProperty.SetValue(inspectorWindow, true);
                    }
                    else
                    {
                        isLockedProperty.SetValue(inspectorWindow, false);
                    }
                }
                else
                {
                    Debug.LogWarning("无法访问当前窗口的 isLocked 属性，请手动锁定窗口");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"锁定 Inspector 窗口时出错，请手动锁定窗口，message: {e.Message}");
            }
        }
        /// <summary>
        /// 绘制网格
        /// </summary>
        private void DrawGrid()
        {
            if (!ShowGridLines || grid.Cells.Count == 0)
            {
                return;
            }

            // 获取特殊单元格坐标
            HexCoord originCoord = new(0, 0);
            HexCoord qAxisPositiveCoord = originCoord.GetNeighbor(0),
            rAxispositiveCoord = originCoord.GetNeighbor(1);

            foreach (var kvp in grid.Cells)
            {
                HexCoord coord = kvp.Key;
                Vector3 center = grid.HexToWorld(coord);

                // 判断单元格类型并设置颜色
                if (coord == originCoord)
                {
                    Handles.color = originCellColor;  // 原点
                }
                else if (coord == qAxisPositiveCoord)
                {
                    Handles.color = qAxisPositiveColor;  // q轴正方向
                }
                else if (coord == rAxispositiveCoord)
                {
                    Handles.color = rAxisPositiveColor;  // r轴正方向
                }
                else
                {
                    Handles.color = gridLineColor;  // 其他
                }

                DrawHexagon(center);
            }
        }
        /// <summary>
        /// 绘制单个六边形
        /// </summary>
        /// <param name="center">六边形中心</param>
        private void DrawHexagon(Vector3 center)
        {
            Vector3[] corners = GetHexagonCorners(center);

            for (int i = 0; i < 6; i++)
            {
                Vector3 current = corners[i];
                Vector3 next = corners[(i + 1) % 6];
                Handles.DrawLine(current, next, gridLineWidth);
            }
        }
        /// <summary>
        /// 获取六边形的六个顶点
        /// </summary>
        /// <param name="center">六边形中心（世界坐标，已应用所有变换）</param>
        /// <returns>顶点数组</returns>
        private Vector3[] GetHexagonCorners(Vector3 center)
        {
            Vector3[] corners = new Vector3[6];
            float angleStep = 60f;
            float startAngle = grid.FlatTopped ? 0f : 30f;

            // 计算旋转后的偏移方向
            Quaternion gridRotation = Quaternion.Euler(grid.GridRotation);
            Quaternion totalRotation = gridRotation;

            if (grid.FollowTransformRotation)
            {
                totalRotation = grid.transform.rotation * gridRotation;
            }

            // 计算缩放（网格缩放 × 物体缩放）
            Vector3 scale = grid.GridScale;
            if (grid.FollowTransformScale)
            {
                scale = Vector3.Scale(scale, grid.transform.lossyScale);
            }

            for (int i = 0; i < 6; i++)
            {
                float angle = (startAngle + i * angleStep) * Mathf.Deg2Rad;

                // 计算局部坐标系中的顶点相对于六边形中心的偏移
                float x = grid.CellRadius * Mathf.Cos(angle);
                float z = grid.CellRadius * Mathf.Sin(angle);

                // 应用缩放到偏移
                Vector3 offset = new(x, 0, z);
                offset = Vector3.Scale(offset, scale);

                // 应用旋转到偏移
                offset = totalRotation * offset;

                // 应用到中心点
                corners[i] = center + offset;
            }

            return corners;
        }
        /// <summary>
        /// 绘制调试选中的单元格
        /// </summary>
        private void DrawDebugSelection()
        {
            if (!grid.Cells.ContainsKey(debugSelectedCoord))
            {
                return;
            }

            Vector3 center = grid.HexToWorld(debugSelectedCoord);

            // 绘制垂直于网格的短射线
            Vector3 gridNormal = grid.GetGridPlaneNormal();
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(2, center, center + 0.5f * grid.CellRadius * gridNormal);

            // 绘制中心点
            Handles.color = Color.magenta;
            Handles.SphereHandleCap(0, center, Quaternion.identity, 0.1f, EventType.Repaint);
        }
        #endregion
    }
}
