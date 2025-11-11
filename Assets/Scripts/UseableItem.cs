using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using DG.Tweening;

public class UseableItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    UseItem uItem;
    [SerializeField] Image image;
    bool isDragged;
    bool onItemArea;
    Vector3 originPos;
    RectTransform rect;
    RectTransform area;
    Vector3[] areaCorners = new Vector3[4];

    public void Setup(UseItem item)
    {
        uItem = item;
        image.sprite = item.sprite;
    }
    public void UseItem()
    {
        switch (uItem.name)
        {
            case "화살":
                TurnManager.Inst.EnemyTakeDmg(10);
                break;
            case "나무 방패":
                TurnManager.Inst.GetShield(false, 10);
                break;
            case "레테의 눈물":
                for (int i = BuffManager.Inst.playerShowBuffs.Count - 1; i >= 0; i--)
                {
                    BuffManager.Inst.playerShowBuffs[i].RemoveShowBuff();
                }
                GameManager.Inst.SetPlayerBuffUI();
                break;
            case "오염된 주사바늘":
                TurnManager.Inst.TakeDmg(-12);
                BuffManager.Inst.AddShowBuff("취약", EBuffAffectType.Player, 2);
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
            UseItem();
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
    }
    
    private void Update()
    {
        if (isDragged)
        {
            this.transform.position = Input.mousePosition;
        }

        DetectItemArea();
    }
}
