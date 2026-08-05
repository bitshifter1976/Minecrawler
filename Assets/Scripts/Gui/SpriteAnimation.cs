using System.Collections;
using UnityEngine;

public class SpriteAnimation : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameTime = 0.08f;

    public void Play()
    {
        StartCoroutine(PlayAnimation());
    }

    private IEnumerator PlayAnimation()
    {
        foreach (var frame in frames)
        {
            spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(frameTime);
        }

        gameObject.SetActive(false);
    }
}