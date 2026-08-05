using System.Collections;
using UnityEngine;

/// <summary>
/// Shared runtime sprites, math and coroutine runner.
/// </summary>
public static class BossExplosionMath
{
    public static float SmoothStep(
        float value)
    {
        value =
            Mathf.Clamp01(
                value);

        return value *
               value *
               (3f - 2f * value);
    }
}

public static class BossExplosionRunner
{
    private sealed class Runner : MonoBehaviour
    {
    }

    private static Runner instance;

    public static void Run(
        IEnumerator routine)
    {
        if (routine == null)
            return;

        EnsureInstance()
            .StartCoroutine(
                routine);
    }

    private static Runner EnsureInstance()
    {
        if (instance != null)
            return instance;

        GameObject runnerObject =
            new("Boss Explosion Runner");

        Object.DontDestroyOnLoad(
            runnerObject);

        instance =
            runnerObject.AddComponent<Runner>();

        return instance;
    }
}

public static class BossExplosionRuntimeSprites
{
    private static Sprite circle;
    private static Sprite smoke;
    private static Sprite spark;
    private static Sprite shard;
    private static Sprite gear;
    private static Sprite scorch;

    public static Sprite Circle =>
        circle ??=
            CreateRadialSprite(
                64,
                false,
                0.72f);

    public static Sprite Smoke =>
        smoke ??=
            CreateNoiseCloudSprite();

    public static Sprite Spark =>
        spark ??=
            CreateRectangleSprite(
                4,
                16);

    public static Sprite Shard =>
        shard ??=
            CreateShardSprite();

    public static Sprite Gear =>
        gear ??=
            CreateGearSprite();

    public static Sprite Scorch =>
        scorch ??=
            CreateScorchSprite();

    private static Sprite CreateRadialSprite(
        int size,
        bool ring,
        float ringPosition)
    {
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
            size * 0.47f;

        for (int y = 0;
             y < size;
             y++)
        {
            for (int x = 0;
                 x < size;
                 x++)
            {
                float normalized =
                    Vector2.Distance(
                        new Vector2(x, y),
                        center) /
                    radius;

                float alpha =
                    ring
                        ? 1f -
                          Mathf.Clamp01(
                              Mathf.Abs(
                                  normalized -
                                  ringPosition) *
                              10f)
                        : 1f -
                          Mathf.Clamp01(
                              normalized);

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

    private static Sprite CreateNoiseCloudSprite()
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
                size * 0.5f,
                size * 0.5f);

        for (int y = 0;
             y < size;
             y++)
        {
            for (int x = 0;
                 x < size;
                 x++)
            {
                float distance =
                    Vector2.Distance(
                        new Vector2(x, y),
                        center) /
                    (size * 0.48f);

                float noise =
                    Mathf.PerlinNoise(
                        x * 0.10f,
                        y * 0.10f);

                float alpha =
                    (1f -
                     Mathf.Clamp01(
                         distance)) *
                    Mathf.Lerp(
                        0.55f,
                        1f,
                        noise);

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

    private static Sprite CreateRectangleSprite(
        int width,
        int height)
    {
        Texture2D texture =
            new(
                width,
                height,
                TextureFormat.RGBA32,
                false);

        Color[] pixels =
            new Color[
                width *
                height];

        for (int index = 0;
             index < pixels.Length;
             index++)
        {
            pixels[index] =
                Color.white;
        }

        texture.SetPixels(
            pixels);

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(
                0f,
                0f,
                width,
                height),
            new Vector2(
                0.5f,
                0.5f),
            height);
    }

    private static Sprite CreateShardSprite()
    {
        const int size = 16;

        Texture2D texture =
            new(
                size,
                size,
                TextureFormat.RGBA32,
                false);

        texture.filterMode =
            FilterMode.Point;

        for (int y = 0;
             y < size;
             y++)
        {
            for (int x = 0;
                 x < size;
                 x++)
            {
                bool inside =
                    x >= y * 0.25f &&
                    x <= size -
                         y * 0.45f;

                texture.SetPixel(
                    x,
                    y,
                    inside
                        ? Color.white
                        : Color.clear);
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

    private static Sprite CreateGearSprite()
    {
        const int size = 32;

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
                size * 0.5f,
                size * 0.5f);

        for (int y = 0;
             y < size;
             y++)
        {
            for (int x = 0;
                 x < size;
                 x++)
            {
                Vector2 delta =
                    new Vector2(x, y) -
                    center;

                float distance =
                    delta.magnitude /
                    (size * 0.5f);

                float angle =
                    Mathf.Atan2(
                        delta.y,
                        delta.x);

                float teeth =
                    Mathf.Sin(
                        angle * 8f) *
                    0.08f;

                bool outer =
                    distance <
                    0.78f + teeth;

                bool inner =
                    distance >
                    0.30f;

                texture.SetPixel(
                    x,
                    y,
                    outer && inner
                        ? Color.white
                        : Color.clear);
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

    private static Sprite CreateScorchSprite()
    {
        const int size = 96;

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
                size * 0.5f,
                size * 0.5f);

        for (int y = 0;
             y < size;
             y++)
        {
            for (int x = 0;
                 x < size;
                 x++)
            {
                Vector2 position =
                    new(x, y);

                float distance =
                    Vector2.Distance(
                        position,
                        center) /
                    (size * 0.48f);

                float noise =
                    Mathf.PerlinNoise(
                        x * 0.08f,
                        y * 0.08f);

                float alpha =
                    (1f -
                     Mathf.Clamp01(
                         distance)) *
                    Mathf.Lerp(
                        0.45f,
                        1f,
                        noise);

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
}
