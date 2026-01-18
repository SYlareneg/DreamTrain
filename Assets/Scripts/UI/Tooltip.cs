using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class Tooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject tooltipPrefab;
    GameObject tooltip;
    RectTransform rect;
    Collider2D col;
    public string tooltipTitle, tooltipTxt;
    public Vector2 tooltipPos;
    public Vector2 tooltipPivot = new Vector2(0, 1);
    public bool tooltipDisable = false;
    bool objectEnter;

    public void SetupTooltip()
    {
        if (tooltip != null || tooltipDisable == true) return;
        Vector3 newPos = new Vector3(tooltipPos.x, tooltipPos.y, 0);
        tooltip = Instantiate(tooltipPrefab, newPos, Utils.QI);
        Canvas canvas = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<Canvas>();
        if (canvas == null) return;
        tooltip.transform.SetParent(canvas.transform, false);
        tooltip.transform.SetAsLastSibling();
        tooltip.GetComponent<Image>().raycastTarget = false;
        var tooltipRect = tooltip.GetComponent<RectTransform>();
        tooltipRect.anchoredPosition = tooltipPos;
        tooltipRect.pivot = tooltipPivot;

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

        TMP_Text[] tooltipTMP = tooltip.GetComponentsInChildren<TMP_Text>();
        tooltipTMP[0].text = tooltipTitle;
        tooltipTMP[1].text = tooltipTxt;
        tooltip.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltip == null || tooltipDisable == true) return;
        tooltip.SetActive(false);
        Destroy(tooltip);
        tooltip = null;
    }

    public void OnPointerEnter(PointerEventData data)
    {
        if (objectEnter) return;
        SetupTooltip();
        objectEnter = true;
    }

    public void OnPointerExit(PointerEventData data)
    {
        if (!objectEnter) return;
        HideTooltip();
        objectEnter = false;
    }

    // void OnMouseEnter()
    // {
    //     if(TurnManager.Inst == null || TurnManager.Inst.isLoading) return;
    //     if (objectEnter) return;
    //     SetupTooltip();
    //     objectEnter = true;
    // }

    // void OnMouseExit()
    // {
    //     if (!objectEnter) return;
    //     HideTooltip();
    //     objectEnter = false;
    // }

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        col = GetComponent<Collider2D>();
        objectEnter = false;
    }

    private void Update()
    {
        if(!objectEnter && tooltip != null) HideTooltip();
        // if(TurnManager.Inst == null || TurnManager.Inst.isLoading) return;

        if(col != null)
        {
            var mp = Input.mousePosition;
            mp.z = -Camera.main.transform.position.z; // z=0 평면
            Vector2 p = Camera.main.ScreenToWorldPoint(mp);

            bool now = col.OverlapPoint(p);

            if (!objectEnter && now) SetupTooltip();
            if (objectEnter && !now) HideTooltip();

            objectEnter = now;
        }
    }

    private void OnDestroy()
    {
        if(tooltip != null) HideTooltip();
    }
}
