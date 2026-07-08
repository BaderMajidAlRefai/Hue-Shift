using UnityEngine;

public class ColorWorldManager : MonoBehaviour
{
    public void ShowOnly(PlatformColor chosenColor)
    {
        var allPlatforms = FindObjectsByType<ColorPlatform>();

        foreach (var platform in allPlatforms)
        {
            bool isChosen = platform.platformColor == chosenColor;

            SpriteRenderer sr = platform.GetComponent<SpriteRenderer>();
            if (sr != null) sr.enabled = isChosen;

            Collider2D col = platform.GetComponent<Collider2D>();
            if (col != null) col.enabled = isChosen;
        }

        Debug.Log($"{chosenColor}");
    }
}
