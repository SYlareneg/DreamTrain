using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RoomDPMarker : MonoBehaviour
{
    Vector2 originalPivot;
    public Image dpIconImage;
    public void Activate()
    {
        RectTransform rt = GetComponent<RectTransform>();
        originalPivot = rt.pivot;
        DOTween.To(() => rt.pivot, (x) => rt.pivot = x, new Vector2(0, originalPivot.y), 0.5f);
    }

    public void DeActivate()
    {
        RectTransform rt = GetComponent<RectTransform>();
        DOTween.To(() => rt.pivot, (x) => rt.pivot = x, originalPivot, 0.5f).SetEase(Ease.OutBack);
    }

    public void SetDPIcon(Sprite icon)
    {
        dpIconImage.sprite = icon;
        dpIconImage.enabled = true;
    }

    void Start()
    {
        dpIconImage.enabled = false;
    }
}
