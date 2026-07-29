using System.Collections;
using UnityEngine;

public class AmbientEffectSpawner : MonoBehaviour
{
    public FallingRock RockPrefab;

    [SerializeField] float minDelay = 2f;
    [SerializeField] float maxDelay = 5f;

    Camera cam;

    void Start()
    {
        cam = Camera.main;
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Random.Range(minDelay, maxDelay));

            SpawnRock();
        }
    }

    void SpawnRock()
    {
        float left =
            cam.ViewportToWorldPoint(new Vector3(0, 1, 0)).x;

        float right =
            cam.ViewportToWorldPoint(new Vector3(1, 1, 0)).x;

        float top =
            cam.ViewportToWorldPoint(new Vector3(0, 1, 0)).y;

        float bottom =
            cam.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;

        float x = Random.Range(left, right - 3f);

        if (RockPrefab == null)
            RockPrefab = Resources.Load<FallingRock>("Prefabs/FallingRock");

        var rock = Instantiate(
            RockPrefab,
            new Vector3(x, top + 1f, 0),
            Quaternion.identity);

        rock.SetGroundHeight(bottom + Random.Range(0f, 5f));
    }
}