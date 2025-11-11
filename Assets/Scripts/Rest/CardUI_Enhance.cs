using UnityEngine;
using UnityEngine.EventSystems;

public class CardUI_Enhance : CardUI, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public void OnPointerEnter(PointerEventData data)
    {
        this.transform.localScale *= 1.3f;
    }
    public void OnPointerExit(PointerEventData data)
    {
        this.transform.localScale /= 1.3f;
    }
    public void OnPointerClick(PointerEventData data)
    {
        NPCMerchantManager.Inst.EnhanceCardSelect(this);
    }
}
