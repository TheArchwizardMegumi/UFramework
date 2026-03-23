# UInput v2.0 - 轴输入支持更新说明

## 📋 更新概述

本次更新为 UInput 添加了完整的轴输入支持，使其能够处理连续的输入值（如扳机力度、鼠标位置等），同时保持了原有的离散按键输入功能。

---

## 🆕 新增功能

### 1. 轴输入抽象操作（AxisName）

新增了 `AxisName` 枚举，用于定义连续轴输入的抽象操作名称：

```csharp
public enum AxisName
{
    mouseX,              // 鼠标水平移动
    mouseY,              // 鼠标垂直移动
    mouseScrollWheel,    // 鼠标滚轮
    horizontal,          // 水平方向（WASD、方向键、手柄左摇杆）
    vertical,            // 垂直方向
}
```

### 2. 连续轴输入获取

新增了获取连续轴值的方法：

```csharp
// 获取平滑处理的轴值
float value = UInput.GetAxis(AxisName.mouseScrollWheel);

// 获取原始轴值（无平滑）
float rawValue = UInput.GetAxisRaw(AxisName.horizontal);
```

### 3. 轴到按键的映射（离散化）

支持将连续轴输入转换为离散的按键事件：

```csharp
// 默认配置：滚轮向下触发 nextWeapon，滚轮向上触发 prevWeapon
if (UInput.GetKeyDown(KeyName.nextWeapon))
{
    // 切换到下一个武器
}
```

### 4. 轴输入条件禁用

轴输入也支持条件禁用功能：

```csharp
// 禁用滚动轴
UInput.DisableAxisInput(AxisName.mouseScrollWheel, "UIInteraction");

// 启用滚动轴
UInput.EnableAxisInput(AxisName.mouseScrollWheel, "UIInteraction");
```

### 5. 自动轴映射更新器

新增 `UInputAxisUpdater` 组件，自动更新轴到按键的映射状态，无需手动调用。

---

## 📁 新增文件

1. **`UInputAxisUpdater.cs`**
   - 自动更新轴映射状态的组件
   - 使用 `[DefaultExecutionOrder(-1000)]` 确保在其他脚本之前执行
   - 自动实例化，无需手动挂载

---

## 🔧 修改文件

### 1. `KeySet.cs`

**新增内容：**
- `Dictionary<AxisName, string> AllAxisBindings` - 轴绑定字典
- `List<AxisToKeyMapping> AxisToKeyMappings` - 轴到按键映射列表
- `AxisToKeyMapping` 类 - 轴映射配置类
- `AxisName` 枚举 - 轴操作名称
- `KeyName` 枚举新增：`nextWeapon`, `prevWeapon`

**新增方法：**
- `InitializeAxisBindings()` - 初始化轴绑定
- `InitializeAxisToKeyMappings()` - 初始化轴到按键映射

### 2. `UInput.cs`

**新增字段：**
- `Dictionary<AxisName, BoolState<string>> allowAxisInput` - 轴输入禁用状态管理
- `Dictionary<KeyName, bool> axisKeyStates` - 轴映射按键状态缓存
- `Dictionary<KeyName, float> axisKeyResetTimes` - 轴映射按键重置时间

**修改方法：**
- `GetKey()` - 支持轴映射的虚拟按键
- `GetKeyDown()` - 支持轴映射的按键按下事件
- `GetKeyUp()` - 支持轴映射的按键释放事件
- `GetMouseScrollWheel()` - 改用轴系统

**新增方法：**
- `GetAxis(AxisName)` - 获取平滑轴值
- `GetAxisRaw(AxisName)` - 获取原始轴值
- `UpdateAxisMappings()` - 更新轴到按键的映射
- `DisableAxisInput(AxisName, string)` - 禁用轴输入
- `EnableAxisInput(AxisName, string)` - 启用轴输入

### 3. `KeyBinding.cs`

**修改方法：**
- `SaveKeyBinding()` - 保存轴绑定和轴映射
- `LoadKeyBindingFromPlayerPref()` - 加载轴绑定和轴映射

**新增方法：**
- `SetAxisBinding(AxisName, string)` - 设置轴绑定
- `AddAxisToKeyMapping()` - 动态添加轴映射
- `RemoveAxisToKeyMapping()` - 移除轴映射
- `ResetToDefaults()` - 重置所有绑定到默认值

---

## 🎮 使用示例

### 示例 1：赛车游戏 - 扳机控制油门

```csharp
public class CarController : MonoBehaviour
{
    private void Update()
    {
        // 获取油门力度（0-1，来自手柄右扳机）
        float acceleration = UInput.GetAxis(AxisName.acceleration);
        ApplyThrottle(acceleration);

        // 获取刹车力度（0-1，来自手柄左扳机）
        float brake = UInput.GetAxis(AxisName.brake);
        ApplyBrake(brake);
    }
}
```

**配置（在 `KeySet.cs` 中）：**
```csharp
axisBindings = new()
{
    {acceleration, "Joy Axis 8"},    // 手柄右扳机
    {brake, "Joy Axis 7"},          // 手柄左扳机
};
```

### 示例 2：武器系统 - 滚轮切换武器

```csharp
public class WeaponSystem : MonoBehaviour
{
    private void Update()
    {
        // 滚轮向下 → 下一个武器
        if (UInput.GetKeyDown(KeyName.nextWeapon))
        {
            SwitchToNextWeapon();
        }

        // 滚轮向上 → 上一个武器
        if (UInput.GetKeyDown(KeyName.prevWeapon))
        {
            SwitchToPrevWeapon();
        }
    }
}
```

### 示例 3：相机控制 - 连续平滑缩放

```csharp
public class CameraController : MonoBehaviour
{
    private void Update()
    {
        // 使用滚轮连续缩放（平滑处理）
        float scrollValue = UInput.GetAxis(AxisName.mouseScrollWheel);
        ZoomCamera(scrollValue * zoomSpeed);
    }
}
```

### 示例 4：角色移动 - 支持多种输入

```csharp
public class PlayerMovement : MonoBehaviour
{
    private void Update()
    {
        // 自动适配：WASD、方向键、手柄左摇杆
        float horizontal = UInput.GetAxis(AxisName.horizontal);
        float vertical = UInput.GetAxis(AxisName.vertical);

        Move(new Vector2(horizontal, vertical));
    }
}
```

### 示例 5：条件禁用输入

```csharp
public class UIManager : MonoBehaviour
{
    private void OnEnable()
    {
        // UI 打开时禁用游戏输入
        UInput.DisableKeyInput(KeyName.interact, "UIOpen");
        UInput.DisableAxisInput(AxisName.mouseScrollWheel, "UIOpen");
    }

    private void OnDisable()
    {
        // UI 关闭时恢复游戏输入
        UInput.EnableKeyInput(KeyName.interact, "UIOpen");
        UInput.EnableAxisInput(AxisName.mouseScrollWheel, "UIOpen");
    }
}
```

---

## 🔑 绑定配置

### 按键绑定（离散输入）

在 `KeySet.cs` 的 `InitializeKeySet()` 中配置：

```csharp
set = new()
{
    {up, KeyCode.W},
    {down, KeyCode.S},
    {left, KeyCode.A},
    {right, KeyCode.D},
    // ... 更多按键
};
```

### 轴绑定（连续输入）

在 `KeySet.cs` 的 `InitializeAxisBindings()` 中配置：

```csharp
axisBindings = new()
{
    {mouseX, "Mouse X"},
    {mouseY, "Mouse Y"},
    {mouseScrollWheel, "Mouse ScrollWheel"},
    {horizontal, "Horizontal"},
    {vertical, "Vertical"},
    {acceleration, "Joy Axis 8"},  // 手柄右扳机
    {brake, "Joy Axis 7"},          // 手柄左扳机
};
```

### 轴到按键映射（离散化）

在 `KeySet.cs` 的 `InitializeAxisToKeyMappings()` 中配置：

```csharp
axisToKeyMappings = new()
{
    new AxisToKeyMapping
    {
        axisName = AxisName.mouseScrollWheel,
        positiveKey = KeyName.nextWeapon,
        negativeKey = KeyName.prevWeapon,
        threshold = 0.1f,
        resetDelay = 0.1f
    },
};
```

**参数说明：**
- `axisName`: 要监控的轴
- `positiveKey`: 轴值 > 阈值时触发的按键
- `negativeKey`: 轴值 < -阈值时触发的按键
- `threshold`: 触发阈值
- `resetDelay`: 重复触发之间的延迟（秒）

---

## 🚀 动态配置

### 动态修改绑定

```csharp
// 修改轴绑定
KeyBinding.SetAxisBinding(AxisName.mouseScrollWheel, "Custom Scroll");

// 添加新的轴映射
KeyBinding.AddAxisToKeyMapping(
    AxisName.acceleration,
    KeyName.forward,
    KeyName.backward,
    threshold: 0.3f,
    resetDelay: 0.2f
);

// 移除轴映射
KeyBinding.RemoveAxisToKeyMapping(AxisName.mouseScrollWheel);
```

### 保存和加载

```csharp
// 保存所有绑定
KeyBinding.SaveKeyBinding();

// 加载所有绑定
KeyBinding.LoadKeyBindingFromPlayerPref();

// 重置为默认值
KeyBinding.ResetToDefaults();
```

---

## ⚙️ 工作原理

### 1. 轴输入流程

```
Unity Input.GetAxis(axisName)
    ↓
UInput.GetAxis(AxisName.xxx)
    ↓
检查是否被禁用
    ↓
返回轴值（-1 到 1）
```

### 2. 轴到按键映射流程

```
Unity Input.GetAxis(axisName)
    ↓
UInput.UpdateAxisMappings() (每帧调用)
    ↓
判断轴值是否超过阈值
    ↓
更新 axisKeyStates
    ↓
UInput.GetKeyDown(KeyName.xxx) 返回 true
```

---

## 📝 兼容性

### 向后兼容

所有原有的按键输入功能完全保留，无需修改现有代码。

### 迁移指南

如果需要在现有项目中使用轴输入：

1. **引入命名空间**：`using UFramework;`
2. **初始化更新器**：`UInputAxisUpdater.Instance`（可选，自动创建）
3. **使用新方法**：
   - 连续输入：`UInput.GetAxis(AxisName.xxx)`
   - 离散事件：`UInput.GetKeyDown(KeyName.xxx)`

---

## 🐛 已知问题

- 轴映射的 `GetKeyUp` 事件不完全支持，因为轴输入是连续的
- 轴映射的 `GetKey` 返回 true 的时间由 `resetDelay` 控制

---

## 📅 更新计划

- [ ] 支持手柄的更多轴（如左/右扳机、摇杆按下）
- [ ] 支持触摸手势识别（滑动、长按等）
- [ ] 添加输入重映射 UI 编辑器工具
- [ ] 支持多玩家输入

---

## 🤝 贡献

如有问题或建议，请提交 Issue 或 Pull Request。
