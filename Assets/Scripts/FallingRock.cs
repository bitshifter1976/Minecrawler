using System.Collections;
using UnityEditor.Animations;
using UnityEngine;

public class FallingRock : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float gravity = 1f;
    [SerializeField] private float minStartSpeed = 0f;
    [SerializeField] private float maxStartSpeed = 0.25f;
    [SerializeField] private float minSpin = -220f;
    [SerializeField] private float maxSpin = 220f;

    [Header("Impact")]
    [SerializeField] private float destroyDelay = 0.6f;

    [Header("References")]
    [SerializeField] private SpriteRenderer rockRenderer;
    [SerializeField] private GameObject impactObject;
    [SerializeField] private SpriteAnimation impactAnimation;
    [SerializeField] private ParticleSystem dustParticles;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip impactClip;

    private float velocity;
    private float spinSpeed;
    private float groundY;
    private bool impacted;

    private CameraShake cameraShake;

    public void SetGroundHeight(float groundHeight)
    {
        groundY = groundHeight;
    }

    private void Awake()
    {
        velocity = Random.Range(minStartSpeed, maxStartSpeed);
        spinSpeed = Random.Range(minSpin, maxSpin);

        float scale = Random.Range(0.3f, 1f);
        transform.localScale = Vector3.one * scale;

        if (impactObject != null)
            impactObject.SetActive(false);

        cameraShake = Camera.main.GetComponent<CameraShake>();
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
            audioSource.PlayOneShot(impactClip, Random.Range(0.1f, 0.25f));

        if (dustParticles != null)
        {
            dustParticles.transform.parent = null;
            dustParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            dustParticles.Play();
            Destroy(dustParticles.gameObject, 3f);
        }

        if (cameraShake != null)
        {
            cameraShake.Shake(0.15f, 0.05f);
        }

        StartCoroutine(DestroyRoutine());
    }

    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}