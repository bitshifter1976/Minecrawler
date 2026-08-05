using UnityEngine;

/// <summary>
/// Two-part movement sound for the Minecrawler miner:
/// - a short rail-joint click for every actual movement step
/// - a quiet rolling loop while the miner keeps moving
///
/// The component observes transform movement, so no MinerController change
/// is required. It also works when grid movement teleports the transform:
/// the rolling loop remains active briefly after every movement step.
/// </summary>
[DisallowMultipleComponent]
public sealed class MinerLocomotiveAudio : MonoBehaviour
{
    [Header("Clips")]
    [SerializeField] private AudioClip railClickClip;
    [SerializeField] private AudioClip rollingLoopClip;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float clickVolume = 0.48f;

    [Range(0f, 1f)]
    [SerializeField] private float rollingVolume = 0.18f;

    [Header("Variation")]
    [SerializeField] private Vector2 clickPitchRange =
        new Vector2(0.96f, 1.04f);

    [SerializeField] private Vector2 rollingPitchRange =
        new Vector2(0.98f, 1.02f);

    [Header("Movement Detection")]
    [SerializeField] private float movementThreshold = 0.001f;

    [Tooltip(
        "How long the rolling loop remains audible after the latest movement. " +
        "Increase this if the miner moves slowly between grid cells.")]
    [SerializeField] private float rollingHoldTime = 0.16f;

    [Tooltip(
        "Prevents multiple rail clicks if another script changes the transform " +
        "several times during the same movement.")]
    [SerializeField] private float minimumClickInterval = 0.035f;

    [Header("Fade")]
    [SerializeField] private float fadeInSpeed = 12f;
    [SerializeField] private float fadeOutSpeed = 8f;

    private AudioSource clickSource;
    private AudioSource rollingSource;

    private Vector3 previousPosition;
    private float lastMovementTime = -100f;
    private float lastClickTime = -100f;
    private float targetRollingVolume;

    private void Awake()
    {
        LoadClips();
        CreateAudioSources();

        previousPosition =
            transform.position;
    }

    private void OnEnable()
    {
        previousPosition =
            transform.position;

        lastMovementTime =
            -100f;

        targetRollingVolume =
            0f;
    }

    private void Update()
    {
        DetectMovement();
        UpdateRollingSound();
    }

    private void LoadClips()
    {
        if (railClickClip == null)
        {
            railClickClip =
                Resources.Load<AudioClip>(
                    "Audio/minerRailClick");
        }

        if (rollingLoopClip == null)
        {
            rollingLoopClip =
                Resources.Load<AudioClip>(
                    "Audio/minerRollingLoop");
        }
    }

    private void CreateAudioSources()
    {
        clickSource =
            gameObject.AddComponent<AudioSource>();

        clickSource.playOnAwake = false;
        clickSource.loop = false;
        clickSource.spatialBlend = 0f;
        clickSource.dopplerLevel = 0f;

        rollingSource =
            gameObject.AddComponent<AudioSource>();

        rollingSource.playOnAwake = false;
        rollingSource.loop = true;
        rollingSource.spatialBlend = 0f;
        rollingSource.dopplerLevel = 0f;
        rollingSource.clip = rollingLoopClip;
        rollingSource.volume = 0f;
    }

    private void DetectMovement()
    {
        Vector3 currentPosition =
            transform.position;

        float movementSquared =
            (currentPosition -
             previousPosition).sqrMagnitude;

        previousPosition =
            currentPosition;

        if (movementSquared <
            movementThreshold *
            movementThreshold)
        {
            return;
        }

        lastMovementTime =
            Time.unscaledTime;

        PlayRailClick();

        if (rollingLoopClip != null &&
            !rollingSource.isPlaying)
        {
            rollingSource.pitch =
                Random.Range(
                    rollingPitchRange.x,
                    rollingPitchRange.y);

            rollingSource.Play();
        }
    }

    private void PlayRailClick()
    {
        if (railClickClip == null ||
            Time.unscaledTime -
            lastClickTime <
            minimumClickInterval)
        {
            return;
        }

        lastClickTime =
            Time.unscaledTime;

        clickSource.pitch =
            Random.Range(
                clickPitchRange.x,
                clickPitchRange.y);

        clickSource.PlayOneShot(
            railClickClip,
            clickVolume);
    }

    private void UpdateRollingSound()
    {
        bool shouldRoll =
            Time.unscaledTime -
            lastMovementTime <=
            rollingHoldTime;

        targetRollingVolume =
            shouldRoll
                ? rollingVolume
                : 0f;

        float fadeSpeed =
            shouldRoll
                ? fadeInSpeed
                : fadeOutSpeed;

        rollingSource.volume =
            Mathf.MoveTowards(
                rollingSource.volume,
                targetRollingVolume,
                fadeSpeed *
                Time.unscaledDeltaTime);

        if (!shouldRoll &&
            rollingSource.isPlaying &&
            rollingSource.volume <= 0.001f)
        {
            rollingSource.Stop();
            rollingSource.volume = 0f;
        }
    }

    private void OnDisable()
    {
        if (clickSource != null)
            clickSource.Stop();

        if (rollingSource != null)
        {
            rollingSource.Stop();
            rollingSource.volume = 0f;
        }
    }
}
