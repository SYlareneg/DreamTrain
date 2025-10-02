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
    public Passive passive;
    public bool isSelected;

    public void Setup(Passive pSet)
    {
        passive = pSet;
        passiveImage.sprite = pSet.sprite;
        passiveNameTMP.text = pSet.name;
        Select(false);
    }

    public void OnPointerClick(PointerEventData data)
    {
        DeckBuildManager.Inst.SelectPassive(passive);
    }

    public void Select(bool onSelect)
    {
        isSelected = onSelect;
        if (!isSelected)
        {
            passiveImage.color = Color.gray;
        }
        else
        {
            passiveImage.color = Color.white;
        }
    }
}
