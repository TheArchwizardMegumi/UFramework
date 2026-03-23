using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using static UFramework.KeySet;

namespace UFramework
{
    public static class UInput
    {
        private static readonly Dictionary<OperationName, BoolState<string>> allowInput;
        private static readonly Dictionary<AxisOperationName, BoolState<string>> allowAxisInput;

        // 轴到按键的映射状态
        private static readonly Dictionary<OperationName, bool> axisKeyStates;
        private static readonly Dictionary<OperationName, float> axisKeyResetTimes;

        static UInput()
        {
            allowInput = new Dictionary<OperationName, BoolState<string>>();
            axisKeyStates = new Dictionary<OperationName, bool>();
            axisKeyResetTimes = new Dictionary<OperationName, float>();
            allowAxisInput = new Dictionary<AxisOperationName, BoolState<string>>();

            // 初始化默认绑定
            InitializeKeyBindings();
            InitializeAxisBindings();
            InitializeAxisToKeyMappings();

            // 遍历 OperationName 枚举的所有值，初始化所有操作
            foreach (OperationName operation in System.Enum.GetValues(typeof(OperationName)))
            {
                allowInput.Add(operation, new BoolState<string>());
                axisKeyStates[operation] = false;
                axisKeyResetTimes[operation] = 0f;
            }

            // 遍历 AxisOperationName 枚举的所有值，初始化所有轴
            foreach (AxisOperationName axisName in System.Enum.GetValues(typeof(AxisOperationName)))
            {
                allowAxisInput.Add(axisName, new BoolState<string>());
            }
            
            // 加载保存的键位绑定（如果有）
            KeyBinding.LoadKeyBindingFromPlayerPref();
        }

        private static bool IsPointerOverUI()
        {
            // 检查是否有EventSystem
            if (EventSystem.current == null)
                return false;

            // 检查鼠标输入
            if (Input.mousePresent)
            {
                if (EventSystem.current.IsPointerOverGameObject())
                    return true;

                // 使用射线检测
                PointerEventData pointerData = new(EventSystem.current)
                {
                    position = Input.mousePosition
                };

                List<RaycastResult> results = new();
                EventSystem.current.RaycastAll(pointerData, results);

                return results.Count > 0;
            }

            // 检查触摸输入
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);

                    // 只检测活跃触摸（按下或移动中）
                    if (touch.phase != TouchPhase.Began &&
                        touch.phase != TouchPhase.Moved &&
                        touch.phase != TouchPhase.Stationary)
                        continue;

                    // 使用射线检测
                    PointerEventData pointerData = new(EventSystem.current)
                    {
                        position = touch.position
                    };

                    List<RaycastResult> results = new();
                    EventSystem.current.RaycastAll(pointerData, results);

                    if (results.Count > 0)
                        return true;
                }
            }

            return false;
        }

        #region Key Input

        /// <summary>
        /// 获取指定操作对应的按键绑定
        /// </summary>
        public static KeyCode GetKeyCode(OperationName operation)
        {
            if (AllKeys.ContainsKey(operation))
                return AllKeys[operation];
            else
                Debug.LogError($"按键名称 {operation} 不存在。");
            return KeyCode.None;
        }
        /// <summary>
        /// 当用户按住指定按键时返回 true
        /// </summary>
        public static bool GetKey(OperationName operation)
        {
            if (!allowInput[operation])
                return false;

            // 检查物理按键
            if (AllKeys.ContainsKey(operation) && Input.GetKey(AllKeys[operation]))
                return true;

            // 检查轴映射的虚拟按键状态
            if (axisKeyStates.ContainsKey(operation) && axisKeyStates[operation])
                return true;

            return false;
        }
        /// <summary>
        /// 当用户释放指定按键的帧期间返回 true
        /// </summary>
        public static bool GetKeyUp(OperationName operation)
        {
            if (!allowInput[operation])
                return false;

            // 检查物理按键抬起
            if (AllKeys.ContainsKey(operation) && Input.GetKeyUp(AllKeys[operation]))
                return true;

            // 检查轴映射的虚拟按键抬起（连续轴不完全支持）
            if (axisKeyStates.ContainsKey(operation))
            {
                // 如果按键上一帧处于活跃状态但当前帧不活跃，触发抬起事件
                if (!axisKeyStates[operation] && axisKeyResetTimes[operation] == 0f)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 当用户开始按下指定按键的帧期间返回 true
        /// </summary>
        public static bool GetKeyDown(OperationName operation)
        {
            if (!allowInput[operation])
                return false;

            // 检查物理按键按下
            if (AllKeys.ContainsKey(operation) && Input.GetKeyDown(AllKeys[operation]))
                return true;

            // 检查轴映射的虚拟按键按下
            if (axisKeyStates.ContainsKey(operation))
            {
                // 找到对应的映射配置以获取 resetDelay
                var mapping = AxisToKeyMappings.FirstOrDefault(m =>
                    m.positiveKey == operation || m.negativeKey == operation);

                // 使用映射的 resetDelay 作为时间窗口（或者默认 0.05s）
                float keyDownWindow = mapping != null ? Mathf.Min(mapping.resetDelay, 0.05f) : 0.05f;

                // 如果当前处于活跃状态且重置计时器刚刚启动，则触发
                if (axisKeyStates[operation] && axisKeyResetTimes[operation] > Time.time - keyDownWindow)
                {
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// 检测不在UI上的触摸输入
        /// </summary>
        public static bool GetTouch()
        {
            return GetTouch(out _);
        }
        /// <summary>
        /// 检测不在UI上的触摸输入，并输出触摸点射线
        /// </summary>
        /// <param name="touchPointRay">触摸点射线</param>
        public static bool GetTouch(out Ray touchPointRay)
        {
            // 检查触摸且不在UI上
            bool isPressingInput = false;
            touchPointRay = new Ray();

            // 检查触摸输入
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    if (Input.GetTouch(i).phase == TouchPhase.Ended)
                    {
                        isPressingInput = true;
                        break;
                    }
                }
            }

            if (isPressingInput && !IsPointerOverUI())
            {
                //触发点击
                touchPointRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                //UEvent.Broadcast(EventCode.ClickInGame, ray);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 根据指定条件禁用指定按键
        /// </summary>
        /// <param name="operation">按键操作</param>
        /// <param name="condition">条件名称</param>
        public static void DisableKeyInput(OperationName operation, string condition)
        {
            allowInput[operation][condition] = false;
        }
        /// <summary>
        /// 批量根据指定条件禁用指定按键
        /// </summary>
        /// <param name="operations">按键操作列表</param>
        /// <param name="condition">条件名称</param>
        public static void DisableKeyInputs(List<OperationName> operations, string condition)
        {
            foreach (OperationName operation in operations)
            {
                DisableKeyInput(operation, condition);
            }
        }
        /// <summary>
        /// 根据指定条件启用指定按键
        /// </summary>
        /// <param name="operation">按键操作</param>
        /// <param name="condition">条件名称</param>
        public static void EnableKeyInput(OperationName operation, string condition)
        {
            allowInput[operation][condition] = true;
        }
        /// <summary>
        /// 批量根据指定条件启用指定按键
        /// </summary>
        /// <param name="operations">按键操作列表</param>
        /// <param name="condition">条件名称</param>
        public static void EnableKeyInputs(List<OperationName> operations, string condition)
        {
            foreach (OperationName operation in operations)
            {
                EnableKeyInput(operation, condition);
            }
        }

        #endregion

        #region Axis Input

        /// <summary>
        /// 获取指定抽象操作对应的轴输入值（连续值）
        /// </summary>
        public static float GetAxis(AxisOperationName axisName)
        {
            if (!allowAxisInput[axisName])
                return 0f;

            if (AllAxisBindings.ContainsKey(axisName))
                return Input.GetAxis(AllAxisBindings[axisName]);
            else
                Debug.LogError($"轴名称 {axisName} 不存在。");
            return 0f;
        }
        /// <summary>
        /// 获取原始轴输入值（无平滑处理）
        /// </summary>
        public static float GetAxisRaw(AxisOperationName axisName)
        {
            if (!allowAxisInput[axisName])
                return 0f;

            if (AllAxisBindings.ContainsKey(axisName))
                return Input.GetAxisRaw(AllAxisBindings[axisName]);
            else
                Debug.LogError($"轴名称 {axisName} 不存在。");
            return 0f;
        }
        /// <summary>
        /// 更新轴到按键的映射状态（需要在 Update 中调用）
        /// </summary>
        public static void UpdateAxisMappings()
        {
            foreach (var mapping in AxisToKeyMappings)
            {
                float axisValue = GetAxis(mapping.axisName);
                float currentTime = Time.time;

                // 检查正向触发
                if (axisValue > mapping.threshold)
                {
                    if (!mapping.wasPositive || currentTime - mapping.lastTriggerTime >= mapping.resetDelay)
                    {
                        // 触发正向按键
                        axisKeyStates[mapping.positiveKey] = true;
                        axisKeyResetTimes[mapping.positiveKey] = currentTime;
                        mapping.lastTriggerTime = currentTime;
                        mapping.wasPositive = true;
                    }
                }
                // 检查负向触发
                else if (axisValue < -mapping.threshold)
                {
                    if (mapping.wasPositive || currentTime - mapping.lastTriggerTime >= mapping.resetDelay)
                    {
                        // 触发负向按键
                        axisKeyStates[mapping.negativeKey] = true;
                        axisKeyResetTimes[mapping.negativeKey] = currentTime;
                        mapping.lastTriggerTime = currentTime;
                        mapping.wasPositive = false;
                    }
                }
                // 当轴回到中心时重置状态
                else
                {
                    if (currentTime > mapping.lastTriggerTime + mapping.resetDelay)
                    {
                        axisKeyStates[mapping.positiveKey] = false;
                        axisKeyStates[mapping.negativeKey] = false;
                    }
                }
            }

            // 更新重置计时器
            var keysToUpdate = new List<OperationName>(axisKeyResetTimes.Keys);
            foreach (var key in keysToUpdate)
            {
                if (axisKeyResetTimes[key] > 0 && Time.time > axisKeyResetTimes[key] + 0.05f)
                {
                    axisKeyResetTimes[key] = 0f;
                }
            }
        }
        /// <summary>
        /// 根据条件禁用指定轴输入
        /// </summary>
        public static void DisableAxisInput(AxisOperationName axisName, string condition)
        {
            allowAxisInput[axisName][condition] = false;
        }
        /// <summary>
        /// 根据条件启用指定轴输入
        /// </summary>
        public static void EnableAxisInput(AxisOperationName axisName, string condition)
        {
            allowAxisInput[axisName][condition] = true;
        }

        #endregion
    }
}