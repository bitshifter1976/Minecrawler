using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public sealed class LogoAnimator : MonoBehaviour
{
    [Header("Floating")]
    [SerializeField] private float floatAmplitude = 6f;
    [SerializeField] private float floatSpeed = 0.55f;

    [Header("Breathing")]
    [Range(0f, 0.1f)]
    [SerializeField] private float breathingAmount = 0.025f;
    [SerializeField] private float breathingSpeed = 0.8f;

    [Header("Random Hop")]
    [SerializeField] private float minimumHopInterval = 5f;
    [SerializeField] private float maximumHopInterval = 8f;
    [SerializeField] private float hopHeight = 5f;
    [SerializeField] private float hopDuration = 0.34f;

    [Header("Menu Wobble")]
    [SerializeField] private float wobbleAngle = 2.5f;
    [SerializeField] private float wobbleDuration = 0.28f;
    [SerializeField] private int wobbleOscillations = 3;

    private RectTransform rectTransform;
    private Vector2 basePosition;
    private Vector3 baseScale;
    private float animationOffset;
    private float nextHopTime;
    private float hopOffset;
    private float wobbleRotation;
    private Coroutine hopCoroutine;
    private Coroutine wobbleCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        basePosition = rectTransform.anchoredPosition;
        baseScale = rectTransform.localScale;
        animationOffset = Random.Range(0f, Mathf.PI * 2f);
        ScheduleNextHop();
    }

    private void Update()
    {
        float time = Time.unscaledTime + animationOffset;

        float floatingOffset =
            Mathf.Sin(time * floatSpeed) * floatAmplitude;

        float breathingScale =
            1f + Mathf.Sin(time * breathingSpeed) * breathingAmount;

        rectTransform.anchoredPosition =
            basePosition + new Vector2(0f, floatingOffset + hopOffset);

        rectTransform.localScale =
            baseScale * breathingScale;

        rectTransform.localRotation =
            Quaternion.Euler(0f, 0f, wobbleRotation);

        if (Time.unscaledTime >= nextHopTime && hopCoroutine == null)
            hopCoroutine = StartCoroutine(HopRoutine());
    }

    public void TriggerMenuWobble()
    {
        if (!isActiveAndEnabled)
            return;

        if (wobbleCoroutine != null)
            StopCoroutine(wobbleCoroutine);

        wobbleCoroutine = StartCoroutine(WobbleRoutine());
    }

    private IEnumerator HopRoutine()
    {
        float elapsed = 0f;

        while (elapsed < hopDuration)
        {
            float progress = Mathf.Clamp01(elapsed / hopDuration);
            hopOffset = Mathf.Sin(progress * Mathf.PI) * hopHeight;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        hopOffset = 0f;
        hopCoroutine = null;
        ScheduleNextHop();
    }

    private IEnumerator WobbleRoutine()
    {
        float elapsed = 0f;
        float oscillations = Mathf.Max(1, wobbleOscillations);

        while (elapsed < wobbleDuration)
        {
            float progress = Mathf.Clamp01(elapsed / wobbleDuration);
            float damping = 1f - progress;

            wobbleRotation =
                Mathf.Sin(progress * Mathf.PI * 2f * oscillations) *
                wobbleAngle *
                damping;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        wobbleRotation = 0f;
        wobbleCoroutine = null;
    }

    private void ScheduleNextHop()
    {
        float minimum = Mathf.Max(0.1f, minimumHopInterval);
        float maximum = Mathf.Max(minimum, maximumHopInterval);

        nextHopTime =
            Time.unscaledTime + Random.Range(minimum, maximum);
    }

    private void OnDisable()
    {
        if (rectTransform == null)
            return;

        hopOffset = 0f;
        wobbleRotation = 0f;

        rectTransform.anchoredPosition = basePosition;
        rectTransform.localScale = baseScale;
        rectTransform.localRotation = Quaternion.identity;

        hopCoroutine = null;
        wobbleCoroutine = null;
    }
}
