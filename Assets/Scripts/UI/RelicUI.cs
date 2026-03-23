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
    Tooltip tooltip1, tooltip2;
    [SerializeField] Vector2 tooltipOffset;

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
        tooltip1 = relicMask1.GetComponent<Tooltip>();
        if (tooltip1)
        {
            tooltip1.tooltipTitle = rItem1.relicName;
            tooltip1.tooltipTxt = rItem1.relicTxt;
        }
        var relicHalf1 = relicMask1.GetComponent<RelicHalfUI>();
        if(relicHalf1 != null)
        {
            relicHalf1.SetRelicHalf(relicItem1);
        }

        if (rItem2 == null)
        {
            relicImg1.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            relicMask1.GetComponent<RectTransform>().anchorMax = new Vector2(1f, 1f);
            relicImg2.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            relicItem2 = rItem2;
            if (relicImg2) relicImg2.sprite = rItem2.relicSprite;
            tooltip2 = relicMask2.GetComponent<Tooltip>();
            if (tooltip2)
            {
                tooltip2.tooltipTitle = rItem2.relicName;
                tooltip2.tooltipTxt = rItem2.relicTxt;
            }
            var relicHalf2 = relicMask2.GetComponent<RelicHalfUI>();
            if(relicHalf2 != null)
            {
                relicHalf2.SetRelicHalf(relicItem2);
            }
        }
    }

    private void Update()
    {
        Vector3 screenCenter = Camera.main.WorldToScreenPoint(new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0));
        if(tooltip1 != null && relicItem1 != null && relicMask1 != null)
        {
            tooltip1.tooltipPos = (Vector2)relicMask1.transform.position - (Vector2)screenCenter;
            tooltip1.tooltipPos.y -= 50;
            tooltip1.tooltipPos.x += 100;
            Debug.Log(relicMask1.transform.position);
            Debug.Log(tooltipOffset);
        }
        
        if(tooltip2 != null && relicItem2 != null && relicMask2 != null)
        {
            tooltip2.tooltipPos = (Vector2)relicMask2.transform.position + tooltipOffset - (Vector2)screenCenter;
            tooltip2.tooltipPos.x += 100;
            tooltip2.tooltipPos.y -= 50;
        }
    }
}
