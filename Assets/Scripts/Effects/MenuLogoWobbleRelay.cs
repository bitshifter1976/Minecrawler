using UnityEngine;
using UnityEngine.EventSystems;

public sealed class MenuLogoWobbleRelay :
    MonoBehaviour,
    ISelectHandler,
    IPointerEnterHandler
{
    [SerializeField] private LogoAnimator logoAnimator;

    public void OnSelect(BaseEventData eventData)
    {
        TriggerWobble();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TriggerWobble();
    }

    private void TriggerWobble()
    {
        if (logoAnimator != null)
            logoAnimator.TriggerMenuWobble();
    }
}
