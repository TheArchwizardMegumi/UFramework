using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UFramework.OperationName;
using static UFramework.AxisOperationName;
using static UnityEngine.KeyCode;

namespace UFramework
{
    public static class KeySet
    {
        private static Dictionary<OperationName, KeyCode> keyBindings;
        private static Dictionary<AxisOperationName, string> axisBindings;
        private static List<AxisToKeyMapping> axisToKeyMappings;

        /// <summary>
        /// 存储所有按键绑定的字典
        /// </summary>
        public static Dictionary<OperationName, KeyCode> AllKeys
        {
            get
            {
                return keyBindings;
            }
        }
        /// <summary>
        /// 存储所有轴绑定的字典
        /// </summary>
        public static Dictionary<AxisOperationName, string> AllAxisBindings
        {
            get
            {
                return axisBindings;
            }
        }
        /// <summary>
        /// 轴到按键映射列表（将连续轴输入转换为离散按键事件）
        /// </summary>
        public static List<AxisToKeyMapping> AxisToKeyMappings
        {
            get
            {
                if (axisToKeyMappings == null)
                    InitializeAxisToKeyMappings();
                return axisToKeyMappings;
            }
        }

        public static void InitializeKeyBindings()
        {
            keyBindings = new()
            {
                // 初始按键
                {up, W } ,
                {down, S },
                {left, A },
                {right, D },
                {interact, E },
                // 相机按键
                {cameraPan, Mouse1 },
            };
        }
        public static void InitializeAxisBindings()
        {
            axisBindings = new()
            {
                // 鼠标轴
                {mouseX, "Mouse X"},
                {mouseY, "Mouse Y"},
                // 滚轮轴
                {mouseScrollWheel, "Mouse ScrollWheel"},
                // 标准轴
                {horizontal, "Horizontal"},
                {vertical, "Vertical"},

                //其它自定义轴
                //示例：汽车油门力度
            };
        }
        public static void InitializeAxisToKeyMappings()
        {
            axisToKeyMappings = new()
            {
                // 相机缩放（鼠标滚轮）
                // 注：Unity 滚轮向上=负值，向下=正值
                new AxisToKeyMapping
                {
                    axisName = mouseScrollWheel,
                    positiveKey = cameraZoomOut,  
                    negativeKey = cameraZoomIn,   
                    threshold = 0.01f,
                    resetDelay = 0.05f
                },
            };
        }
    }

    /// <summary>
    /// 将连续轴输入转换为离散按键事件的映射配置
    /// </summary>
    [Serializable]
    public class AxisToKeyMapping
    {
        public AxisOperationName axisName;           // 要监听的轴
        public OperationName positiveKey;         // 当轴值大于阈值时触发的按键
        public OperationName negativeKey;         // 当轴值小于负阈值时触发的按键
        public float threshold;             // 触发按键的最小值
        public float resetDelay;            // 连续同向输入时按键可以再次触发的延迟时间（秒）

        [NonSerialized]
        public float lastTriggerTime;        // 上次触发按键的时间
        [NonSerialized]
        public bool wasPositive;             // 上次触发是正向还是负向
    }

    public enum OperationName
    {
        //按键输入
        up,
        down,
        left,
        right,
        interact,
        cameraPan,

        //离散轴输入
        cameraZoomIn,
        cameraZoomOut,
    }

    /// <summary>
    /// 连续轴输入名称
    /// </summary>
    public enum AxisOperationName
    {
        //标准轴，默认绑定到Unity同名轴上
        mouseX,
        mouseY,
        mouseScrollWheel,
        horizontal,
        vertical,

        //自定义轴
        //示例：acceleration(在InitializeAxisBindings中绑定到Unity支持的轴中，手柄扳机模拟轴需在项目中配置)
    }
}