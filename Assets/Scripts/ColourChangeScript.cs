using UnityEngine;
using UnityEngine.InputSystem;

public class ColourChangeScript : MonoBehaviour
{
    [Header("Colour World Manager")]
    [SerializeField] private ColorWorldManager colorWorldManager;

    [Header("Current active colour")]
    [SerializeField] private PlatformColor currentColor = PlatformColor.Blue;

    void Start()
    {
        if (colorWorldManager != null)
            colorWorldManager.ShowOnly(currentColor);
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.digit1Key.wasPressedThisFrame) SelectColour(PlatformColor.Blue);
        if (keyboard.digit2Key.wasPressedThisFrame) SelectColour(PlatformColor.Yellow);
        if (keyboard.digit3Key.wasPressedThisFrame) SelectColour(PlatformColor.Red);
    }

    public void SelectColour(PlatformColor colour)
    {
        currentColor = colour;
        if (colorWorldManager != null)
            colorWorldManager.ShowOnly(currentColor);
    }

    public PlatformColor GetCurrentColour() => currentColor;
}
