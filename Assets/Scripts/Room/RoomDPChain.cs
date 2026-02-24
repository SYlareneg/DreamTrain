using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class RoomDPChain : MonoBehaviour, IPointerDownHandler, IDragHandler, IEndDragHandler
{
    Vector2 originalPivot;
    Vector3 originalPosition;
    [SerializeField] float offset;
    public GameObject[] chainArrows;
    public List<Vector2> arrowOriginalPositions = new List<Vector2>();
    public Sequence arrowSeq;
    public void Activate()
    {
        GetComponent<Image>().enabled = true;
        RectTransform rt = GetComponent<RectTransform>();
        originalPivot = rt.pivot;
        DOTween.To(() => rt.pivot, (x) => rt.pivot = x, new Vector2(0.6f, originalPivot.y), 0.5f)
        .OnComplete(() =>
        {
            arrowSeq = DOTween.Sequence();
            foreach(var arrow in chainArrows)
            {
                arrowSeq.Join(arrow.transform.DOLocalMoveX(arrow.transform.localPosition.x - 20, 0.8f).SetEase(Ease.InOutSine));
                foreach(var arrowSR in arrow.GetComponentsInChildren<Image>())
                {
                    arrowSeq.Join(arrowSR.DOColor(Color.white, 0.8f).SetEase(Ease.InOutSine));
                }
            }
            arrowSeq.SetLoops(-1, LoopType.Yoyo);
            arrowSeq.Play();
        });

        arrowOriginalPositions.Clear();
        foreach(var arrow in chainArrows)
        {
            arrow.SetActive(true);
            arrowOriginalPositions.Add(arrow.transform.localPosition);
            foreach(var arrowSR in arrow.GetComponentsInChildren<Image>())
            {
                Color c = arrowSR.color;
                c *= 0.5f;
                c.a = 1f;
                arrowSR.color = c;
            }
        }
        
    }

    public void DeActivate()
    {
        RectTransform rt = GetComponent<RectTransform>();
        DOTween.To(() => rt.pivot, (x) => rt.pivot = x, originalPivot, 0.5f).SetEase(Ease.OutBack);

        // arrowSeq.Pause();
        foreach(var arrow in chainArrows)
        {
            arrow.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        GetComponent<AudioSource>().PlayOneShot(GetComponent<AudioSource>().clip);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position += new Vector3(eventData.delta.x, 0, 0);
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, originalPosition.x - GetComponent<RectTransform>().rect.width / 2 - offset, originalPosition.x), transform.position.y, transform.position.z);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if(transform.position.x <= originalPosition.x - GetComponent<RectTransform>().rect.width / 2 - offset)
        {
            RoomDPManager.Inst.StartGame();
        }
        else
        {
            arrowSeq.Play();
        }
        DOTween.To(() => transform.position, (x) => transform.position = x, originalPosition, 0.5f).SetEase(Ease.OutBack);
    }

    private void Start()
    {
        originalPivot = GetComponent<RectTransform>().pivot;
        originalPosition = transform.position;
    }
}
