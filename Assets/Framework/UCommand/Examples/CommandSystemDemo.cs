using UnityEngine;
using UnityEngine.UI;
using UFramework.Examples;

namespace UFramework.Examples
{
    public class CommandSystemDemo : MonoBehaviour
    {
        [Header("演示对象")]
        [SerializeField] private Transform targetObject;
        [SerializeField] private float moveDistance = 2f;
        [SerializeField] private float rotateAngle = 45f;

        [Header("UI 显示")]
        [SerializeField] private Text undoCountText;
        [SerializeField] private Text redoCountText;
        [SerializeField] private Text currentQueueText;

        private string currentQueue = "Global";

        private void Start()
        {
            // 注册不同的命令队列
            UpdateQueueDisplay();
        }

        private void Update()
        {
            HandleInput();
            UpdateUI();
        }

        private void HandleInput()
        {
            // 移动控制
            if (Input.GetKeyDown(KeyCode.W))
            {
                var newPos = targetObject.position + Vector3.forward * moveDistance;
                UCommandManager.Instance.Execute(new MoveObjectCommand(targetObject, newPos), currentQueue);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                var newPos = targetObject.position + Vector3.back * moveDistance;
                UCommandManager.Instance.Execute(new MoveObjectCommand(targetObject, newPos), currentQueue);
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                var newPos = targetObject.position + Vector3.left * moveDistance;
                UCommandManager.Instance.Execute(new MoveObjectCommand(targetObject, newPos), currentQueue);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                var newPos = targetObject.position + Vector3.right * moveDistance;
                UCommandManager.Instance.Execute(new MoveObjectCommand(targetObject, newPos), currentQueue);
            }

            // 旋转控制
            if (Input.GetKeyDown(KeyCode.Q))
            {
                var newRot = targetObject.rotation * Quaternion.Euler(0, -rotateAngle, 0);
                UCommandManager.Instance.Execute(new RotateObjectCommand(targetObject, newRot), currentQueue);
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                var newRot = targetObject.rotation * Quaternion.Euler(0, rotateAngle, 0);
                UCommandManager.Instance.Execute(new RotateObjectCommand(targetObject, newRot), currentQueue);
            }

            // 撤销/重做
            if (Input.GetKeyDown(KeyCode.Z))
            {
                UCommandManager.Instance.Undo(currentQueue);
            }
            if (Input.GetKeyDown(KeyCode.Y))
            {
                UCommandManager.Instance.Redo(currentQueue);
            }

            // 切换队列
            if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchQueue("Global");
            if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchQueue("Player");
            if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchQueue("Enemy");

            // 清空队列
            if (Input.GetKeyDown(KeyCode.C))
            {
                UCommandManager.Instance.ClearQueue(currentQueue);
                Debug.Log($"已清空队列: {currentQueue}");
            }
        }

        private void SwitchQueue(string queueName)
        {
            currentQueue = queueName;
            UpdateQueueDisplay();
            Debug.Log($"切换到队列: {currentQueue}");
        }

        private void UpdateUI()
        {
            if (undoCountText != null)
            {
                undoCountText.text = $"撤销: {UCommandManager.Instance.GetUndoCount(currentQueue)}";
            }
            if (redoCountText != null)
            {
                redoCountText.text = $"重做: {UCommandManager.Instance.GetRedoCount(currentQueue)}";
            }
        }

        private void UpdateQueueDisplay()
        {
            if (currentQueueText != null)
            {
                currentQueueText.text = $"当前队列: {currentQueue}";
            }
        }

        // 公共方法供 UI 按钮调用
        public void OnUndoClick()
        {
            UCommandManager.Instance.Undo(currentQueue);
        }

        public void OnRedoClick()
        {
            UCommandManager.Instance.Redo(currentQueue);
        }

        public void OnClearClick()
        {
            UCommandManager.Instance.ClearQueue(currentQueue);
        }

        public void OnMoveForwardClick()
        {
            var newPos = targetObject.position + Vector3.forward * moveDistance;
            UCommandManager.Instance.Execute(new MoveObjectCommand(targetObject, newPos), currentQueue);
        }

        public void OnMoveBackClick()
        {
            var newPos = targetObject.position + Vector3.back * moveDistance;
            UCommandManager.Instance.Execute(new MoveObjectCommand(targetObject, newPos), currentQueue);
        }

        public void OnRotateLeftClick()
        {
            var newRot = targetObject.rotation * Quaternion.Euler(0, -rotateAngle, 0);
            UCommandManager.Instance.Execute(new RotateObjectCommand(targetObject, newRot), currentQueue);
        }

        public void OnRotateRightClick()
        {
            var newRot = targetObject.rotation * Quaternion.Euler(0, rotateAngle, 0);
            UCommandManager.Instance.Execute(new RotateObjectCommand(targetObject, newRot), currentQueue);
        }

        public void OnSetGlobalQueue() => SwitchQueue("Global");
        public void OnSetPlayerQueue() => SwitchQueue("Player");
        public void OnSetEnemyQueue() => SwitchQueue("Enemy");
    }
}
