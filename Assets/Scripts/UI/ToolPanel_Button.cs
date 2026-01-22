using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class ToolPanel_Button : MonoBehaviour, IPointerClickHandler
{
    private Image arrow;
    Sequence arrowSeq;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        arrow = transform.GetChild(0).GetComponent<Image>();
        arrowSeq = DOTween.Sequence();
        arrowSeq.Append(arrow.DOFade(0.2f, 0.8f));
        arrowSeq.Append(arrow.DOFade(1f, 0.8f));
        arrowSeq.SetLoops(-1, LoopType.Yoyo);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(arrow.gameObject.activeSelf || arrowSeq.IsPlaying())
        {
            arrowSeq.Kill();
            arrow.gameObject.SetActive(false);
        }
        ToolPanel panel = transform.parent.GetComponent<ToolPanel>();
        panel.TogglePanel();
    }
}
