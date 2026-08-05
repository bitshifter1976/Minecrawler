using System.Collections;
using UnityEngine;

/// <summary>
/// Brief cinematic slow motion using real-time duration.
/// Restores the previous time scale safely.
/// </summary>
public static class BossExplosionSlowMotion
{
    private static int activeRequests;
    private static float savedTimeScale = 1f;

    public static void Play(
        float timeScale,
        float realSeconds)
    {
        BossExplosionRunner.Run(
            SlowMotionRoutine(
                Mathf.Clamp(
                    timeScale,
                    0.02f,
                    1f),
                Mathf.Max(
                    0.01f,
                    realSeconds)));
    }

    private static IEnumerator SlowMotionRoutine(
        float timeScale,
        float realSeconds)
    {
        if (activeRequests == 0)
        {
            savedTimeScale =
                Time.timeScale;
        }

        activeRequests++;

        Time.timeScale =
            Mathf.Min(
                Time.timeScale,
                timeScale);

        yield return new WaitForSecondsRealtime(
            realSeconds);

        activeRequests--;

        if (activeRequests <= 0)
        {
            activeRequests = 0;

            Time.timeScale =
                savedTimeScale;
        }
    }
}
