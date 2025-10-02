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
    public GameObject originObject;
    [SerializeField] public TMP_Text availableNumTMP;
    public int availableNum;

    public virtual void OnBeginDrag(PointerEventData data)
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
                if(availableNum == 0)
                {
                    SetBlank();
                }
            }
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
                    if(hitcard.item == this.item)
                    {
                        hitcard.availableNum++;
                        hitflag--;
                    }
                    else if(hitcard.item != null)
                    {
                        Destroy(DeckBuildManager.Inst.draggingCardUI);
                        if(hitcard.originObject == null)
                        {
                            DeckBuildManager.Inst.changeCardNum(hitcard.item, hitcard.availableNum);
                            hitcard.Setup(this.item);
                            hitcard.availableNum = 1;
                            hitcard.originObject = originObject;
                            DeckBuildManager.Inst.isLoading = false;
                            if(availableNum == 0)
                            {
                                this.item = null;
                                this.originObject = null;
                            }
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
                        DeckBuildManager.Inst.draggingCardUI.transform.DOMove(hitcard.originObject.transform.position, 0.5f)
                        .OnComplete(() => {
                            CardUI_Draggable originCardUI = hitcard.originObject.GetComponent<CardUI_Draggable>();
                            originCardUI.availableNum += hitcard.availableNum;
                            originCardUI.SetAlpha(1.0f);
                            hitcard.availableNum = 1;
                            hitcard.originObject = originObject;
                            DeckBuildManager.Inst.isLoading = false;
                            if(availableNum == 0)
                            {
                                this.item = null;
                                this.originObject = null;
                            }
                            Destroy(DeckBuildManager.Inst.draggingCardUI);
                            DeckBuildManager.Inst.draggingCardUI = null;
                        });
                        return;
                    }
                    else
                    {
                        hitcard.Setup(DeckBuildManager.Inst.draggingCardUI.GetComponent<CardUI_DeckBuild>().item);
                        hitcard.availableNum = 1;
                        hitcard.originObject = originObject;
                        hitflag--;
                    }
                }
            }
            if(hitflag == 0)
            {
                DeckBuildManager.Inst.isLoading = false;
                if(availableNum == 0)
                {
                    this.item = null;
                    this.originObject = null;
                }
                Destroy(DeckBuildManager.Inst.draggingCardUI);
                DeckBuildManager.Inst.draggingCardUI = null;
            }
            else
            {
                if(originObject == null)
                {
                    DeckBuildManager.Inst.changeCardNum(this.item, 1);
                    DeckBuildManager.Inst.isLoading = false;
                    if(availableNum == 0)
                    {
                        this.item = null;
                        this.originObject = null;
                    }
                    Destroy(DeckBuildManager.Inst.draggingCardUI);
                    DeckBuildManager.Inst.draggingCardUI = null;
                    return;
                }
                DeckBuildManager.Inst.draggingCardUI.transform.DOMove(originObject.transform.position, 0.5f)
                .OnComplete(() => {
                    CardUI_Draggable originCardUI = originObject.GetComponent<CardUI_Draggable>();
                    originCardUI.availableNum++;
                    originCardUI.SetAlpha(1.0f);
                    DeckBuildManager.Inst.isLoading = false;
                    if(availableNum == 0)
                    {
                        this.item = null;
                        this.originObject = null;
                    }
                    Destroy(DeckBuildManager.Inst.draggingCardUI);
                    DeckBuildManager.Inst.draggingCardUI = null;
                });
            }
        }
    }

    private void Update()
    {
        if(availableNum <= 0)
        {
            availableNumTMP.text = "";
        }
        else
        {
            availableNumTMP.text = "x" + availableNum.ToString();
        }
    }
}
