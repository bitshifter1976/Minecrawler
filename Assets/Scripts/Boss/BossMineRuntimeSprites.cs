using UnityEngine;

/// <summary>
/// Procedural sprites for boss mines.
/// No external mine artwork is required.
/// </summary>
internal static class BossMineRuntimeSprites
{
    private static Sprite body;
    private static Sprite light;
    private static Sprite explosion;
    private static Sprite dust;

    public static Sprite Body =>
        body ??=
            CreateBody();

    public static Sprite Light =>
        light ??=
            CreateRadial(
                32,
                0.46f,
                false);

    public static Sprite Explosion =>
        explosion ??=
            CreateExplosion();

    public static Sprite Dust =>
        dust ??=
            CreateRadial(
                64,
                0.48f,
                false);

    private static Sprite CreateBody()
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
                Vector2 delta =
                    new Vector2(x, y) -
                    center;

                float normalized =
                    delta.magnitude /
                    (size * 0.46f);

                float angle =
                    Mathf.Atan2(
                        delta.y,
                        delta.x);

                float teeth =
                    Mathf.Sin(
                        angle * 12f) *
                    0.045f;

                bool outer =
                    normalized <=
                    1f + teeth;

                bool innerRing =
                    normalized >= 0.62f &&
                    normalized <= 0.79f;

                bool centerPlate =
                    normalized <= 0.58f;

                float alpha =
                    outer
                        ? 1f
                        : 0f;

                float shade =
                    centerPlate
                        ? 0.82f
                        : innerRing
                            ? 0.58f
                            : 0.36f;

                texture.SetPixel(
                    x,
                    y,
                    new Color(
                        shade,
                        shade,
                        shade,
                        alpha));
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

    private static Sprite CreateExplosion()
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
                Vector2 delta =
                    new Vector2(x, y) -
                    center;

                float distance =
                    delta.magnitude /
                    (size * 0.48f);

                float angle =
                    Mathf.Atan2(
                        delta.y,
                        delta.x);

                float spikes =
                    Mathf.Sin(
                        angle * 9f) *
                    0.16f;

                float alpha =
                    1f -
                    Mathf.Clamp01(
                        distance -
                        spikes);

                float hot =
                    1f -
                    Mathf.Clamp01(
                        distance * 1.6f);

                texture.SetPixel(
                    x,
                    y,
                    new Color(
                        1f,
                        Mathf.Lerp(
                            0.20f,
                            0.95f,
                            hot),
                        Mathf.Lerp(
                            0f,
                            0.35f,
                            hot),
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

    private static Sprite CreateRadial(
        int size,
        float radiusScale,
        bool ring)
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
                size * 0.5f,
                size * 0.5f);

        float radius =
            size * radiusScale;

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
                    radius;

                float alpha =
                    ring
                        ? 1f -
                          Mathf.Clamp01(
                              Mathf.Abs(
                                  distance -
                                  0.72f) *
                              10f)
                        : 1f -
                          Mathf.Clamp01(
                              distance);

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
