using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class RoomDPSlider : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Vector2 originalPivot;
    Vector3 originalScale;
    public void Activate()
    {
        GetComponent<Image>().enabled = true;
        RectTransform rt = GetComponent<RectTransform>();
        originalPivot = rt.pivot;
        DOTween.To(() => rt.pivot, (x) => rt.pivot = x, new Vector2(1 - originalPivot.x, originalPivot.y), 0.5f);
    }

    public void DeActivate()
    {
        RectTransform rt = GetComponent<RectTransform>();
        DOTween.To(() => rt.pivot, (x) => rt.pivot = x, originalPivot, 0.5f).SetEase(Ease.OutBack);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = originalScale * 1.1f;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = originalScale;
    }

    private void Start()
    {
        originalPivot = GetComponent<RectTransform>().pivot;
        originalScale = transform.localScale;
    }
}
