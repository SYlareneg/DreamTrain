using UnityEngine;
using System.Collections.Generic;

public class MapNodeObject : MonoBehaviour
{
    public MapNode mapNode;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float expandSize = 1.1f;
    Vector3 originScale;
    Tooltip tooltip;
    
    private void OnMouseEnter()
    {
        if(mapNode == null) return;
        if(MapManager.Inst.curNode.childNodes.Find(x => x == mapNode.ID) == null) return;

        transform.localScale = originScale * expandSize;
        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;
    }

    private void OnMouseOver()
    {
        if(mapNode == null) return;
        if(MapManager.Inst.curNode.childNodes.Find(x => x == mapNode.ID) == null) return;

        transform.localScale = originScale * expandSize;
        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;
    }

    private void OnMouseExit()
    {
        if(mapNode == null) return;
        if(MapManager.Inst.curNode.childNodes.Find(x => x == mapNode.ID) == null) return;
        if(mapNode == MapManager.Inst.curNode || MapManager.Inst.player_moveable == false) return;

        transform.localScale = originScale;
        Color color = spriteRenderer.color;
        color.a = 0.5f;
        spriteRenderer.color = color;
    }

    private void OnMouseUpAsButton()
    {
        if(mapNode == null) return;
        if(MapManager.Inst.curNode.childNodes.Find(x => x == mapNode.ID) == null) return;
        if(MapManager.Inst.player_moveable == false) return;
        MapManager.Inst.MovePlayerTo(mapNode);
    }

    private void Start()
    {
        originScale = transform.localScale;
        Color color = spriteRenderer.color;
        color.a = 0.5f;
        spriteRenderer.color = color;
        tooltip = GetComponent<Tooltip>();
    }

    private void Update()
    {
        if(mapNode != null && mapNode == MapManager.Inst.curNode)
        {
            transform.localScale = originScale * expandSize;
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
        if(tooltip != null)
        {
            tooltip.tooltipTitle = mapNode.title;
            tooltip.tooltipTxt = mapNode.text;
            tooltip.tooltipPos = Camera.main.WorldToScreenPoint(transform.position) - Camera.main.WorldToScreenPoint(Vector3.zero);
            tooltip.tooltipPos += MapManager.Inst.tooltipOffset;
        }
    }
}
