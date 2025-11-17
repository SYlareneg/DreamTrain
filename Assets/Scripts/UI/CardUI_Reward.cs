using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardUI_Reward : CardUI, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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
        GameManager.Inst.AddCardReward(this.item);
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.3f).OnComplete(() =>
        {
            GameManager.Inst.EndCardReward();
        });
    }
}
