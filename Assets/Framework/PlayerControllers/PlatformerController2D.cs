using UnityEngine;

/// <summary>
/// 移动 / 跳跃 / 二段跳
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlatformerController2D : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private string moveAxis = "Horizontal";

    [SerializeField] private string jumpButton = "Jump";

    [Header("Move")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Jump")]
    [SerializeField] private float jumpImpulse = 12f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Skills (Initial State)")]
    [SerializeField] private bool hasJump = true;
    [SerializeField] private bool hasDoubleJump = false;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;

    private Rigidbody2D rb;
    private float moveX;
    private bool jumpQueued;

    private bool isGrounded;
    private bool wasGrounded;
    private int jumpsUsedInAir; // 0=没跳, 1=一跳, 2=二段跳

    public void UnlockJump() => hasJump = true;

    public void LockJump()
    {
        hasJump = false;
        hasDoubleJump = false;
    }

    public void UnlockDoubleJump()
    {
        hasJump = true;
        hasDoubleJump = true;
    }

    public void LockDoubleJump() => hasDoubleJump = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        moveX = Input.GetAxisRaw(moveAxis);

        if (Input.GetButtonDown(jumpButton))
            jumpQueued = true;

        wasGrounded = isGrounded;
        isGrounded = CheckGrounded();

        if (!wasGrounded && isGrounded)
            jumpsUsedInAir = 0;

        if (enableDebugLog)
            Debug.Log($"grounded={isGrounded}, used={jumpsUsedInAir}");
    }

    private void FixedUpdate()
    {
        ApplyHorizontalMove();

        if (jumpQueued)
        {
            TryJump();
            jumpQueued = false;
        }
    }

    private void ApplyHorizontalMove()
    {
        Vector2 vel = rb.velocity;
        vel.x = moveX * moveSpeed;
        rb.velocity = vel;
    }

    private void TryJump()
    {
        if (!hasJump) return;

        int maxJumps = hasDoubleJump ? 2 : 1;

        if (jumpsUsedInAir >= maxJumps)
            return;

        DoJump();
        jumpsUsedInAir++;
    }

    private void DoJump()
    {
        Vector2 vel = rb.velocity;
        vel.y = 0f;
        rb.velocity = vel;

        rb.AddForce(Vector2.up * jumpImpulse, ForceMode2D.Impulse);

        isGrounded = false;
        wasGrounded = false;
    }

    private bool CheckGrounded()
    {
        if (groundCheck == null) return false;

        return Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
#endif
}
