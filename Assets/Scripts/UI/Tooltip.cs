using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class Tooltip : MonoBehaviour
{
    public void SetupTooltip(Vector3 tooltipPos, string tooltipTxt)
    {
        DeckBuildManager.Inst.tooltip.transform.position = tooltipPos;
        DeckBuildManager.Inst.tooltipTxt.text = tooltipTxt;
        DeckBuildManager.Inst.tooltip.gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        DeckBuildManager.Inst.tooltip.gameObject.SetActive(false);
    }
}
