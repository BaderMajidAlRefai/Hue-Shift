using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    [Header("Colour")]
    [SerializeField] private PlatformColor enemyColor;
    [SerializeField] private PlayerController player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float elevationThreshold = 2f;
    [SerializeField] private LayerMask obstacleMask = 0;

    [Header("Bobble")]
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float bobHeight = 0.2f;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite movingSprite;

    [Header("Player Tag")]
    [SerializeField] private string playerTag = "Player";

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Collider2D enemyCollider;
    private Vector3 startPos;
    private float bobOffset;
    private bool isActive;
    private bool isChasing;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        enemyCollider = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.linearDamping = 5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        startPos = transform.position;
        bobOffset = Random.Range(0f, Mathf.PI * 2f);

        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
    }

    void Update()
    {
        if (player == null) return;

        isActive = enemyColor == PlatformColor.Neutral || player.GetCurrentColor() == enemyColor;

        if (isActive)
        {
            Show();
            if (isChasing)
                SetMovingSprite();
            else
                SetIdleSprite();
            if (enemyColor != PlatformColor.Neutral)
              spriteRenderer.flipX = player.transform.position.x > transform.position.x;
        }
        else
        {
            Hide();
            SetIdleSprite();
        }
    }

    void FixedUpdate()
    {
        if (player == null || !isActive) return;

        bool sameElevation = Mathf.Abs(rb.position.y - player.transform.position.y) < elevationThreshold;
        bool canSeePlayer = HasLineOfSight();

        isChasing = sameElevation && canSeePlayer;

        float targetVelX = isChasing
            ? Mathf.Sign(player.transform.position.x - rb.position.x) * moveSpeed
            : 0f;

        rb.linearVelocity = new Vector2(targetVelX, rb.linearVelocity.y);

        Bobble();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(playerTag))
        {
            RestartScene();
        }
    }

    private void Bobble()
    {
        float targetY = startPos.y + Mathf.Sin((Time.time + bobOffset) * bobSpeed) * bobHeight;
        float yVelocity = (targetY - rb.position.y) / Time.fixedDeltaTime;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, yVelocity);
    }

    private void Show()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        if (enemyCollider != null) enemyCollider.enabled = true;
    }

    private void Hide()
    {
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (enemyCollider != null) enemyCollider.enabled = false;
    }

    private void SetIdleSprite()
    {
        if (idleSprite != null && spriteRenderer.sprite != idleSprite)
            spriteRenderer.sprite = idleSprite;
    }

    private void SetMovingSprite()
    {
        if (movingSprite != null && spriteRenderer.sprite != movingSprite)
            spriteRenderer.sprite = movingSprite;
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, elevationThreshold * 2f, 0f));

        if (player != null)
        {
            bool canSee = HasLineOfSight();
            Gizmos.color = canSee ? Color.green : Color.red;
            Gizmos.DrawLine(transform.position, player.transform.position);
        }
    }

    private bool HasLineOfSight()
    {
        if (player == null) return false;
        Vector2 origin = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 direction = (Vector2)player.transform.position - origin;
        float distance = direction.magnitude;
        RaycastHit2D hit = Physics2D.Raycast(origin, direction.normalized, distance, obstacleMask);
        if (hit.collider == null) return true;
        if (hit.collider == enemyCollider) return true;
        if (hit.collider.gameObject.CompareTag(playerTag)) return true;
        return false;
    }
}
