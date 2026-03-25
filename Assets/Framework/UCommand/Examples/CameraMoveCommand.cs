using UnityEngine;

namespace UFramework.Examples
{
    public class CameraMoveCommand : Command
    {
        private readonly Transform cameraTransform;
        private readonly Vector3 targetPosition;
        private Vector3 originalPosition;

        public CameraMoveCommand(Transform cameraTransform, Vector3 targetPosition)
        {
            this.cameraTransform = cameraTransform;
            this.targetPosition = targetPosition;
        }

        public override bool CanUndo => true;

        public override void Execute()
        {
            originalPosition = cameraTransform.position;
            cameraTransform.position = targetPosition;
        }

        public override void Undo()
        {
            cameraTransform.position = originalPosition;
        }
    }
}
