using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class CardUI_DeckBuild : CardUI, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public GameObject cardUIPrefab;
    public GraphicRaycaster raycaster;

    public virtual void OnBeginDrag(PointerEventData data)
    {
        if(DeckBuildManager.Inst.isLoading == false && item.num > 0)
        {
            DeckBuildManager.Inst.isLoading = true;
            DeckBuildManager.Inst.draggingCardUI = Instantiate(cardUIPrefab, this.transform.position, Utils.QI);
            DeckBuildManager.Inst.draggingCardUI.transform.SetParent(DeckBuildManager.Inst.backgroundPanel.transform);
            DeckBuildManager.Inst.draggingCardUI.GetComponent<RectTransform>().sizeDelta = this.GetComponent<RectTransform>().sizeDelta;
            var draggingCard = DeckBuildManager.Inst.draggingCardUI.GetComponent<CardUI_DeckBuild>();
            draggingCard.Setup(this.item);
            this.item = null;
            SetBlank();
        }
    }

    public void OnDrag(PointerEventData data)
    {
        if(DeckBuildManager.Inst.draggingCardUI != null)
        {
            DeckBuildManager.Inst.draggingCardUI.transform.position = Input.mousePosition;
        }
    }

    public virtual void OnEndDrag(PointerEventData data)
    {
        if(DeckBuildManager.Inst.draggingCardUI != null)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(data, results);
            int hitflag = 1;
            foreach(var result in results)
            {
                CardUI_DeckBuild hitcard = result.gameObject.GetComponent<CardUI_DeckBuild>();
                if(hitcard != null && result.gameObject != DeckBuildManager.Inst.draggingCardUI)
                {
                    if(hitcard.item != null)
                    {
                        Item tempItem = hitcard.item;
                        hitcard.item.num++;
                        hitcard.Setup(DeckBuildManager.Inst.draggingCardUI.GetComponent<CardUI_DeckBuild>().item);
                        Destroy(DeckBuildManager.Inst.draggingCardUI);
                        var originCard = DeckBuildManager.Inst.FindInCardListByName(hitcard.item.name);
                        if (originCard == null)
                        {
                            DeckBuildManager.Inst.isLoading = false;
                            DeckBuildManager.Inst.draggingCardUI = null;
                            return;
                        }
                        DeckBuildManager.Inst.draggingCardUI = Instantiate(cardUIPrefab, hitcard.transform.position, Utils.QI);
                        DeckBuildManager.Inst.draggingCardUI.transform.SetParent(DeckBuildManager.Inst.backgroundPanel.transform);
                        DeckBuildManager.Inst.draggingCardUI.GetComponent<RectTransform>().sizeDelta = this.GetComponent<RectTransform>().sizeDelta;
                        var draggingCard = DeckBuildManager.Inst.draggingCardUI.GetComponent<CardUI_DeckBuild>();
                        draggingCard.Setup(tempItem);
                        DeckBuildManager.Inst.draggingCardUI.transform.DOMove(originCard.transform.position, 0.5f)
                        .OnComplete(() => {
                            Destroy(DeckBuildManager.Inst.draggingCardUI);
                            DeckBuildManager.Inst.isLoading = false;
                            DeckBuildManager.Inst.draggingCardUI = null;
                        });
                        return;
                    }
                    else
                    {
                        hitcard.Setup(DeckBuildManager.Inst.draggingCardUI.GetComponent<CardUI_DeckBuild>().item);
                        hitflag--;
                    }
                }
            }
            if(hitflag == 0)
            {
                DeckBuildManager.Inst.isLoading = false;
                Destroy(DeckBuildManager.Inst.draggingCardUI);
                DeckBuildManager.Inst.draggingCardUI = null;
            }
            else
            {
                var originCard = DeckBuildManager.Inst.FindInCardListByName(DeckBuildManager.Inst.draggingCardUI.GetComponent<CardUI_DeckBuild>().item.name);
                if (originCard == null)
                {
                    Destroy(DeckBuildManager.Inst.draggingCardUI);
                    DeckBuildManager.Inst.isLoading = false;
                    DeckBuildManager.Inst.draggingCardUI = null;
                    return;
                }
                DeckBuildManager.Inst.draggingCardUI.transform.DOMove(originCard.transform.position, 0.5f)
                .OnComplete(() => {
                    originCard.item.num++;
                    Destroy(DeckBuildManager.Inst.draggingCardUI);
                    DeckBuildManager.Inst.isLoading = false;
                    DeckBuildManager.Inst.draggingCardUI = null;
                });
            }
        }
    }
}
