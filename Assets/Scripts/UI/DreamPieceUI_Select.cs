using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DreamPieceUI_Select : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image personaImage;
    [SerializeField] Image shadowImage;
    [SerializeField] TMP_Text dreamPieceNameTMP;
    public DreamPiece_Reference dreamPiece;
    public bool isSelected;

    public void Setup(DreamPiece_Reference dp)
    {
        dreamPiece = dp;
        dreamPieceNameTMP.text = dp.name;
        if (dp.persona.isEnhanced)
        {
            personaImage.sprite = dp.persona.enhancedPassive.sprite;
        }
        else
        {
            personaImage.sprite = dp.persona.sprite;
        }
        if (dp.shadow.isEnhanced)
        {
            shadowImage.sprite = dp.shadow.enhancedPassive.sprite;
        }
        else
        {
            shadowImage.sprite = dp.shadow.sprite;
        }
        Select(false, Color.white);
    }

    public void OnPointerClick(PointerEventData data)
    {
        if(isSelected) return;
        NPCPassiveSelectManager.Inst.SetPassive(this);
    }

    public void Select(bool onSelect, Color color)
    {
        personaImage.GetComponent<Outline>().effectColor = color;
        shadowImage.GetComponent<Outline>().effectColor = color;
        isSelected = onSelect;
        if (!isSelected)
        {
            personaImage.GetComponent<Outline>().enabled = false;
            shadowImage.GetComponent<Outline>().enabled = false;
        }
        else
        {
            personaImage.GetComponent<Outline>().enabled = true;
            shadowImage.GetComponent<Outline>().enabled = true;
        }
    }
}
