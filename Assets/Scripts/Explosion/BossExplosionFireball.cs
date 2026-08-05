using UnityEngine;

/// <summary>
/// Plays the imported top-down explosion frames.
/// Uses BossVisuals.LoadExplosionFrames().
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BossExplosionFireball : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;

    private float framesPerSecond;
    private float age;
    private float startDelay;
    private float scale;
    private float alphaMultiplier;
    private float rotationSpeed;
    private int frameIndex;
    private bool started;

    public static void Create(
        Vector3 position,
        Transform parent,
        float scale,
        int sortingOrder,
        float startDelay = 0f,
        float alphaMultiplier = 1f)
    {
        GameObject fireballObject =
            new("Boss Explosion Fireball");

        fireballObject.transform.position =
            position;

        fireballObject.transform.SetParent(
            parent);

        SpriteRenderer renderer =
            fireballObject.AddComponent<SpriteRenderer>();

        renderer.sortingOrder =
            sortingOrder;

        BossExplosionFireball fireball =
            fireballObject.AddComponent<BossExplosionFireball>();

        fireball.scale =
            Mathf.Max(
                0.2f,
                scale);

        fireball.startDelay =
            Mathf.Max(
                0f,
                startDelay);

        fireball.alphaMultiplier =
            Mathf.Clamp01(
                alphaMultiplier);
    }

    private void Awake()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();

        frames =
            BossVisuals.LoadExplosionFrames();

        framesPerSecond = 8.5f;

        rotationSpeed =
            Random.Range(
                -10f,
                10f);

        transform.localScale =
            Vector3.zero;

        if (frames == null ||
            frames.Length == 0)
        {
            Debug.LogWarning(
                "Boss explosion frames were not found.");

            Destroy(gameObject);
            return;
        }

        spriteRenderer.sprite =
            frames[0];

        Color color =
            spriteRenderer.color;

        color.a =
            alphaMultiplier;

        spriteRenderer.color =
            color;
    }

    private void Update()
    {
        age +=
            Time.unscaledDeltaTime;

        if (!started)
        {
            if (age < startDelay)
                return;

            started = true;
            age = 0f;
        }

        float growProgress =
            Mathf.Clamp01(
                age / 0.16f);

        transform.localScale =
            Vector3.one *
            Mathf.Lerp(
                scale * 0.30f,
                scale,
                BossExplosionMath.SmoothStep(
                    growProgress));

        transform.Rotate(
            0f,
            0f,
            rotationSpeed *
            Time.unscaledDeltaTime);

        float frameDuration =
            1f /
            Mathf.Max(
                1f,
                framesPerSecond);

        int desiredFrame =
            Mathf.FloorToInt(
                age / frameDuration);

        if (desiredFrame >= frames.Length)
        {
            Destroy(gameObject);
            return;
        }

        if (desiredFrame != frameIndex)
        {
            frameIndex =
                desiredFrame;

            if (frames[frameIndex] != null)
            {
                spriteRenderer.sprite =
                    frames[frameIndex];
            }
        }
    }
}
