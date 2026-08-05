using UnityEngine;

/// <summary>
/// Flying metal, lava and rock fragments.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BossExplosionDebris : MonoBehaviour
{
    private SpriteRenderer renderer;
    private Vector3 velocity;
    private float rotationSpeed;
    private float age;
    private float lifetime;

    public static void CreateBurst(
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
            GameObject fragmentObject =
                new("Boss Explosion Debris");

            fragmentObject.transform.position =
                position;

            fragmentObject.transform.SetParent(
                parent);

            SpriteRenderer renderer =
                fragmentObject.AddComponent<SpriteRenderer>();

            renderer.sortingOrder =
                sortingOrder;

            BossExplosionDebris fragment =
                fragmentObject.AddComponent<BossExplosionDebris>();

            fragment.Initialize(
                scale,
                index);
        }
    }

    private void Initialize(
        float effectScale,
        int index)
    {
        renderer =
            GetComponent<SpriteRenderer>();

        renderer.sprite =
            index % 4 == 0
                ? BossExplosionRuntimeSprites.Gear
                : BossExplosionRuntimeSprites.Shard;

        bool lava =
            Random.value > 0.48f;

        renderer.color =
            lava
                ? new Color(
                    1f,
                    Random.Range(
                        0.18f,
                        0.50f),
                    0.01f,
                    1f)
                : new Color(
                    Random.Range(
                        0.12f,
                        0.28f),
                    Random.Range(
                        0.10f,
                        0.22f),
                    Random.Range(
                        0.08f,
                        0.18f),
                    1f);

        float angle =
            Random.Range(
                0f,
                Mathf.PI * 2f);

        float speed =
            Random.Range(
                1.4f,
                4.1f) *
            Mathf.Lerp(
                1f,
                1.25f,
                Mathf.InverseLerp(
                    1f,
                    3f,
                    effectScale));

        velocity =
            new Vector3(
                Mathf.Cos(angle),
                Mathf.Sin(angle),
                0f) *
            speed;

        rotationSpeed =
            Random.Range(
                -720f,
                720f);

        lifetime =
            Random.Range(
                0.68f,
                1.30f);

        transform.localScale =
            Vector3.one *
            Random.Range(
                0.045f,
                0.13f) *
            Mathf.Lerp(
                1f,
                1.30f,
                Mathf.InverseLerp(
                    1f,
                    3f,
                    effectScale));
    }

    private void Update()
    {
        age +=
            Time.unscaledDeltaTime;

        velocity *=
            Mathf.Pow(
                0.22f,
                Time.unscaledDeltaTime);

        transform.position +=
            velocity *
            Time.unscaledDeltaTime;

        transform.Rotate(
            0f,
            0f,
            rotationSpeed *
            Time.unscaledDeltaTime);

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
