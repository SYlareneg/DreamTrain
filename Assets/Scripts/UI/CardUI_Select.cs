using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardUI_Select : CardUI, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData data)
    {
        CardManager.Inst.UnSelectCard(this.gameObject);
    }
}
