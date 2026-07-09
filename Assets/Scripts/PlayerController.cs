using UnityEngine;
using UnityEngine.InputSystem;
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D), typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintMultiplier = 1.6f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float deceleration = 35f;
    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float fallGravityMultiplier = 2.5f;
    [SerializeField] private float lowJumpGravityMultiplier = 3f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer = ~0;
    [Header("Sprites")]
    [SerializeField] private PlayerSpriteSet neutralSprites;
    [SerializeField] private PlayerSpriteSet redSprites;
    [SerializeField] private PlayerSpriteSet blueSprites;
    [SerializeField] private PlayerSpriteSet yellowSprites;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private float moveInput;
    private bool sprintInput;
    private bool isGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float baseGravity;
    private PlatformColor currentColor = PlatformColor.Red;
    private PlayerAnimState currentAnimState;
    private bool facingRight = true;
    private enum PlayerAnimState { Idle, Walking, Jumping }
    [System.Serializable]
    private struct PlayerSpriteSet
    {
        public Sprite idle;
        public Sprite walking;
        public Sprite jumping;
    }
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<Collider2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.gravityScale = 1f;
        rb.linearDamping = 0f;
        baseGravity = rb.gravityScale;
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) box = gameObject.AddComponent<BoxCollider2D>();
        PhysicsMaterial2D noFriction = new PhysicsMaterial2D();
        noFriction.friction = 0f;
        noFriction.bounciness = 0f;
        box.sharedMaterial = noFriction;
        if (groundCheckPoint == null)
        {
            GameObject go = new GameObject("GroundCheck");
            go.transform.parent = transform;
            go.transform.localPosition = Vector3.zero;
            groundCheckPoint = go.transform;
        }
    }
    void Start()
    {
        UpdateSprite();
    }
    void Update()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null)
        {
            float horizontal = 0f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  horizontal -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal += 1f;
            moveInput = horizontal;
            sprintInput = kb.shiftKey.isPressed;
            if (kb.spaceKey.wasPressedThisFrame)
                jumpBufferTimer = jumpBufferTime;
        }
        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;
        if (moveInput > 0.01f)
            facingRight = true;
        else if (moveInput < -0.01f)
            facingRight = false;
        spriteRenderer.flipX = !facingRight;
        PlayerAnimState newState;
        if (!isGrounded)
            newState = PlayerAnimState.Jumping;
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            newState = PlayerAnimState.Walking;
        else
            newState = PlayerAnimState.Idle;
        if (newState != currentAnimState)
        {
            currentAnimState = newState;
            UpdateSprite();
        }
    }
    void FixedUpdate()
    {
        GroundCheck();
        float targetSpeed = moveInput * moveSpeed * (sprintInput ? sprintMultiplier : 1f);
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float newVelX = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, accelRate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(newVelX, rb.linearVelocity.y);
        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }
        if (rb.linearVelocity.y < 0f)
        {
            rb.gravityScale = baseGravity * fallGravityMultiplier;
        }
        else if (rb.linearVelocity.y > 0f && Keyboard.current != null && !Keyboard.current.spaceKey.isPressed)
        {
            rb.gravityScale = baseGravity * lowJumpGravityMultiplier;
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
        if (rb.linearVelocity.y < -25f)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -25f);
    }
    private void GroundCheck()
    {
        if (playerCollider != null && groundCheckPoint != null)
        {
            Vector3 bottomCenter = playerCollider.bounds.center;
            bottomCenter.y = playerCollider.bounds.min.y;
            groundCheckPoint.position = bottomCenter;
        }
        Vector3 checkPos = groundCheckPoint != null
            ? groundCheckPoint.position
            : transform.position;
        isGrounded = Physics2D.OverlapCircle(checkPos, groundCheckRadius, groundLayer);
    }
    public void SetColor(PlatformColor color)
    {
        currentColor = color;
        UpdateSprite();
    }
    private void UpdateSprite()
    {
        PlayerSpriteSet set = currentColor switch
        {
            PlatformColor.Red    => redSprites,
            PlatformColor.Yellow => yellowSprites,
            PlatformColor.Blue   => blueSprites,
            _                    => neutralSprites,
        };
        Sprite sprite = currentAnimState switch
        {
            PlayerAnimState.Walking => set.walking,
            PlayerAnimState.Jumping => set.jumping,
            _                       => set.idle,
        };
        if (sprite != null)
            spriteRenderer.sprite = sprite;
    }
    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
    public bool IsGrounded() => isGrounded;
    public bool IsSprinting() => sprintInput && Mathf.Abs(moveInput) > 0.1f;
    public float GetMoveInput() => moveInput;
    public Rigidbody2D GetRigidbody() => rb;
    public PlatformColor GetCurrentColor() => currentColor;
}
