using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UFramework
{
    [RequireComponent(typeof(Camera))]
    public class CameraController_FixedPlane : MonoBehaviour
    {
        [Header("平移设置")]
        [Tooltip("平移速度系数：0.5 = 物体完全跟随指针移动，值越大移动越快")]
        [SerializeField] private float panSpeed = 0.5f;
        
        [Tooltip("平移阻尼值（惯性）：值越大惯性越小，建议范围 3-10")]
        [SerializeField] private float panDamping = 5f;
        
        [Tooltip("点击判定阈值（屏幕百分比）：低于此值的移动被视为点击而非拖动，2% = 约10-20像素")]
        [SerializeField] private float panThreshold = 0.02f;

        [Header("缩放设置")]
        [Tooltip("缩放速度系数：值越大缩放越快，建议范围 0.3-1.0")]
        [SerializeField] private float zoomSpeed = 0.5f;
        
        [Tooltip("缩放阻尼值（惯性）：值越大惯性越小，建议范围 5-15")]
        [SerializeField] private float zoomDamping = 8f;
        
        [Tooltip("最小缩放距离：相机能接近目标平面的最近距离")]
        [SerializeField] private float minZoomDistance = 5f;
        
        [Tooltip("最大缩放距离：相机能远离目标平面的最远距离")]
        [SerializeField] private float maxZoomDistance = 50f;
        
        [Tooltip("使用硬限制：开启后缩放时直接限制范围，关闭后允许越界后自动修正")]
        [SerializeField] private bool useHardLimit = false;
        
        [Tooltip("自动修正力度：越界后自动修正的力度系数，值越大修正越快，建议范围 5-20")]
        [SerializeField] private float clampStiffness = 10f;

        private Camera _camera;
        private Plane _targetPlane;     //正在追踪拍摄的目标平面
        private Vector2 _lastTouchPosition;    // 上一帧单指触摸位置
        private Vector2 _touchStartPos;        // 触摸开始时的位置（用于判断点击还是滑动）
        private Vector2 _oldPosition1;         // 上一帧双指第一指位置
        private Vector2 _oldPosition2;         // 上一帧双指第二指位置
        private Vector2 _mouseStartPos;        // 鼠标按下时的位置（用于判断点击还是拖动）
        private Vector2 _lastMousePosition;    // 上一帧鼠标位置（用于计算每帧移动量）

        // 惯性相关变量
        private Vector3 _panVelocity;          // 平移速度向量
        private float _zoomVelocity;          // 缩放速度
        private bool _isPanning;              // 是否正在平移

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            //设定默认追踪的目标平面(x-z平面)
            SetTargetPlane(new Plane(Vector3.up, Vector3.zero));
        }
        private void Update()
        {
            // 触摸输入优先级高于鼠标输入
            if (Input.touchCount > 0)
            {
                // 有触摸输入时，禁用鼠标输入
                _isPanning = false;

                // 触摸输入处理
                if (Input.touchCount == 1)
                {
                    HandlePan(_targetPlane);
                }
                else if (Input.touchCount == 2)
                {
                    HandleZoom();
                }
                else
                {
                    // 三指及以上触摸，清除状态
                    _isPanning = false;
                }
            }
            else
            {
                // 没有触摸输入时，处理鼠标输入
                HandleMouseInput();
            }

            // 检查并自动修正缩放距离
            CheckAndClampDistance();

            // 应用惯性效果（每帧衰减速度）
            ApplyInertia();
        }

        /// <summary>
        /// 设定要追踪的目标平面
        /// </summary>
        /// <param name="targetPlane"></param>
        public void SetTargetPlane(Plane targetPlane)
        {
            _targetPlane = targetPlane;
        }
        /// <summary>
        /// 处理单指平移（触屏）
        /// </summary>
        private void HandlePan(Plane targetPlane)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                _touchStartPos = touch.position;
                _lastTouchPosition = touch.position;
                _panVelocity = Vector3.zero;  // 触摸开始时清空速度
                _isPanning = true;
            }
            else if (touch.phase == TouchPhase.Moved)
            {
                // 计算屏幕移动量（每帧）
                Vector2 screenDelta = touch.position - _lastTouchPosition;
                float frameMoveDistance = new Vector2(screenDelta.x / Screen.width, screenDelta.y / Screen.height).magnitude;

                // 如果每帧移动量太小，认为是抖动或停顿，更新位置但不移动相机
                if (frameMoveDistance < 0.001f)
                {
                    _lastTouchPosition = touch.position;
                    return;
                }

                // 使用射线检测法：让目标平面上的点完全跟随指针移动
                // 1. 从上一帧屏幕位置发射射线到目标平面，获取起始世界点
                Ray startRay = _camera.ScreenPointToRay(_lastTouchPosition);
                float startEnter;
                if (_targetPlane.Raycast(startRay, out startEnter))
                {
                    Vector3 startPoint = startRay.GetPoint(startEnter);

                    // 2. 从当前屏幕位置发射射线到目标平面，获取目标世界点
                    Ray endRay = _camera.ScreenPointToRay(touch.position);
                    float endEnter;
                    if (_targetPlane.Raycast(endRay, out endEnter))
                    {
                        Vector3 endPoint = endRay.GetPoint(endEnter);

                        // 3. 计算世界空间移动量（平面上的两点距离）
                        Vector3 worldMove = endPoint - startPoint;

                        // 4. 反向移动相机，使起始点跟随指针移动到目标点
                        Vector3 actualMove = -worldMove * panSpeed;
                        transform.Translate(actualMove, Space.World);

                        _panVelocity = actualMove / Time.deltaTime;
                    }
                }

                _lastTouchPosition = touch.position;
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                // 触摸结束时，检查是否是点击
                Vector2 totalDelta = touch.position - _touchStartPos;
                float moveDistance = new Vector2(totalDelta.x / Screen.width, totalDelta.y / Screen.height).magnitude;

                // 如果移动距离小于阈值，认为是点击，清除速度，不应用惯性
                if (moveDistance < panThreshold)
                {
                    _panVelocity = Vector3.zero;
                    _isPanning = false;
                }
            }
        }
        /// <summary>
        /// 处理双指缩放（触屏）
        /// </summary>
        private void HandleZoom()
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began)
            {
                _oldPosition1 = Input.GetTouch(0).position;
                _oldPosition2 = Input.GetTouch(1).position;
                _zoomVelocity = 0f;
                _isPanning = false;  // 双指操作时标记为不在平移，避免被误判为点击
                return;
            }

            if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
            {
                Vector2 tempPosition1 = Input.GetTouch(0).position;
                Vector2 tempPosition2 = Input.GetTouch(1).position;

                float oldDistance = Vector2.Distance(_oldPosition1, _oldPosition2);  // 上一帧双指距离
                float newDistance = Vector2.Distance(tempPosition1, tempPosition2);   // 当前双指距离

                // 防止除零错误
                if (oldDistance < 0.01f)
                {
                    _oldPosition1 = tempPosition1;
                    _oldPosition2 = tempPosition2;
                    return;
                }

                if (Mathf.Abs(oldDistance - newDistance) > 0.01f)
                {
                    // 计算相对变化比例
                    float distanceRatio = newDistance / oldDistance;

                    // 获取当前到目标平面的距离
                    float currentDistance = GetDistanceToPlane(_targetPlane);

                    // 计算缩放量（基于当前距离）
                    float delta = (distanceRatio - 1f) * zoomSpeed * currentDistance;

                    // 沿相机前向量的反方向移动
                    Vector3 zoomDirection = -transform.forward;
                    Vector3 moveVector = zoomDirection * delta;

                    if (useHardLimit)
                    {
                        // 硬限制模式：直接限制移动范围
                        Vector3 newPosition = transform.position + moveVector;
                        float newDistanceToPlane = GetDistanceToPlane(_targetPlane, newPosition);

                        if (newDistanceToPlane >= minZoomDistance && newDistanceToPlane <= maxZoomDistance)
                        {
                            // 在范围内，应用移动
                            transform.position = newPosition;
                            _zoomVelocity = Vector3.Project(moveVector, zoomDirection).magnitude / Time.deltaTime *
                                Mathf.Sign(Vector3.Dot(moveVector, zoomDirection));
                        }
                        else
                        {
                            // 超出范围，限制到边界
                            float clampedDelta = Mathf.Clamp(currentDistance + moveVector.magnitude * Mathf.Sign(Vector3.Dot(moveVector, zoomDirection)),
                                minZoomDistance, maxZoomDistance) - currentDistance;
                            transform.position += zoomDirection * clampedDelta;
                            _zoomVelocity = 0f;
                        }
                    }
                    else
                    {
                        // 自动修正模式：不限制范围，由自动修正机制处理
                        transform.position += moveVector;
                        _zoomVelocity = Vector3.Project(moveVector, zoomDirection).magnitude / Time.deltaTime *
                            Mathf.Sign(Vector3.Dot(moveVector, zoomDirection));
                    }

                    _oldPosition1 = tempPosition1;
                    _oldPosition2 = tempPosition2;
                }
            }
        }
        /// <summary>
        /// 计算相机到目标平面的距离
        /// </summary>
        /// <param name="plane">目标平面</param>
        /// <returns>相机到平面的有符号距离</returns>
        private float GetDistanceToPlane(Plane plane)
        {
            return plane.GetDistanceToPoint(transform.position);
        }
        /// <summary>
        /// 计算指定位置到目标平面的距离
        /// </summary>
        /// <param name="plane">目标平面</param>
        /// <param name="position">世界坐标位置</param>
        /// <returns>指定位置到平面的有符号距离</returns>
        private float GetDistanceToPlane(Plane plane, Vector3 position)
        {
            return plane.GetDistanceToPoint(position);
        }
        /// <summary>
        /// 处理鼠标输入（平移和缩放）
        /// </summary>
        private void HandleMouseInput()
        {
            // 处理鼠标平移
            if (UInput.GetKey(OperationName.cameraPan))
            {
                HandleMousePan(_targetPlane);
            }
            else
            {
                _isPanning = false;
            }

            // 处理鼠标缩放（离散轴输入）
            if (UInput.GetKeyDown(OperationName.cameraZoomIn))
            {
                HandleMouseZoom(1f);
            }
            else if (UInput.GetKeyDown(OperationName.cameraZoomOut))
            {
                HandleMouseZoom(-1f);
            }
        }
        /// <summary>
        /// 处理鼠标平移
        /// </summary>
        private void HandleMousePan(Plane targetPlane)
        {
            // 检测鼠标按下
            if (UInput.GetKeyDown(OperationName.cameraPan))
            {
                _mouseStartPos = Input.mousePosition;
                _lastMousePosition = Input.mousePosition;
                _panVelocity = Vector3.zero;
                _isPanning = true;
                return;
            }

            // 鼠标移动处理
            if (_isPanning)
            {
                Vector2 currentMousePos = Input.mousePosition;

                // 计算屏幕移动量（每帧）
                Vector2 screenDelta = currentMousePos - _lastMousePosition;
                float frameMoveDistance = new Vector2(screenDelta.x / Screen.width, screenDelta.y / Screen.height).magnitude;

                // 如果每帧移动量太小，认为是抖动或停顿，更新位置但不移动相机
                if (frameMoveDistance < 0.001f)
                {
                    _lastMousePosition = currentMousePos;
                    return;
                }

                // 使用射线检测法：让目标平面上的点完全跟随指针移动
                // 1. 从上一帧屏幕位置发射射线到目标平面，获取起始世界点
                Ray startRay = _camera.ScreenPointToRay(_lastMousePosition);
                float startEnter;
                if (_targetPlane.Raycast(startRay, out startEnter))
                {
                    Vector3 startPoint = startRay.GetPoint(startEnter);

                    // 2. 从当前屏幕位置发射射线到目标平面，获取目标世界点
                    Ray endRay = _camera.ScreenPointToRay(currentMousePos);
                    float endEnter;
                    if (_targetPlane.Raycast(endRay, out endEnter))
                    {
                        Vector3 endPoint = endRay.GetPoint(endEnter);

                        // 3. 计算世界空间移动量（平面上的两点距离）
                        Vector3 worldMove = endPoint - startPoint;

                        // 4. 反向移动相机，使起始点跟随指针移动到目标点
                        Vector3 actualMove = -worldMove * panSpeed;
                        transform.Translate(actualMove, Space.World);

                        // 计算瞬时速度（用于惯性）
                        _panVelocity = actualMove / Time.deltaTime;
                    }
                }

                _lastMousePosition = currentMousePos;
            }
        }
        /// <summary>
        /// 处理鼠标滚轮缩放（离散）
        /// </summary>
        /// <param name="direction">缩放方向：1 为放大，-1 为缩小</param>
        private void HandleMouseZoom(float direction)
        {
            // 计算缩放量（固定步长）
            float currentDistance = GetDistanceToPlane(_targetPlane);
            float delta = direction * zoomSpeed * currentDistance * 0.1f;

            // 沿相机前向量的反方向移动
            Vector3 zoomDirection = -transform.forward;
            Vector3 moveVector = zoomDirection * delta;

            if (useHardLimit)
            {
                // 硬限制模式：直接限制移动范围
                Vector3 newPosition = transform.position + moveVector;
                float newDistanceToPlane = GetDistanceToPlane(_targetPlane, newPosition);

                if (newDistanceToPlane >= minZoomDistance && newDistanceToPlane <= maxZoomDistance)
                {
                    // 在范围内，应用移动
                    transform.position = newPosition;
                    _zoomVelocity = Vector3.Project(moveVector, zoomDirection).magnitude / Time.deltaTime *
                        Mathf.Sign(Vector3.Dot(moveVector, zoomDirection));
                }
                else
                {
                    // 超出范围，限制到边界
                    float clampedDelta = Mathf.Clamp(currentDistance + delta, minZoomDistance, maxZoomDistance) - currentDistance;
                    transform.position += zoomDirection * clampedDelta;
                }
            }
            else
            {
                // 自动修正模式：不限制范围，由自动修正机制处理
                transform.position += moveVector;
                _zoomVelocity = Vector3.Project(moveVector, zoomDirection).magnitude / Time.deltaTime *
                    Mathf.Sign(Vector3.Dot(moveVector, zoomDirection));
            }
        }
        /// <summary>
        /// 应用惯性效果
        /// </summary>
        private void ApplyInertia()
        {
            // 平移惯性
            if (_panVelocity.magnitude > 0.001f)
            {
                // 使用阻尼公式衰减速度：velocity *= (1 / (1 + damping * deltaTime))
                _panVelocity *= (1f / (1f + panDamping * Time.deltaTime));

                // 应用惯性移动
                Vector3 inertiaMove = _panVelocity * Time.deltaTime;
                transform.Translate(inertiaMove, Space.World);

                // 速度足够小时停止
                if (_panVelocity.magnitude < 0.001f)
                {
                    _panVelocity = Vector3.zero;
                }
            }

            // 缩放惯性
            if (Mathf.Abs(_zoomVelocity) > 0.001f)
            {
                // 使用阻尼公式衰减速度
                _zoomVelocity *= (1f / (1f + zoomDamping * Time.deltaTime));

                // 沿相机前向量的反方向应用惯性
                Vector3 zoomDirection = -transform.forward;
                Vector3 inertiaMove = _zoomVelocity * Time.deltaTime * zoomDirection;

                if (useHardLimit)
                {
                    // 硬限制模式：限制惯性移动范围
                    Vector3 newPosition = transform.position + inertiaMove;
                    float newDistanceToPlane = GetDistanceToPlane(_targetPlane, newPosition);

                    if (newDistanceToPlane >= minZoomDistance && newDistanceToPlane <= maxZoomDistance)
                    {
                        // 在范围内，应用惯性移动
                        transform.position = newPosition;
                    }
                    else
                    {
                        // 超出范围，停止惯性
                        _zoomVelocity = 0f;
                    }
                }
                else
                {
                    // 自动修正模式：不限制范围，由自动修正机制处理
                    transform.position += inertiaMove;
                }

                // 速度足够小时停止
                if (Mathf.Abs(_zoomVelocity) < 0.001f)
                {
                    _zoomVelocity = 0f;
                }
            }
        }
        /// <summary>
        /// 检查并自动修正相机到目标平面的距离，确保在允许范围内
        /// 使用基于弹簧力的物理修正机制
        /// </summary>
        private void CheckAndClampDistance()
        {
            // 获取当前到目标平面的距离
            float currentDistance = GetDistanceToPlane(_targetPlane);

            // 检查是否超出范围
            if (currentDistance < minZoomDistance || currentDistance > maxZoomDistance)
            {
                // 计算目标距离（边界值）
                float targetDistance = currentDistance < minZoomDistance ? minZoomDistance : maxZoomDistance;

                // 计算超出边界的距离
                float exceedDistance = currentDistance < minZoomDistance
                    ? minZoomDistance - currentDistance
                    : currentDistance - maxZoomDistance;

                // 计算修正力：超出越远，修正力越大
                // 使用弹簧力公式：F = k * x，其中 k 是弹簧系数，x 是超出距离
                float correctionForce = clampStiffness * exceedDistance;

                // 将修正力转换为速度（考虑当前时间步长）
                float correctionSpeed = correctionForce * Time.deltaTime;

                // 沿相机前向量的反方向移动修正
                Vector3 zoomDirection = -transform.forward;
                Vector3 correctionMove = zoomDirection * correctionSpeed;

                // 对于超出上界的情况，需要反向移动
                if (currentDistance > maxZoomDistance)
                {
                    correctionMove = -correctionMove;
                }

                // 应用修正移动
                transform.position += correctionMove;

                // 如果修正后仍在范围内，添加阻尼以避免震荡
                float newDistance = GetDistanceToPlane(_targetPlane);
                if (newDistance >= minZoomDistance && newDistance <= maxZoomDistance)
                {
                    // 在范围内时，添加额外的阻尼效果，使相机平稳停在边界
                    Vector3 toBoundary = zoomDirection * (targetDistance - newDistance);
                    transform.position += toBoundary * 0.5f;
                }
            }
        }
    }
}
