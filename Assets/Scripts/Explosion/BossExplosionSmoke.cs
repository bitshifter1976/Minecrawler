using UnityEngine;

/// <summary>
/// Dark mine smoke with no white dust.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BossExplosionSmoke : MonoBehaviour
{
    private SpriteRenderer renderer;
    private Vector3 velocity;
    private float age;
    private float lifetime;
    private float targetScale;
    private float rotationSpeed;

    public static void CreateColumn(
        Vector3 position,
        Transform parent,
        float scale,
        int count,
        int sortingOrder)
    {
        for (int index = 0;
             index < count;
             index++)
        {
            GameObject smokeObject =
                new("Boss Explosion Dark Smoke");

            smokeObject.transform.position =
                position +
                new Vector3(
                    Random.Range(
                        -0.16f,
                        0.16f) *
                    scale,
                    Random.Range(
                        -0.10f,
                        0.18f) *
                    scale,
                    0f);

            smokeObject.transform.SetParent(
                parent);

            SpriteRenderer renderer =
                smokeObject.AddComponent<SpriteRenderer>();

            renderer.sortingOrder =
                sortingOrder + index;

            BossExplosionSmoke smoke =
                smokeObject.AddComponent<BossExplosionSmoke>();

            smoke.Initialize(
                scale,
                index);
        }
    }

    private void Initialize(
        float scale,
        int index)
    {
        renderer =
            GetComponent<SpriteRenderer>();

        renderer.sprite =
            BossExplosionRuntimeSprites.Smoke;

        float shade =
            Random.Range(
                0.08f,
                0.22f);

        renderer.color =
            new Color(
                shade * 1.10f,
                shade,
                shade * 0.92f,
                Random.Range(
                    0.38f,
                    0.62f));

        velocity =
            new Vector3(
                Random.Range(
                    -0.10f,
                    0.10f),
                Random.Range(
                    0.14f,
                    0.32f),
                0f);

        lifetime =
            Random.Range(
                1.20f,
                2.30f);

        targetScale =
            Random.Range(
                0.55f,
                1.05f) *
            scale;

        rotationSpeed =
            Random.Range(
                -18f,
                18f);

        transform.localScale =
            Vector3.one *
            targetScale *
            0.18f;
    }

    private void Update()
    {
        age +=
            Time.unscaledDeltaTime;

        float progress =
            Mathf.Clamp01(
                age / lifetime);

        transform.position +=
            velocity *
            Time.unscaledDeltaTime;

        transform.Rotate(
            0f,
            0f,
            rotationSpeed *
            Time.unscaledDeltaTime);

        transform.localScale =
            Vector3.one *
            Mathf.Lerp(
                targetScale * 0.18f,
                targetScale,
                BossExplosionMath.SmoothStep(
                    progress));

        Color color =
            renderer.color;

        color.a *=
            Mathf.Pow(
                0.18f,
                Time.unscaledDeltaTime);

        renderer.color =
            color;

        if (progress >= 1f)
            Destroy(gameObject);
    }
}
