using UnityEngine;
using UnityEngine.EventSystems;

public class RelicHalfUI_Enhance : RelicHalfUI, IPointerClickHandler
{
    public bool clickable;

    public override void SetRelicHalf(RelicItem rItem)
    {
        base.SetRelicHalf(rItem);
        if (rItem.isEnhanced)
        {
            Color relicColor = relicImg.color;
            relicColor.a = 0.3f;
            relicImg.color = relicColor;
            clickable = false;
        }
        else
        {
            Color relicColor = relicImg.color;
            relicColor.a = 1f;
            relicImg.color = relicColor;
            clickable = true;
        }
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (clickable == false) return;
        NPCMerchantManager.Inst.EnhanceRelicSelect(this);
    }
}
