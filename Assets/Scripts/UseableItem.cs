using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

public class UseableItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] UseItem uItem;
    [SerializeField] Image image;
    bool isDragged;
    bool onItemArea;
    int onEnemyArea;
    Vector3 originPos;
    RectTransform rect;
    RectTransform area;
    Vector3[] areaCorners = new Vector3[4];
    Tooltip tooltip;

    public void Setup(UseItem item)
    {
        uItem = item;
        image.sprite = item.sprite;
    }
    public void UseItem(int enemyIdx)
    {
        switch (uItem.name)
        {
            case "화살":
                TurnManager.Inst.EnemyTakeDmg(uItem.useValue[0], EDamageSource.UseableItem, enemyIdx);
                break;
            case "나무 방패":
                TurnManager.Inst.GetShield(false, uItem.useValue[0], EDamageSource.UseableItem);
                break;
            case "레테의 눈물":
                for (int i = BuffManager.Inst.playerShowBuffs.Count - 1; i >= 0; i--)
                {
                    BuffManager.Inst.playerShowBuffs[i].RemoveShowBuff();
                }
                GameManager.Inst.SetPlayerBuffUI();
                break;
            case "오염된 주사바늘":
                TurnManager.Inst.TakeDmg(-uItem.useValue[0], EDamageSource.UseableItem);
                BuffManager.Inst.AddShowBuff("취약", EBuffAffectType.Player, uItem.useValue[1], false);
                break;
            default:
                Debug.LogWarning("등록되지 않은 아이템 사용!");
                break;
        }
        TurnManager.OnUseableItemUse?.Invoke();
    }

    void DetectItemArea()
    {
        area.GetWorldCorners(areaCorners);
        Vector3 pos = rect.position;
        onItemArea = pos.x > areaCorners[0].x && pos.x < areaCorners[2].x && pos.y > areaCorners[0].y && pos.y < areaCorners[2].y;
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int enemylayer = LayerMask.NameToLayer("EnemyCardArea");
        int layerMask = LayerMask.GetMask("EnemyCardArea");
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, layerMask);
        var enemyHits = Array.Find(hits, x => x.collider.gameObject.layer == enemylayer);
        if(enemyHits.collider != null)
        {
            Transform enemyPos = enemyHits.collider.transform;
            onEnemyArea = EnemyManager.Inst.FindEnemyIdxByPos(enemyPos);

            if(isDragged && !onItemArea && uItem != null && uItem.isSingleTarget == true)
            {
                enemyPos.Find("EnemyImg/EnemyHighlight").gameObject.SetActive(true);
            }
        }
        else
        {
            onEnemyArea = 0;
            EnemyManager.Inst.enemyPos.Find("EnemyImg/EnemyHighlight").gameObject.SetActive(false);
            for(int i = 0; i < Enemy.maxSubEnemyNum; i++)
            {
                if(EnemyManager.Inst.subEnemies[i] != null && EnemyManager.Inst.subEnemies[i].name != null)
                {
                    onEnemyArea = -1;
                    EnemyManager.Inst.subEnemyPos[i].Find("EnemyImg/EnemyHighlight").gameObject.SetActive(false);
                }
            }

            if(isDragged && !onItemArea && uItem != null && uItem.isSingleTarget == true && onEnemyArea == 0)
            {
                EnemyManager.Inst.enemyPos.Find("EnemyImg/EnemyHighlight").gameObject.SetActive(true);
            }
        }
    }

    public void OnPointerDown(PointerEventData data)
    {
        if (TurnManager.Inst.isLoading == true) return;
        if (uItem == null) return;
        isDragged = true;
        originPos = this.transform.position;
        this.transform.SetParent(UseableItemManager.Inst.itemCanvas.transform);
    }

    public void OnPointerUp(PointerEventData data)
    {
        isDragged = false;
        if (uItem == null) return;
        if (!onItemArea)
        {
            if(uItem.isSingleTarget == true && onEnemyArea == -1)
            {
                this.transform.DOMove(originPos, 0.5f, false).OnComplete(() =>
                {
                    this.transform.SetParent(UseableItemManager.Inst.itemListScroll.transform);
                });
                return;
            }
            UseItem(onEnemyArea);
            //UseableItemManager.Inst.playerItemSO.useableItems.Remove(uItem);
            Destroy(this.gameObject);
        }
        else
        {
            this.transform.DOMove(originPos, 0.5f, false).OnComplete(() =>
            {
                this.transform.SetParent(UseableItemManager.Inst.itemListScroll.transform);
            });
        }
    }

    private void Start()
    {
        rect = this.GetComponent<RectTransform>();
        area = UseableItemManager.Inst.itemArea;
        tooltip = this.GetComponent<Tooltip>();
    }
    
    private void Update()
    {
        if (isDragged)
        {
            this.transform.position = Input.mousePosition;
        }

        DetectItemArea();

        if(tooltip != null)
        {
            tooltip.tooltipPos = new Vector2(this.transform.position.x - Screen.width / 2, this.transform.position.y - Screen.height / 2);
            if (isDragged)
            {
                tooltip.enabled = false;
            }
            else
            {
                tooltip.enabled = true;
            }
        }
        else
        {
            tooltip = this.GetComponent<Tooltip>();
        }
    }
}
