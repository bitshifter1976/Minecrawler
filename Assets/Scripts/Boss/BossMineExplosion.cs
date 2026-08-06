using UnityEngine;

/// <summary>
/// Small orange mine explosion used for fatal mine contact.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BossMineExplosion : MonoBehaviour
{
    private SpriteRenderer renderer;
    private float age;
    private float scale;
    private const float Lifetime = 0.55f;

    public static void Create(
        Vector3 position,
        Transform parent,
        float scale)
    {
        GameObject explosionObject =
            new("Boss Mine Explosion");

        explosionObject.transform.position =
            position;

        explosionObject.transform.SetParent(
            parent);

        SpriteRenderer renderer =
            explosionObject.AddComponent<SpriteRenderer>();

        renderer.sortingOrder = 105;

        BossMineExplosion explosion =
            explosionObject.AddComponent<BossMineExplosion>();

        explosion.scale =
            Mathf.Max(
                0.25f,
                scale);
    }

    private void Awake()
    {
        renderer =
            GetComponent<SpriteRenderer>();

        renderer.sprite =
            BossMineRuntimeSprites.Explosion;

        renderer.color =
            new Color(
                1f,
                0.30f,
                0.01f,
                1f);

        AudioClip clip =
            Resources.Load<AudioClip>(
                "Audio/mineExplosion");

        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(
                clip,
                transform.position,
                0.75f);
        }

        Camera.main?
            .GetComponent<CameraShake>()?
            .Shake(
                0.72f,
                0.20f);
    }

    private void Update()
    {
        age +=
            Time.unscaledDeltaTime;

        float progress =
            Mathf.Clamp01(
                age / Lifetime);

        float expansion =
            Mathf.Sin(
                progress *
                Mathf.PI);

        transform.localScale =
            Vector3.one *
            Mathf.Lerp(
                0.15f,
                1.20f,
                expansion) *
            scale;

        transform.Rotate(
            0f,
            0f,
            180f *
            Time.unscaledDeltaTime);

        Color color =
            renderer.color;

        color.a =
            1f -
            progress;

        renderer.color =
            color;

        if (progress >= 1f)
            Destroy(gameObject);
    }
}

[RequireComponent(typeof(SpriteRenderer))]
internal sealed class BossMineDust : MonoBehaviour
{
    private SpriteRenderer renderer;
    private float age;
    private const float Lifetime = 0.28f;

    public static void Create(
        Vector3 position,
        Transform parent)
    {
        GameObject dustObject =
            new("Boss Mine Drop Dust");

        dustObject.transform.position =
            position;

        dustObject.transform.SetParent(
            parent);

        dustObject.AddComponent<SpriteRenderer>();
        dustObject.AddComponent<BossMineDust>();
    }

    private void Awake()
    {
        renderer =
            GetComponent<SpriteRenderer>();

        renderer.sprite =
            BossMineRuntimeSprites.Dust;

        renderer.sortingOrder = 8;

        renderer.color =
            new Color(
                0.28f,
                0.20f,
                0.14f,
                0.42f);
    }

    private void Update()
    {
        age +=
            Time.deltaTime;

        float progress =
            Mathf.Clamp01(
                age / Lifetime);

        transform.localScale =
            Vector3.one *
            Mathf.Lerp(
                0.12f,
                0.65f,
                progress);

        Color color =
            renderer.color;

        color.a =
            0.42f *
            (1f - progress);

        renderer.color =
            color;

        if (progress >= 1f)
            Destroy(gameObject);
    }
}
