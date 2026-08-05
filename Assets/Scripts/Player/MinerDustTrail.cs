using UnityEngine;

/// <summary>
/// Verwendet exakt das ParticleSystem aus dem FallingRock-Prefab
/// als Fahrstaub für Miner und Lore.
/// </summary>
public sealed class MinerDustTrail : MonoBehaviour
{
    [Header("Falling Rock Dust")]
    [SerializeField] private FallingRock fallingRockPrefab;

    [Header("Position")]
    [SerializeField] private float distanceBehind = 0.30f;
    [SerializeField] private float verticalOffset = -0.18f;

    [Header("Amount")]
    [SerializeField] private float emissionMultiplier = 0.55f;
    [SerializeField] private float minimumInterval = 0.04f;

    private ParticleSystem dustTemplate;
    private float lastEmissionTime;


    public void SetEmissionMultiplier(
        float multiplier)
    {
        emissionMultiplier =
            Mathf.Clamp(
                multiplier,
                0.05f,
                2f);
    }

    private void Awake()
    {
        LoadDustTemplate();
    }

    public void EmitMovementDust(
        Vector3 previousPosition,
        Vector2Int movementDirection)
    {
        if (Time.unscaledTime - lastEmissionTime <
            minimumInterval)
        {
            return;
        }

        if (dustTemplate == null)
            LoadDustTemplate();

        if (dustTemplate == null)
        {
            Debug.LogWarning(
                "MinerDustTrail: No ParticleSystem was found " +
                "inside the FallingRock prefab.");

            return;
        }

        lastEmissionTime =
            Time.unscaledTime;

        Vector3 direction =
            new Vector3(
                movementDirection.x,
                movementDirection.y,
                0f).normalized;

        // Verwende bewusst die aktuelle Minerposition.
        // Dadurch entsteht der Effekt nicht mehr unten links.
        Vector3 spawnPosition =
            transform.position -
            direction * distanceBehind +
            Vector3.up * verticalOffset;

        ParticleSystem dust =
            Instantiate(
                dustTemplate,
                spawnPosition,
                Quaternion.identity);

        dust.gameObject.name =
            "Miner FallingRock Dust";

        dust.transform.SetParent(null);
        dust.gameObject.SetActive(true);

        ParticleSystem.MainModule main =
            dust.main;

        ParticleSystem.EmissionModule emission =
            dust.emission;

        // Derselbe Effekt wie beim Falling Rock,
        // aber für permanente Bewegung etwas reduziert.
        if (emission.rateOverTime.constantMax > 0f)
        {
            emission.rateOverTime =
                new ParticleSystem.MinMaxCurve(
                    emission.rateOverTime.constantMin *
                    emissionMultiplier,
                    emission.rateOverTime.constantMax *
                    emissionMultiplier);
        }

        BurstDustSystems(dust);

        float destroyDelay =
            main.duration +
            main.startLifetime.constantMax +
            1f;

        Destroy(
            dust.gameObject,
            destroyDelay);
    }

    private void LoadDustTemplate()
    {
        if (fallingRockPrefab == null)
        {
            fallingRockPrefab =
                Resources.Load<FallingRock>(
                    "Prefabs/FallingRock");
        }

        if (fallingRockPrefab == null)
        {
            Debug.LogWarning(
                "MinerDustTrail: Prefabs/FallingRock was not found.");

            return;
        }

        ParticleSystem[] systems =
            fallingRockPrefab.GetComponentsInChildren<ParticleSystem>(
                true);

        if (systems == null ||
            systems.Length == 0)
        {
            Debug.LogWarning(
                "MinerDustTrail: FallingRock has no child ParticleSystem.");

            return;
        }

        // Bevorzugt ein ParticleSystem mit 'dust' im Namen.
        foreach (ParticleSystem system in systems)
        {
            if (system.name.ToLowerInvariant().Contains("dust"))
            {
                dustTemplate = system;
                return;
            }
        }

        dustTemplate = systems[0];
    }

    private static void BurstDustSystems(
        ParticleSystem root)
    {
        ParticleSystem[] systems =
            root.GetComponentsInChildren<ParticleSystem>(
                true);

        foreach (ParticleSystem system in systems)
        {
            system.gameObject.SetActive(true);

            system.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear);

            system.Play(true);
        }
    }
}
