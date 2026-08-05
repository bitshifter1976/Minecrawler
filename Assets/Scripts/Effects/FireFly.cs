using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FireFly : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float minSpeed = 0.2f;
    [SerializeField] private float maxSpeed = 0.5f;

    [SerializeField] private float wanderStrength = 0.5f;
    [SerializeField] private float directionChangeInterval = 1.5f;

    [Header("Lifetime")]
    [SerializeField] private float minLifetime = 5f;
    [SerializeField] private float maxLifetime = 10f;

    [Header("Glow")]
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float minAlpha = 0.4f;
    [SerializeField] private float maxAlpha = 1f;

    private SpriteRenderer spriteRenderer;

    private Vector2 direction;
    private float speed;

    private float nextDirectionChange;
    private float destroyTime;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        speed = Random.Range(minSpeed, maxSpeed);

        direction = Random.insideUnitCircle.normalized;

        nextDirectionChange =
            Time.time + directionChangeInterval;

        destroyTime =
            Time.time + Random.Range(minLifetime, maxLifetime);

        transform.localScale *= Random.Range(0.015f, 0.03f);
    }

    private void Update()
    {
        // Richtung langsam ändern
        if (Time.time >= nextDirectionChange)
        {
            direction += Random.insideUnitCircle * wanderStrength;
            direction.Normalize();

            nextDirectionChange =
                Time.time + Random.Range(1f, 2.5f);
        }

        transform.position +=
            (Vector3)(direction * speed * Time.deltaTime);

        // leicht auf und ab schweben
        transform.position +=
            Vector3.up *
            Mathf.Sin(Time.time * 3f) *
            0.2f *
            Time.deltaTime;

        // Pulsieren
        float t =
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        Color c = spriteRenderer.color;
        c.a = Mathf.Lerp(minAlpha, maxAlpha, t);
        spriteRenderer.color = c;

        // kleines Funkeln
        float scale =
            Mathf.Lerp(0.9f, 1.1f, t);

        transform.localScale =
            Vector3.one * scale;

        // verschwinden
        if (Time.time >= destroyTime)
        {
            Destroy(gameObject);
        }
    }
}