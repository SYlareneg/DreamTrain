using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;

public class RelicUI : MonoBehaviour
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
        Tooltip tooltip = GetComponent<Tooltip>();
        if (tooltip) tooltip.tooltipTxt = rItem.relicTxt;
    }
}
