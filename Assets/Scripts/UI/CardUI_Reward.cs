using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardUI_Reward : CardUI, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData data)
    {
        GameManager.Inst.AddCardReward(this.item);
        Sequence sequence = DOTween.Sequence();
        sequence.AppendInterval(0.3f).OnComplete(() =>
        {
            GameManager.Inst.EndCardReward();
        });

        GetComponent<AudioSource>().PlayOneShot(SoundManager.Inst.UISelectSFX);
    }
}
