using UnityEngine;

namespace UFramework.Examples
{
    public class CommandExample : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;
        
        private void Start()
        {
            // 示例1：基本命令执行
            var moveCommand = new CameraMoveCommand(_mainCamera.transform, new Vector3(5f, 3f, -10f));
            UCommandManager.Instance.Execute(moveCommand);
            
            // 示例2：指定队列执行
            var playerMoveCommand = new CameraMoveCommand(_mainCamera.transform, new Vector3(10f, 5f, -15f));
            UCommandManager.Instance.Execute(playerMoveCommand, "Player");
            
            // 示例3：批量执行
            var command1 = new CameraMoveCommand(_mainCamera.transform, new Vector3(1f, 1f, -10f));
            var command2 = new CameraMoveCommand(_mainCamera.transform, new Vector3(2f, 2f, -10f));
            UCommandManager.Instance.Execute(command1, command2);
            
            // 示例4：撤销
            if (UCommandManager.Instance.CanUndo())
            {
                UCommandManager.Instance.Undo();
            }
            
            // 示例5：撤销指定步数
            UCommandManager.Instance.Undo(2);
            
            // 示例6：重做
            if (UCommandManager.Instance.CanRedo())
            {
                UCommandManager.Instance.Redo();
            }
            
            // 示例7：操作特定队列
            UCommandManager.Instance.Undo("Player");
            UCommandManager.Instance.Redo("Player");
        }
        
        private void Update()
        {
            // 示例：键盘快捷键
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    UCommandManager.Instance.Undo();
                }
            }
            
            if (Input.GetKeyDown(KeyCode.Y))
            {
                Debug.Log("REDO");
                if (Input.GetKey(KeyCode.LeftControl))
                {
                    UCommandManager.Instance.Redo();
                }
            }
        }
    }
}
