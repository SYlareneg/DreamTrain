using UnityEngine;
using UnityEngine.UI;

public class RelicHalfUI : MonoBehaviour
{
    public Image relicImg;
    public RelicItem relicItem;
    
    public virtual void SetRelicHalf(RelicItem rItem)
    {
        relicItem = rItem;
        relicImg.sprite = rItem.relicSprite;
        Tooltip tooltip = GetComponent<Tooltip>();
        if(tooltip != null)
        {
            tooltip.tooltipTitle = relicItem.relicName;
            tooltip.tooltipTxt = relicItem.relicTxt;
        }
    }
}
