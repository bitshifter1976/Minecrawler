using System.Collections;
using UnityEngine;

/// <summary>
/// Strong opening camera hit followed by smaller aftershocks.
/// </summary>
public static class BossExplosionCamera
{
    public static void PlayImpact(
        float strength,
        float duration)
    {
        Camera.main?
            .GetComponent<CameraShake>()?
            .Shake(
                strength,
                duration);

        BossExplosionRunner.Run(
            AftershockRoutine());
    }

    public static void PlayAftershock(
        float strength,
        float duration)
    {
        Camera.main?
            .GetComponent<CameraShake>()?
            .Shake(
                strength,
                duration);
    }

    private static IEnumerator AftershockRoutine()
    {
        yield return new WaitForSecondsRealtime(
            0.34f);

        PlayAftershock(
            0.78f,
            0.18f);

        yield return new WaitForSecondsRealtime(
            0.24f);

        PlayAftershock(
            0.42f,
            0.14f);
    }
}
