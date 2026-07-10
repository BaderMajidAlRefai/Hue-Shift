using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
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
    [SerializeField] private float fallGravityMultiplier = 4f;
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
    [Header("Sounds")]
    [SerializeField] private float walkSoundCooldown = 0.3f;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private float moveInput;
    private bool sprintInput;
    private bool isGrounded;
    private float coyoteTimer;
    private float jumpBufferTimer;
    private float baseGravity;
    private PlatformColor currentColor = PlatformColor.Neutral;
    private PlayerAnimState currentAnimState;
    private bool facingRight = true;
    private enum PlayerAnimState { Idle, Walking, Jumping }
    private AudioSource audioSource;
    private AudioClip walkSound;
    private AudioClip jumpSound;
    private AudioClip landSound;
    private float walkSoundTimer;
    private bool wasGrounded;
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
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        walkSound = Resources.Load<AudioClip>("Audio/step1");
        jumpSound = Resources.Load<AudioClip>("Audio/jumping");
        landSound = Resources.Load<AudioClip>("Audio/landing");
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
        Gamepad gp = GetGamepad();
        Joystick js = GetJoystick();

        float horizontal = 0f;
        bool usingController = false;

        if (gp != null)
        {
            if (gp.leftStick.left.isPressed || gp.dpad.left.isPressed) { horizontal -= 1f; usingController = true; }
            if (gp.leftStick.right.isPressed || gp.dpad.right.isPressed) { horizontal += 1f; usingController = true; }
            if (gp.aButton.wasPressedThisFrame || gp.dpad.down.wasPressedThisFrame)
                jumpBufferTimer = jumpBufferTime;
        }

        if (js != null && !usingController)
        {
            if (js.stick.left.isPressed) { horizontal -= 1f; usingController = true; }
            if (js.stick.right.isPressed) { horizontal += 1f; usingController = true; }
        }

        if (js != null && js.stick.up != null && js.stick.up.wasPressedThisFrame)
            jumpBufferTimer = jumpBufferTime;

        if (gp == null && js == null)
        {
            foreach (var device in InputSystem.devices)
            {
                if (device is Keyboard || device is Gamepad || device is Joystick) continue;
                foreach (var child in device.children)
                {
                    if (child is ButtonControl btn && !btn.synthetic && btn.wasPressedThisFrame)
                    {
                        jumpBufferTimer = jumpBufferTime;
                        goto foundJump;
                    }
                }
            }
            foundJump:;
        }

        if (!usingController)
        {
            foreach (var device in InputSystem.devices)
            {
                if (device is Keyboard) continue;
                if (device is Gamepad || device is Joystick) continue;
                foreach (var child in device.children)
                {
                    if (child is StickControl stick)
                    {
                        Vector2 val = stick.ReadValue();
                        if (Mathf.Abs(val.x) > 0.3f)
                        {
                            horizontal = Mathf.Clamp(val.x, -1f, 1f);
                            usingController = true;
                            break;
                        }
                    }
                }
                if (usingController) break;
            }
        }

        if (!usingController && kb != null)
        {
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  horizontal -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) horizontal += 1f;
        }

        if (kb != null)
        {
            if (kb.spaceKey.wasPressedThisFrame)
                jumpBufferTimer = jumpBufferTime;
            if (kb.rKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        moveInput = horizontal;
        sprintInput = kb != null && kb.shiftKey.isPressed;

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

        if (isGrounded && Mathf.Abs(moveInput) > 0.1f)
        {
            walkSoundTimer -= Time.deltaTime;
            if (walkSoundTimer <= 0f)
            {
                PlaySound(walkSound, 0.8f);
                walkSoundTimer = walkSoundCooldown;
            }
        }
        else
        {
            walkSoundTimer = 0f;
        }

        if (isGrounded && !wasGrounded)
            PlaySound(landSound, 0.4f);

        wasGrounded = isGrounded;
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
            PlaySound(jumpSound, 0.2f);
        }
        if (rb.linearVelocity.y < 0f)
        {
            rb.gravityScale = baseGravity * fallGravityMultiplier;
        }
        else if (rb.linearVelocity.y > 0f && !IsJumpHeld())
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
            PlatformColor.Neutral => neutralSprites,
            PlatformColor.Red     => redSprites,
            PlatformColor.Yellow  => yellowSprites,
            PlatformColor.Blue    => blueSprites,
            _                     => neutralSprites,
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

    private void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip, volume);
    }

    private bool IsJumpHeld()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed) return true;
        Gamepad gp = GetGamepad();
        if (gp != null && gp.aButton.isPressed) return true;
        Joystick js = GetJoystick();
        if (js != null && js.stick.up != null && js.stick.up.isPressed) return true;
        foreach (var device in InputSystem.devices)
        {
            if (device is Keyboard || device is Gamepad || device is Joystick) continue;
            foreach (var child in device.children)
            {
                if (child is ButtonControl btn && !btn.synthetic && btn.isPressed)
                    return true;
            }
        }
        return false;
    }

    private static Gamepad GetGamepad()
    {
        if (Gamepad.current != null) return Gamepad.current;
        foreach (var device in InputSystem.devices)
            if (device is Gamepad gp) return gp;
        return null;
    }

    private static Joystick GetJoystick()
    {
        if (Joystick.current != null) return Joystick.current;
        foreach (var device in InputSystem.devices)
            if (device is Joystick js) return js;
        return null;
    }
}
