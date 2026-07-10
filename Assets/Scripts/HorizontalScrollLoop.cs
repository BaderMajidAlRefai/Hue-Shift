using UnityEngine;

public class HorizontalScrollLoop : MonoBehaviour
{
    [Header("Sprites (drag your images here)")]
    [SerializeField] private Sprite[] sprites;

    [Header("Scroll")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private bool randomizeSpeed = false;
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 4f;
    [SerializeField] private bool randomizeDirection = false;
    [SerializeField] private int sortOrder = -10;

    private GameObject[] layers;
    private float[] layerSpeeds;
    private float scaledWidth;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        if (cam == null || sprites == null || sprites.Length == 0) return;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        layers = new GameObject[sprites.Length];
        layerSpeeds = new float[sprites.Length];

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null) continue;

            GameObject obj = new GameObject($"Layer_{i}");
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;

            SpriteRenderer sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprites[i];
            sr.sortingOrder = sortOrder;

            float spriteWidth = sprites[i].rect.width / sprites[i].pixelsPerUnit;
            float spriteHeight = sprites[i].rect.height / sprites[i].pixelsPerUnit;
            float scale = camHeight / spriteHeight;
            obj.transform.localScale = new Vector3(scale, scale, 1f);

            if (i == 0) scaledWidth = spriteWidth * scale * 1.1f;

            float spacing = scaledWidth;
            float startX = -spacing * (sprites.Length - 1) / 2f;
            obj.transform.localPosition = new Vector3(startX + i * spacing, 0f, 0f);

            float s = randomizeSpeed ? Random.Range(minSpeed, maxSpeed) : speed;
            if (randomizeDirection && Random.value > 0.5f) s *= -1f;
            layerSpeeds[i] = s;
            layers[i] = obj;
        }
    }

    void Update()
    {
        if (layers == null || cam == null) return;

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;
        float screenHalf = camWidth / 2f + scaledWidth;

        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i] == null) continue;

            layers[i].transform.Translate(Vector3.right * layerSpeeds[i] * Time.deltaTime);

            float x = layers[i].transform.position.x;

            if (layerSpeeds[i] > 0 && x > screenHalf)
            {
                layers[i].transform.position = new Vector3(
                    x - scaledWidth * layers.Length,
                    0f,
                    layers[i].transform.position.z
                );
            }
            else if (layerSpeeds[i] < 0 && x < -screenHalf)
            {
                layers[i].transform.position = new Vector3(
                    x + scaledWidth * layers.Length,
                    0f,
                    layers[i].transform.position.z
                );
            }
        }
    }
}
