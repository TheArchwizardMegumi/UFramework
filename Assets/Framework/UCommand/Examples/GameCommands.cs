using UnityEngine;

namespace UFramework.Examples
{
    /// <summary>
    /// 对象位置移动命令
    /// </summary>
    public class MoveObjectCommand : Command
    {
        private readonly Transform _transform;
        private readonly Vector3 targetPosition;
        private readonly Vector3 originalPosition;

        public MoveObjectCommand(Transform transform, Vector3 targetPosition)
        {
            _transform = transform;
            this.targetPosition = targetPosition;
            originalPosition = transform.position;
        }

        public override bool CanUndo => true;

        public override void Execute()
        {
            _transform.position = targetPosition;
        }
        public override void Undo()
        {
            _transform.position = originalPosition;
        }
    }

    /// <summary>
    /// 对象旋转命令
    /// </summary>
    public class RotateObjectCommand : Command
    {
        private readonly Transform _transform;
        private readonly Quaternion targetRotation;
        private readonly Quaternion originalRotation;

        public RotateObjectCommand(Transform transform, Quaternion targetRotation)
        {
            this._transform = transform;
            this.targetRotation = targetRotation;
            originalRotation = transform.rotation;
        }

        public override bool CanUndo => true;
        public override void Execute()
        {
            _transform.rotation = targetRotation;
        }
        public override void Undo()
        {
            _transform.rotation = originalRotation;
        }
    }

    /// <summary>
    /// 不可撤销的命令示例
    /// </summary>
    public class IrreversibleCommand : Command
    {
        private readonly string actionName;

        public IrreversibleCommand(string actionName)
        {
            this.actionName = actionName;
        }

        public override bool CanUndo => false;
        public override void Execute()
        {
            Debug.Log($"执行不可撤销的操作: {actionName}");
        }
    }
}
