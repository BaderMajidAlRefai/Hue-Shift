using UnityEngine;
[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class Key : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool disableOnCollect = true;
    public bool IsCollected { get; private set; }
    private SpriteRenderer spriteRenderer;
    private Collider2D keyCollider;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        keyCollider = GetComponent<Collider2D>();
        keyCollider.isTrigger = true;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsCollected) return;
        if (other.CompareTag(playerTag))
        {
            Collect();
        }
    }
    private void Collect()
    {
        IsCollected = true;
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
        if (disableOnCollect && keyCollider != null)
            keyCollider.enabled = false;
        Debug.Log("Key collected!");
    }
    public void ResetKey()
    {
        IsCollected = false;
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;
        if (keyCollider != null)
            keyCollider.enabled = true;
    }
}
