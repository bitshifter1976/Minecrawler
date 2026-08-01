using UnityEngine;

/// <summary>
/// Spielt die acht importierten Boss-Explosionsframes ab
/// und zerstört das Effektobjekt anschließend.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BossExplosionAnimation : MonoBehaviour
{
    [SerializeField] private float framesPerSecond = 14f;
    [SerializeField] private int sortingOrder = 20;
    [SerializeField] private float scale = 1.35f;

    private SpriteRenderer spriteRenderer;
    private Sprite[] frames;
    private float frameTimer;
    private int frameIndex;

    public static void Create(
        Vector3 position,
        Transform parent = null)
    {
        GameObject effectObject =
            new("Boss Explosion");

        effectObject.transform.position = position;
        effectObject.transform.SetParent(parent);

        effectObject.AddComponent<SpriteRenderer>();
        effectObject.AddComponent<BossExplosionAnimation>();
    }

    private void Awake()
    {
        spriteRenderer =
            GetComponent<SpriteRenderer>();

        spriteRenderer.sortingOrder =
            sortingOrder;

        transform.localScale =
            Vector3.one * scale;

        frames =
            BossVisuals.LoadExplosionFrames();

        if (frames == null ||
            frames.Length == 0 ||
            frames[0] == null)
        {
            Debug.LogWarning(
                "Boss explosion frames were not found.");

            Destroy(gameObject);
            return;
        }

        spriteRenderer.sprite = frames[0];
    }

    private void Update()
    {
        if (frames == null ||
            frames.Length == 0)
        {
            return;
        }

        float frameDuration =
            1f / Mathf.Max(1f, framesPerSecond);

        frameTimer += Time.deltaTime;

        while (frameTimer >= frameDuration)
        {
            frameTimer -= frameDuration;
            frameIndex++;

            if (frameIndex >= frames.Length)
            {
                Destroy(gameObject);
                return;
            }

            if (frames[frameIndex] != null)
                spriteRenderer.sprite = frames[frameIndex];
        }
    }
}
