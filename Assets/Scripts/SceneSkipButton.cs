using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class SceneSkipButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Scene to Load")]
    [SerializeField] private string sceneName;
    [SerializeField] private int sceneBuildIndex = -1;
    [Header("Visual Feedback")]
    [SerializeField] private Color hoverColor = Color.yellow;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float hoverScale = 1.1f;
    [Header("Audio (Optional)")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;

    private TextMeshProUGUI tmpText;
    private Text uiText;
    private Color originalColor;
    private Vector3 originalScale;
    private AudioSource audioSource;

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        uiText = GetComponent<Text>();
        originalColor = GetTextColor();
        originalScale = transform.localScale;

        // Add BoxCollider2D if not present (needed for raycasting)
        if (GetComponent<Collider2D>() == null)
        {
            BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
            // Auto-size to text bounds
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                col.size = rt.sizeDelta;
            }
        }

        // Add AudioSource for sound effects
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!string.IsNullOrEmpty(sceneName))
        {
            PlaySound(clickSound);
            SceneManager.LoadScene(sceneName);
        }
        else if (sceneBuildIndex >= 0)
        {
            PlaySound(clickSound);
            SceneManager.LoadScene(sceneBuildIndex);
        }
        else
        {
            Debug.LogError($"SceneSkipButton on '{gameObject.name}': No scene specified! Set sceneName or sceneBuildIndex.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetTextColor(hoverColor);
        transform.localScale = originalScale * hoverScale;
        PlaySound(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetTextColor(originalColor);
        transform.localScale = originalScale;
    }

    private Color GetTextColor()
    {
        if (tmpText != null) return tmpText.color;
        if (uiText != null) return uiText.color;
        return Color.white;
    }

    private void SetTextColor(Color color)
    {
        if (tmpText != null) tmpText.color = color;
        else if (uiText != null) uiText.color = color;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
            audioSource.PlayOneShot(clip);
    }
}
