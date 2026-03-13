using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UFramework
{
    [RequireComponent(typeof(Camera))]
    public class CameraController_FixedPlane : MonoBehaviour
    {
        private Tween _cameraTween;  // 相机动画 Tween

        [Header("平移设置")]
        [SerializeField] private float panSpeed = 1f;           // 平移速度（1.0 = 滑动屏幕像素比例等于视野移动比例）
        [SerializeField] private float panDamping = 5f;         // 平移阻尼（惯性），值越小惯性越大
        [SerializeField] private float panThreshold = 0.3f;     // 滑动判定阈值（屏幕百分比），低于此值认为是点击

        [Header("缩放设置")]
        [SerializeField] private float zoomSpeed = 0.5f;       // 缩放速度
        [SerializeField] private float zoomDamping = 8f;        // 缩放阻尼（惯性），值越小惯性越大
        [SerializeField] private float minZoomDistance = 5f;   // 最小缩放距离
        [SerializeField] private float maxZoomDistance = 50f;  // 最大缩放距离
        [SerializeField] private bool useHardLimit = false;    // 是否使用硬限制（true: 用户输入时直接限制，false: 越界后自动修正）
        [SerializeField] private float clampDuration = 0.15f;  // 自动修正到限制范围的时间（秒）

        private Camera _camera;
        private Plane _targetPlane;     //正在追踪拍摄的目标平面
        private Vector2 _lastTouchPosition;    // 上一帧单指触摸位置
        private Vector2 _touchStartPos;        // 触摸开始时的位置（用于判断点击还是滑动）
        private Vector2 _oldPosition1;         // 上一帧双指第一指位置
        private Vector2 _oldPosition2;         // 上一帧双指第二指位置
        private Vector2 _mouseStartPos;        // 鼠标开始时的位置（用于判断点击还是拖动）

        // 惯性相关变量
        private Vector3 _panVelocity;          // 平移速度向量
        private float _zoomVelocity;          // 缩放速度
        private bool _isPanning;              // 是否正在平移
        private bool _isZooming;               // 是否正在缩放

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            //设定默认追踪的目标平面(x-z平面)
            SetTargetPlane(new Plane(Vector3.up, Vector3.zero));
        }
        private void OnDestroy()
        {
            _cameraTween?.Kill();
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
                    _isZooming = false;
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
                // 计算从触摸开始到现在的总移动距离
                Vector2 totalDelta = touch.position - _touchStartPos;
                float moveDistance = new Vector2(totalDelta.x / Screen.width, totalDelta.y / Screen.height).magnitude;

                // 如果移动距离小于阈值，认为是点击，不移动相机
                if (moveDistance < panThreshold)
                    return;

                Vector2 deltaPosition = touch.position - _lastTouchPosition;  // 计算滑动距离（像素）

                // 计算摄像机实际视野范围（世界坐标）
                float aspect = _camera.aspect;  // 屏幕宽高比
                float halfFOVTan = Mathf.Tan((_camera.fieldOfView * 0.5f) * Mathf.Deg2Rad);  // 视野半角正切值
                float distanceToPlane = GetDistanceToPlane(targetPlane);  // 相机到目标平面的实际距离
                float height = distanceToPlane * halfFOVTan * 2;  // 视野高度
                float width = height * aspect;  // 视野宽度

                // 获取目标平面的法向量和相机的右向量
                Vector3 planeNormal = targetPlane.normal;
                Vector3 cameraRight = transform.right;

                // 将相机的右向量投影到目标平面
                Vector3 rightOnPlane = Vector3.ProjectOnPlane(cameraRight, planeNormal).normalized;

                // 计算相机的上向量（垂直于右向量和目标平面法向量）
                Vector3 upOnPlane = Vector3.Cross(planeNormal, rightOnPlane).normalized;

                // 根据屏幕滑动比例和视野范围计算移动方向
                float horizontalAmount = -deltaPosition.x / Screen.width * width;  // 水平移动量
                float verticalAmount = -deltaPosition.y / Screen.height * height;   // 垂直移动量

                // 使用投影到目标平面的向量计算移动方向
                Vector3 moveDirection = rightOnPlane * horizontalAmount + upOnPlane * verticalAmount;

                // 直接移动相机并记录速度（用于惯性）
                Vector3 actualMove = moveDirection * panSpeed;
                transform.Translate(actualMove, Space.World);

                _panVelocity = actualMove / Time.deltaTime;  // 记录瞬时速度

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
                _isZooming = true;
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
            if (UInput.GetKey(KeyName.cameraPan))
            {
                HandleMousePan(_targetPlane);
            }
            else
            {
                _isPanning = false;
            }

            // 处理鼠标缩放（滚轮）
            float scroll = UInput.GetMouseScrollWheel();
            if (Mathf.Abs(scroll) > 0.01f)
            {
                HandleMouseZoom(-scroll);
            }
        }
        /// <summary>
        /// 处理鼠标平移
        /// </summary>
        private void HandleMousePan(Plane targetPlane)
        {
            // 检测鼠标按下
            if (Input.GetKeyDown(KeyCode.Mouse2))
            {
                _mouseStartPos = Input.mousePosition;
                _panVelocity = Vector3.zero;
                _isPanning = true;
                return;
            }

            // 鼠标移动处理
            if (_isPanning)
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                // 鼠标移动量太小时不处理
                if (Mathf.Abs(mouseX) < 0.001f && Mathf.Abs(mouseY) < 0.001f)
                    return;

                // 计算摄像机实际视野范围
                float aspect = _camera.aspect;
                float halfFOVTan = Mathf.Tan((_camera.fieldOfView * 0.5f) * Mathf.Deg2Rad);
                float distanceToPlane = GetDistanceToPlane(targetPlane);
                float height = distanceToPlane * halfFOVTan * 2;
                float width = height * aspect;

                // 获取目标平面的法向量和相机的右向量
                Vector3 planeNormal = targetPlane.normal;
                Vector3 cameraRight = transform.right;

                // 将相机的右向量投影到目标平面
                Vector3 rightOnPlane = Vector3.ProjectOnPlane(cameraRight, planeNormal).normalized;

                // 计算相机的上向量（垂直于右向量和目标平面法向量）
                Vector3 upOnPlane = Vector3.Cross(planeNormal, rightOnPlane).normalized;

                // 计算移动量
                float horizontalAmount = -mouseX * panSpeed * width * 0.02f;
                float verticalAmount = -mouseY * panSpeed * height * 0.02f;

                // 使用投影到目标平面的向量计算移动方向
                Vector3 moveDirection = rightOnPlane * horizontalAmount + upOnPlane * verticalAmount;

                // 直接移动相机并记录速度（用于惯性）
                transform.Translate(moveDirection, Space.World);
                _panVelocity = moveDirection / Time.deltaTime;
            }
        }
        /// <summary>
        /// 处理鼠标滚轮缩放
        /// </summary>
        private void HandleMouseZoom(float scroll)
        {
            // 计算缩放量
            float delta = scroll * zoomSpeed * 10f;

            // 沿相机前向量的反方向移动
            Vector3 zoomDirection = -transform.forward;
            Vector3 moveVector = zoomDirection * delta;

            if (useHardLimit)
            {
                // 硬限制模式：直接限制移动范围
                float currentDistance = GetDistanceToPlane(_targetPlane);
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
                }
            }
            else
            {
                // 自动修正模式：不限制范围，由自动修正机制处理
                transform.position += moveVector;
                _zoomVelocity = Vector3.Project(moveVector, zoomDirection).magnitude / Time.deltaTime *
                    Mathf.Sign(Vector3.Dot(moveVector, zoomDirection));
            }

            _isZooming = true;
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
        /// </summary>
        private void CheckAndClampDistance()
        {
            // 获取当前到目标平面的距离
            float currentDistance = GetDistanceToPlane(_targetPlane);

            // 检查是否超出范围
            if (currentDistance < minZoomDistance || currentDistance > maxZoomDistance)
            {
                // 终止之前的修正动画
                _cameraTween?.Kill();

                // 计算目标距离
                float targetDistance = Mathf.Clamp(currentDistance, minZoomDistance, maxZoomDistance);
                float distanceDelta = targetDistance - currentDistance;

                // 沿相机前向量的反方向移动
                Vector3 zoomDirection = -transform.forward;
                Vector3 targetPosition = transform.position + zoomDirection * distanceDelta;

                // 使用 Ease.OutQuad 曲线平滑移动到目标位置
                _cameraTween = transform.DOMove(targetPosition, clampDuration).SetEase(Ease.OutQuad);
            }
        }
    }
}
