using System.Collections;
using UnityEngine;

public class FireFlySpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private FireFly fireFlyPrefab;

    [Header("Spawn Timing")]
    [SerializeField] private float minDelay = 1.5f;
    [SerializeField] private float maxDelay = 4f;

    [Header("Maximum")]
    [SerializeField] private int maxFireFlies = 12;

    [Header("Spawn Area - Viewport")]
    [Range(0f, 1f)]
    [SerializeField] private float leftViewport = 0.03f;

    [Range(0f, 1f)]
    [SerializeField] private float rightViewport = 0.63f;

    [Range(0f, 1f)]
    [SerializeField] private float bottomViewport = 0.15f;

    [Range(0f, 1f)]
    [SerializeField] private float topViewport = 0.85f;

    [Header("Depth")]
    [SerializeField] private float worldZ = 0f;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError(
                "FireFlySpawner: No camera tagged as MainCamera found.",
                this);

            enabled = false;
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float delay = Random.Range(minDelay, maxDelay);
            yield return new WaitForSeconds(delay);

            TrySpawnFireFly();
        }
    }

    private void TrySpawnFireFly()
    {
        int currentCount = FindObjectsByType<FireFly>().Length;
        if (currentCount >= maxFireFlies)
            return;

        float viewportX = Random.Range(leftViewport, rightViewport);
        float viewportY = Random.Range(bottomViewport, topViewport);

        Vector3 spawnPosition =
            cam.ViewportToWorldPoint(
                new Vector3(
                    viewportX,
                    viewportY,
                    Mathf.Abs(cam.transform.position.z - worldZ)));

        spawnPosition.z = worldZ;

        if (fireFlyPrefab == null)
            fireFlyPrefab = Resources.Load<FireFly>("Prefabs/FireFly");

        Instantiate(
            fireFlyPrefab,
            spawnPosition,
            Quaternion.identity);
    }

    private void OnValidate()
    {
        if (minDelay < 0.1f)
            minDelay = 0.1f;

        if (maxDelay < minDelay)
            maxDelay = minDelay;

        if (maxFireFlies < 1)
            maxFireFlies = 1;

        if (rightViewport < leftViewport)
            rightViewport = leftViewport;

        if (topViewport < bottomViewport)
            topViewport = bottomViewport;
    }
}