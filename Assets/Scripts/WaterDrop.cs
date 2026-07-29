using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class WaterDrop : MonoBehaviour
{
    [Header("Movement")]
    public float gravity = 10f;
    public float minStartSpeed = 0f;
    public float maxStartSpeed = 1f;

    [Header("Random")]
    public float minScale = 0.8f;
    public float maxScale = 1.2f;

    [Header("Impact")]
    public float destroyDelay = 0.6f;

    [Header("References")]
    public SpriteRenderer dropRenderer;
    public GameObject splashObject;

    [Header("Sound")]
    public AudioClip splashClip;

    private SpriteAnimation splashAnimation;
    private AudioSource audioSource;

    private float velocity;
    private float groundY;
    private bool splashed;

    public void SetGroundHeight(float y)
    {
        groundY = y;
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        splashAnimation = splashObject.GetComponent<SpriteAnimation>();

        velocity = Random.Range(minStartSpeed, maxStartSpeed);

        float scale = Random.Range(minScale, maxScale);
        transform.localScale = Vector3.one * scale;

        audioSource.pitch = Random.Range(0.9f, 1.1f);
        audioSource.volume = Random.Range(0.2f, 0.5f);
        audioSource.panStereo = Random.Range(-0.8f, 0.8f);

        splashObject.SetActive(false);
    }

    private void Update()
    {
        if (splashed)
            return;

        velocity += gravity * Time.deltaTime;

        transform.position += Vector3.down * velocity * Time.deltaTime;

        if (transform.position.y <= groundY)
            Splash();
    }

    private void Splash()
    {
        splashed = true;

        if (dropRenderer != null)
            dropRenderer.enabled = false;

        if (splashObject != null)
        {
            splashObject.SetActive(true);

            if (splashAnimation != null)
                splashAnimation.Play();
        }

        if (splashClip != null)
            audioSource.PlayOneShot(splashClip);

        Destroy(gameObject, destroyDelay);
    }
}