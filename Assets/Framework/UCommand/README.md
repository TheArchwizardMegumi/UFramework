# UFramework 命令系统 (UCommand)

## 概述

UCommand 是一个基于命令模式实现的灵活命令管理系统，支持多队列管理、撤销/重做操作等功能。

## 核心组件

### 1. ICommand 接口
所有命令的基础接口：
```csharp
public interface ICommand
{
    bool CanUndo { get; }
    void Execute();
    void Undo();
    void Redo();
}
```

### 2. UCommand 抽象基类
提供了基本的命令实现框架，推荐继承此类创建自定义命令：
```csharp
public abstract class UCommand : ICommand
{
    public abstract bool CanUndo { get; }
    public abstract void Execute();
    public virtual void Undo() { /* 默认实现 */ }
    public virtual void Redo() { Execute(); }
}
```

### 3. UCommandManager
命令管理器，负责命令的执行、撤销、重做和多队列管理。

## 使用方法

### 创建自定义命令

```csharp
public class CameraMoveCommand : UCommand
{
    private readonly Transform cameraTransform;
    private readonly Vector3 targetPosition;
    private Vector3 originalPosition;

    public CameraMoveCommand(Transform cameraTransform, Vector3 targetPosition)
    {
        cameraTransform = cameraTransform;
        targetPosition = targetPosition;
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
```

### 执行命令

```csharp
// 基本执行
var command = new CameraMoveCommand(Camera.main.transform, newPos);
UCommandManager.Instance.Execute(command);

// 指定队列执行
UCommandManager.Instance.Execute(command, "Player");

// 批量执行
var commands = new ICommand[] { cmd1, cmd2, cmd3 };
UCommandManager.Instance.Execute(commands);
```

### 撤销和重做

```csharp
// 撤销
UCommandManager.Instance.Undo();
UCommandManager.Instance.Undo(5); // 撤销5步
UCommandManager.Instance.Undo("Player"); // 撤销指定队列

// 重做
UCommandManager.Instance.Redo();
UCommandManager.Instance.Redo(3); // 重做3步
UCommandManager.Instance.Redo("Player"); // 重做指定队列
```

### 队列管理

```csharp
// 检查状态
bool canUndo = UCommandManager.Instance.CanUndo();
bool canRedo = UCommandManager.Instance.CanRedo();
int undoCount = UCommandManager.Instance.GetUndoCount();
int redoCount = UCommandManager.Instance.GetRedoCount();

// 清空队列
UCommandManager.Instance.ClearQueue("Player");
UCommandManager.Instance.ClearAll(); // 清空所有队列

// 获取所有队列名称
string[] queues = UCommandManager.Instance.GetQueueNames();
```

## 预设队列

系统默认注册了以下队列：
- `Global`: 全局命令队列

## 设计原则

1. **职责分离**: 命令类负责具体逻辑的封装，被操作对象保持纯净
2. **多队列支持**: 不同类别的命令使用不同的队列，互不干扰
3. **撤销/重做**: 所有可撤销的命令都支持撤销和重做操作
4. **灵活性**: 支持不可撤销的命令（设置 `CanUndo = false`）

## 注意事项

- 命令执行后会自动清空重做栈
- 不可撤销的命令不会被加入撤销栈
- 每个队列独立管理自己的撤销/重做状态
- 队列不存在时会自动创建

## 示例场景

查看 `UCommand/Examples/` 目录中的示例代码：
- `CameraMoveCommand.cs`: 相机移动命令示例
- `GameCommands.cs`: 游戏常用命令集合
- `CommandSystemDemo.cs`: 完整的命令系统演示场景
