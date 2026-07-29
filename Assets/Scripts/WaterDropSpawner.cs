using System.Collections;
using UnityEngine;

public class WaterDropSpawner : MonoBehaviour
{
    [SerializeField] private WaterDrop waterDropPrefab;

    [SerializeField] private float minDelay = 1f;
    [SerializeField] private float maxDelay = 5f;

    [SerializeField] private float spawnOffset = 0.5f;
    [SerializeField] private float groundOffset = 0.3f;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Random.Range(minDelay, maxDelay));

            SpawnDrop();
        }
    }

    private void SpawnDrop()
    {
        float left = cam.ViewportToWorldPoint(new Vector3(0.02f, 1f, 0)).x;
        float right = cam.ViewportToWorldPoint(new Vector3(0.6f, 1f, 0)).x;
        float top = cam.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;
        float bottom = cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        float x = Random.Range(left, right);

        if (waterDropPrefab == null)
            waterDropPrefab = Resources.Load<WaterDrop>("Prefabs/WaterDrop");

        WaterDrop drop = Instantiate(
            waterDropPrefab,
            new Vector3(x, top + spawnOffset, 0),
            Quaternion.identity);

        drop.SetGroundHeight(bottom + groundOffset);
    }
}