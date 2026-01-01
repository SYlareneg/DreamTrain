using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MapNodeTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] GameObject tooltipPrefab;
    [SerializeField] GameObject typeIconPrefab;
    [SerializeField] Sprite[] encounterTypeIcons;
    GameObject tooltip;
    RectTransform rect;
    public string tooltipTitle, tooltipTxt;
    public Vector2 tooltipPos;
    public Vector2 tooltipPivot = new Vector2(0, 1);
    public bool tooltipDisable = false;
    bool objectEnter;

    public void SetupTooltip()
    {
        if (tooltip != null || tooltipDisable == true) return;
        Vector3 tooltipScreenPos = Camera.main.WorldToScreenPoint(tooltipPos + MapManager.Inst.tooltipOffset) - Camera.main.WorldToScreenPoint(Camera.main.transform.position);
        Vector3 newPos = new Vector3(tooltipScreenPos.x, tooltipScreenPos.y, 0);
        tooltip = Instantiate(tooltipPrefab, newPos, Utils.QI);
        Canvas canvas = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<Canvas>();
        if (canvas == null) return;
        tooltip.transform.SetParent(canvas.transform, false);
        tooltip.transform.SetAsLastSibling();
        tooltip.GetComponent<Image>().raycastTarget = false;
        var tooltipRect = tooltip.GetComponent<RectTransform>();
        tooltipRect.anchoredPosition = new Vector2(tooltipScreenPos.x, tooltipScreenPos.y);
        tooltipRect.pivot = tooltipPivot;

        Vector3[] corners = new Vector3[4];
        tooltipRect.GetWorldCorners(corners);
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        bool outRight = corners[2].x > screenSize.x;
        bool outLeft = corners[0].x < 0;
        bool outTop = corners[1].y > screenSize.y;
        bool outBottom = corners[0].y < 0;
        Vector2 pivot = tooltipRect.pivot;
        if (outRight)
        {
            pivot.x = 1;
            tooltip.transform.position += Camera.main.WorldToScreenPoint(new Vector3(MapManager.Inst.tooltipOffset.x * -2, 0, 0)) - Camera.main.WorldToScreenPoint(Vector3.zero);
            tooltipRect.anchoredPosition += new Vector2(MapManager.Inst.tooltipOffset.x * -2, 0);
        }
        else if (outLeft) pivot.x = 0;
        if (outTop) pivot.y = 1;
        else if (outBottom)
        {
            pivot.y = 0;
            tooltip.transform.position += Camera.main.WorldToScreenPoint(new Vector3(0, MapManager.Inst.tooltipOffset.y * -2, 0)) - Camera.main.WorldToScreenPoint(Vector3.zero);
            tooltipRect.anchoredPosition += new Vector2(0, MapManager.Inst.tooltipOffset.y * -2);
        }
        tooltipRect.pivot = pivot;

        TMP_Text[] tooltipTMP = tooltip.GetComponentsInChildren<TMP_Text>();
        tooltipTMP[0].text = tooltipTitle;
        tooltipTMP[1].text = tooltipTxt;

        Transform mapNodeType = tooltip.transform.Find("TooltipTitle/MapNodeType");
        if (mapNodeType != null)
        {
            List<string> nextNodeIDs = MapManager.Inst.curNode.childNodes;
            MapNode thisNode = GetComponent<MapNodeObject>().mapNode;
            if (nextNodeIDs.Contains(thisNode.ID) == false)
            {
                for(int i = 0; i < thisNode.encounterNum; i++)
                {
                    var typeIcon = Instantiate(typeIconPrefab, mapNodeType);
                    typeIcon.GetComponent<Image>().sprite = encounterTypeIcons[0];
                }
            }
            else
            {
                List<EncounterType> encounterTypes = MapManager.Inst.GetEncounterType(thisNode.locationID);
                for(int i = 0; i < encounterTypes.Count; i++)
                {
                    var typeIcon = Instantiate(typeIconPrefab, mapNodeType);
                    typeIcon.GetComponent<Image>().sprite = encounterTypeIcons[(int)encounterTypes[i] + 1];
                }
            }
        }
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

    void OnMouseEnter()
    {
        if (objectEnter) return;
        SetupTooltip();
        objectEnter = true;
    }

    void OnMouseExit()
    {
        if (!objectEnter) return;
        HideTooltip();
        objectEnter = false;
    }

    private void Start()
    {
        rect = GetComponent<RectTransform>();
        objectEnter = false;
    }

    private void Update()
    {
        if (objectEnter) return;
        if(tooltip != null) HideTooltip();
    }
}
