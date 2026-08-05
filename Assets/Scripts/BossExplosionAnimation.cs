using UnityEngine;

/// <summary>
/// Top-down boss explosion with optional spritesheet animation,
/// flash, shockwave and flying lava-rock debris.
///
/// Preferred asset:
/// Resources/Art/BossFX/BossExplosionTopDown.png
///
/// The preferred sheet may contain 8, 12 or 16 horizontal frames.
/// If the sheet is missing, the original individual explosion sprites
/// loaded by BossVisuals are used as a fallback.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BossExplosionAnimation : MonoBehaviour
{
    [SerializeField] private float framesPerSecond = 8f;
    [SerializeField] private int sortingOrder = 100;
    [SerializeField] private float scale = 2.25f;

    [SerializeField] private float holdLastFrameSeconds = 0.20f;
    [SerializeField] private float fadeOutSeconds = 0.30f;

    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;
    private float frameTimer;
    private int frameIndex;
    private bool animationFinished;
    private float endingTimer;

    private AudioSource audioSource;
    private AudioClip explosionClip;

    public static void Create(
        Vector3 position,
        Transform parent = null,
        float customScale = 2.25f)
    {
        GameObject effectObject =
            new("Boss Explosion");

        effectObject.transform.position =
            position;

        effectObject.transform.SetParent(
            parent);

        effectObject.AddComponent<SpriteRenderer>();

        BossExplosionAnimation animation =
            effectObject.AddComponent<BossExplosionAnimation>();

        animation.scale =
            Mathf.Max(
                0.05f,
                customScale);
    }

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.75f;

        explosionClip = Resources.Load<AudioClip>("Audio/explosion");
        if (explosionClip != null)
        {
            audioSource.clip = explosionClip;
            audioSource.Play();
        }

        spriteRenderer =
            GetComponent<SpriteRenderer>();

        spriteRenderer.sortingOrder =
            sortingOrder;

        transform.localScale =
            Vector3.one * scale;

        frames =
            BossVisuals.LoadExplosionFrames();

        CreateFlash();
        CreateShockwave();
        SpawnDebris();

        Camera.main?
            .GetComponent<CameraShake>()?
            .Shake(
                Mathf.Lerp(
                    1.1f,
                    1.8f,
                    Mathf.InverseLerp(
                        1f,
                        3f,
                        scale)),
                Mathf.Lerp(
                    0.45f,
                    0.80f,
                    Mathf.InverseLerp(
                        1f,
                        3f,
                        scale)));

        if (frames == null ||
            frames.Length == 0)
        {
            Debug.LogWarning(
                "Boss explosion frames were not found.");

            Destroy(
                gameObject,
                0.12f);

            return;
        }

        spriteRenderer.sprite =
            frames[0];
    }

    private void Update()
    {
        if (frames == null ||
            frames.Length == 0)
        {
            return;
        }

        if (animationFinished)
        {
            UpdateEnding();
            return;
        }

        float frameDuration =
            1f /
            Mathf.Max(
                1f,
                framesPerSecond);

        frameTimer +=
            Time.unscaledDeltaTime;

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex++;

            if (frameIndex >= frames.Length)
            {
                frameIndex =
                    frames.Length - 1;

                animationFinished = true;
                endingTimer = 0f;
                return;
            }

            if (frames[frameIndex] != null)
            {
                spriteRenderer.sprite =
                    frames[frameIndex];
            }
        }
    }

    private void UpdateEnding()
    {
        endingTimer +=
            Time.unscaledDeltaTime;

        if (endingTimer <=
            holdLastFrameSeconds)
        {
            return;
        }

        float progress =
            Mathf.Clamp01(
                (endingTimer -
                 holdLastFrameSeconds) /
                Mathf.Max(
                    0.01f,
                    fadeOutSeconds));

        Color color =
            spriteRenderer.color;

        color.a =
            1f - progress;

        spriteRenderer.color =
            color;

        if (progress >= 1f)
            Destroy(gameObject);
    }

    private void CreateFlash()
    {
        GameObject flashObject =
            new("Boss Explosion Flash");

        flashObject.transform.position =
            transform.position;

        flashObject.transform.SetParent(
            transform.parent);

        BossExplosionFlash flash =
            flashObject.AddComponent<BossExplosionFlash>();

        flash.Initialize(
            sortingOrder + 2,
            scale);
    }

    private void CreateShockwave()
    {
        GameObject waveObject =
            new("Boss Explosion Shockwave");

        waveObject.transform.position =
            transform.position;

        waveObject.transform.SetParent(
            transform.parent);

        BossExplosionShockwave wave =
            waveObject.AddComponent<BossExplosionShockwave>();

        wave.Initialize(
            sortingOrder + 1,
            scale);
    }

    private void SpawnDebris()
    {
        int count =
            Random.Range(
                12,
                18);

        for (int index = 0;
             index < count;
             index++)
        {
            GameObject debrisObject =
                new("Boss Explosion Debris");

            debrisObject.transform.position =
                transform.position;

            debrisObject.transform.SetParent(
                transform.parent);

            BossExplosionDebris debris =
                debrisObject.AddComponent<BossExplosionDebris>();

            debris.Initialize(
                sortingOrder + 3,
                scale);
        }
    }
}

[RequireComponent(typeof(SpriteRenderer))]
internal sealed class BossExplosionFlash : MonoBehaviour
{
    private SpriteRenderer renderer;
    private float age;
    private float effectScale;
    private const float Lifetime = 0.24f;

    public void Initialize(
        int sortingOrder,
        float effectScale)
    {
        this.effectScale =
            Mathf.Max(
                1f,
                effectScale);

        renderer =
            GetComponent<SpriteRenderer>();

        renderer.sprite =
            BossExplosionRuntimeSprites.Circle;

        renderer.sortingOrder =
            sortingOrder;

        renderer.color =
            new Color(
                1f,
                0.68f,
                0.12f,
                0.92f);

        transform.localScale =
            Vector3.one *
            0.15f *
            effectScale;
    }

    private void Update()
    {
        age += Time.unscaledDeltaTime;

        float progress =
            Mathf.Clamp01(
                age / Lifetime);

        transform.localScale =
            Vector3.one *
            Mathf.Lerp(
                0.15f,
                1.35f,
                progress) *
            effectScale;

        Color color =
            renderer.color;

        color.a =
            0.92f *
            (1f - progress);

        renderer.color =
            color;

        if (progress >= 1f)
            Destroy(gameObject);
    }
}

[RequireComponent(typeof(SpriteRenderer))]
internal sealed class BossExplosionShockwave : MonoBehaviour
{
    private SpriteRenderer renderer;
    private float age;
    private float effectScale;
    private const float Lifetime = 0.62f;

    public void Initialize(
        int sortingOrder,
        float effectScale)
    {
        this.effectScale =
            Mathf.Max(
                1f,
                effectScale);

        renderer =
            GetComponent<SpriteRenderer>();

        renderer.sprite =
            BossExplosionRuntimeSprites.Ring;

        renderer.sortingOrder =
            sortingOrder;

        renderer.color =
            new Color(
                1f,
                0.30f,
                0.04f,
                0.72f);

        transform.localScale =
            Vector3.one *
            0.28f *
            effectScale;
    }

    private void Update()
    {
        age += Time.deltaTime;

        float progress =
            Mathf.Clamp01(
                age / Lifetime);

        transform.localScale =
            Vector3.one *
            Mathf.Lerp(
                0.28f,
                2.15f,
                progress) *
            effectScale;

        Color color =
            renderer.color;

        color.a =
            0.72f *
            (1f - progress);

        renderer.color =
            color;

        if (progress >= 1f)
            Destroy(gameObject);
    }
}

[RequireComponent(typeof(SpriteRenderer))]
internal sealed class BossExplosionDebris : MonoBehaviour
{
    private SpriteRenderer renderer;
    private Vector3 velocity;
    private float effectScale;
    private float angularVelocity;
    private float age;
    private float lifetime;

    public void Initialize(
        int sortingOrder,
        float effectScale)
    {
        this.effectScale =
            Mathf.Max(
                1f,
                effectScale);

        renderer =
            GetComponent<SpriteRenderer>();

        renderer.sprite =
            BossExplosionRuntimeSprites.Square;

        renderer.sortingOrder =
            sortingOrder;

        bool lavaPiece =
            Random.value > 0.45f;

        renderer.color =
            lavaPiece
                ? new Color(
                    1f,
                    Random.Range(
                        0.20f,
                        0.52f),
                    0.02f,
                    1f)
                : new Color(
                    0.17f,
                    0.12f,
                    0.10f,
                    1f);

        float angle =
            Random.Range(
                0f,
                Mathf.PI * 2f);

        float speed =
            Random.Range(
                1.2f,
                3.2f) *
            Mathf.Lerp(
                1f,
                1.35f,
                effectScale - 1f);

        velocity =
            new Vector3(
                Mathf.Cos(angle),
                Mathf.Sin(angle),
                0f) *
            speed;

        angularVelocity =
            Random.Range(
                -520f,
                520f);

        lifetime =
            Random.Range(
                0.48f,
                0.90f);

        transform.localScale =
            Vector3.one *
            Random.Range(
                0.045f,
                0.13f) *
            Mathf.Lerp(
                1f,
                1.25f,
                effectScale - 1f);
    }

    private void Update()
    {
        age += Time.deltaTime;

        velocity *=
            Mathf.Pow(
                0.15f,
                Time.deltaTime);

        transform.position +=
            velocity *
            Time.deltaTime;

        transform.Rotate(
            0f,
            0f,
            angularVelocity *
            Time.deltaTime);

        float progress =
            Mathf.Clamp01(
                age / lifetime);

        Color color =
            renderer.color;

        color.a =
            1f - progress;

        renderer.color =
            color;

        if (progress >= 1f)
            Destroy(gameObject);
    }
}

internal static class BossExplosionRuntimeSprites
{
    private static Sprite circle;
    private static Sprite ring;
    private static Sprite square;

    public static Sprite Circle =>
        circle ??= CreateRadialSprite(
            false);

    public static Sprite Ring =>
        ring ??= CreateRadialSprite(
            true);

    public static Sprite Square =>
        square ??= CreateSquareSprite();

    private static Sprite CreateRadialSprite(
        bool isRing)
    {
        const int size = 64;

        Texture2D texture =
            new(
                size,
                size,
                TextureFormat.RGBA32,
                false);

        texture.filterMode =
            FilterMode.Bilinear;

        Vector2 center =
            new(
                (size - 1) * 0.5f,
                (size - 1) * 0.5f);

        float radius =
            size * 0.46f;

        for (int y = 0;
             y < size;
             y++)
        {
            for (int x = 0;
                 x < size;
                 x++)
            {
                float normalizedDistance =
                    Vector2.Distance(
                        new Vector2(x, y),
                        center) /
                    radius;

                float alpha;

                if (isRing)
                {
                    alpha =
                        1f -
                        Mathf.Clamp01(
                            Mathf.Abs(
                                normalizedDistance -
                                0.72f) *
                            10f);
                }
                else
                {
                    alpha =
                        1f -
                        Mathf.Clamp01(
                            normalizedDistance);
                }

                texture.SetPixel(
                    x,
                    y,
                    new Color(
                        1f,
                        1f,
                        1f,
                        alpha * alpha));
            }
        }

        texture.Apply();

        return Sprite.Create(
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
    }

    private static Sprite CreateSquareSprite()
    {
        Texture2D texture =
            new(
                2,
                2,
                TextureFormat.RGBA32,
                false);

        texture.SetPixels(
            new[]
            {
                Color.white,
                Color.white,
                Color.white,
                Color.white
            });

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(
                0f,
                0f,
                2f,
                2f),
            new Vector2(
                0.5f,
                0.5f),
            2f);
    }
}
