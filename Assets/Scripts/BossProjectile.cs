using UnityEngine;

/// <summary>
/// Bossgeschoss mit optionalen Abprallern.
/// Berührt es den Miner, wird sofort Game Over ausgelöst.
/// </summary>
public sealed class BossProjectile : MonoBehaviour
{
    [SerializeField] private float maximumLifetime = 9f;
    [SerializeField] private float minerHitRadius = 0.22f;

    private Vector2 direction;
    private float speed;
    private float age;
    private int remainingBounces;
    private Vector2Int previousGridPosition;
    private bool initialized;

    public void Initialize(
        Vector2 movementDirection,
        float movementSpeed,
        int bounces = 0)
    {
        direction =
            movementDirection.normalized;

        speed =
            Mathf.Max(0.1f, movementSpeed);

        remainingBounces =
            Mathf.Max(0, bounces);

        previousGridPosition =
            ToGrid(transform.position);

        SpriteRenderer renderer =
            GetComponent<SpriteRenderer>();

        if (renderer == null)
        {
            renderer =
                gameObject.AddComponent<SpriteRenderer>();
        }

        bool ricochet =
            remainingBounces > 0;

        bool spriteApplied =
            BossGraphicsInstaller.ApplyProjectileSprite(
                renderer,
                ricochet);

        if (!spriteApplied)
        {
            renderer.color =
                ricochet
                    ? new Color(0.25f, 0.85f, 1f)
                    : new Color(1f, 0.35f, 0.08f);
        }

        renderer.sortingOrder = 8;

        transform.localScale =
            Vector3.one * 0.34f;

        initialized = true;
        RotateSprite();
    }

    private void Update()
    {
        if (!initialized)
            return;

        MineGameManager game =
            MineGameManager.Instance;

        MineBoard board =
            game?.Board;

        if (game == null ||
            board == null)
        {
            Destroy(gameObject);
            return;
        }

        if (game.State != GameState.Playing)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 previousPosition =
            transform.position;

        transform.position +=
            (Vector3)(
                direction *
                speed *
                Time.deltaTime);

        age += Time.deltaTime;

        if (age >= maximumLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (CheckMinerHit(
                game,
                board,
                previousPosition,
                transform.position))
        {
            return;
        }

        Vector2Int currentGridPosition =
            ToGrid(transform.position);

        if (currentGridPosition ==
            previousGridPosition)
        {
            return;
        }

        if (!board.IsInside(currentGridPosition))
        {
            Destroy(gameObject);
            return;
        }

        if (board.IsWall(currentGridPosition) ||
            board.GetObstacle(currentGridPosition) != null)
        {
            if (remainingBounces <= 0)
            {
                Destroy(gameObject);
                return;
            }

            transform.position =
                previousPosition;

            Bounce(
                board,
                currentGridPosition);

            remainingBounces--;
            RotateSprite();

            currentGridPosition =
                ToGrid(transform.position);
        }

        previousGridPosition =
            currentGridPosition;
    }

    private bool CheckMinerHit(
        MineGameManager game,
        MineBoard board,
        Vector2 previousPosition,
        Vector2 currentPosition)
    {
        MinerController miner =
            board.Miner;

        if (miner == null ||
            game.State != GameState.Playing)
        {
            return false;
        }

        Vector2 minerPosition =
            miner.transform.position;

        Vector2 movement =
            currentPosition -
            previousPosition;

        float lengthSquared =
            movement.sqrMagnitude;

        float t =
            lengthSquared <= 0.000001f
                ? 0f
                : Mathf.Clamp01(
                    Vector2.Dot(
                        minerPosition -
                        previousPosition,
                        movement) /
                    lengthSquared);

        Vector2 closestPoint =
            previousPosition +
            movement * t;

        if (Vector2.Distance(
                closestPoint,
                minerPosition) >
            minerHitRadius)
        {
            return false;
        }

        game.BossProjectileHit();
        Destroy(gameObject);
        return true;
    }

    private void Bounce(
        MineBoard board,
        Vector2Int blockedPosition)
    {
        Vector2Int horizontalTest = new(
            blockedPosition.x,
            previousGridPosition.y);

        Vector2Int verticalTest = new(
            previousGridPosition.x,
            blockedPosition.y);

        bool horizontalBlocked =
            !board.IsInside(horizontalTest) ||
            board.IsWall(horizontalTest) ||
            board.GetObstacle(horizontalTest) != null;

        bool verticalBlocked =
            !board.IsInside(verticalTest) ||
            board.IsWall(verticalTest) ||
            board.GetObstacle(verticalTest) != null;

        if (horizontalBlocked &&
            verticalBlocked)
        {
            direction = -direction;
        }
        else if (horizontalBlocked)
        {
            direction.x = -direction.x;
        }
        else
        {
            direction.y = -direction.y;
        }

        direction.Normalize();
    }

    private static Vector2Int ToGrid(
        Vector3 position)
    {
        return new Vector2Int(
            Mathf.RoundToInt(position.x),
            Mathf.RoundToInt(position.y));
    }

    private void RotateSprite()
    {
        float angle =
            Mathf.Atan2(
                direction.y,
                direction.x) *
            Mathf.Rad2Deg;

        transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                angle);
    }
}
