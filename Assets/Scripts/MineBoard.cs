using System.Collections.Generic;
using UnityEngine;
using Color = UnityEngine.Color;

public sealed class MineBoard
{
    private readonly Transform parent;
    private readonly Sprite squareSprite;
    private readonly Sprite wallSprite;
    private readonly Sprite rockSprite;
    private readonly Sprite coalSprite;
    private MineTail tail;
    private readonly HashSet<Vector2Int> walls = new();
    private readonly Dictionary<Vector2Int, CoalPickup> coal = new();
    private readonly Dictionary<Vector2Int, BreakableObstacle> obstacles = new();
    private readonly Dictionary<Vector2Int, LevelBoss> bosses = new();
    private GameObject exitDoorObject;
    private Sprite floorSprite;
    private Sprite doorClosedSprite;
    private Sprite doorOpenSprite;
    private Sprite cartSprite;
    private Sprite minerSprite;
    private Sprite keySprite;
    private Sprite bossSprite;
    private Sprite bossProjectileSprite;
    private int levelNumber;
    private KeyPickup spawnedKey;

    public MinerController Miner { get; private set; }
    public Vector2Int ExitPosition { get; private set; }
    public bool ExitExists { get; private set; }
    public bool ExitOpen { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int RemainingCoal => coal.Count;
    public int RemainingObstacles => obstacles.Count;
    public int RemainingBosses => bosses.Count;
    public LevelBoss ActiveBoss
    {
        get
        {
            foreach (LevelBoss boss in bosses.Values)
                return boss;

            return null;
        }
    }
    public bool IsLevelCleared =>
        RemainingCoal == 0 &&
        RemainingObstacles == 0 &&
        RemainingBosses == 0;
    public MineTail Tail => tail;
    public KeyPickup SpawnedKey => spawnedKey;

    public MineBoard(Transform parent, Sprite squareSprite, Sprite wallSprite, Sprite rockSprite, Sprite coalSprite, Sprite floorSprite, Sprite doorClosedSprite, Sprite doorOpenSprite, Sprite cartSprite, Sprite minerSprite, Sprite keySprite, Sprite bossSprite, Sprite bossProjectileSprite)
    {
        this.parent = parent;
        this.squareSprite = squareSprite;
        this.wallSprite = wallSprite;
        this.rockSprite = rockSprite;
        this.coalSprite = coalSprite;
        this.floorSprite = floorSprite;
        this.doorClosedSprite = doorClosedSprite;
        this.doorOpenSprite = doorOpenSprite;
        this.cartSprite = cartSprite;
        this.minerSprite = minerSprite;
        this.keySprite = keySprite;
        this.bossSprite = bossSprite;
        this.bossProjectileSprite = bossProjectileSprite;
        tail = new MineTail();
    }

    public bool Build(MineLevelData level, int currentLevelNumber)
    {
        if (level == null)
            return false;

        Clear();

        levelNumber = currentLevelNumber;
        Width = level.Width;
        Height = level.Height;

        bool playerCreated = false;
        bool exitCreated = false;

        for (int row = 0; row < level.Height; row++)
        {
            int y = level.Height - 1 - row;

            for (int x = 0; x < level.Width; x++)
            {
                Vector2Int position =
                    new(x, y);

                char tileCharacter =
                    level.GetTile(x, row);

                CreateTile(
                    "Floor",
                    position,
                    Color.white,
                    -2,
                    1f,
                    floorSprite,
                    true
                );

                switch (tileCharacter)
                {
                    case '#':
                        CreateWall(position, 1f, 1);
                        break;

                    case 'C':
                        CreateCoal(position);
                        break;

                    case 'B':
                        CreateObstacle(position, 0.75f, 1);
                        break;

                    case 'X':
                        CreateBoss(position);
                        break;

                    case 'P':
                        if (!playerCreated)
                        {
                            CreateMiner(position);
                            playerCreated = true;
                        }
                        else
                        {
                            Debug.LogWarning(
                                "Das Level enthält mehr als einen Spieler."
                            );
                        }

                        break;

                    case 'E':
                        if (!exitCreated)
                        {
                            CreateExit(position);
                            exitCreated = true;
                        }
                        else
                        {
                            Debug.LogWarning(
                                "Das Level enthält mehr als einen Ausgang."
                            );
                        }

                        break;

                    case '.':
                    case ' ':
                        break;

                    default:
                        Debug.LogWarning(
                            $"Unbekanntes Levelzeichen " +
                            $"'{tileCharacter}' bei " +
                            $"X={x}, Y={y}."
                        );
                        break;
                }
            }
        }

        if (!playerCreated)
        {
            Debug.LogError(
                "Das Level enthält keinen Spieler 'P'."
            );

            return false;
        }

        if (!exitCreated)
        {
            Debug.LogError(
                "Das Level enthält keinen Ausgang 'E'."
            );

            return false;
        }

        ExitOpen = false;
        UpdateExitAppearance();

        return true;
    }

    public void Clear()
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            GameObject child =
                parent.GetChild(i).gameObject;

            child.SetActive(false);
            Object.Destroy(child);
        }

        walls.Clear();
        coal.Clear();
        obstacles.Clear();
        bosses.Clear();

        Miner = null;

        exitDoorObject = null;
        ExitPosition = default;
        ExitExists = false;
        ExitOpen = false;

        Width = 0;
        Height = 0;
    }

    public bool IsInside(Vector2Int position)
    {
        return position.x >= 0 &&
               position.x < Width &&
               position.y >= 0 &&
               position.y < Height;
    }

    public bool IsWall(Vector2Int position)
    {
        return walls.Contains(position);
    }

    public bool IsExit(Vector2Int position)
    {
        return ExitExists && position == ExitPosition;
    }

    public CoalPickup GetCoal(Vector2Int position)
    {
        coal.TryGetValue(
            position,
            out CoalPickup pickup
        );

        return pickup;
    }

    public BreakableObstacle GetObstacle(
        Vector2Int position)
    {
        obstacles.TryGetValue(
            position,
            out BreakableObstacle obstacle
        );

        return obstacle;
    }


    public LevelBoss GetBoss(Vector2Int position)
    {
        bosses.TryGetValue(
            position,
            out LevelBoss boss);

        return boss;
    }


    public bool CanBossMoveTo(
        LevelBoss boss,
        Vector2Int position)
    {
        if (boss == null)
            return false;

        if (!IsInside(position))
            return false;

        if (IsWall(position) ||
            IsExit(position))
        {
            return false;
        }

        if (GetObstacle(position) != null ||
            GetCoal(position) != null ||
            GetBoss(position) != null)
        {
            return false;
        }

        if (spawnedKey != null &&
            spawnedKey.GridPosition == position)
        {
            return false;
        }

        if (tail.Contains(position))
            return false;

        // Das Spielerfeld ist erlaubt, damit der Boss den Miner treffen kann.
        return true;
    }

    public bool TryReserveBossMove(
        LevelBoss boss,
        Vector2Int targetPosition)
    {
        if (!CanBossMoveTo(
                boss,
                targetPosition))
        {
            return false;
        }

        Vector2Int oldPosition =
            boss.GridPosition;

        bosses.Remove(oldPosition);
        bosses[targetPosition] = boss;

        boss.ReserveGridPosition(
            targetPosition);

        return true;
    }

    public void RemoveBoss(LevelBoss boss)
    {
        if (boss == null)
            return;

        bosses.Remove(boss.GridPosition);
        Object.Destroy(boss.gameObject);
    }

    public void RemoveObstacle(
        BreakableObstacle obstacle)
    {
        if (obstacle == null)
            return;

        obstacles.Remove(obstacle.GridPosition);

        Object.Destroy(obstacle.gameObject);
    }

    public bool RemoveCoal(CoalPickup pickup)
    {
        if (pickup == null)
            return false;

        bool removed =
            coal.Remove(pickup.GridPosition);

        if (removed)
            Object.Destroy(pickup.gameObject);

        return removed;
    }

    public void OpenExit()
    {
        if (!ExitExists || ExitOpen)
            return;

        ExitOpen = true;

        UpdateExitAppearance();
    }

    public TailSegment CreateTailSegment(
        Vector2Int position)
    {
        return CreateActor<TailSegment>(
            "Coal Cart",
            position,
            Color.white,
            2,
            1f,
            cartSprite
        );
    }

    private void CreateWall(Vector2Int position, float scale, int order)
    {
        walls.Add(position);

        CreateTile(
            "Wall",
            position,
            Color.white,
            order,
            scale,
            wallSprite
        );
    }

    private void CreateCoal(Vector2Int position)
    {
        CoalPickup pickup =
            CreateActor<CoalPickup>(
                "Coal",
                position,
                Color.white,
                1,
                0.95f,
                coalSprite
            );

        coal[position] = pickup;
    }

    private void CreateObstacle(Vector2Int position, float scale, int order)
    {
        BreakableObstacle obstacle =
            CreateActor<BreakableObstacle>(
                "Breakable Rock",
                position,
                Color.white,
                order,
                scale,
                rockSprite
            );

        obstacles[position] = obstacle;
    }


    private void CreateBoss(Vector2Int position)
    {
        LevelBoss boss =
            CreateActor<LevelBoss>(
                "Level Boss",
                position,
                Color.white,
                4,
                1f,
                bossSprite);

        bosses[position] = boss;
        boss.Initialize(
            levelNumber,
            Miner,
            bossProjectileSprite);
    }

    private void CreateMiner(Vector2Int position)
    {
        Miner = CreateActor<MinerController>(
            "Miner",
            position,
            new Color(0.95f, 0.72f, 0.18f),
            3,
            1f,
            minerSprite
        );
    }

    private void CreateExit(Vector2Int position)
    {
        ExitExists = true;
        ExitPosition = position;

        exitDoorObject = CreateTile(
            "Exit Door",
            position,
            Color.white,
            1,
            1f,
            doorClosedSprite
        );
    }

    private void UpdateExitAppearance()
    {
        if (exitDoorObject == null)
            return;

        var renderer = exitDoorObject.GetComponent<SpriteRenderer>();
        if (renderer == null)
            return;

        if (ExitOpen)
            renderer.sprite = doorOpenSprite;
        else
            renderer.sprite = doorClosedSprite;
    }

    private T CreateActor<T>(
        string objectName,
        Vector2Int position,
        Color color,
        int order,
        float scale,
        Sprite sprite = null
    ) where T : GridActor
    {
        GameObject gameObject = CreateTile(
            objectName,
            position,
            color,
            order,
            scale,
            sprite
        );

        T actor = gameObject.AddComponent<T>();

        actor.SetGridPosition(position);

        return actor;
    }

    private GameObject CreateTile(
        string objectName,
        Vector2Int position,
        Color color,
        int order,
        float scale,
        Sprite sprite = null,
        bool rotate = false)
    {
        GameObject gameObject =
            new(objectName);

        gameObject.transform.SetParent(parent);

        gameObject.transform.position =
            new Vector3(
                position.x,
                position.y,
                0f
            );

        gameObject.transform.localScale =
            Vector3.one * scale;

        if (rotate)
        {
            var rand = Random.Range(0, 4);
            var angle = 0f;
            if (rand == 0)
                angle = 0f;
            else if (rand == 1)
                angle = 90f;
            else if (rand == 2)
                angle = 180f;
            else if (rand == 3)
                angle = 270f;
            gameObject.transform.Rotate(0f, 0f, angle);
        }

        SpriteRenderer renderer =
            gameObject.AddComponent<SpriteRenderer>();

        if (sprite != null)
            renderer.sprite = sprite;
        else
            renderer.sprite = squareSprite;
        renderer.color = color;
        renderer.sortingOrder = order;

        return gameObject;
    }
    public void RemoveKey()
    {
        if (spawnedKey == null)
            return;

        GameObject.Destroy(spawnedKey.gameObject);
        spawnedKey = null;
    }

    public void SpawnKey()
    {
        if (spawnedKey != null)
            return;

        var pos = FindFreeKeyPosition();
        var keyTile = CreateTile("Key", pos, Color.yellow, 2, 0.5f, keySprite);
        spawnedKey = keyTile.AddComponent<KeyPickup>();
        spawnedKey.SetGridPosition(pos);
    }

    private Vector2Int FindFreeKeyPosition()
    {
        Vector2Int minerPosition = Miner.GridPosition;
        Vector2Int bestPosition = minerPosition;
        float bestDistance = -1f;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                Vector2Int position = new(x, y);

                if (!IsValidKeyPosition(position))
                    continue;

                float distance =
                    Vector2Int.Distance(minerPosition, position);

                if (distance > bestDistance)
                {
                    bestDistance = distance;
                    bestPosition = position;
                }
            }
        }

        return bestPosition;
    }

    private bool IsValidKeyPosition(Vector2Int position)
    {
        if (!IsInside(position))
            return false;

        if (IsWall(position))
            return false;

        if (IsExit(position))
            return false;

        if (GetObstacle(position) != null)
            return false;

        if (GetCoal(position) != null)
            return false;

        if (GetBoss(position) != null)
            return false;

        if (tail.Contains(position))
            return false;

        if (Miner.GridPosition == position)
            return false;

        return true;
    }

    public void CollectKey()
    {
        if (spawnedKey == null)
            return;

        GameObject.Destroy(spawnedKey.gameObject);
        spawnedKey = null;

        OpenExit();
    }
}