using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class CardUI_Draggable : CardUI, IBeginDragHandler, IDragHandler, IEndDragHandler
{   public GameObject cardUIPrefab;
    public GraphicRaycaster raycaster;
    [SerializeField] public TMP_Text availableNumTMP;

    public void OnBeginDrag(PointerEventData data)
    {
        if(DeckBuildManager.Inst.isLoading == false && item.num > 0)
        {
            DeckBuildManager.Inst.isLoading = true;
            DeckBuildManager.Inst.draggingCardUI = Instantiate(cardUIPrefab, this.transform.position, Utils.QI);
            DeckBuildManager.Inst.draggingCardUI.transform.SetParent(DeckBuildManager.Inst.backgroundPanel.transform);
            DeckBuildManager.Inst.draggingCardUI.GetComponent<RectTransform>().sizeDelta = this.GetComponent<RectTransform>().sizeDelta;
            var draggingCard = DeckBuildManager.Inst.draggingCardUI.GetComponent<CardUI_DeckBuild>();
            draggingCard.Setup(this.item);
            item.num--;
        }
    }

    public void OnDrag(PointerEventData data)
    {
        if(DeckBuildManager.Inst.draggingCardUI != null)
        {
            DeckBuildManager.Inst.draggingCardUI.transform.position = Input.mousePosition;
        }
    }
    
    public void OnEndDrag(PointerEventData data)
    {
        if (DeckBuildManager.Inst.draggingCardUI != null)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(data, results);
            int hitflag = 1;
            foreach (var result in results)
            {
                CardUI_DeckBuild hitcard = result.gameObject.GetComponent<CardUI_DeckBuild>();
                if (hitcard != null && result.gameObject != DeckBuildManager.Inst.draggingCardUI)
                {
                    if (hitcard.item != null)
                    {
                        Item tempItem = hitcard.item;
                        hitcard.item.num++;
                        hitcard.Setup(DeckBuildManager.Inst.draggingCardUI.GetComponent<CardUI_DeckBuild>().item);
                        Destroy(DeckBuildManager.Inst.draggingCardUI);
                        var originCard = DeckBuildManager.Inst.FindInCardListByName(tempItem.name);
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
                        .OnComplete(() =>
                        {
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
            if (hitflag == 0)
            {
                Destroy(DeckBuildManager.Inst.draggingCardUI);
                DeckBuildManager.Inst.isLoading = false;
                DeckBuildManager.Inst.draggingCardUI = null;
            }
            else
            {
                DeckBuildManager.Inst.draggingCardUI.transform.DOMove(transform.position, 0.5f)
                .OnComplete(() =>
                {
                    item.num++;
                    Destroy(DeckBuildManager.Inst.draggingCardUI);
                    DeckBuildManager.Inst.isLoading = false;
                    DeckBuildManager.Inst.draggingCardUI = null;
                });
            }
        }
    }

    private void Update()
    {
        if(item.num == 0)
        {
            this.SetAlpha(0.4f);
        }
        else
        {
            this.SetAlpha(1.0f);
        }
        if(item.num == 0)
        {
            availableNumTMP.text = "";
        }
        else
        {
            availableNumTMP.text = "x" + item.num.ToString();
        }
    }
}
