using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;

public class RelicUI : MonoBehaviour
{
    [SerializeField] Image relicImg1;
    [SerializeField] GameObject relicMask1;
    [SerializeField] Image relicImg2;
    [SerializeField] GameObject relicMask2;

    public RelicItem relicItem1, relicItem2;

    public void Setup(RelicItem rItem1, RelicItem rItem2)
    {
        if (rItem1 == null)
        {
            this.relicItem1 = null;
            this.relicItem2 = null;
            return;
        }

        this.relicItem1 = rItem1;
        if (relicImg1) relicImg1.sprite = rItem1.relicSprite;
        Tooltip tooltip = relicMask1.GetComponent<Tooltip>();
        if (tooltip)
        {
            tooltip.tooltipTitle = rItem1.relicName;
            tooltip.tooltipTxt = rItem1.relicTxt;
        }

        if (rItem2 == null)
        {
            relicImg1.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            relicMask1.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            relicImg2.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            if (relicImg2) relicImg2.sprite = rItem2.relicSprite;
            Tooltip tooltip2 = relicMask2.GetComponent<Tooltip>();
            if (tooltip2)
            {
                tooltip2.tooltipTitle = rItem2.relicName;
                tooltip2.tooltipTxt = rItem2.relicTxt;
            }
        }
    }
}
