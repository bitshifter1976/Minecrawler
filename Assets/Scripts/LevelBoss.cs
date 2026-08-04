using System.Collections;
using UnityEngine;

public enum BossAttackPattern
{
    AimedSingle,
    Cross,
    AimedBurst,
    TripleLane,
    RapidFire,
    Ricochet,
    MineLayer,
    CrossAndMines,
    Berserker,
    MineKing
}

/// <summary>
/// Single animated Mine Guardian used for every boss level.
/// It is invulnerable, slowly follows the miner and shoots projectiles.
/// Direct contact is fatal to the miner.
/// </summary>
[RequireComponent(typeof(LevelBossSpriteAnimator))]
public sealed class LevelBoss : GridActor
{
    private int hitPoints;
    private int maximumHitPoints;
    private int bossTier;

    private float fireInterval;
    private float projectileSpeed;
    private float fireTimer;
    private bool shootingEnabled = true;

    private bool destroyed;
    private bool berserkActive;
    private bool attackInProgress;
    private bool movementInProgress;

    private float moveTimer;
    private float moveInterval;
    private float moveDuration;

    private float wobbleTime;
    private float wobbleSpeed;
    private float wobbleAngle;
    private float movementBlockedUntil;
    private bool initialized;

    [Header("Visual")]
    [SerializeField] private float bossScale = 0.92f;
    [SerializeField] private float healthBarWidth = 0.95f;
    [SerializeField] private float healthBarHeight = 0.10f;
    [SerializeField] private float healthBarOffsetY = 0.72f;

    [Header("Combat")]
    [SerializeField] private float minimumFireDistance = 2.25f;

    private GameObject healthBarRoot;
    private SpriteRenderer healthBarBackground;
    private SpriteRenderer healthBarFill;
    private static Sprite healthBarSprite;

    private BossAttackPattern attackPattern;
    private SpriteRenderer spriteRenderer;
    private LevelBossSpriteAnimator spriteAnimator;

    public int HitPoints => hitPoints;
    public int MaximumHitPoints => maximumHitPoints;
    public int BossTier => bossTier;
    public bool IsDestroyed => destroyed;
    public BossAttackPattern AttackPattern => attackPattern;
    public string BossName => "Mine Guardian";

    public void Initialize(
        int levelNumber,
        MinerController miner,
        Sprite fallbackProjectileSprite)
    {
        bossTier =
            Mathf.Clamp(
                levelNumber / 10,
                1,
                10);

        float progress =
            (bossTier - 1f) / 9f;

        maximumHitPoints =
            GetHitPoints(bossTier);

        hitPoints =
            maximumHitPoints;

        fireInterval =
            Mathf.Lerp(
                2.7f,
                0.52f,
                progress);

        projectileSpeed =
            Mathf.Lerp(
                3.1f,
                9.2f,
                progress);

        GameDifficulty difficulty =
            GameSettings.Load().GameDifficulty;

        shootingEnabled =
            difficulty != GameDifficulty.Easy;

        switch (difficulty)
        {
            case GameDifficulty.Hardcore:
                // Faster firing cadence and faster projectiles.
                fireInterval *= 0.65f;
                projectileSpeed *= 1.28f;
                break;

            case GameDifficulty.Normal:
                // Keep the current behaviour unchanged.
                break;

            case GameDifficulty.Easy:
                // The boss still moves and must be avoided,
                // but never fires projectiles.
                break;
        }

        fireTimer =
            shootingEnabled
                ? Random.Range(
                    fireInterval * 0.55f,
                    fireInterval)
                : float.PositiveInfinity;

        attackPattern =
            GetAttackPattern(bossTier);

        // Der Boss bewegt sich bewusst deutlich langsamer als der Miner.
        moveInterval = Mathf.Lerp(2.65f, 1.55f, progress);
        moveDuration = Mathf.Lerp(0.72f, 0.48f, progress);
        moveTimer = Random.Range(moveInterval * 0.5f, moveInterval);

        wobbleSpeed = Mathf.Lerp(2.1f, 3.8f, progress);
        wobbleAngle = Mathf.Lerp(3.5f, 7f, progress);

        spriteRenderer =
            GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            spriteRenderer =
                gameObject.AddComponent<SpriteRenderer>();
        }

        spriteRenderer.color =
            Color.white;

        spriteRenderer.sortingOrder = 4;

        spriteAnimator =
            GetComponent<LevelBossSpriteAnimator>();
        transform.localScale =
            Vector3.one * bossScale;

        initialized = true;
    }

    private void Update()
    {
        AnimateWobble();

        if (destroyed)
            return;

        MineGameManager game =
            MineGameManager.Instance;

        MinerController miner =
            game?.Board?.Miner;

        if (game == null ||
            !game.IsPlaying ||
            miner == null)
        {
            return;
        }

        UpdateBerserkMode();
        UpdateMovement(game.Board, miner);

        if (attackInProgress)
            return;

        if (!shootingEnabled)
            return;

        // Im Nahbereich verfolgt und rammt der Boss den Miner,
        // schießt aber nicht. So bleibt der Nahkampf fair und lesbar.
        if (IsMinerTooCloseToFire(miner))
            return;

        fireTimer -= Time.deltaTime;

        if (fireTimer > 0f)
            return;

        StartCoroutine(
            FirePattern(miner));
    }

    /// <summary>
    /// Entspricht BreakableObstacle.Hit().
    /// true bedeutet, dass der Boss zerstört wurde.
    /// </summary>
    public void ReserveGridPosition(
        Vector2Int position)
    {
        GridPosition = position;
    }

    public bool Hit()
    {
        // The Mine Guardian is an avoidance hazard and cannot be damaged.
        return false;
    }

    public void PlayDestroyedEffect()
    {
        BossExplosionAnimation.Create(
            transform.position,
            transform.parent);

        Camera.main?
            .GetComponent<CameraShake>()?
            .Shake(
                0.65f,
                0.24f);
    }

    private IEnumerator FirePattern(
        MinerController miner)
    {
        attackInProgress = true;

        Vector2 aimed =
            GetCardinalDirection(
                miner.transform.position -
                transform.position);

        switch (attackPattern)
        {
            case BossAttackPattern.AimedSingle:
                SpawnProjectile(aimed, false);
                break;

            case BossAttackPattern.Cross:
                FireCross(false);
                break;

            case BossAttackPattern.AimedBurst:
                yield return FireBurst(
                    aimed,
                    3,
                    0.16f,
                    false);
                break;

            case BossAttackPattern.TripleLane:
                FireTripleLane(
                    aimed,
                    false);
                break;

            case BossAttackPattern.RapidFire:
                yield return FireBurst(
                    aimed,
                    6,
                    0.10f,
                    false);
                break;

            case BossAttackPattern.Ricochet:
                SpawnProjectile(
                    aimed,
                    true);

                FirePerpendicular(
                    aimed,
                    true);
                break;

            case BossAttackPattern.MineLayer:
                SpawnMine();
                SpawnProjectile(
                    aimed,
                    false);
                break;

            case BossAttackPattern.CrossAndMines:
                FireCross(false);
                SpawnMine();
                break;

            case BossAttackPattern.Berserker:
                yield return FireBurst(
                    aimed,
                    berserkActive ? 8 : 4,
                    0.09f,
                    false);

                FirePerpendicular(
                    aimed,
                    false);
                break;

            case BossAttackPattern.MineKing:
                FireCross(true);
                FireTripleLane(
                    aimed,
                    true);

                SpawnMine();

                yield return new WaitForSeconds(
                    0.12f);

                SpawnProjectile(
                    aimed,
                    true);
                break;
        }

        float interval =
            berserkActive
                ? fireInterval * 0.48f
                : fireInterval;

        fireTimer = interval;
        attackInProgress = false;
    }

    private IEnumerator FireBurst(
        Vector2 direction,
        int count,
        float delay,
        bool ricochet)
    {
        for (int index = 0;
             index < count;
             index++)
        {
            if (destroyed)
                yield break;

            MinerController miner =
                MineGameManager.Instance?.Board?.Miner;

            if (miner == null ||
                IsMinerTooCloseToFire(miner))
            {
                yield break;
            }

            SpawnProjectile(
                direction,
                ricochet);

            yield return new WaitForSeconds(
                delay);
        }
    }

    private void FireCross(
        bool ricochet)
    {
        SpawnProjectile(
            Vector2.up,
            ricochet);

        SpawnProjectile(
            Vector2.right,
            ricochet);

        SpawnProjectile(
            Vector2.down,
            ricochet);

        SpawnProjectile(
            Vector2.left,
            ricochet);
    }

    private void FirePerpendicular(
        Vector2 aimed,
        bool ricochet)
    {
        if (Mathf.Abs(aimed.x) > 0.1f)
        {
            SpawnProjectile(
                Vector2.up,
                ricochet);

            SpawnProjectile(
                Vector2.down,
                ricochet);
        }
        else
        {
            SpawnProjectile(
                Vector2.left,
                ricochet);

            SpawnProjectile(
                Vector2.right,
                ricochet);
        }
    }

    private void FireTripleLane(
        Vector2 aimed,
        bool ricochet)
    {
        SpawnProjectile(
            aimed,
            ricochet);

        FirePerpendicular(
            aimed,
            ricochet);
    }

    private void SpawnProjectile(
        Vector2 direction,
        bool ricochet)
    {
        MinerController miner =
            MineGameManager.Instance?.Board?.Miner;

        if (miner == null ||
            IsMinerTooCloseToFire(miner))
        {
            return;
        }

        GameObject projectileObject =
            new("Boss Projectile");

        projectileObject.transform.position =
            transform.position;

        projectileObject.transform.SetParent(
            transform.parent);

        BossProjectile projectile =
            projectileObject.AddComponent<BossProjectile>();

        int bounces =
            ricochet
                ? Mathf.Clamp(
                    1 + bossTier / 3,
                    1,
                    4)
                : 0;

        float speedMultiplier =
            berserkActive
                ? 1.25f
                : 1f;

        projectile.Initialize(
            direction,
            projectileSpeed *
            speedMultiplier,
            bounces);
    }

    private void SpawnMine()
    {
        MineGameManager game =
            MineGameManager.Instance;

        MineBoard board =
            game?.Board;

        if (board == null ||
            board.Miner == null)
        {
            return;
        }

        Vector2Int[] offsets =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        int startIndex =
            Random.Range(
                0,
                offsets.Length);

        for (int index = 0;
             index < offsets.Length;
             index++)
        {
            Vector2Int position =
                GridPosition +
                offsets[
                    (startIndex + index) %
                    offsets.Length];

            if (!IsFreeMinePosition(
                    board,
                    position))
            {
                continue;
            }

            GameObject mineObject =
                new("Boss Mine");

            mineObject.transform.SetParent(
                transform.parent);

            BossMine mine =
                mineObject.AddComponent<BossMine>();

            mine.Initialize(
                position,
                4.5f,
                0.42f);

            return;
        }
    }

    private static bool IsFreeMinePosition(
        MineBoard board,
        Vector2Int position)
    {
        return
            board.IsInside(position) &&
            !board.IsWall(position) &&
            !board.IsExit(position) &&
            board.GetObstacle(position) == null &&
            board.GetBoss(position) == null &&
            board.GetCoal(position) == null &&
            board.Miner.GridPosition != position &&
            !board.Tail.Contains(position);
    }


    private bool IsMinerTooCloseToFire(
        MinerController miner)
    {
        if (miner == null)
            return true;

        return Vector2.Distance(
            transform.position,
            miner.transform.position) <
            minimumFireDistance;
    }

    private void CreateHealthBar()
    {
        if (healthBarRoot != null)
            return;

        healthBarRoot =
            new GameObject("Boss Health Bar");

        healthBarRoot.transform.SetParent(
            transform.parent);

        GameObject backgroundObject =
            new("Background");

        backgroundObject.transform.SetParent(
            healthBarRoot.transform);

        healthBarBackground =
            backgroundObject.AddComponent<SpriteRenderer>();

        healthBarBackground.sprite =
            GetHealthBarSprite();

        healthBarBackground.color =
            new Color(
                0.08f,
                0.03f,
                0.03f,
                0.96f);

        healthBarBackground.sortingOrder = 20;

        GameObject fillObject =
            new("Fill");

        fillObject.transform.SetParent(
            healthBarRoot.transform);

        healthBarFill =
            fillObject.AddComponent<SpriteRenderer>();

        healthBarFill.sprite =
            GetHealthBarSprite();

        healthBarFill.color =
            new Color(
                0.20f,
                0.90f,
                0.22f,
                1f);

        healthBarFill.sortingOrder = 21;

        UpdateHealthBarPosition();
    }

    private void UpdateHealthBarPosition()
    {
        if (healthBarRoot == null)
            return;

        healthBarRoot.transform.position =
            transform.position +
            Vector3.up * healthBarOffsetY;

        // Die Anzeige bleibt gerade, auch wenn der Boss wackelt.
        healthBarRoot.transform.rotation =
            Quaternion.identity;
    }

    private void UpdateHealthBar()
    {
        if (healthBarRoot == null ||
            healthBarBackground == null ||
            healthBarFill == null)
        {
            return;
        }

        float ratio =
            maximumHitPoints <= 0
                ? 0f
                : Mathf.Clamp01(
                    (float)hitPoints /
                    maximumHitPoints);

        healthBarBackground.transform.localPosition =
            Vector3.zero;

        healthBarBackground.transform.localScale =
            new Vector3(
                healthBarWidth,
                healthBarHeight,
                1f);

        healthBarFill.transform.localScale =
            new Vector3(
                healthBarWidth * ratio,
                healthBarHeight * 0.68f,
                1f);

        healthBarFill.transform.localPosition =
            new Vector3(
                -healthBarWidth *
                (1f - ratio) *
                0.5f,
                0f,
                -0.01f);

        healthBarFill.color =
            Color.Lerp(
                new Color(0.92f, 0.08f, 0.04f),
                new Color(0.20f, 0.90f, 0.22f),
                ratio);

        healthBarRoot.SetActive(
            !destroyed &&
            hitPoints > 0);
    }

    private static Sprite GetHealthBarSprite()
    {
        if (healthBarSprite != null)
            return healthBarSprite;

        Texture2D texture =
            new Texture2D(1, 1)
            {
                name = "Runtime Boss Health Bar",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave
            };

        texture.SetPixel(
            0,
            0,
            Color.white);

        texture.Apply();

        healthBarSprite =
            Sprite.Create(
                texture,
                new Rect(
                    0f,
                    0f,
                    1f,
                    1f),
                new Vector2(
                    0.5f,
                    0.5f),
                1f);

        healthBarSprite.name =
            "Runtime Boss Health Bar Sprite";

        return healthBarSprite;
    }

    private void OnDestroy()
    {
        if (healthBarRoot != null)
            Object.Destroy(healthBarRoot);
    }

    private void UpdateBerserkMode()
    {
        if (bossTier < 9 ||
            berserkActive)
        {
            return;
        }

        if (hitPoints <=
            Mathf.CeilToInt(
                maximumHitPoints *
                0.5f))
        {
            berserkActive = true;
        }
    }


    private void UpdateMovement(
        MineBoard board,
        MinerController miner)
    {
        if (destroyed ||
            movementInProgress ||
            attackInProgress ||
            Time.time < movementBlockedUntil)
        {
            return;
        }

        moveTimer -= Time.deltaTime;

        if (moveTimer > 0f)
            return;

        moveTimer = moveInterval;

        Vector2Int target =
            FindNextMoveTarget(
                board,
                miner.GridPosition);

        if (target == GridPosition)
            return;

        if (target == miner.GridPosition)
        {
            MineGameManager.Instance?
                .BossProjectileHit();

            return;
        }

        if (!board.TryReserveBossMove(
                this,
                target))
        {
            return;
        }

        StartCoroutine(
            MoveToGridPosition(
                target));
    }

    private Vector2Int FindNextMoveTarget(
        MineBoard board,
        Vector2Int playerPosition)
    {
        Vector2Int difference =
            playerPosition - GridPosition;

        Vector2Int horizontal =
            difference.x == 0
                ? Vector2Int.zero
                : difference.x > 0
                    ? Vector2Int.right
                    : Vector2Int.left;

        Vector2Int vertical =
            difference.y == 0
                ? Vector2Int.zero
                : difference.y > 0
                    ? Vector2Int.up
                    : Vector2Int.down;

        bool preferHorizontal =
            Mathf.Abs(difference.x) >=
            Mathf.Abs(difference.y);

        Vector2Int first =
            preferHorizontal
                ? horizontal
                : vertical;

        Vector2Int second =
            preferHorizontal
                ? vertical
                : horizontal;

        Vector2Int firstTarget =
            GridPosition + first;

        if (first != Vector2Int.zero &&
            board.CanBossMoveTo(
                this,
                firstTarget))
        {
            return firstTarget;
        }

        Vector2Int secondTarget =
            GridPosition + second;

        if (second != Vector2Int.zero &&
            board.CanBossMoveTo(
                this,
                secondTarget))
        {
            return secondTarget;
        }

        // Falls der direkte Weg blockiert ist, darf der Boss seitlich ausweichen.
        Vector2Int[] alternatives =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        Vector2Int bestPosition =
            GridPosition;

        int bestDistance =
            int.MaxValue;

        foreach (Vector2Int direction in alternatives)
        {
            Vector2Int candidate =
                GridPosition + direction;

            if (!board.CanBossMoveTo(
                    this,
                    candidate))
            {
                continue;
            }

            int distance =
                Mathf.Abs(
                    playerPosition.x -
                    candidate.x) +
                Mathf.Abs(
                    playerPosition.y -
                    candidate.y);

            if (distance >= bestDistance)
                continue;

            bestDistance = distance;
            bestPosition = candidate;
        }

        return bestPosition;
    }

    private IEnumerator MoveToGridPosition(
        Vector2Int targetPosition)
    {
        movementInProgress = true;

        Vector2Int direction =
            targetPosition -
            GridPosition;

        spriteAnimator?
            .SetMovementDirection(
                direction);

        Vector3 start =
            transform.position;

        Vector3 target =
            new(
                targetPosition.x,
                targetPosition.y,
                start.z);

        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            float t =
                Mathf.Clamp01(
                    elapsed /
                    moveDuration);

            t = t * t *
                (3f - 2f * t);

            transform.position =
                Vector3.Lerp(
                    start,
                    target,
                    t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        movementInProgress = false;
    }

    private void AnimateWobble()
    {
        wobbleTime += Time.deltaTime;

        float angle =
            Mathf.Sin(
                wobbleTime *
                wobbleSpeed) *
            wobbleAngle;

        transform.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                angle);
    }

    private IEnumerator HitFlash()
    {
        if (spriteRenderer == null)
            yield break;

        Color original =
            spriteRenderer.color;

        spriteRenderer.color =
            Color.white;

        yield return new WaitForSeconds(
            0.08f);

        if (spriteRenderer != null)
            spriteRenderer.color = original;
    }

    private static int GetHitPoints(
        int tier)
    {
        int[] values =
        {
            4, 6, 8, 11, 14,
            18, 22, 27, 33, 42
        };

        return values[
            Mathf.Clamp(
                tier - 1,
                0,
                values.Length - 1)];
    }

    private static BossAttackPattern GetAttackPattern(
        int tier)
    {
        return
            (BossAttackPattern)
            Mathf.Clamp(
                tier - 1,
                0,
                9);
    }

    private static string GetBossName(
        int tier)
    {
        string[] names =
        {
            "MINE GUARD",
            "CAVE TURRET",
            "DEEP SNIPER",
            "TRIPLE CORE",
            "RAPID DRILL",
            "RICOCHET ENGINE",
            "MINE LAYER",
            "ROCK SUMMONER",
            "BERSERKER",
            "MINE KING"
        };

        return names[
            Mathf.Clamp(
                tier - 1,
                0,
                names.Length - 1)];
    }

    private static Vector2 GetCardinalDirection(
        Vector3 difference)
    {
        if (Mathf.Abs(difference.x) >=
            Mathf.Abs(difference.y))
        {
            return difference.x >= 0f
                ? Vector2.right
                : Vector2.left;
        }

        return difference.y >= 0f
            ? Vector2.up
            : Vector2.down;
    }
}
