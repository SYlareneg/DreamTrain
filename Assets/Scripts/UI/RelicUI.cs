using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;

public class RelicUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image backgroundImg;
    [SerializeField] Image relicImg;
    [SerializeField] TMP_Text relicNameTMP;
    [SerializeField] TMP_Text relicTextTMP;

    public RelicItem relicItem;

    public void Setup(RelicItem rItem)
    {
        if (rItem == null)
        {
            this.relicItem = null;
            return;
        }

        this.relicItem = rItem;

        if (relicImg) relicImg.sprite = rItem.relicSprite;
        if (relicNameTMP) relicNameTMP.text = rItem.relicName;
        if (relicTextTMP) relicTextTMP.text = rItem.relicTxt;
    }

    public void OnPointerEnter(PointerEventData data)
    {
        var tooltipComponent = GetComponent<Tooltip>();
        if (tooltipComponent)
        {
            tooltipComponent.SetupTooltip(this.transform.position, relicItem.relicTxt);
        }
    }

    public void OnPointerExit(PointerEventData data)
    {
        var tooltipComponent = GetComponent<Tooltip>();
        if (tooltipComponent)
        {
            tooltipComponent.HideTooltip();
        }
    }
}
