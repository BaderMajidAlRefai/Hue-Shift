using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
[RequireComponent(typeof(Collider2D))]
public class Door : MonoBehaviour
{
    [Header("Key Reference")]
    [SerializeField] private Key requiredKey;
    [Header("Scene to Load")]
    [SerializeField] private string sceneName = "NextLevel";
    [SerializeField] private int sceneBuildIndex = -1;
    [Header("Interaction")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private KeyCode legacyInteractKey = KeyCode.E;
    [Header("Prompts (optional)")]
    [SerializeField] private GameObject lockedPrompt;
    [SerializeField] private GameObject unlockedPrompt;
    private bool playerInRange;
    private SpriteRenderer spriteRenderer;
    void Awake()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (lockedPrompt != null) lockedPrompt.SetActive(false);
        if (unlockedPrompt != null) unlockedPrompt.SetActive(false);
    }
    void Update()
    {
        if (!playerInRange) return;
        bool hasKey = requiredKey != null && requiredKey.IsCollected;
        if (lockedPrompt != null)
            lockedPrompt.SetActive(!hasKey);
        if (unlockedPrompt != null)
            unlockedPrompt.SetActive(hasKey);
        if (hasKey && (WasInteractPressed()))
        {
            LoadNextScene();
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            if (lockedPrompt != null) lockedPrompt.SetActive(false);
            if (unlockedPrompt != null) unlockedPrompt.SetActive(false);
        }
    }
    private bool WasInteractPressed()
    {
        Keyboard kb = Keyboard.current;
        if (kb != null)
            return kb.eKey.wasPressedThisFrame;
        return Input.GetKeyDown(legacyInteractKey);
    }
    private void LoadNextScene()
    {
        if (sceneBuildIndex >= 0)
        {
            SceneManager.LoadScene(sceneBuildIndex);
        }
        else if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Door: No scene specified! Set sceneName or sceneBuildIndex.");
        }
    }
    void OnDrawGizmosSelected()
    {
        if (requiredKey != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, requiredKey.transform.position);
        }
    }
}
