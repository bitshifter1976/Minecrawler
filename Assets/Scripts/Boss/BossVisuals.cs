using System;
using UnityEngine;

/// <summary>
/// Central loader for boss graphics.
/// </summary>
public static class BossVisuals
{
    private const string BossFolder =
        "Art/Bosses/";

    private const string FxFolder =
        "Art/BossFX/";

    public static Texture2D LoadBossWalkTexture()
    {
        return Resources.Load<Texture2D>(
            BossFolder +
            "LevelBossWalk");
    }

    public static Sprite LoadBoss(
        int bossTier)
    {
        int level =
            Mathf.Clamp(
                bossTier,
                1,
                10) *
            10;

        return Resources.Load<Sprite>(
            $"{BossFolder}" +
            $"BossLevel{level:000}");
    }

    public static Sprite LoadBoss()
    {
        return LoadBoss(1);
    }

    public static Sprite LoadProjectile(
        bool ricochet = false)
    {
        string spriteName =
            ricochet
                ? "BossProjectileRicochet"
                : "BossProjectile";

        return Resources.Load<Sprite>(
            FxFolder +
            spriteName);
    }

    public static Sprite LoadMine()
    {
        return Resources.Load<Sprite>(
            FxFolder +
            "BossMine");
    }

    public static Sprite[] LoadExplosionFrames()
    {
        Texture2D sheet =
            Resources.Load<Texture2D>(
                FxFolder +
                "BossExplosion");

        if (sheet != null)
        {
            Sprite[] sheetFrames =
                SliceHorizontalSheet(
                    sheet);

            if (sheetFrames.Length > 0)
                return sheetFrames;
        }

        Sprite[] fallbackFrames =
            new Sprite[12];

        for (int index = 0;
             index < fallbackFrames.Length;
             index++)
        {
            fallbackFrames[index] =
                Resources.Load<Sprite>(
                    $"{FxFolder}" +
                    $"BossExplosion_{index + 1:00}");
        }

        return Array.FindAll(
            fallbackFrames,
            sprite => sprite != null);
    }

    private static Sprite[] SliceHorizontalSheet(
        Texture2D texture)
    {
        int[] supportedCounts =
        {
            16,
            12,
            8
        };

        int frameCount = 0;

        foreach (int count in supportedCounts)
        {
            if (texture.width % count != 0)
                continue;

            int frameWidth =
                texture.width / count;

            if (frameWidth != texture.height)
                continue;

            frameCount = count;
            break;
        }

        if (frameCount == 0)
        {
            Debug.LogWarning(
                $"BossExplosion has unsupported dimensions " +
                $"{texture.width}x{texture.height}. " +
                "Use a horizontal 8, 12 or 16 frame sheet " +
                "with square cells.");

            return Array.Empty<Sprite>();
        }

        int size =
            texture.height;

        Sprite[] frames =
            new Sprite[frameCount];

        for (int index = 0;
             index < frameCount;
             index++)
        {
            frames[index] =
                Sprite.Create(
                    texture,
                    new Rect(
                        index * size,
                        0f,
                        size,
                        size),
                    new Vector2(
                        0.5f,
                        0.5f),
                    size,
                    0,
                    SpriteMeshType.FullRect);
        }

        return frames;
    }
}
