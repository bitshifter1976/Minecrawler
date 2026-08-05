using UnityEngine;

/// <summary>
/// Bright sparks and long-lived embers.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BossExplosionSparks : MonoBehaviour
{
    private SpriteRenderer renderer;
    private Vector3 velocity;
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
            GameObject sparkObject =
                new("Boss Explosion Spark");

            sparkObject.transform.position =
                position;

            sparkObject.transform.SetParent(
                parent);

            SpriteRenderer renderer =
                sparkObject.AddComponent<SpriteRenderer>();

            renderer.sortingOrder =
                sortingOrder;

            BossExplosionSparks spark =
                sparkObject.AddComponent<BossExplosionSparks>();

            spark.Initialize(
                scale);
        }
    }

    private void Initialize(
        float scale)
    {
        renderer =
            GetComponent<SpriteRenderer>();

        renderer.sprite =
            BossExplosionRuntimeSprites.Spark;

        renderer.color =
            Random.value > 0.50f
                ? new Color(
                    1f,
                    0.78f,
                    0.12f,
                    1f)
                : new Color(
                    1f,
                    0.22f,
                    0.01f,
                    1f);

        float angle =
            Random.Range(
                0f,
                Mathf.PI * 2f);

        float speed =
            Random.Range(
                1.0f,
                4.8f) *
            Mathf.Lerp(
                1f,
                1.18f,
                Mathf.InverseLerp(
                    1f,
                    3f,
                    scale));

        velocity =
            new Vector3(
                Mathf.Cos(angle),
                Mathf.Sin(angle),
                0f) *
            speed;

        lifetime =
            Random.Range(
                0.50f,
                1.40f);

        transform.localScale =
            new Vector3(
                Random.Range(
                    0.02f,
                    0.045f),
                Random.Range(
                    0.08f,
                    0.18f),
                1f);
    }

    private void Update()
    {
        age +=
            Time.unscaledDeltaTime;

        velocity *=
            Mathf.Pow(
                0.12f,
                Time.unscaledDeltaTime);

        transform.position +=
            velocity *
            Time.unscaledDeltaTime;

        if (velocity.sqrMagnitude > 0.001f)
        {
            float angle =
                Mathf.Atan2(
                    velocity.y,
                    velocity.x) *
                Mathf.Rad2Deg -
                90f;

            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle);
        }

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
