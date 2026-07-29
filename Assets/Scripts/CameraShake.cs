using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    Vector3 originalPos;

    void Awake()
    {
        originalPos = transform.localPosition;
    }

    public void Shake(float duration, float strength)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeRoutine(duration, strength));
    }

    IEnumerator ShakeRoutine(float duration, float strength)
    {
        float timer = 0f;

        while (timer < duration)
        {
            transform.localPosition =
                originalPos +
                (Vector3)Random.insideUnitCircle * strength;

            timer += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}