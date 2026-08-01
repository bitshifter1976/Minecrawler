using UnityEngine;

/// <summary>
/// Weist den zur Bossstufe passenden Sprite zu.
/// </summary>
public static class BossGraphicsInstaller
{
    public static bool ApplyBossSprite(
        SpriteRenderer renderer,
        int bossTier)
    {
        if (renderer == null)
            return false;

        Sprite sprite = BossVisuals.LoadBoss(bossTier);

        if (sprite == null)
        {
            Debug.LogWarning(
                $"Boss sprite for tier {bossTier} was not found.");
            return false;
        }

        renderer.sprite = sprite;
        return true;
    }

    public static bool ApplyProjectileSprite(
        SpriteRenderer renderer,
        bool ricochet)
    {
        if (renderer == null)
            return false;

        Sprite sprite =
            BossVisuals.LoadProjectile(ricochet);

        if (sprite == null)
        {
            Debug.LogWarning(
                ricochet
                    ? "Ricochet projectile sprite was not found."
                    : "Boss projectile sprite was not found.");

            return false;
        }

        renderer.sprite = sprite;
        return true;
    }

    public static bool ApplyMineSprite(
        SpriteRenderer renderer)
    {
        if (renderer == null)
            return false;

        Sprite sprite = BossVisuals.LoadMine();

        if (sprite == null)
        {
            Debug.LogWarning(
                "Boss mine sprite was not found.");
            return false;
        }

        renderer.sprite = sprite;
        return true;
    }
}
