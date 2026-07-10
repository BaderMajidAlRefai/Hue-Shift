using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class ColourChangeScript : MonoBehaviour
{
    [Header("Colour World Manager")]
    [SerializeField] private ColorWorldManager colorWorldManager;

    [Header("Player (for sprite change)")]
    [SerializeField] private PlayerController player;

    [Header("Current active colour")]
    [SerializeField] private PlatformColor currentColor = PlatformColor.Neutral;
    [Header("Sound")]
    private AudioClip switchSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        switchSound = Resources.Load<AudioClip>("Audio/Toggle Glasses");

        if (colorWorldManager != null)
            colorWorldManager.ShowOnly(currentColor);

        if (player != null)
            player.SetColor(currentColor);
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        var gamepad = GetGamepad();
        var joystick = GetJoystick();

        if (keyboard != null)
        {
            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.jKey.wasPressedThisFrame) SelectColour(PlatformColor.Blue);
            if (keyboard.digit2Key.wasPressedThisFrame || keyboard.kKey.wasPressedThisFrame) SelectColour(PlatformColor.Yellow);
            if (keyboard.digit3Key.wasPressedThisFrame || keyboard.lKey.wasPressedThisFrame) SelectColour(PlatformColor.Red);
        }

        if (gamepad != null)
        {
            if (gamepad.yButton.wasPressedThisFrame) SelectColour(PlatformColor.Blue);
            if (gamepad.bButton.wasPressedThisFrame) SelectColour(PlatformColor.Yellow);
            if (gamepad.xButton.wasPressedThisFrame) SelectColour(PlatformColor.Red);
        }

        if (joystick != null)
        {
            ButtonColorPress(joystick, 0, PlatformColor.Blue);
            ButtonColorPress(joystick, 2, PlatformColor.Red);
            ButtonColorPress(joystick, 3, PlatformColor.Yellow);
        }
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

    private void ButtonColorPress(Joystick js, int index, PlatformColor colour)
    {
        int count = 0;
        foreach (var child in js.children)
        {
            if (child is ButtonControl btn && !btn.synthetic)
            {
                if (count == index) { if (btn.wasPressedThisFrame) SelectColour(colour); return; }
                count++;
            }
        }
    }

    public void SelectColour(PlatformColor colour)
    {
        if (colour == currentColor) return;
        currentColor = colour;
        if (switchSound != null && audioSource != null)
            audioSource.PlayOneShot(switchSound, 0.2f);
        if (colorWorldManager != null)
            colorWorldManager.ShowOnly(currentColor);
        if (player != null)
            player.SetColor(currentColor);
    }

    public PlatformColor GetCurrentColour() => currentColor;
}
