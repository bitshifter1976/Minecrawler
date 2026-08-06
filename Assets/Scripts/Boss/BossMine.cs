using UnityEngine;

/// <summary>
/// Armed mine left by the level boss.
/// Contact with an active mine immediately triggers Game Over.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BossMine : MonoBehaviour
{
    private enum MineState
    {
        Dropping,
        Arming,
        Armed,
        Removed
    }

    [SerializeField] private float armDelay = 0.52f;
    [SerializeField] private float dropDuration = 0.20f;
    [SerializeField] private float minerHitRadius = 0.22f;
    [SerializeField] private int sortingOrder = 9;

    private SpriteRenderer bodyRenderer;
    private SpriteRenderer lightRenderer;
    private Transform visualRoot;
    private BossMineManager owner;

    private Vector2Int gridPosition;
    private Vector3 dropStart;
    private Vector3 dropTarget;
    private MineState state;
    private float age;
    private float blinkTimer;
    private bool lightOn;

    private const float MineVisualScale = 0.52f;

    public Vector2Int GridPosition =>
        gridPosition;

    public void Initialize(
        Vector2Int position,
        Vector3 bossWorldPosition,
        float armingSeconds,
        BossMineManager manager)
    {
        gridPosition = position;
        owner = manager;
        armDelay =
            Mathf.Max(
                0.15f,
                armingSeconds);

        dropStart =
            bossWorldPosition +
            new Vector3(
                0f,
                0.20f,
                0f);

        dropTarget =
            new Vector3(
                position.x,
                position.y,
                0f);

        transform.position =
            dropStart;

        CreateVisuals();
        PlaySound(
            "Audio/mineDrop",
            0.48f);

        state =
            MineState.Dropping;
    }

    private void Update()
    {
        MineGameManager game =
            MineGameManager.Instance;

        if (game == null ||
            game.Board == null)
        {
            RemoveSilently();
            return;
        }

        if (game.State !=
            GameState.Playing)
        {
            return;
        }

        age +=
            Time.deltaTime;

        switch (state)
        {
            case MineState.Dropping:
                UpdateDrop();
                break;

            case MineState.Arming:
                UpdateArming();
                break;

            case MineState.Armed:
                UpdateArmed(game);
                break;
        }
    }

    public void RemoveSilently()
    {
        if (state ==
            MineState.Removed)
        {
            return;
        }

        state =
            MineState.Removed;

        owner?
            .NotifyMineRemoved(
                this);

        Destroy(gameObject);
    }

    public void ExplodeWithoutGameOver()
    {
        if (state ==
            MineState.Removed)
        {
            return;
        }

        state =
            MineState.Removed;

        BossMineExplosion.Create(
            transform.position,
            transform.parent,
            0.85f);

        owner?
            .NotifyMineRemoved(
                this);

        Destroy(gameObject);
    }

    private void UpdateDrop()
    {
        float progress =
            Mathf.Clamp01(
                age /
                Mathf.Max(
                    0.01f,
                    dropDuration));

        float eased =
            1f -
            Mathf.Pow(
                1f - progress,
                3f);

        transform.position =
            Vector3.Lerp(
                dropStart,
                dropTarget,
                eased);

        float squash =
            Mathf.Sin(
                progress *
                Mathf.PI) *
            0.08f;

        visualRoot.localScale =
            new Vector3(
                MineVisualScale * (1f + squash),
                MineVisualScale * (1f - squash),
                1f);

        if (progress < 1f)
            return;

        transform.position =
            dropTarget;

        visualRoot.localScale =
            Vector3.one *
            MineVisualScale;

        state =
            MineState.Arming;

        age = 0f;

        BossMineDust.Create(
            transform.position,
            transform.parent);
    }

    private void UpdateArming()
    {
        float progress =
            Mathf.Clamp01(
                age /
                armDelay);

        float pulse =
            1f +
            Mathf.Sin(
                progress *
                Mathf.PI *
                6f) *
            0.045f;

        visualRoot.localScale =
            Vector3.one *
            MineVisualScale *
            pulse;

        lightRenderer.enabled =
            Mathf.FloorToInt(
                progress * 8f) %
            2 == 0;

        if (age < armDelay)
            return;

        state =
            MineState.Armed;

        age = 0f;
        blinkTimer = 0f;
        lightOn = true;
        lightRenderer.enabled = true;

        PlaySound(
            "Audio/mineArm",
            0.40f);
    }

    private void UpdateArmed(
        MineGameManager game)
    {
        blinkTimer +=
            Time.deltaTime;

        if (blinkTimer >= 0.34f)
        {
            blinkTimer = 0f;
            lightOn = !lightOn;
            lightRenderer.enabled =
                lightOn;
        }

        float pulse =
            1f +
            Mathf.Sin(
                Time.time *
                5.5f) *
            0.025f;

        visualRoot.localScale =
            Vector3.one *
            MineVisualScale *
            pulse;

        MinerController miner =
            game.Board.Miner;

        if (miner == null)
            return;

        float distance =
            Vector2.Distance(
                transform.position,
                miner.transform.position);

        if (distance >
            minerHitRadius)
        {
            return;
        }

        TriggerMinerCollision(
            game);
    }

    private void TriggerMinerCollision(
        MineGameManager game)
    {
        if (state !=
            MineState.Armed)
        {
            return;
        }

        state =
            MineState.Removed;

        BossMineExplosion.Create(
            transform.position,
            transform.parent,
            1.05f);

        owner?
            .NotifyMineRemoved(
                this);

        game.BossProjectileHit();

        Destroy(gameObject);
    }

    private void CreateVisuals()
    {
        bodyRenderer =
            GetComponent<SpriteRenderer>();

        bodyRenderer.sprite =
            BossMineRuntimeSprites.Body;

        bodyRenderer.sortingOrder =
            sortingOrder;

        bodyRenderer.color =
            new Color(
                0.20f,
                0.18f,
                0.16f,
                1f);

        visualRoot =
            bodyRenderer.transform;

        visualRoot.localScale =
            Vector3.one *
            MineVisualScale;

        GameObject lightObject =
            new("Mine Warning Light");

        lightObject.transform.SetParent(
            transform,
            false);

        lightObject.transform.localPosition =
            new Vector3(
                0f,
                0.03f,
                0f);

        lightRenderer =
            lightObject.AddComponent<SpriteRenderer>();

        lightRenderer.sprite =
            BossMineRuntimeSprites.Light;

        lightRenderer.sortingOrder =
            sortingOrder + 1;

        lightRenderer.color =
            new Color(
                1f,
                0.10f,
                0.02f,
                1f);

        lightObject.transform.localScale =
            Vector3.one *
            0.34f;
    }

    private void PlaySound(
        string path,
        float volume)
    {
        AudioClip clip =
            Resources.Load<AudioClip>(
                path);

        if (clip == null)
            return;

        AudioSource.PlayClipAtPoint(
            clip,
            transform.position,
            volume);
    }
}
