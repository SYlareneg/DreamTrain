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
        var tooltipRect = DeckBuildManager.Inst.tooltip.GetComponent<RectTransform>();

        Vector3[] corners = new Vector3[4];
        tooltipRect.GetWorldCorners(corners);
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        bool outRight = corners[2].x > screenSize.x;
        bool outLeft = corners[0].x < 0;
        bool outTop = corners[1].y > screenSize.y;
        bool outBottom = corners[0].y < 0;
        Vector2 pivot = tooltipRect.pivot;
        if (outRight) pivot.x = 1;
        else if (outLeft) pivot.x = 0;
        if (outTop) pivot.y = 1;
        else if (outBottom) pivot.y = 0;
        tooltipRect.pivot = pivot;

        DeckBuildManager.Inst.tooltipTxt.text = tooltipTxt;
        DeckBuildManager.Inst.tooltip.SetActive(true);
    }

    public void HideTooltip()
    {
        DeckBuildManager.Inst.tooltip.SetActive(false);
    }
}
