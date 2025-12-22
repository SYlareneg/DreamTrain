using UnityEngine;
using System.Collections.Generic;

public class MapNodeObject : MonoBehaviour
{
    public MapNode mapNode;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float expandSize = 1.1f;
    Vector3 originScale;
    
    private void OnMouseEnter()
    {
        if(mapNode == null) return;
        if(MapManager.Inst.curNode.childNodes.Find(x => x == mapNode) == null) return;

        transform.localScale = originScale * expandSize;
        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;
    }

    private void OnMouseOver()
    {
        if(mapNode == null) return;
        if(MapManager.Inst.curNode.childNodes.Find(x => x == mapNode) == null) return;

        transform.localScale = originScale * expandSize;
        Color color = spriteRenderer.color;
        color.a = 1f;
        spriteRenderer.color = color;
    }

    private void OnMouseExit()
    {
        if(mapNode == null) return;
        if(MapManager.Inst.curNode.childNodes.Find(x => x == mapNode) == null) return;
        if(mapNode == MapManager.Inst.curNode || MapManager.Inst.player_moveable == false) return;

        transform.localScale = originScale;
        Color color = spriteRenderer.color;
        color.a = 0.5f;
        spriteRenderer.color = color;
    }

    private void OnMouseUpAsButton()
    {
        if(mapNode == null) return;
        if(MapManager.Inst.curNode.childNodes.Find(x => x == mapNode) == null) return;
        if(MapManager.Inst.player_moveable == false) return;
        MapManager.Inst.MovePlayerTo(mapNode);
    }

    private void Start()
    {
        originScale = transform.localScale;
        Color color = spriteRenderer.color;
        color.a = 0.5f;
        spriteRenderer.color = color;
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
    }
}
