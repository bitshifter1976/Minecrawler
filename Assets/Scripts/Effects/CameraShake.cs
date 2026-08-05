using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Optional UI Background")]
    [SerializeField] private RectTransform background;

    private Vector3 originalCameraPosition;
    private Vector2 originalBackgroundPosition;

    private Coroutine shakeCoroutine;

    private void Awake()
    {
        originalCameraPosition = transform.localPosition;

        if (background != null)
            originalBackgroundPosition = background.anchoredPosition;
    }

    public void Shake(float duration, float strength)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(
            ShakeRoutine(duration, strength));
    }

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        originalCameraPosition = transform.localPosition;

        if (background != null)
            originalBackgroundPosition = background.anchoredPosition;

        float timer = 0f;

        while (timer < duration)
        {
            Vector2 offset =
                Random.insideUnitCircle * strength;

            transform.localPosition =
                originalCameraPosition +
                new Vector3(offset.x, offset.y, 0f);

            if (background != null)
            {
                // UI verwendet Pixel, daher stärkere Skalierung notwendig
                background.anchoredPosition =
                    originalBackgroundPosition +
                    offset * 100f;
            }

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = originalCameraPosition;

        if (background != null)
            background.anchoredPosition = originalBackgroundPosition;

        shakeCoroutine = null;
    }

    private void OnDisable()
    {
        transform.localPosition = originalCameraPosition;

        if (background != null)
            background.anchoredPosition = originalBackgroundPosition;
    }
}