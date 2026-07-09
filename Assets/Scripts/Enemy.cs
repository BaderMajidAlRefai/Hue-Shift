using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class Enemy : MonoBehaviour
{
    [Header("Colour")]
    [SerializeField] private PlatformColor enemyColor;
    [SerializeField] private PlayerController player;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float elevationThreshold = 2f;

    [Header("Sprites")]
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite movingSprite;

    [Header("Player Tag")]
    [SerializeField] private string playerTag = "Player";

    private SpriteRenderer spriteRenderer;
    private Collider2D enemyCollider;
    private Vector3 startPos;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyCollider = GetComponent<Collider2D>();
        enemyCollider.isTrigger = true;
        startPos = transform.position;

        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
    }

    void Start()
    {
        SetIdleSprite();
    }

    void Update()
    {
        if (player == null) return;

        bool isActive = player.GetCurrentColor() == enemyColor;
        bool sameElevation = Mathf.Abs(transform.position.y - player.transform.position.y) < elevationThreshold;

        if (isActive && sameElevation)
        {
            ChasePlayer();
            SetMovingSprite();
        }
        else
        {
            SetIdleSprite();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            RestartScene();
        }
    }

    private void ChasePlayer()
    {
        float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;

        spriteRenderer.flipX = direction > 0f;
    }

    private void SetIdleSprite()
    {
        if (idleSprite != null)
            spriteRenderer.sprite = idleSprite;
    }

    private void SetMovingSprite()
    {
        if (movingSprite != null)
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
    }
}
