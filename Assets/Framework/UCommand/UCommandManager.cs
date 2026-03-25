using System.Collections.Generic;
using UnityEngine;

namespace UFramework
{
    public class UCommandManager : Singleton<UCommandManager>
    {
        [SerializeField] 
        private int maxHistorySize = 100;
        [SerializeField] 
        private bool enableDebugLog = false;
        private readonly Dictionary<string, CommandQueue> commandQueues = new();

        private const string defaultQueue = "Global";

        protected override void OnInit()
        {
            RegisterQueue("Global");
        }

        private CommandQueue GetQueue(string queueTag)
        {
            if (!commandQueues.TryGetValue(queueTag, out var queue))
            {
                RegisterQueue(queueTag);
                return commandQueues[queueTag];
            }
            return queue;
        }
        private void RegisterQueue(string queueName)
        {
            if (!commandQueues.ContainsKey(queueName))
            {
                commandQueues[queueName] = new CommandQueue();
                
                if (enableDebugLog)
                {
                    Debug.Log($"命令队列 '{queueName}' 已创建.");
                }
            }
        }

        public void Execute(ICommand command, string queueTag = "Global")
        {
            GetQueue(queueTag).Execute(command);
            
            if (enableDebugLog)
            {
                Debug.Log($"正在队列 '{queueTag}' 中执行命令 {command}");
            }
        }
        public void Execute(params ICommand[] commands)
        {
            ExecuteBatch(commands, defaultQueue);
        }
        public void ExecuteBatch(ICommand[] commands, string queueTag = "Global")
        {
            for (int i = 0; i < commands.Length; i++)
            {
                Execute(commands[i], queueTag);
            }
        }
        public void Undo(string queueTag = "Global")
        {
            GetQueue(queueTag).Undo();
            
            if (enableDebugLog)
            {
                Debug.Log($"正在队列 '{queueTag}' 中撤回命令");
            }
        }
        public void Undo(int steps, string queueTag = "Global")
        {
            for (int i = 0; i < steps; i++)
            {
                Undo(queueTag);
            }
        }
        public void Undo(params string[] queueTags)
        {
            for(int i = 0;  i < queueTags.Length; i++)
            {
                Undo(queueTags[i]);
            }
        }
        public void Redo(string queueTag = "Global")
        {
            GetQueue(queueTag).Redo();
            
            if (enableDebugLog)
            {
                Debug.Log($"正在队列 '{queueTag}' 中重做命令");
            }
        }
        public void Redo(int steps, string queueTag = "Global")
        {
            for (int i = 0; i < steps; i++)
            {
                Redo(queueTag);
            }
        }
        public void Redo(params string[] queueTags)
        {
            for (int i = 0; i < queueTags.Length; i++)
            {
                Redo(queueTags[i]);
            }
        }
        public void ClearQueue(string queueTag)
        {
            GetQueue(queueTag).Clear();
            
            if (enableDebugLog)
            {
                Debug.Log($"清除命令队列 '{queueTag}'.");
            }
        }
        public void ClearAll()
        {
            foreach (var queue in commandQueues.Values)
            {
                queue.Clear();
            }
            
            if (enableDebugLog)
            {
                Debug.Log("清除所有命令队列");
            }
        }
        public bool CanUndo(string queueTag = "Global")
        {
            return GetQueue(queueTag).CanUndo;
        }
        public bool CanRedo(string queueTag = "Global")
        {
            return GetQueue(queueTag).CanRedo;
        }
        public int GetUndoCount(string queueTag = "Global")
        {
            return GetQueue(queueTag).UndoCount;
        }
        public int GetRedoCount(string queueTag = "Global")
        {
            return GetQueue(queueTag).RedoCount;
        }
        public string[] GetQueueNames()
        {
            var keys = new string[commandQueues.Count];
            commandQueues.Keys.CopyTo(keys, 0);
            return keys;
        }

        private class CommandQueue
        {
            public Stack<ICommand> undoStack = new();
            public Stack<ICommand> redoStack = new();

            public bool CanUndo => undoStack.Count > 0;
            public bool CanRedo => redoStack.Count > 0;
            public int UndoCount => undoStack.Count;
            public int RedoCount => redoStack.Count;

            public void Execute(ICommand command)
            {
                command.Execute();
                undoStack.Push(command);
                redoStack.Clear();
            }
            public void Undo()
            {
                if (!CanUndo) return;

                var command = undoStack.Pop();
                if (command.CanUndo)
                {
                    command.Undo();
                    redoStack.Push(command);
                }
            }
            public void Redo()
            {
                if (!CanRedo) return;

                var command = redoStack.Pop();
                command.Execute();
                undoStack.Push(command);
            }
            public void Clear()
            {
                undoStack.Clear();
                redoStack.Clear();
            }
        }
    }
}
