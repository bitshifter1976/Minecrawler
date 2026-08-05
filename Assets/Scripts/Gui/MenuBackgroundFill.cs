using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scales a UI Image like CSS "background-size: cover":
/// the complete Canvas area is filled while the sprite keeps its aspect ratio.
/// Parts outside the screen are cropped instead of creating black bars.
/// Attach this component to the menu background Image.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public sealed class MenuBackgroundFill : MonoBehaviour
{
    [Header("Behaviour")]
    [SerializeField] private bool keepAsFirstSibling = true;
    [SerializeField] private bool disableRaycastTarget = true;

    private RectTransform rectTransform;
    private Image image;
    private RectTransform parentRectTransform;

    private Vector2Int lastScreenSize;
    private Vector2 lastParentSize;
    private Sprite lastSprite;

    private void Awake()
    {
        CacheReferences();
        ApplyBackgroundLayout();
    }

    private void OnEnable()
    {
        CacheReferences();
        ApplyBackgroundLayout();
    }

    private void Update()
    {
        if (NeedsRefresh())
            ApplyBackgroundLayout();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (!isActiveAndEnabled)
            return;

        CacheReferences();
        ApplyBackgroundLayout();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        CacheReferences();
        ApplyBackgroundLayout();
    }
#endif

    [ContextMenu("Apply Background Fill")]
    public void ApplyBackgroundLayout()
    {
        CacheReferences();

        if (rectTransform == null ||
            image == null ||
            parentRectTransform == null)
        {
            return;
        }

        if (keepAsFirstSibling)
            rectTransform.SetAsFirstSibling();

        if (disableRaycastTarget)
            image.raycastTarget = false;

        image.type = Image.Type.Simple;
        image.preserveAspect = false;

        Sprite sprite = image.sprite;

        if (sprite == null ||
            sprite.rect.width <= 0f ||
            sprite.rect.height <= 0f)
        {
            StretchWithoutAspectCalculation();
            RememberCurrentState();
            return;
        }

        Rect parentRect = parentRectTransform.rect;

        float parentWidth =
            Mathf.Max(
                1f,
                parentRect.width);

        float parentHeight =
            Mathf.Max(
                1f,
                parentRect.height);

        float spriteAspect =
            sprite.rect.width /
            sprite.rect.height;

        float parentAspect =
            parentWidth /
            parentHeight;

        float fittedWidth;
        float fittedHeight;

        if (parentAspect > spriteAspect)
        {
            // Canvas is relatively wider:
            // fill width and crop at top/bottom.
            fittedWidth = parentWidth;
            fittedHeight = parentWidth / spriteAspect;
        }
        else
        {
            // Canvas is relatively taller:
            // fill height and crop at left/right.
            fittedHeight = parentHeight;
            fittedWidth = parentHeight * spriteAspect;
        }

        rectTransform.anchorMin =
            new Vector2(
                0.5f,
                0.5f);

        rectTransform.anchorMax =
            new Vector2(
                0.5f,
                0.5f);

        rectTransform.pivot =
            new Vector2(
                0.5f,
                0.5f);

        rectTransform.anchoredPosition =
            Vector2.zero;

        rectTransform.sizeDelta =
            new Vector2(
                fittedWidth,
                fittedHeight);

        rectTransform.localScale =
            Vector3.one;

        RememberCurrentState();
    }

    private void StretchWithoutAspectCalculation()
    {
        rectTransform.anchorMin =
            Vector2.zero;

        rectTransform.anchorMax =
            Vector2.one;

        rectTransform.pivot =
            new Vector2(
                0.5f,
                0.5f);

        rectTransform.offsetMin =
            Vector2.zero;

        rectTransform.offsetMax =
            Vector2.zero;

        rectTransform.localScale =
            Vector3.one;
    }

    private bool NeedsRefresh()
    {
        CacheReferences();

        if (rectTransform == null ||
            image == null ||
            parentRectTransform == null)
        {
            return false;
        }

        Vector2 parentSize =
            parentRectTransform.rect.size;

        return lastScreenSize.x != Screen.width ||
               lastScreenSize.y != Screen.height ||
               lastParentSize != parentSize ||
               lastSprite != image.sprite;
    }

    private void RememberCurrentState()
    {
        lastScreenSize =
            new Vector2Int(
                Screen.width,
                Screen.height);

        lastParentSize =
            parentRectTransform != null
                ? parentRectTransform.rect.size
                : Vector2.zero;

        lastSprite =
            image != null
                ? image.sprite
                : null;
    }

    private void CacheReferences()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        if (image == null)
            image = GetComponent<Image>();

        if (parentRectTransform == null &&
            rectTransform != null)
        {
            parentRectTransform =
                rectTransform.parent as RectTransform;
        }
    }
}
