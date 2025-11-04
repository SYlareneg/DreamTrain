using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardUI_Select : CardUI, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData data)
    {
        int idx = CardManager.Inst.selectedCardList.IndexOf(this.gameObject);
        CardManager.Inst.selectedCardList.Remove(this.gameObject);
        Destroy(this.gameObject);
        if (CardManager.Inst.cardSelectMode == ECardSelectMode.Discard)
        {
            CardManager.Inst.discardCardList[idx].gameObject.SetActive(true);
            CardManager.Inst.EnlargeCard(false, CardManager.Inst.discardCardList[idx]);
            CardManager.Inst.discardCardList.RemoveAt(idx);
        }
        CardManager.Inst.cardSelectNum++;
    }
}
