using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class MapNodeObject : MonoBehaviour
{
    public MapNode mapNode;
    [SerializeField] SpriteRenderer spriteRenderer;
    [SerializeField] float expandSize = 1.1f;
    [SerializeField] float blinkInterval = 1f;
    Vector3 originScale;
    Tooltip tooltip;
    Sequence blinkSeq;
    
    private void OnMouseEnter()
    {
        if(mapNode == null) return;
        if(MapManager.Inst.curNode.childNodes.Find(x => x == mapNode.ID) == null) return;

        transform.localScale = originScale * expandSize;
        spriteRenderer.sprite = mapNode.nodeImg;
        // Color color = spriteRenderer.color;
        // color.a = 1f;
        // spriteRenderer.color = color;

        MapManager.Inst.lookatNode = this;
    }

    private void OnMouseOver()
    {
        if(mapNode == null) return;
        if(MapManager.Inst.curNode.childNodes.Find(x => x == mapNode.ID) == null) return;

        transform.localScale = originScale * expandSize;
        spriteRenderer.sprite = mapNode.nodeImg;
        // Color color = spriteRenderer.color;
        // color.a = 1f;
        // spriteRenderer.color = color;
    }

    private void OnMouseExit()
    {
        if(mapNode == null) return;
        if(MapManager.Inst.curNode.childNodes.Find(x => x == mapNode.ID) == null) return;
        if(mapNode == MapManager.Inst.curNode || MapManager.Inst.player_moveable == false) return;

        transform.localScale = originScale;
        spriteRenderer.sprite = mapNode.hideNodeImg;
        // Color color = spriteRenderer.color;
        // color.a = 0.5f;
        // spriteRenderer.color = color;

        MapManager.Inst.lookatNode = null;
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
        tooltip = GetComponent<Tooltip>();
        originScale = transform.localScale;
        // Color color = spriteRenderer.color;
        // color.a = 0.5f;
        // spriteRenderer.color = color;
        blinkSeq = DOTween.Sequence()
            .Append(transform.DOScale(originScale * expandSize, blinkInterval / 2))
            .Join(spriteRenderer.DOColor(Color.black, blinkInterval / 2))
            .Append(transform.DOScale(originScale, blinkInterval / 2))
            .Join(spriteRenderer.DOColor(Color.white, blinkInterval / 2))
            .SetLoops(-1)
            .SetAutoKill(false);
    }

    private void Update()
    {
        if(mapNode != null && mapNode == MapManager.Inst.curNode)
        {
            transform.localScale = originScale * expandSize;
            spriteRenderer.sprite = mapNode.nodeImg;
            // Color color = spriteRenderer.color;
            // color.a = 1f;
            // spriteRenderer.color = color;
        }
        if(tooltip != null)
        {
            tooltip.tooltipTitle = mapNode.title;
            tooltip.tooltipTxt = mapNode.text;
            tooltip.tooltipPos = Camera.main.WorldToScreenPoint(transform.position) - Camera.main.WorldToScreenPoint(Vector3.zero);
            tooltip.tooltipPos += MapManager.Inst.tooltipOffset;
        }

        if(MapManager.Inst.curNode.childNodes.Find(x => x == mapNode.ID) == null && blinkSeq.IsActive())
        {
            blinkSeq.Kill();
            transform.localScale = originScale;
        }
        else if(MapManager.Inst.curNode.childNodes.Find(x => x == mapNode.ID) != null)
        {
            if (MapManager.Inst.lookatNode != null)
            {
                blinkSeq.Pause();
                if(MapManager.Inst.lookatNode != this)
                {
                    transform.localScale = originScale;
                    spriteRenderer.color = Color.white;
                }
            }
            else
            {
                if(!blinkSeq.IsActive() || !blinkSeq.IsPlaying())
                {
                    blinkSeq.Restart();
                }
                else
                {
                    blinkSeq.Play();
                }
            }
        }
    }

    private void OnDestroy()
    {
        if(blinkSeq != null && blinkSeq.IsActive())
        {
            blinkSeq.Kill();
        }
    }
}
