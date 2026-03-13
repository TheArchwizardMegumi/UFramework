using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static UFramework.KeySet;

namespace UFramework
{
    public static class UInput
    {
        private static readonly Dictionary<KeyName, BoolState<string>> allowInput;
        static UInput()
        {
            allowInput = new Dictionary<KeyName, BoolState<string>>();
            foreach (var key in AllKeys)
            {
                allowInput.Add(key.Key, new BoolState<string>());
            }
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

        /// <summary>
        /// 获取指定操作对应的按键绑定
        /// </summary>
        public static KeyCode GetKeyCode(KeyName operation)
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
        public static bool GetKey(KeyName operation)
        {
            if (!allowInput[operation])
                return false;
            if (AllKeys.ContainsKey(operation))
                return Input.GetKey(AllKeys[operation]);
            else
                Debug.LogError($"按键名称 {operation} 不存在。");
            return false;
        }
        /// <summary>
        /// 当用户释放指定按键的帧期间返回 true
        /// </summary>
        public static bool GetKeyUp(KeyName operation)
        {
            if (!allowInput[operation])
                return false;
            if (AllKeys.ContainsKey(operation))
                return Input.GetKeyUp(AllKeys[operation]);
            else
                Debug.LogError($"按键名称 {operation} 不存在。");
            return false;
        }
        /// <summary>
        /// 当用户开始按下指定按键的帧期间返回 true
        /// </summary>
        public static bool GetKeyDown(KeyName operation)
        {
            if (!allowInput[operation])
                return false;
            if (AllKeys.ContainsKey(operation))
                return Input.GetKeyDown(AllKeys[operation]);
            else
                Debug.LogError($"按键名称 {operation} 不存在。");
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
        public static void DisableKeyInput(KeyName operation, string condition)
        {
            allowInput[operation][condition] = false;
        }
        /// <summary>
        /// 批量根据指定条件禁用指定按键
        /// </summary>
        /// <param name="operations">按键操作列表</param>
        /// <param name="condition">条件名称</param>
        public static void DisableKeyInputs(List<KeyName> operations, string condition)
        {
            foreach (KeyName operation in operations)
            {
                DisableKeyInput(operation, condition);
            }
        }
        /// <summary>
        /// 根据指定条件启用指定按键
        /// </summary>
        /// <param name="operation">按键操作</param>
        /// <param name="condition">条件名称</param>
        public static void EnableKeyInput(KeyName operation, string condition)
        {
            allowInput[operation][condition] = true;
        }
        /// <summary>
        /// 批量根据指定条件启用指定按键
        /// </summary>
        /// <param name="operations">按键操作列表</param>
        /// <param name="condition">条件名称</param>
        public static void EnableKeyInputs(List<KeyName> operations, string condition)
        {
            foreach (KeyName operation in operations)
            {
                EnableKeyInput(operation, condition);
            }
        }
        /// <summary>
        /// 获取鼠标滚轮滚动值（-1 到 1）
        /// </summary>
        public static float GetMouseScrollWheel()
        {
            return Input.GetAxis("Mouse ScrollWheel");
        }
    }
}