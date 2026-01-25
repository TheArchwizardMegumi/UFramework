using UnityEngine;

/// <summary>
/// 移动 + 冲刺
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TopDownController2D : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private string horizontalAxis = "Horizontal";
    [SerializeField] private string verticalAxis = "Vertical";
    [SerializeField] private string dashButton = "Dash";

    [Header("Move")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Dash")]
    [SerializeField] private float dashSpeedMultiplier = 2.2f;
    [SerializeField] private float dashDuration = 0.18f;
    [SerializeField] private float dashCooldown = 0.35f;

    [Header("Physics")]
    [Tooltip("是否忽略惯性，惯性在 RigidBody 的 Linear Drag 调")]
    [SerializeField] private bool snapVelocity = true;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;

    private Rigidbody2D rb;

    private Vector2 moveInput;
    private Vector2 lastNonZeroMoveDir = Vector2.right;

    // dash state
    private bool dashQueued;
    private bool isDashing;
    private float dashEndTime;
    private float nextDashTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
    }

    private void Update()
    {
        float x = Input.GetAxisRaw(horizontalAxis);
        float y = Input.GetAxisRaw(verticalAxis);
        moveInput = new Vector2(x, y);

        // 记录上一次非零方向（无输入也能按方向冲刺）
        if (moveInput.sqrMagnitude > 0.0001f)
            lastNonZeroMoveDir = moveInput.normalized;

        // 冲刺输入排队，从而避免 Update/fixed 的时序问题
        if (Input.GetButtonDown(dashButton))
            dashQueued = true;

        // 冲刺自动结束
        if (isDashing && Time.time >= dashEndTime)
            isDashing = false;

        if (enableDebugLog)
            Debug.Log($"move={moveInput}, dashing={isDashing}");
    }

    private void FixedUpdate()
    {
        if (dashQueued)
        {
            TryStartDash();
            dashQueued = false;
        }

        ApplyMovement();
    }

    private void TryStartDash()
    {
        if (Time.time < nextDashTime) return;

        isDashing = true;
        dashEndTime = Time.time + dashDuration;
        nextDashTime = Time.time + dashCooldown;
    }

    private void ApplyMovement()
    {
        Vector2 dir = moveInput;

        if (dir.sqrMagnitude > 1f)
            dir.Normalize();

        float speed = moveSpeed;

        // 冲刺速度与方向
        if (isDashing)
        {
            speed *= dashSpeedMultiplier;

            if (dir.sqrMagnitude < 0.0001f)
                dir = lastNonZeroMoveDir;
            else
                dir = dir.normalized;
        }

        Vector2 targetVel = dir * speed;

        if (snapVelocity)
        {
            rb.velocity = targetVel;
        }
        else
        {
            Vector2 delta = targetVel - rb.velocity;
            rb.AddForce(delta, ForceMode2D.Force);
        }
    }
}
