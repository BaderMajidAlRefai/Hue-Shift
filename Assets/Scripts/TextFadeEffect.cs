using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TextFadeEffect : MonoBehaviour
{
    [Header("Fade Timing")]
    [SerializeField] private float fadeInDuration = 0.5f;
    [SerializeField] private float visibleDuration = 2f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    [SerializeField] private bool startVisible = false;
    [SerializeField] private bool loop = false;
    [SerializeField] private float loopDelay = 0f;

    [Header("Optional: Wait for another text to fade out first")]
    [SerializeField] private TextFadeEffect waitBeforeFadeIn;

    private TextMeshProUGUI tmpText;
    private Text uiText;
    private Color originalColor;
    private float timer;
    private bool isFadingIn;
    private bool isFadingOut;
    private bool isWaiting;
    private bool isWaitingForOther;
    private bool hasStarted;
    private float loopTimer;

    void Awake()
    {
        tmpText = GetComponent<TextMeshProUGUI>();
        uiText = GetComponent<Text>();
        originalColor = GetColor();

        if (!startVisible)
            SetAlpha(0f);
        else
            SetAlpha(1f);
    }

    void Update()
    {
        if (waitBeforeFadeIn != null && !isWaitingForOther)
        {
            if (waitBeforeFadeIn.GetAlpha() > 0f)
                return;
            isWaitingForOther = true;
        }

        if (loop && hasStarted)
        {
            loopTimer += Time.deltaTime;
            if (loopTimer < loopDelay) return;
        }

        if (!hasStarted)
            hasStarted = true;

        if (isFadingIn)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Clamp01(timer / fadeInDuration);
            SetAlpha(alpha);

            if (alpha >= 1f)
            {
                isFadingIn = false;
                timer = 0f;
                isWaiting = true;
            }
        }
        else if (isWaiting)
        {
            timer += Time.deltaTime;
            if (timer >= visibleDuration)
            {
                isWaiting = false;
                timer = 0f;
                isFadingOut = true;
            }
        }
        else if (isFadingOut)
        {
            timer += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(timer / fadeOutDuration);
            SetAlpha(alpha);

            if (alpha <= 0f)
            {
                isFadingOut = false;
                timer = 0f;

                if (loop)
                {
                    loopTimer = 0f;
                    isFadingIn = true;
                }
            }
        }
    }

    public void StartFade()
    {
        hasStarted = true;
        isWaitingForOther = false;
        timer = 0f;
        loopTimer = 0f;
        isFadingIn = true;
        isFadingOut = false;
        isWaiting = false;
    }

    public void ResetAndStart()
    {
        SetAlpha(0f);
        StartFade();
    }

    public float GetAlpha()
    {
        if (tmpText != null) return tmpText.color.a;
        if (uiText != null) return uiText.color.a;
        return 1f;
    }

    private void SetAlpha(float alpha)
    {
        if (tmpText != null)
        {
            Color c = tmpText.color;
            c.a = alpha;
            tmpText.color = c;
        }
        else if (uiText != null)
        {
            Color c = uiText.color;
            c.a = alpha;
            uiText.color = c;
        }
    }

    private Color GetColor()
    {
        if (tmpText != null) return tmpText.color;
        if (uiText != null) return uiText.color;
        return Color.white;
    }
}
