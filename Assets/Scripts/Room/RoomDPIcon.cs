using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class RoomDPIcon : MonoBehaviour, IPointerClickHandler
{
    public DreamPiece_Reference dreamPiece;
    [SerializeField] Image iconImage;
    [SerializeField] Image highlight;
    [SerializeField] Sprite[] highlightSprites;
    [SerializeField] Image selectHover;
    Sequence selectHoverSeq;

    public void Setup(DreamPiece_Reference dp)
    {
        dreamPiece = dp;
        if(RoomDPManager.Inst.isShowingPersona)
        {
            iconImage.sprite = dp.persona.sprite;
        }
        else
        {
            iconImage.sprite = dp.shadow.sprite;
        }
        highlight.gameObject.SetActive(false);
        highlight.sprite = highlightSprites[dp.persona.dreamPieceNum];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(dreamPiece.name != "고양이의 꿈") return;
        foreach(var dpIcon in RoomDPManager.Inst.roomDPIcons)
        {
            dpIcon.highlight.gameObject.SetActive(false);
        }
        highlight.gameObject.SetActive(true);
        RoomDPManager.Inst.SetDreamPieceView(dreamPiece);
    }

    void Start()
    {
        if(dreamPiece.name != "고양이의 꿈")
        {
            var imgList = GetComponentsInChildren<Image>();
            foreach(var img in imgList)
            {
                img.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            }
        }
        highlight.gameObject.SetActive(false);

        selectHoverSeq = DOTween.Sequence().SetLoops(-1, LoopType.Yoyo);
        selectHoverSeq.Append(selectHover.DOFade(0.3f, 0.5f));
        selectHoverSeq.Join(selectHover.transform.DOLocalMoveY(10f, 0.5f).SetRelative());
        selectHoverSeq.Append(selectHover.DOFade(1f, 0.5f));
        selectHoverSeq.Join(selectHover.transform.DOLocalMoveY(-10f, 0.5f).SetRelative());
        selectHoverSeq.Pause();
    }

    void Update()
    {
        if(dreamPiece.name != "고양이의 꿈") return;
        if (RoomDPManager.Inst.isInit)
        {
            selectHover.gameObject.SetActive(true);
            selectHoverSeq.Play();
        }
        else
        {
            selectHover.gameObject.SetActive(false);
            selectHoverSeq.Pause();
        }
    }

    void OnDestroy()
    {
        selectHoverSeq.Kill();
    }
}
