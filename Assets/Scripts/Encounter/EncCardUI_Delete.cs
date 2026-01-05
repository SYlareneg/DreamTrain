using UnityEngine;
using UnityEngine.EventSystems;

public class EncCardUI_Delete : CardUI, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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
        EncSofaManager.Inst.DeleteCardSelect(this);
    }
}