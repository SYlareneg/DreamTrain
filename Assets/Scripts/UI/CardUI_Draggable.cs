using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class CardUI_Draggable : CardUI_DeckBuild
{
    [SerializeField] public TMP_Text availableNumTMP;

    public override void OnBeginDrag(PointerEventData data)
    {
        if(DeckBuildManager.Inst.isLoading == false && availableNum > 0)
        {
            DeckBuildManager.Inst.isLoading = true;
            DeckBuildManager.Inst.draggingCardUI = Instantiate(cardUIPrefab, this.transform.position, Utils.QI);
            DeckBuildManager.Inst.draggingCardUI.transform.SetParent(DeckBuildManager.Inst.backgroundPanel.transform);
            DeckBuildManager.Inst.draggingCardUI.GetComponent<RectTransform>().sizeDelta = this.GetComponent<RectTransform>().sizeDelta;
            var draggingCard = DeckBuildManager.Inst.draggingCardUI.GetComponent<CardUI_DeckBuild>();
            draggingCard.Setup(this.item);
            draggingCard.availableNum = 0;
            if(availableNum > 0)
            {
                availableNum--;
            }
        }
    }

    public override void OnEndDrag(PointerEventData data)
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
                    if(hitcard.item == this.item)
                    {
                        hitcard.availableNum++;
                        hitflag--;
                    }
                    else if(hitcard.item != null)
                    {
                        Destroy(DeckBuildManager.Inst.draggingCardUI);
                        var originCard = DeckBuildManager.Inst.FindInCardListByName(hitcard.item.name);
                        if (originCard == null)
                        {
                            DeckBuildManager.Inst.changeCardNum(hitcard.item, hitcard.availableNum);
                            hitcard.Setup(this.item);
                            hitcard.availableNum = 1;
                            DeckBuildManager.Inst.isLoading = false;
                            DeckBuildManager.Inst.draggingCardUI = null;
                            return;
                        }
                        DeckBuildManager.Inst.draggingCardUI = Instantiate(cardUIPrefab, hitcard.transform.position, Utils.QI);
                        DeckBuildManager.Inst.draggingCardUI.transform.SetParent(DeckBuildManager.Inst.backgroundPanel.transform);
                        DeckBuildManager.Inst.draggingCardUI.GetComponent<RectTransform>().sizeDelta = this.GetComponent<RectTransform>().sizeDelta;
                        var draggingCard = DeckBuildManager.Inst.draggingCardUI.GetComponent<CardUI_DeckBuild>();
                        draggingCard.Setup(hitcard.item);
                        draggingCard.availableNum = 0;
                        hitcard.Setup(this.item);
                        DeckBuildManager.Inst.draggingCardUI.transform.DOMove(originCard.transform.position, 0.5f)
                        .OnComplete(() => {
                            originCard.availableNum += hitcard.availableNum;
                            hitcard.availableNum = 1;
                            DeckBuildManager.Inst.isLoading = false;
                            Destroy(DeckBuildManager.Inst.draggingCardUI);
                            DeckBuildManager.Inst.draggingCardUI = null;
                        });
                        return;
                    }
                    else
                    {
                        hitcard.Setup(DeckBuildManager.Inst.draggingCardUI.GetComponent<CardUI_DeckBuild>().item);
                        hitcard.availableNum = 1;
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
                DeckBuildManager.Inst.draggingCardUI.transform.DOMove(transform.position, 0.5f)
                .OnComplete(() => {
                    availableNum++;
                    DeckBuildManager.Inst.isLoading = false;
                    Destroy(DeckBuildManager.Inst.draggingCardUI);
                    DeckBuildManager.Inst.draggingCardUI = null;
                });
            }
        }
    }

    private void Update()
    {
        if(availableNum == 0)
        {
            this.SetAlpha(0.4f);
        }
        else
        {
            this.SetAlpha(1.0f);
        }
        if(availableNum == 0)
        {
            availableNumTMP.text = "";
        }
        else
        {
            availableNumTMP.text = "x" + availableNum.ToString();
        }
    }
}
