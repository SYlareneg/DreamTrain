using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class MapNodeTooltip : Tooltip
{
    [SerializeField] GameObject mapNodeTypeIconPrefab;
    [SerializeField] GameObject mapNodeType;
    [SerializeField] Sprite[] mapNodeTypeIcons;
    public void SetupTooltip(EncounterType encounterType)
    {
        base.SetupTooltip();
        var mapNodeTypeIcon = Instantiate(mapNodeTypeIconPrefab, mapNodeType.transform, false);
        Image icon = mapNodeTypeIcon.GetComponent<Image>();
        icon.sprite = mapNodeTypeIcons[(int)encounterType];
    }
}
