using UnityEngine;

/// <summary>
/// Runtime walking animation for the horizontal eight-frame boss sheet.
/// The sprite size is calibrated so bossScale 0.92 occupies about one tile.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class LevelBossSpriteAnimator : MonoBehaviour
{
    private const int FrameCount = 8;
    private const float PixelsPerUnit = 192f;

    [SerializeField] private float framesPerSecond = 9f;
    [SerializeField] private bool animateOnlyWhileMoving = true;

    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;
    private Vector3 previousPosition;
    private float frameTimer;
    private int frameIndex;

    private void Awake()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();

        previousPosition =
            transform.position;

        LoadFrames();
    }

    private void Update()
    {
        if (frames == null ||
            frames.Length == 0)
        {
            return;
        }

        bool moving =
            (transform.position -
             previousPosition).sqrMagnitude >
            0.000001f;

        previousPosition =
            transform.position;

        if (animateOnlyWhileMoving &&
            !moving)
        {
            frameIndex = 0;
            frameTimer = 0f;
            spriteRenderer.sprite = frames[0];
            return;
        }

        frameTimer += Time.deltaTime;

        float duration =
            1f /
            Mathf.Max(
                1f,
                framesPerSecond);

        while (frameTimer >= duration)
        {
            frameTimer -= duration;
            frameIndex =
                (frameIndex + 1) %
                frames.Length;

            spriteRenderer.sprite =
                frames[frameIndex];
        }
    }

    public void SetMovementDirection(
        Vector2Int direction)
    {
        if (spriteRenderer == null)
            return;

        if (direction.x != 0)
        {
            spriteRenderer.flipX =
                direction.x < 0;
        }
    }

    private void LoadFrames()
    {
        Texture2D texture =
            Resources.Load<Texture2D>(
                "Art/LevelBossWalk");

        if (texture == null)
        {
            Debug.LogWarning(
                "LevelBossWalk not found at " +
                "Resources/Art/LevelBossWalk.",
                this);

            return;
        }

        int frameWidth =
            texture.width /
            FrameCount;

        frames =
            new Sprite[FrameCount];

        for (int index = 0;
             index < FrameCount;
             index++)
        {
            frames[index] =
                Sprite.Create(
                    texture,
                    new Rect(
                        index * frameWidth,
                        0f,
                        frameWidth,
                        texture.height),
                    new Vector2(
                        0.5f,
                        0.5f),
                    PixelsPerUnit,
                    0,
                    SpriteMeshType.FullRect);
        }

        spriteRenderer.sprite =
            frames[0];
    }
}
