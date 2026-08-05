using UnityEngine;

/// <summary>
/// Permanent dark scorch mark left on the mine floor.
/// It is a child of the level board and is removed when the level reloads.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BossExplosionGroundScorch : MonoBehaviour
{
    public static void Create(
        Vector3 position,
        Transform parent,
        float scale,
        int sortingOrder)
    {
        GameObject scorchObject =
            new("Boss Ground Scorch");

        scorchObject.transform.position =
            new Vector3(
                position.x,
                position.y,
                position.z);

        scorchObject.transform.SetParent(
            parent);

        SpriteRenderer renderer =
            scorchObject.AddComponent<SpriteRenderer>();

        renderer.sprite =
            BossExplosionRuntimeSprites.Scorch;

        renderer.sortingOrder =
            sortingOrder;

        renderer.color =
            new Color(
                0.13f,
                0.055f,
                0.025f,
                0.72f);

        scorchObject.transform.localScale =
            new Vector3(
                scale,
                scale * 0.72f,
                1f);

        scorchObject.transform.rotation =
            Quaternion.Euler(
                0f,
                0f,
                Random.Range(
                    0f,
                    360f));
    }
}
