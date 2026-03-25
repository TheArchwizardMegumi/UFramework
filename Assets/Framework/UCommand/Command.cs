using UnityEngine;

namespace UFramework
{
    public abstract class Command : ICommand
    {
        public abstract bool CanUndo { get; }

        public abstract void Execute();

        public virtual void Undo()
        {
            if (!CanUndo)
            {
                Debug.LogWarning($"命令 {GetType().Name} 无法撤回.");
            }
        }

        public virtual void Redo()
        {
            Execute();
        }
    }
}
