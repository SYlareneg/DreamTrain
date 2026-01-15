using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RoomDPIcon : MonoBehaviour, IPointerClickHandler
{
    public DreamPiece_Reference dreamPiece;

    public void Setup(DreamPiece_Reference dp)
    {
        dreamPiece = dp;
        if(RoomDPManager.Inst.isShowingPersona)
        {
            GetComponent<Image>().sprite = dp.persona.sprite;
        }
        else
        {
            GetComponent<Image>().sprite = dp.shadow.sprite;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(dreamPiece.name != "고양이의 꿈") return;
        RoomDPManager.Inst.SetDreamPieceView(dreamPiece);
    }
}
