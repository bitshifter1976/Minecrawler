using UnityEngine;

/// <summary>
/// Lädt die Bossgrafiken aus dem importierten MinecrawlerBossPack.
/// Die Dateien müssen unter einem Resources-Ordner liegen.
/// </summary>
public static class BossVisuals
{
    private const string BossFolder = "Art/Bosses/";
    private const string FxFolder = "Art/BossFX/";

    public static Sprite LoadBoss(int bossTier)
    {
        int level = Mathf.Clamp(bossTier, 1, 10) * 10;

        return Resources.Load<Sprite>(
            $"{BossFolder}BossLevel{level:000}");
    }

    public static Sprite LoadBoss()
    {
        return LoadBoss(1);
    }

    public static Sprite LoadProjectile(bool ricochet = false)
    {
        string spriteName = ricochet
            ? "BossProjectileRicochet"
            : "BossProjectile";

        return Resources.Load<Sprite>(
            FxFolder + spriteName);
    }

    public static Sprite LoadMine()
    {
        return Resources.Load<Sprite>(
            FxFolder + "BossMine");
    }

    public static Sprite[] LoadExplosionFrames()
    {
        Sprite[] frames = new Sprite[8];

        for (int index = 0; index < frames.Length; index++)
        {
            frames[index] = Resources.Load<Sprite>(
                $"{FxFolder}BossExplosion{index + 1:00}");
        }

        return frames;
    }
}
