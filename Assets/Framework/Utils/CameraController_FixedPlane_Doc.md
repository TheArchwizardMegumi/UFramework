# CameraController_FixedPlane 使用文档

## 概述

`CameraController_FixedPlane` 是一个用于追踪拍摄固定平面的透视相机控制器。它支持触屏和鼠标两种输入方式，提供流畅的平移和缩放操作，并带有惯性效果。

## 功能特性

- ✅ 平移操作：在与目标平面平行的方向上移动相机
- ✅ 缩放操作：沿相机朝向靠近/远离目标平面
- ✅ 惯性效果：平移和缩放都支持可配置的惯性阻尼
- ✅ 双输入支持：触屏（单指平移、双指缩放）和鼠标（右键平移、滚轮缩放）
- ✅ 触摸与鼠标互斥：触摸输入优先，自动切换输入方式
- ✅ 点击检测：支持滑动阈值设置，区分点击和拖动操作

## 使用方法

### 1. 基本设置

1. 创建一个透视相机（Perspective）
2. 将 `CameraController_FixedPlane` 脚本挂载到相机上
3. 相机会自动初始化，默认追踪 X-Z 平面（水平面）

### 2. 设置目标平面

```csharp
using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    public CameraController_FixedPlane cameraController;
    
    void Start()
    {
        // 设置目标平面（参数：法向量, 平面上的一个点）
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        cameraController.SetTargetPlane(groundPlane);
        
        // 示例：设置倾斜平面
        Vector3 planeNormal = new Vector3(0, 0.707f, 0.707f);
        Plane tiltedPlane = new Plane(planeNormal, Vector3.zero);
        cameraController.SetTargetPlane(tiltedPlane);
    }
}
```

### 3. 参数配置

在 Inspector 中可调整以下参数：

#### 平移设置
| 参数 | 默认值 | 说明 |
|------|--------|------|
| Pan Speed | 1.0f | 平移速度，1.0 表示滑动屏幕像素比例等于视野移动比例 |
| Pan Damping | 5.0f | 平移阻尼，值越小惯性越大 |
| Pan Threshold | 0.3f | 滑动判定阈值（屏幕百分比），低于此值认为是点击 |

#### 缩放设置
| 参数 | 默认值 | 说明 |
|------|--------|------|
| Zoom Speed | 0.5f | 缩放速度 |
| Zoom Damping | 8.0f | 缩放阻尼，值越小惯性越大 |
| Min Zoom Distance | 5.0f | 最小缩放距离（相机到平面的最小距离） |
| Max Zoom Distance | 50.0f | 最大缩放距离（相机到平面的最大距离） |

## 操作方式

### 触屏操作

| 操作 | 手势 | 功能 |
|------|------|------|
| 平移 | 单指拖动 | 在与目标平面平行的方向移动相机 |
| 缩放 | 双指捏合/张开 | 沿相机朝向靠近/远离目标平面 |

### 鼠标操作

| 操作 | 按键/动作 | 功能 |
|------|-----------|------|
| 平移 | 鼠标右键拖动 | 在与目标平面平行的方向移动相机 |
| 缩放 | 鼠标滚轮 | 沿相机朝向靠近/远离目标平面 |

**注意**：可以通过修改 `KeySet.cs` 中的键位绑定来更改鼠标操作按键。

## 工作原理

### 平移原理

1. 计算相机到目标平面的距离
2. 根据相机视角和距离计算视野范围
3. 将相机的右向量和前向量投影到目标平面
4. 根据屏幕滑动比例在目标平面内移动相机

### 缩放原理

1. 获取相机到目标平面的当前距离
2. 沿相机前向量的反方向移动相机
3. 限制最小/最大距离范围
4. 支持任意角度的相机朝向

## 使用示例

### 场景 1：2.5D 等视角游戏

```csharp
public class IsometricCamera : MonoBehaviour
{
    public CameraController_FixedPlane cameraController;
    public float isometricAngle = 45f;
    
    void Start()
    {
        // 设置水平平面
        cameraController.SetTargetPlane(new Plane(Vector3.up, Vector3.zero));
        
        // 设置等视角旋转
        transform.rotation = Quaternion.Euler(isometricAngle, 45f, 0f);
    }
}
```

### 场景 2：斜视角 RTS 游戏

```csharp
public class RTSCamera : MonoBehaviour
{
    public CameraController_FixedPlane cameraController;
    public float tiltAngle = 30f;
    
    void Start()
    {
        // 设置地面平面
        cameraController.SetTargetPlane(new Plane(Vector3.up, Vector3.zero));
        
        // 设置斜视角
        transform.rotation = Quaternion.Euler(tiltAngle, 0f, 0f);
    }
}
```

### 场景 3：自定义倾斜平面

```csharp
public class CustomPlaneCamera : MonoBehaviour
{
    public CameraController_FixedPlane cameraController;
    public Transform planeAnchor;
    
    void Start()
    {
        // 根据锚点对象设置平面
        Vector3 normal = planeAnchor.up;
        Vector3 point = planeAnchor.position;
        cameraController.SetTargetPlane(new Plane(normal, point));
        
        // 相机正对平面
        transform.LookAt(planeAnchor.position);
    }
}
```

## 自定义键位绑定

如需修改鼠标操作按键，编辑 `Assets/Framework/UInput/Customization/KeySet.cs`：

```csharp
// 在 InitializeKeySet() 方法中
set = new()
{
    // ... 其他键位
    {cameraPan, Mouse1 },      // 改为鼠标左键
};
```

## 注意事项

1. **相机类型**：必须使用透视相机（Perspective），正交相机不适用
2. **平面设置**：确保目标平面的法向量与相机朝向大致相反，以获得最佳缩放效果
3. **距离限制**：`minZoomDistance` 和 `maxZoomDistance` 是相机到平面的距离，不是世界坐标值
4. **输入互斥**：触摸和鼠标输入会互斥，触摸操作优先级更高
5. **惯性调整**：根据游戏节奏调整阻尼参数，动作游戏建议较大阻尼，策略游戏建议较小阻尼

## 故障排除

### 相机无法平移
- 检查是否移动距离超过 `Pan Threshold` 设置值
- 确认相机类型为透视相机

### 缩放效果不符合预期
- 检查目标平面是否正确设置
- 确认相机朝向是否与平面法向量大致相反

### 触摸不响应
- 确认设备支持触屏输入
- 检查 Canvas 是否阻挡了触摸事件

## 依赖项

- Unity 内置组件：Camera
- DOTween（DG.Tweening）：用于相机动画功能
- UFramework.UInput：用于统一的输入管理

## 版本历史

- v1.0 - 初始版本
  - 支持触屏和鼠标输入
  - 支持平移和缩放
  - 支持惯性效果
  - 支持自定义目标平面
