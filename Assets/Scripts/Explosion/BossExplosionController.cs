using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Orchestrates the complete boss-death presentation.
///
/// Timeline:
/// 0.00 s  white-yellow flash, strong camera impact and slow motion
/// 0.05 s  primary fireball
/// 0.20 s  metal, lava and rock debris
/// 0.36 s  secondary offset explosion
/// 0.48 s  dark smoke column
/// 0.72 s  sparks and embers
/// 0.88 s  permanent ground scorch
/// 1.05 s  objective callback, for example key spawning
/// </summary>
public sealed class BossExplosionController : MonoBehaviour
{
    [Header("Sequence")]
    [SerializeField] private float primaryScale = 2.25f;
    [SerializeField] private float secondaryScaleFactor = 0.62f;
    [SerializeField] private float objectiveCallbackDelay = 1.05f;
    [SerializeField] private float cleanupDelay = 4.5f;

    private Action onSequenceMilestone;
    private Transform effectParent;

    public static void Create(
        Vector3 position,
        Transform parent = null,
        float customScale = 2.25f,
        Action onSequenceMilestone = null)
    {
        GameObject sequenceObject =
            new("Boss Death Sequence");

        sequenceObject.transform.position =
            position;

        sequenceObject.transform.SetParent(
            parent);

        BossExplosionController controller =
            sequenceObject.AddComponent<BossExplosionController>();

        controller.primaryScale =
            Mathf.Clamp(
                customScale,
                1f,
                4f);

        controller.effectParent =
            parent;

        controller.onSequenceMilestone =
            onSequenceMilestone;
    }

    private void Start()
    {
        StartCoroutine(
            PlaySequence());
    }

    private IEnumerator PlaySequence()
    {
        Vector3 center =
            transform.position;

        BossExplosionSlowMotion.Play(
            0.15f,
            0.15f);

        BossExplosionCamera.PlayImpact(
            1.65f,
            0.38f);

        BossExplosionAudio.PlayMain(
            center,
            0.95f);

        BossExplosionFlash.Create(
            center,
            effectParent,
            primaryScale,
            120);

        yield return new WaitForSecondsRealtime(
            0.05f);

        BossExplosionFireball.Create(
            center,
            effectParent,
            primaryScale,
            110,
            0f,
            1f);

        yield return new WaitForSecondsRealtime(
            0.15f);

        BossExplosionDebris.CreateBurst(
            center,
            effectParent,
            primaryScale,
            28,
            124);

        BossExplosionSparks.CreateBurst(
            center,
            effectParent,
            primaryScale,
            34,
            126);

        yield return new WaitForSecondsRealtime(
            0.16f);

        Vector2 secondaryOffset =
            UnityEngine.Random.insideUnitCircle *
            0.28f *
            primaryScale;

        Vector3 secondaryPosition =
            center +
            new Vector3(
                secondaryOffset.x,
                secondaryOffset.y,
                0f);

        BossExplosionAudio.PlaySecondary(
            secondaryPosition,
            0.62f);

        BossExplosionFireball.Create(
            secondaryPosition,
            effectParent,
            primaryScale *
            secondaryScaleFactor,
            114,
            0.06f,
            0.78f);

        BossExplosionCamera.PlayAftershock(
            0.70f,
            0.20f);

        yield return new WaitForSecondsRealtime(
            0.12f);

        BossExplosionSmoke.CreateColumn(
            center,
            effectParent,
            primaryScale,
            10,
            116);

        yield return new WaitForSecondsRealtime(
            0.24f);

        BossExplosionSparks.CreateBurst(
            center,
            effectParent,
            primaryScale * 0.80f,
            18,
            127);

        yield return new WaitForSecondsRealtime(
            0.16f);

        BossExplosionGroundScorch.Create(
            center,
            effectParent,
            primaryScale * 0.78f,
            -1);

        float remainingCallbackDelay =
            Mathf.Max(
                0f,
                objectiveCallbackDelay -
                0.88f);

        if (remainingCallbackDelay > 0f)
        {
            yield return new WaitForSecondsRealtime(
                remainingCallbackDelay);
        }

        Action callback =
            onSequenceMilestone;

        onSequenceMilestone = null;
        callback?.Invoke();

        float remainingCleanupDelay =
            Mathf.Max(
                0f,
                cleanupDelay -
                objectiveCallbackDelay);

        yield return new WaitForSecondsRealtime(
            remainingCleanupDelay);

        Destroy(gameObject);
    }
}
