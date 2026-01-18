using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RoomDPIcon : MonoBehaviour, IPointerClickHandler
{
    public DreamPiece_Reference dreamPiece;
    [SerializeField] Image iconImage;
    [SerializeField] Image highlight;
    [SerializeField] Sprite[] highlightSprites;

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
    }
}
