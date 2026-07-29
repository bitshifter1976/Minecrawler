using System.Collections;
using UnityEditor.Animations;
using UnityEngine;

public class FallingRock : MonoBehaviour
{
    [Header("Movement")]
    public float gravity = 1f;
    public float minStartSpeed = 0f;
    public float maxStartSpeed = 0.25f;
    public float minSpin = -220f;
    public float maxSpin = 220f;

    [Header("Impact")]
    public float destroyDelay = 0.6f;

    [Header("References")]
    public SpriteRenderer rockRenderer;
    public GameObject impactObject;
    private SpriteAnimation impactAnimation;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip impactClip;

    private float velocity;
    private float spinSpeed;
    private float groundY;
    private bool impacted;

    public void SetGroundHeight(float groundHeight)
    {
        groundY = groundHeight;
    }

    private void Awake()
    {
        velocity = Random.Range(minStartSpeed, maxStartSpeed);
        spinSpeed = Random.Range(minSpin, maxSpin);

        impactAnimation = GetComponent<SpriteAnimation>();

        float scale = Random.Range(0.8f, 1.3f);
        transform.localScale = Vector3.one * scale;

        groundY += Random.Range(-5f, 1f);

        if (impactObject != null)
            impactObject.SetActive(false);
    }

    private void Update()
    {
        if (impacted)
            return;

        velocity += gravity * Time.deltaTime;

        transform.position += Vector3.down * velocity * Time.deltaTime;
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);

        if (transform.position.y <= groundY)
        {
            DoImpact();
        }
    }

    private void DoImpact()
    {
        impacted = true;

        Vector3 p = transform.position;
        p.y = groundY;
        transform.position = p;

        if (rockRenderer != null)
            rockRenderer.enabled = false;

        if (impactObject != null)
            impactObject.SetActive(true);

        if (impactAnimation != null)
            impactAnimation.Play();

        if (audioSource != null && impactClip != null)
            audioSource.PlayOneShot(impactClip);

        StartCoroutine(DestroyRoutine());
    }

    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}