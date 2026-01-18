using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class RoomDPChain : MonoBehaviour, IDragHandler, IEndDragHandler
{
    Vector2 originalPivot;
    Vector3 originalPosition;
    public void Activate()
    {
        GetComponent<Image>().enabled = true;
        RectTransform rt = GetComponent<RectTransform>();
        originalPivot = rt.pivot;
        DOTween.To(() => rt.pivot, (x) => rt.pivot = x, new Vector2(0.5f, originalPivot.y), 0.5f);
    }

    public void DeActivate()
    {
        RectTransform rt = GetComponent<RectTransform>();
        DOTween.To(() => rt.pivot, (x) => rt.pivot = x, originalPivot, 0.5f).SetEase(Ease.OutBack);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position += new Vector3(eventData.delta.x, 0, 0);
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, originalPosition.x - GetComponent<RectTransform>().rect.width / 2, originalPosition.x), transform.position.y, transform.position.z);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(transform.position.x <= originalPosition.x - GetComponent<RectTransform>().rect.width / 2)
        {
            RoomDPManager.Inst.StartGame();
        }
        DOTween.To(() => transform.position, (x) => transform.position = x, originalPosition, 0.5f).SetEase(Ease.OutBack);
    }

    private void Start()
    {
        originalPivot = GetComponent<RectTransform>().pivot;
        originalPosition = transform.position;
    }
}
