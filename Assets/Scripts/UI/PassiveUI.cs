using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PassiveUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image passiveImage;
    [SerializeField] TMP_Text passiveNameTMP;
    public DreamPiece dreamPiece;
    public EPassiveType pType;
    public bool isSelected;

    public void Setup(DreamPiece dp, EPassiveType p)
    {
        dreamPiece = dp;
        pType = p;
        if (p == EPassiveType.Persona)
        {
            passiveImage.sprite = dp.persona.sprite;
            passiveNameTMP.text = dp.persona.name;
        }
        else if (p == EPassiveType.Shadow)
        {
            passiveImage.sprite = dp.shadow.sprite;
            passiveNameTMP.text = dp.shadow.name;
        }
        Select(false);
    }

    public void OnPointerClick(PointerEventData data)
    {
        DeckBuildManager.Inst.SelectPassive(dreamPiece, pType);
    }

    public void Select(bool onSelect)
    {
        isSelected = onSelect;
        if (!isSelected)
        {
            passiveImage.color = Color.black;
        }
        else
        {
            passiveImage.color = Color.white;
        }
    }
}
