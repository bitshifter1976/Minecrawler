using UnityEngine;

/// <summary>
/// Vom Boss gelegte Mine.
/// Nach einer kurzen Aktivierungszeit führt eine Berührung
/// durch den Miner sofort zu Game Over.
/// </summary>
public sealed class BossMine : MonoBehaviour
{
    [SerializeField] private float armDelay = 0.45f;

    private Vector2Int gridPosition;
    private float lifetime;
    private float hitRadius;
    private float age;

    public Vector2Int GridPosition =>
        gridPosition;

    public void Initialize(
        Vector2Int position,
        float maximumLifetime,
        float minerHitRadius)
    {
        gridPosition = position;

        lifetime =
            Mathf.Max(
                1f,
                maximumLifetime);

        hitRadius =
            Mathf.Max(
                0.1f,
                minerHitRadius);

        transform.position =
            new Vector3(
                position.x,
                position.y,
                0f);

        SpriteRenderer renderer =
            GetComponent<SpriteRenderer>();

        if (renderer == null)
        {
            renderer =
                gameObject.AddComponent<SpriteRenderer>();
        }

        bool spriteApplied =
            BossGraphicsInstaller.ApplyMineSprite(
                renderer);

        if (!spriteApplied)
        {
            renderer.color =
                new Color(
                    0.95f,
                    0.15f,
                    0.05f);
        }

        renderer.sortingOrder = 7;

        transform.localScale =
            Vector3.one * 0.7f;
    }

    private void Update()
    {
        MineGameManager game =
            MineGameManager.Instance;

        MinerController miner =
            game?.Board?.Miner;

        if (game == null ||
            game.Board == null)
        {
            Destroy(gameObject);
            return;
        }

        if (!game.IsPlaying)
            return;

        age += Time.deltaTime;

        AnimatePulse();

        if (age >= lifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (age < armDelay ||
            miner == null)
        {
            return;
        }

        float distance =
            Vector2.Distance(
                transform.position,
                miner.transform.position);

        if (distance > hitRadius)
            return;

        game.BossProjectileHit();
        Destroy(gameObject);
    }

    private void AnimatePulse()
    {
        float pulse =
            (Mathf.Sin(age * 8f) + 1f) *
            0.5f;

        transform.localScale =
            Vector3.one *
            Mathf.Lerp(
                0.55f,
                0.78f,
                pulse);
    }
}
