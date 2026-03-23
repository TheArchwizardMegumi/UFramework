# UInput - Unity 输入系统封装

一个将抽象操作与实际输入分离的输入管理系统，支持灵活的按键绑定、轴输入和条件禁用。

---

## ✨ 特性

- 🎯 **抽象操作与实际输入分离**：使用 `KeyName`/`AxisName` 枚举定义操作，而非硬编码 `KeyCode`
- 🔧 **灵活的按键绑定**：支持动态修改和保存按键配置
- 📊 **轴输入支持**：支持连续的轴输入（如扳机力度、鼠标位置）
- 🔄 **轴离散化映射**：将轴输入转换为按键事件（如滚轮切换武器）
- 🚫 **条件禁用系统**：支持基于条件的输入禁用
- 💾 **持久化存储**：自动保存和加载按键配置
- 🎮 **多平台支持**：键盘、鼠标、手柄、触摸

---

## 📦 快速开始

### 1. 配置默认绑定

编辑 `Customization/KeySet.cs`：

```csharp
// 添加新的按键操作
public enum KeyName
{
    up,
    down,
    left,
    right,
    interact,
    cameraPan,
    nextWeapon,  // 新增
    prevWeapon,  // 新增
}

// 添加新的轴操作
public enum AxisName
{
    mouseX,
    mouseY,
    mouseScrollWheel,
    horizontal,
    vertical,
    acceleration,  // 新增
    brake,         // 新增
}

// 初始化按键绑定
public static void InitializeKeySet()
{
    set = new()
    {
        {up, KeyCode.W},
        {down, KeyCode.S},
        {left, KeyCode.A},
        {right, KeyCode.D},
        {interact, KeyCode.E},
        {cameraPan, KeyCode.Mouse2},
    };
}

// 初始化轴绑定
public static void InitializeAxisBindings()
{
    axisBindings = new()
    {
        {mouseX, "Mouse X"},
        {mouseY, "Mouse Y"},
        {mouseScrollWheel, "Mouse ScrollWheel"},
        {horizontal, "Horizontal"},
        {vertical, "Vertical"},
    };
}

// 初始化轴到按键映射
public static void InitializeAxisToKeyMappings()
{
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
}
```

### 2. 在游戏启动时加载绑定

```csharp
void Start()
{
    KeyBinding.LoadKeyBindingFromPlayerPref();
}
```

### 3. 使用抽象操作获取输入

**按键输入（离散）：**
```csharp
if (UInput.GetKey(KeyName.right))
{
    MoveRight();
}

if (UInput.GetKeyDown(KeyName.interact))
{
    Interact();
}

if (UInput.GetKeyUp(KeyName.jump))
{
    Jump();
}
```

**轴输入（连续）：**
```csharp
// 获取平滑处理的轴值
float horizontal = UInput.GetAxis(AxisName.horizontal);
float vertical = UInput.GetAxis(AxisName.vertical);

// 获取原始轴值（无平滑）
float rawValue = UInput.GetAxisRaw(AxisName.mouseX);
```

**轴到按键（离散化）：**
```csharp
// 滚轮向下触发
if (UInput.GetKeyDown(KeyName.nextWeapon))
{
    SwitchToNextWeapon();
}
```

---

## 📚 API 参考

### 按键输入（UInput）

| 方法 | 描述 |
|------|------|
| `GetKey(KeyName operation)` | 按键按住时返回 true |
| `GetKeyDown(KeyName operation)` | 按键按下帧返回 true |
| `GetKeyUp(KeyName operation)` | 按键释放帧返回 true |
| `GetKeyCode(KeyName operation)` | 获取操作对应的 KeyCode |

### 轴输入（UInput）

| 方法 | 描述 |
|------|------|
| `GetAxis(AxisName axisName)` | 获取平滑处理的轴值 |
| `GetAxisRaw(AxisName axisName)` | 获取原始轴值（无平滑） |
| `GetMouseScrollWheel()` | 获取鼠标滚轮值（轴输入的便捷方法） |

### 条件禁用（UInput）

| 方法 | 描述 |
|------|------|
| `DisableKeyInput(KeyName operation, string condition)` | 根据条件禁用按键 |
| `EnableKeyInput(KeyName operation, string condition)` | 根据条件启用按键 |
| `DisableKeyInputs(List<KeyName> operations, string condition)` | 批量禁用按键 |
| `EnableKeyInputs(List<KeyName> operations, string condition)` | 批量启用按键 |
| `DisableAxisInput(AxisName axisName, string condition)` | 根据条件禁用轴 |
| `EnableAxisInput(AxisName axisName, string condition)` | 根据条件启用轴 |

### 按键绑定（KeyBinding）

| 方法 | 描述 |
|------|------|
| `SetKeyBinding(KeyName keyToChange, KeyCode newKey)` | 修改按键绑定 |
| `SetAxisBinding(AxisName axisToChange, string unityAxisName)` | 修改轴绑定 |
| `AddAxisToKeyMapping(...)` | 动态添加轴到按键映射 |
| `RemoveAxisToKeyMapping(AxisName axisName)` | 移除轴到按键映射 |
| `SaveKeyBinding()` | 保存所有绑定 |
| `LoadKeyBindingFromPlayerPref()` | 加载所有绑定 |
| `ResetToDefaults()` | 重置为默认值 |

---

## 🎮 使用场景

### 场景 1：角色移动

```csharp
public class PlayerController : MonoBehaviour
{
    private void Update()
    {
        // 方向移动（支持键盘和手柄）
        float horizontal = UInput.GetAxis(AxisName.horizontal);
        float vertical = UInput.GetAxis(AxisName.vertical);

        Vector3 movement = new Vector3(horizontal, 0, vertical);
        transform.position += movement * speed * Time.deltaTime;

        // 跳跃（按键）
        if (UInput.GetKeyDown(KeyName.jump))
        {
            Jump();
        }
    }
}
```

### 场景 2：武器系统

```csharp
public class WeaponSystem : MonoBehaviour
{
    private void Update()
    {
        // 射击
        if (UInput.GetKey(KeyName.shoot))
        {
            Shoot();
        }

        // 滚轮切换武器
        if (UInput.GetKeyDown(KeyName.nextWeapon))
        {
            SwitchToNextWeapon();
        }

        // 上一把武器
        if (UInput.GetKeyDown(KeyName.prevWeapon))
        {
            SwitchToPrevWeapon();
        }

        // 装弹
        if (UInput.GetKeyDown(KeyName.reload))
        {
            Reload();
        }
    }
}
```

### 场景 3：赛车游戏

```csharp
public class CarController : MonoBehaviour
{
    private void Update()
    {
        // 油门力度（手柄扳机或键盘 W/S）
        float throttle = UInput.GetAxis(AxisName.acceleration);
        ApplyThrottle(throttle);

        // 刹车力度
        float brake = UInput.GetAxis(AxisName.brake);
        ApplyBrake(brake);

        // 转向
        float steering = UInput.GetAxis(AxisName.horizontal);
        Steer(steering);
    }
}
```

### 场景 4：相机控制

```csharp
public class CameraController : MonoBehaviour
{
    private void Update()
    {
        // 鼠标拖动平移
        if (UInput.GetKey(KeyName.cameraPan))
        {
            float mouseX = UInput.GetAxis(AxisName.mouseX);
            float mouseY = UInput.GetAxis(AxisName.mouseY);
            PanCamera(mouseX, mouseY);
        }

        // 滚轮缩放
        float scroll = UInput.GetAxis(AxisName.mouseScrollWheel);
        ZoomCamera(scroll);
    }
}
```

### 场景 5：UI 输入禁用

```csharp
public class PauseMenu : MonoBehaviour
{
    private void OnEnable()
    {
        // 暂停时禁用游戏输入
        UInput.DisableKeyInput(KeyName.shoot, "Pause");
        UInput.DisableKeyInput(KeyName.jump, "Pause");
        UInput.DisableAxisInput(AxisName.mouseScrollWheel, "Pause");
    }

    private void OnDisable()
    {
        // 恢复时重新启用
        UInput.EnableKeyInput(KeyName.shoot, "Pause");
        UInput.EnableKeyInput(KeyName.jump, "Pause");
        UInput.EnableAxisInput(AxisName.mouseScrollWheel, "Pause");
    }
}
```

---

## ⚙️ 配置详解

### KeyName 枚举（按键操作）

定义所有离散按键操作：

```csharp
public enum KeyName
{
    // 移动
    up,
    down,
    left,
    right,

    // 交互
    interact,
    jump,
    shoot,
    reload,

    // 相机
    cameraPan,

    // 武器
    nextWeapon,
    prevWeapon,

    // ... 更多操作
}
```

### AxisName 枚举（轴操作）

定义所有连续轴操作：

```csharp
public enum AxisName
{
    // 鼠标
    mouseX,
    mouseY,
    mouseScrollWheel,

    // 标准轴
    horizontal,
    vertical,

    // 手柄
    acceleration,  // 右扳机
    brake,         // 左扳机
    leftTrigger,   // 左扳机
    rightTrigger,  // 右扳机
}
```

### AxisToKeyMapping（轴到按键映射）

将连续轴输入转换为离散按键事件：

| 参数 | 类型 | 描述 |
|------|------|------|
| `axisName` | AxisName | 要监控的轴 |
| `positiveKey` | KeyName | 轴值 > 阈值时触发的按键 |
| `negativeKey` | KeyName | 轴值 < -阈值时触发的按键 |
| `threshold` | float | 触发阈值（默认 0.1） |
| `resetDelay` | float | 重复触发延迟（秒，默认 0.1） |

---

## 🔄 轴输入 vs 按键输入

| 特性 | 按键输入 | 轴输入 |
|------|---------|--------|
| 数据类型 | bool | float |
| 用途 | 开关事件（射击、跳跃） | 连续控制（移动、油门） |
| 方法 | `GetKey/Down/Up` | `GetAxis/AxisRaw` |
| 绑定 | KeyCode → KeyName | Unity Axis → AxisName |
| 平滑处理 | 无 | GetAxis 平滑，GetAxisRaw 原始 |

---

## 📁 文件结构

```
Assets/Framework/UInput/
├── Customization/
│   └── KeySet.cs                 # 绑定配置（需要修改）
├── Doc_Keybinding.txt             # 旧版文档（已废弃）
├── KeyBinding.cs                 # 绑定保存/加载
├── UInput.cs                     # 输入管理类
├── UInputAxisUpdater.cs          # 轴映射自动更新器
├── README.md                     # 本文档
└── CHANGELOG_v2_Axis_Support.md  # v2.0 更新日志
```

---

## 🐛 注意事项

1. **初始化顺序**：确保在游戏开始时调用 `KeyBinding.LoadKeyBindingFromPlayerPref()`
2. **轴映射更新器**：`UInputAxisUpdater` 会自动创建，无需手动实例化
3. **阈值设置**：轴到按键映射的 `threshold` 应根据输入设备灵敏度调整
4. **重置延迟**：`resetDelay` 影响按键触发频率，过小可能导致重复触发
5. **UI 交互**：使用 `UInput.GetTouch()` 自动检测不在 UI 上的触摸

---

## 🚀 进阶用法

### 自定义轴绑定

```csharp
// 添加自定义轴（在 KeySet.cs 中）
public enum AxisName
{
    // ... 现有轴
    customAxis1,
    customAxis2,
}

// 初始化自定义轴（在 KeySet.cs 中）
public static void InitializeAxisBindings()
{
    axisBindings = new()
    {
        // ... 现有绑定
        {customAxis1, "Custom Axis 1"},  // 在 Input Manager 中配置
        {customAxis2, "Custom Axis 2"},
    };
}
```

### 动态修改映射

```csharp
// 运行时添加新映射
KeyBinding.AddAxisToKeyMapping(
    AxisName.acceleration,
    KeyName.forward,
    KeyName.backward,
    threshold: 0.3f,
    resetDelay: 0.2f
);
```

### 批量禁用

```csharp
List<KeyName> gameplayKeys = new()
{
    KeyName.shoot,
    KeyName.jump,
    KeyName.interact
};

// 暂停时批量禁用
UInput.DisableKeyInputs(gameplayKeys, "Pause");

// 恢复时批量启用
UInput.EnableKeyInputs(gameplayKeys, "Pause");
```

---

## 📖 版本历史

- **v2.0** - 添加轴输入支持
  - 新增 `AxisName` 枚举
  - 新增 `GetAxis/GetAxisRaw` 方法
  - 新增轴到按键映射系统
  - 新增 `UInputAxisUpdater` 组件
  - 扩展 `KeyBinding` 支持轴绑定

- **v1.0** - 初始版本
  - 基本按键绑定
  - 条件禁用系统
  - 持久化存储

---

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

---

## 📄 许可证

本项目采用 MIT 许可证。
