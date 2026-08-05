using UnityEngine;

/// <summary>
/// Einsammelbarer Schlüssel mit Puls-, Schwebe-, Dreh- und Leuchteffekt.
/// Der Glow wird vollständig per Code erzeugt und benötigt kein zusätzliches Sprite.
/// </summary>
public sealed class KeyPickup : MonoBehaviour
{
    [Header("Pulse")]
    [SerializeField] private float pulseSpeed = 3.2f;
    [SerializeField] private float minimumScale = 0.82f;
    [SerializeField] private float maximumScale = 1.22f;

    [Header("Floating")]
    [SerializeField] private float floatingSpeed = 2.2f;
    [SerializeField] private float floatingDistance = 0.10f;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 35f;

    [Header("Glow")]
    [SerializeField] private Color glowColor =
        new Color(1f, 0.82f, 0.18f, 1f);

    [SerializeField] private float glowPulseSpeed = 4.5f;
    [SerializeField] private float glowMinimumScale = 1.5f;
    [SerializeField] private float glowMaximumScale = 2.4f;
    [SerializeField] private float glowMinimumAlpha = 0.18f;
    [SerializeField] private float glowMaximumAlpha = 0.65f;

    [Header("Blink")]
    [SerializeField] private float blinkInterval = 1.25f;
    [SerializeField] private float blinkDuration = 0.16f;
    [SerializeField] private float blinkIntensity = 1.35f;

    private static Sprite glowSprite;

    private Vector3 baseScale;
    private Vector3 basePosition;
    private float animationOffset;
    private float blinkTimer;

    private SpriteRenderer keyRenderer;
    private SpriteRenderer glowRenderer;
    private GameObject glowObject;

    public Vector2Int GridPosition
    {
        get;
        private set;
    }

    private void Awake()
    {
        baseScale = transform.localScale;
        basePosition = transform.position;

        animationOffset =
            Random.Range(
                0f,
                Mathf.PI * 2f);

        keyRenderer =
            GetComponent<SpriteRenderer>();

        CreateGlow();

        blinkTimer =
            Random.Range(
                0.2f,
                blinkInterval);
    }

    public void SetGridPosition(
        Vector2Int position)
    {
        GridPosition = position;

        basePosition =
            new Vector3(
                position.x,
                position.y,
                transform.position.z);

        transform.position =
            basePosition;

        if (baseScale == Vector3.zero)
            baseScale = transform.localScale;
    }

    private void Update()
    {
        AnimateKey();
        AnimateGlow();
        AnimateBlink();
    }

    private void AnimateKey()
    {
        float pulse =
            (Mathf.Sin(
                Time.time * pulseSpeed +
                animationOffset) + 1f) * 0.5f;

        float scaleMultiplier =
            Mathf.Lerp(
                minimumScale,
                maximumScale,
                pulse);

        transform.localScale =
            baseScale *
            scaleMultiplier;

        float verticalOffset =
            Mathf.Sin(
                Time.time * floatingSpeed +
                animationOffset) *
            floatingDistance;

        transform.position =
            basePosition +
            Vector3.up *
            verticalOffset;

        transform.Rotate(
            0f,
            0f,
            rotationSpeed *
            Time.deltaTime);
    }

    private void AnimateGlow()
    {
        if (glowRenderer == null)
            return;

        float pulse =
            (Mathf.Sin(
                Time.time * glowPulseSpeed +
                animationOffset) + 1f) * 0.5f;

        float scale =
            Mathf.Lerp(
                glowMinimumScale,
                glowMaximumScale,
                pulse);

        glowObject.transform.position =
            transform.position;

        glowObject.transform.rotation =
            Quaternion.identity;

        glowObject.transform.localScale =
            Vector3.one *
            scale;

        Color color =
            glowColor;

        color.a =
            Mathf.Lerp(
                glowMinimumAlpha,
                glowMaximumAlpha,
                pulse);

        glowRenderer.color =
            color;
    }

    private void AnimateBlink()
    {
        blinkTimer -=
            Time.deltaTime;

        if (blinkTimer > 0f)
            return;

        blinkTimer =
            blinkInterval;

        StartCoroutine(
            BlinkRoutine());
    }

    private System.Collections.IEnumerator BlinkRoutine()
    {
        float elapsed = 0f;

        while (elapsed < blinkDuration)
        {
            float normalized =
                Mathf.Clamp01(
                    elapsed /
                    blinkDuration);

            float intensity =
                Mathf.Sin(
                    normalized *
                    Mathf.PI);

            if (keyRenderer != null)
            {
                keyRenderer.color =
                    Color.Lerp(
                        Color.white,
                        glowColor *
                        blinkIntensity,
                        intensity);
            }

            if (glowRenderer != null)
            {
                Color color =
                    glowRenderer.color;

                color.a =
                    Mathf.Lerp(
                        glowMaximumAlpha,
                        1f,
                        intensity);

                glowRenderer.color =
                    color;
            }

            elapsed +=
                Time.deltaTime;

            yield return null;
        }

        if (keyRenderer != null)
            keyRenderer.color = Color.white;
    }

    private void CreateGlow()
    {
        glowObject =
            new GameObject(
                "Key Glow");

        glowObject.transform.SetParent(
            transform.parent);

        glowObject.transform.position =
            transform.position;

        glowRenderer =
            glowObject.AddComponent<SpriteRenderer>();

        glowRenderer.sprite =
            GetGlowSprite();

        glowRenderer.sortingOrder =
            keyRenderer != null
                ? keyRenderer.sortingOrder - 1
                : 1;

        glowRenderer.color =
            new Color(
                glowColor.r,
                glowColor.g,
                glowColor.b,
                glowMinimumAlpha);
    }

    private static Sprite GetGlowSprite()
    {
        if (glowSprite != null)
            return glowSprite;

        const int size = 64;

        Texture2D texture =
            new Texture2D(
                size,
                size,
                TextureFormat.RGBA32,
                false);

        texture.name =
            "Runtime Key Glow";

        texture.filterMode =
            FilterMode.Bilinear;

        texture.wrapMode =
            TextureWrapMode.Clamp;

        Vector2 center =
            new Vector2(
                (size - 1) * 0.5f,
                (size - 1) * 0.5f);

        float maximumDistance =
            center.x;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance =
                    Vector2.Distance(
                        new Vector2(x, y),
                        center);

                float normalized =
                    Mathf.Clamp01(
                        1f -
                        distance /
                        maximumDistance);

                float alpha =
                    normalized *
                    normalized;

                texture.SetPixel(
                    x,
                    y,
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha));
            }
        }

        texture.Apply();

        glowSprite =
            Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    size,
                    size),
                new Vector2(
                    0.5f,
                    0.5f),
                size);

        glowSprite.name =
            "Runtime Key Glow Sprite";

        return glowSprite;
    }

    private void OnDestroy()
    {
        if (glowObject != null)
            Destroy(glowObject);
    }
}
