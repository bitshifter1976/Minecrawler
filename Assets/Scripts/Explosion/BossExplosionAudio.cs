using UnityEngine;

/// <summary>
/// Layered boss-explosion audio.
/// Missing optional clips are simply skipped.
/// </summary>
public static class BossExplosionAudio
{
    public static void PlayMain(
        Vector3 position,
        float volume)
    {
        Play(
            "Audio/explosion",
            position,
            volume,
            0.96f);

        Play(
            "Audio/explosionMetal",
            position,
            volume * 0.65f,
            0.88f);

        Play(
            "Audio/explosionRock",
            position,
            volume * 0.55f,
            0.92f);

        Play(
            "Audio/explosionEcho",
            position,
            volume * 0.45f,
            0.82f);
    }

    public static void PlaySecondary(
        Vector3 position,
        float volume)
    {
        Play(
            "Audio/explosion",
            position,
            volume,
            Random.Range(
                1.04f,
                1.12f));
    }

    private static void Play(
        string resourcePath,
        Vector3 position,
        float volume,
        float pitch)
    {
        AudioClip clip =
            Resources.Load<AudioClip>(
                resourcePath);

        if (clip == null)
            return;

        GameObject audioObject =
            new("Boss Explosion Audio");

        audioObject.transform.position =
            position;

        AudioSource source =
            audioObject.AddComponent<AudioSource>();

        source.clip =
            clip;

        source.playOnAwake =
            false;

        source.loop =
            false;

        source.spatialBlend =
            0f;

        source.volume =
            Mathf.Clamp01(
                volume);

        source.pitch =
            Mathf.Clamp(
                pitch,
                0.5f,
                1.5f);

        source.Play();

        Object.Destroy(
            audioObject,
            clip.length /
            Mathf.Max(
                0.05f,
                Mathf.Abs(source.pitch)) +
            0.20f);
    }
}
