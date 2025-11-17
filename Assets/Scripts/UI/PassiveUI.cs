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
    public DreamPiece_Reference dreamPiece;
    public EPassiveType pType;
    public bool isSelected;

    public void Setup(DreamPiece_Reference dp, EPassiveType p)
    {
        dreamPiece = dp;
        pType = p;
        if (p == EPassiveType.Persona)
        {
            if (dp.persona.isEnhanced)
            {
                passiveImage.sprite = dp.persona.enhancedPassive.sprite;
                passiveNameTMP.text = dp.name;
            }
            else
            {
                passiveImage.sprite = dp.persona.sprite;
                passiveNameTMP.text = dp.name;
            }
        }
        else if (p == EPassiveType.Shadow)
        {
            if (dp.shadow.isEnhanced)
            {
                passiveImage.sprite = dp.shadow.enhancedPassive.sprite;
                passiveNameTMP.text = dp.name;
            }
            else
            {
                passiveImage.sprite = dp.shadow.sprite;
                passiveNameTMP.text = dp.name;
            }
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
