using System.Collections;
using UnityEngine;

/// <summary>
/// Runtime animator for the 5x4 top-down LevelBossWalk spritesheet.
///
/// Row order in the PNG:
/// 0 = Down / Front
/// 1 = Right
/// 2 = Up / Back
/// 3 = Left
///
/// Each row contains five equally sized walk frames.
/// No Animator Controller or manual Unity slicing is required.
/// </summary>
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class LevelBossSpriteAnimator : MonoBehaviour
{
    private const int Columns = 5;
    private const int Rows = 4;
    private const int FrameCount = Columns * Rows;

    private enum Facing
    {
        Down = 0,
        Right = 1,
        Up = 2,
        Left = 3
    }

    [Header("Animation")]
    [SerializeField] private float walkFramesPerSecond = 8f;
    [SerializeField] private float idleFramesPerSecond = 2.6f;
    [SerializeField] private float pixelsPerUnit = 220f;

    [Header("Visual Size")]
    [SerializeField, Range(1f, 2.5f)]
    private float visualScale = 1.78f;

    [SerializeField, Range(-0.5f, 0.5f)]
    private float visualOffsetY = 0.10f;

    [SerializeField] private bool createShadow = true;
    [SerializeField, Range(0.1f, 1f)]
    private float shadowOpacity = 0.32f;

    [Header("Heavy Movement")]
    [SerializeField] private float walkBobHeight = 0.050f;
    [SerializeField] private float walkTiltAngle = 1.8f;
    [SerializeField] private bool spawnStepDust = true;

    [Header("Optional Audio")]
    [SerializeField] private AudioClip stepClip;
    [Range(0f, 1f)]
    [SerializeField] private float stepVolume = 0.28f;

    private readonly int[] idleSequence =
    {
        0, 1, 0, 2
    };

    private readonly int[] walkSequence =
    {
        0, 1, 2, 3, 4, 3, 2, 1
    };

    private SpriteRenderer rootRenderer;
    private SpriteRenderer visualRenderer;
    private Transform visualTransform;
    private SpriteRenderer shadowRenderer;
    private Transform shadowTransform;
    private AudioSource audioSource;

    private Sprite[] frames;
    private Facing facing = Facing.Down;
    private bool moving;
    private bool locked;
    private float frameTimer;
    private int sequenceIndex;
    private int previousWalkFrame = -1;
    private Vector3 baseLocalPosition;
    private Vector3 baseLocalScale;

    private void Awake()
    {
        CreateVisualRenderer();
        LoadFrames();

        if (stepClip == null)
        {
            stepClip =
                Resources.Load<AudioClip>(
                    "Audio/bossStep");
        }

        audioSource =
            gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        audioSource.dopplerLevel = 0f;

        SetCurrentSprite(0);
    }

    private void Update()
    {
        if (locked ||
            frames == null ||
            frames.Length != FrameCount)
        {
            return;
        }

        int[] sequence =
            moving
                ? walkSequence
                : idleSequence;

        float framesPerSecond =
            moving
                ? walkFramesPerSecond
                : idleFramesPerSecond;

        frameTimer += Time.deltaTime;

        float frameDuration =
            1f / Mathf.Max(
                1f,
                framesPerSecond);

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            sequenceIndex =
                (sequenceIndex + 1) %
                sequence.Length;

            int localFrame =
                sequence[sequenceIndex];

            SetCurrentSprite(localFrame);

            if (moving)
                HandleHeavyStep(localFrame);
        }

        UpdateMovementPose();
    }

    public void SetMovementDirection(
        Vector2Int direction)
    {
        if (direction == Vector2Int.zero)
            return;

        if (Mathf.Abs(direction.x) >
            Mathf.Abs(direction.y))
        {
            facing =
                direction.x > 0
                    ? Facing.Right
                    : Facing.Left;
        }
        else
        {
            facing =
                direction.y > 0
                    ? Facing.Up
                    : Facing.Down;
        }

        sequenceIndex = 0;
        frameTimer = 0f;
        SetCurrentSprite(0);
    }

    public void FaceDirection(
        Vector2 direction)
    {
        Vector2Int gridDirection;

        if (Mathf.Abs(direction.x) >=
            Mathf.Abs(direction.y))
        {
            gridDirection =
                direction.x >= 0f
                    ? Vector2Int.right
                    : Vector2Int.left;
        }
        else
        {
            gridDirection =
                direction.y >= 0f
                    ? Vector2Int.up
                    : Vector2Int.down;
        }

        SetMovementDirection(
            gridDirection);
    }

    public void SetMoving(bool value)
    {
        if (moving == value)
            return;

        moving = value;
        sequenceIndex = 0;
        frameTimer = 0f;
        previousWalkFrame = -1;

        if (!moving)
        {
            visualTransform.localPosition =
                baseLocalPosition;

            visualTransform.localRotation =
                Quaternion.identity;
        }

        SetCurrentSprite(0);
    }

    public void PlayAttack()
    {
        if (!isActiveAndEnabled)
            return;

        StartCoroutine(
            AttackRoutine());
    }

    public void PlayHit()
    {
        if (!isActiveAndEnabled)
            return;

        StopCoroutine(
            nameof(HitRoutine));

        StartCoroutine(
            HitRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        locked = true;

        SetCurrentSprite(2);

        Vector3 startScale =
            baseLocalScale;

        visualTransform.localScale =
            startScale * 1.08f;

        yield return new WaitForSeconds(
            0.09f);

        visualTransform.localScale =
            startScale;

        locked = false;
    }

    private IEnumerator HitRoutine()
    {
        Color originalColor =
            visualRenderer.color;

        Vector3 originalScale =
            baseLocalScale;

        locked = true;
        visualRenderer.color =
            new Color(
                1f,
                0.78f,
                0.28f,
                1f);

        visualTransform.localScale =
            originalScale * 1.12f;

        yield return new WaitForSecondsRealtime(
            0.055f);

        visualRenderer.color =
            Color.white;

        visualTransform.localScale =
            originalScale * 0.94f;

        yield return new WaitForSecondsRealtime(
            0.055f);

        visualRenderer.color =
            originalColor;

        visualTransform.localScale =
            originalScale;

        locked = false;
    }

    private void HandleHeavyStep(
        int localFrame)
    {
        if (localFrame == previousWalkFrame)
            return;

        previousWalkFrame =
            localFrame;

        bool isImpactFrame =
            localFrame == 1 ||
            localFrame == 3;

        if (!isImpactFrame)
            return;

        if (audioSource != null &&
            stepClip != null)
        {
            audioSource.pitch =
                Random.Range(
                    0.94f,
                    1.05f);

            audioSource.PlayOneShot(
                stepClip,
                stepVolume);
        }

        if (spawnStepDust)
            SpawnDust();
    }

    private void UpdateMovementPose()
    {
        if (!moving)
        {
            visualTransform.localPosition =
                Vector3.Lerp(
                    visualTransform.localPosition,
                    baseLocalPosition,
                    Time.deltaTime * 12f);

            visualTransform.localRotation =
                Quaternion.Slerp(
                    visualTransform.localRotation,
                    Quaternion.identity,
                    Time.deltaTime * 12f);

            return;
        }

        float phase =
            sequenceIndex /
            (float)Mathf.Max(
                1,
                walkSequence.Length - 1);

        float bob =
            Mathf.Sin(
                phase *
                Mathf.PI *
                2f) *
            walkBobHeight;

        float tilt =
            Mathf.Sin(
                phase *
                Mathf.PI *
                2f) *
            walkTiltAngle;

        visualTransform.localPosition =
            baseLocalPosition +
            Vector3.up * bob;

        visualTransform.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                tilt);
    }

    private void CreateVisualRenderer()
    {
        rootRenderer =
            GetComponent<SpriteRenderer>();

        GameObject visualObject =
            new("Boss Visual");

        visualObject.transform.SetParent(
            transform,
            false);

        visualTransform =
            visualObject.transform;

        visualRenderer =
            visualObject.AddComponent<SpriteRenderer>();

        visualRenderer.sortingLayerID =
            rootRenderer.sortingLayerID;

        visualRenderer.sortingOrder =
            Mathf.Max(
                rootRenderer.sortingOrder,
                8);

        visualRenderer.color =
            Color.white;

        visualRenderer.material =
            rootRenderer.material;

        baseLocalPosition =
            new Vector3(
                0f,
                visualOffsetY,
                0f);

        baseLocalScale =
            Vector3.one *
            visualScale;

        visualTransform.localPosition =
            baseLocalPosition;

        visualTransform.localScale =
            baseLocalScale;

        if (createShadow)
            CreateShadow();

        rootRenderer.enabled = false;
    }

    public void ConfigureVisual(
        float scale,
        float offsetY)
    {
        visualScale =
            Mathf.Clamp(
                scale,
                1f,
                2.5f);

        visualOffsetY =
            Mathf.Clamp(
                offsetY,
                -0.5f,
                0.5f);

        baseLocalPosition =
            new Vector3(
                0f,
                visualOffsetY,
                0f);

        baseLocalScale =
            Vector3.one *
            visualScale;

        if (visualTransform != null)
        {
            visualTransform.localPosition =
                baseLocalPosition;

            visualTransform.localScale =
                baseLocalScale;
        }

        UpdateShadowTransform();
    }

    public void HideVisual()
    {
        if (visualRenderer != null)
            visualRenderer.enabled = false;

        if (shadowRenderer != null)
            shadowRenderer.enabled = false;

        if (rootRenderer != null)
            rootRenderer.enabled = false;
    }

    private void CreateShadow()
    {
        GameObject shadowObject =
            new("Boss Shadow");

        shadowObject.transform.SetParent(
            transform,
            false);

        shadowTransform =
            shadowObject.transform;

        shadowRenderer =
            shadowObject.AddComponent<SpriteRenderer>();

        shadowRenderer.sprite =
            CreateShadowSprite();

        shadowRenderer.sortingLayerID =
            rootRenderer.sortingLayerID;

        shadowRenderer.sortingOrder =
            3;

        shadowRenderer.color =
            new Color(
                0f,
                0f,
                0f,
                shadowOpacity);

        UpdateShadowTransform();
    }

    private void UpdateShadowTransform()
    {
        if (shadowTransform == null)
            return;

        shadowTransform.localPosition =
            new Vector3(
                0f,
                -0.20f,
                0f);

        shadowTransform.localScale =
            new Vector3(
                visualScale * 0.82f,
                visualScale * 0.42f,
                1f);
    }

    private static Sprite CreateShadowSprite()
    {
        const int size = 64;

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
                (size - 1) * 0.5f,
                (size - 1) * 0.5f);

        float radius =
            size * 0.46f;

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
                    1f -
                    Mathf.Clamp01(distance);

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

    private void LoadFrames()
    {
        Texture2D texture =
            Resources.Load<Texture2D>(
                "Art/Bosses/LevelBossWalk");

        if (texture == null)
        {
            Debug.LogError(
                "LevelBossWalk.png was not found at " +
                "Resources/Art/Bosses/LevelBossWalk.",
                this);

            return;
        }

        if (texture.width % Columns != 0 ||
            texture.height % Rows != 0)
        {
            Debug.LogError(
                $"LevelBossWalk has invalid dimensions " +
                $"{texture.width}x{texture.height}. " +
                "The image must be divisible by 5 columns and 4 rows.",
                this);

            return;
        }

        int frameWidth =
            texture.width / Columns;

        int frameHeight =
            texture.height / Rows;

        frames =
            new Sprite[FrameCount];

        for (int sourceRow = 0;
             sourceRow < Rows;
             sourceRow++)
        {
            // Unity texture coordinates start at the bottom.
            int textureRow =
                Rows - 1 - sourceRow;

            for (int column = 0;
                 column < Columns;
                 column++)
            {
                int index =
                    sourceRow *
                    Columns +
                    column;

                Rect rect =
                    new(
                        column * frameWidth,
                        textureRow * frameHeight,
                        frameWidth,
                        frameHeight);

                frames[index] =
                    Sprite.Create(
                        texture,
                        rect,
                        new Vector2(
                            0.5f,
                            0.5f),
                        pixelsPerUnit,
                        0,
                        SpriteMeshType.FullRect);
            }
        }
    }

    private void SetCurrentSprite(
        int localFrame)
    {
        if (frames == null ||
            frames.Length != FrameCount)
        {
            return;
        }

        int frameIndex =
            (int)facing *
            Columns +
            Mathf.Clamp(
                localFrame,
                0,
                Columns - 1);

        visualRenderer.sprite =
            frames[frameIndex];
    }

    private void SpawnDust()
    {
        for (int index = 0;
             index < 3;
             index++)
        {
            GameObject dustObject =
                new("Boss Step Dust");

            dustObject.transform.position =
                transform.position +
                new Vector3(
                    Random.Range(
                        -0.32f,
                        0.32f) *
                    visualScale,
                    Random.Range(
                        -0.30f,
                        -0.12f) *
                    Mathf.Lerp(
                        1f,
                        1.35f,
                        visualScale - 1f),
                    0f);

            dustObject.transform.SetParent(
                transform.parent);

            BossStepDust dust =
                dustObject.AddComponent<BossStepDust>();

            dust.Initialize();
        }
    }
}

/// <summary>
/// Small procedural dust puff used by the heavy boss walk.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
internal sealed class BossStepDust : MonoBehaviour
{
    private static Sprite sprite;
    private SpriteRenderer renderer;
    private Vector3 velocity;
    private float age;
    private float lifetime;

    public void Initialize()
    {
        renderer =
            GetComponent<SpriteRenderer>();

        renderer.sprite =
            GetSprite();

        renderer.sortingOrder = 3;
        renderer.color =
            new Color(
                0.34f,
                0.25f,
                0.18f,
                0.48f);

        transform.localScale =
            Vector3.one *
            Random.Range(
                0.06f,
                0.12f);

        velocity =
            new Vector3(
                Random.Range(
                    -0.18f,
                    0.18f),
                Random.Range(
                    0.04f,
                    0.17f),
                0f);

        lifetime =
            Random.Range(
                0.28f,
                0.44f);
    }

    private void Update()
    {
        age += Time.deltaTime;

        transform.position +=
            velocity *
            Time.deltaTime;

        transform.localScale +=
            Vector3.one *
            Time.deltaTime *
            0.18f;

        float alpha =
            1f -
            Mathf.Clamp01(
                age / lifetime);

        Color color =
            renderer.color;

        color.a =
            alpha * 0.48f;

        renderer.color =
            color;

        if (age >= lifetime)
            Destroy(gameObject);
    }

    private static Sprite GetSprite()
    {
        if (sprite != null)
            return sprite;

        const int size = 16;

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
                (size - 1) * 0.5f,
                (size - 1) * 0.5f);

        float radius =
            size * 0.45f;

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
                        center);

                float alpha =
                    1f -
                    Mathf.Clamp01(
                        distance / radius);

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

        sprite =
            Sprite.Create(
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

        return sprite;
    }
}
