using UnityEngine;

/// <summary>
/// White-yellow opening flash and expanding orange shockwave.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BossExplosionFlash : MonoBehaviour
{
    private SpriteRenderer renderer;
    private float age;
    private float scale;
    private const float Lifetime = 0.26f;

    public static void Create(
        Vector3 position,
        Transform parent,
        float scale,
        int sortingOrder)
    {
        GameObject flashObject =
            new("Boss Explosion Flash");

        flashObject.transform.position =
            position;

        flashObject.transform.SetParent(
            parent);

        SpriteRenderer renderer =
            flashObject.AddComponent<SpriteRenderer>();

        renderer.sortingOrder =
            sortingOrder;

        BossExplosionFlash flash =
            flashObject.AddComponent<BossExplosionFlash>();

        flash.scale =
            Mathf.Max(
                0.3f,
                scale);
    }

    private void Awake()
    {
        renderer =
            GetComponent<SpriteRenderer>();

        renderer.sprite =
            BossExplosionRuntimeSprites.Circle;

        renderer.color =
            new Color(
                1f,
                0.88f,
                0.42f,
                0.98f);
    }

    private void Update()
    {
        age +=
            Time.unscaledDeltaTime;

        float progress =
            Mathf.Clamp01(
                age / Lifetime);

        transform.localScale =
            Vector3.one *
            Mathf.Lerp(
                0.10f,
                1.35f,
                BossExplosionMath.SmoothStep(
                    progress)) *
            scale;

        Color color =
            renderer.color;

        color.a =
            0.98f *
            (1f - progress);

        renderer.color =
            color;

        if (progress >= 1f)
            Destroy(gameObject);
    }
}
