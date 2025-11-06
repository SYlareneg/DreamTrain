using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;
using DG.Tweening;

public enum ECardSelectMode
{
    Hide, Duplicate, Discard
};
public class CardManager : MonoBehaviour
{
    public static CardManager Inst { get; private set; }
    void Awake() => Inst = this;

    public ItemSO itemSO;
    public ItemSO playerDeckSO;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject cardUIPrefab;
    [SerializeField] GameObject cardUISelectPrefab;
    public List<Card> myCards;
    [SerializeField] Transform cardSpawnPoint;
    [SerializeField] Transform cardDiscardPoint;
    [SerializeField] Transform myCardLeft;
    [SerializeField] Transform myCardRight;
    [SerializeField] ECardState eCardState;

    public List<Item> itemDeck;
    public List<Item> itemDraw;
    public List<Item> itemDiscard;
    public Card selectedCard;

    public bool isMyCardDrag;
    bool onMyCardArea;
    public ECardSelectMode cardSelectMode;
    public int cardSelectNum;
    public List<GameObject> selectedCardList;
    public List<Card> discardCardList;
    [SerializeField] GameObject cardSelectScreen;
    [SerializeField] GameObject selectedCards;
    [SerializeField] TMP_Text selectModeText;
    [SerializeField] Button cardSelectButton;

    public int useCount;
    public int useCount_Turn;
    
    [SerializeField] Canvas uiCanvas;
    [SerializeField] GameObject tooltipPrefab;
    private Camera mainCam;
    private GameObject tooltip;
    private RectTransform canvasRect;
    public KeywordSO keywordSO;

    enum ECardState { Nothing, CanMouseOver, CanMouseDrag }

    public List<CardUI> ItemBufferToCardUIList(List<Item> items)
    {
        List<CardUI> cardList = new List<CardUI>();
        List<Item> sortedItemList = items.OrderBy(x => x.name).ToList();
        Vector3 standardListPosition = GameManager.Inst.cardListScroll.transform.position;

        foreach(Item item in sortedItemList)
        {
            var cardObject = Instantiate(cardUIPrefab, standardListPosition, Utils.QI);
            cardObject.transform.SetParent(GameManager.Inst.cardListScroll.transform);
            var card = cardObject.GetComponent<CardUI>();

            card.Setup(item);
            cardList.Add(card);
        }

        return cardList;
    }

    public Item PopItem()
    {
        if(itemDraw.Count == 0)
        {
            while(itemDiscard.Count > 0)
            {
                itemDraw.Add(itemDiscard[0]);
                itemDiscard.RemoveAt(0);
            }

            ShuffleDeck();
        }

        if(itemDraw.Count == 0)
        {
            return null;
        }
        Item item = itemDraw[0];
        itemDraw.RemoveAt(0);
        return item;
    }

    public void InitializeItemBuffer()
    {
        itemDeck = new List<Item>();
        itemDraw = new List<Item>();
        itemDiscard = new List<Item>();
        
        foreach(Item itemInDeck in playerDeckSO.items)
        {
            for(int j = 0; j < itemInDeck.num; j++)
            {
                itemDeck.Add(itemInDeck);
                itemDraw.Add(itemInDeck);
            }
        }
    }

    public void ShuffleDeck()
    {
        for(int i = 0; i < itemDraw.Count; i++)
        {
            int rand = Random.Range(i, itemDraw.Count);
            Item temp = itemDraw[i];
            itemDraw[i] = itemDraw[rand];
            itemDraw[rand] = temp; 
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        mainCam = Camera.main;
        canvasRect = uiCanvas.GetComponent<RectTransform>();

        TurnManager.OnAddCard += AddCard;
        TurnManager.OnDiscardCard += DiscardCard;
        TurnManager.OnPlayerTurnStart += () => { useCount_Turn = 0; };
    }

    private void OnDestroy()
    {
        TurnManager.OnAddCard = null;
        TurnManager.OnDiscardCard = null;
        TurnManager.OnPlayerTurnStart = null;
    }

    // Update is called once per frame
    private void Update()
    {
        if(isMyCardDrag)
        {
            CardDrag();
        }

        DetectCardArea();
        SetECardState();
    }

    public void CreateCardInHand(Item item)
    {
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        var card = cardObject.GetComponent<Card>();
        card.Setup(item);
        if(card.item != null)
        {
            myCards.Add(card);
        }
        else
        {
            Destroy(card.gameObject);
        }

        SetOriginOrder();
        CardAlignment();

        if(TurnManager.Inst.maxCardCount < myCards.Count)
        {
            StartCoroutine(DiscardSingleCard(card));
        }
    }

    void AddCard()
    {
        CreateCardInHand(PopItem());
    }

    IEnumerator DiscardSingleCard(Card card)
    {
        if (card.item.isRemain == false)
        {
            myCards.Remove(card);
        }
        
        if (card.item.isVolatile == false && card.item.isRemain == false)
        {
            itemDiscard.Add(card.item);
            card.MoveTransform(new PRS(cardDiscardPoint.position, Utils.QI, new Vector3(1, 1, 1)), true, 0.7f);
        }

        SetOriginOrder();
        CardAlignment();
        yield return new WaitForSeconds(0.7f);
        if (card.item.isRemain == false)
        {
            Destroy(card.gameObject);
        }
    }

    void DiscardCard()
    {
        int cnt = myCards.Count;
        for (int i = 0; i < cnt; i++)
        {
            StartCoroutine(DiscardSingleCard(myCards[0]));
        }
    }

    public void CardSelectModeTransit(ECardSelectMode mode, int selectNum)
    {
        cardSelectMode = mode;
        cardSelectNum = selectNum;
        cardSelectScreen.SetActive(mode != ECardSelectMode.Hide);
        TurnManager.Inst.isLoading = mode != ECardSelectMode.Hide;
        cardSelectButton.interactable = false;

        if (mode == ECardSelectMode.Duplicate)
        {
            selectModeText.text = "복제할 카드를 " + selectNum.ToString() + "장 선택하십시오.";
        }
        else if (mode == ECardSelectMode.Discard)
        {
            selectModeText.text = "버릴 카드를" + selectNum.ToString() + "장 선택하십시오.";
        }
        
        if (selectNum >= myCards.Count)
        {
            foreach (Card card in myCards)
            {
                SelectCard(card);
            }
            SelectCardDone();
        }
    }

    public void SelectCard(Card card)
    {
        if (cardSelectNum < 0) return;
        if (cardSelectNum == 0)
        {
            Destroy(selectedCardList[0]);
            selectedCardList.RemoveAt(0);
            if (cardSelectMode == ECardSelectMode.Discard)
            {
                discardCardList[0].gameObject.SetActive(true);
                EnlargeCard(false, discardCardList[0]);
                discardCardList.RemoveAt(0);
            }
            cardSelectNum++;
        }
        GameObject selectedCardUI = Instantiate(cardUISelectPrefab, selectedCards.transform.position, Utils.QI);
        selectedCardUI.transform.SetParent(selectedCards.transform, false);
        CardUI_Select cUI = selectedCardUI.GetComponent<CardUI_Select>();
        cUI.Setup(card.item);
        selectedCardList.Add(selectedCardUI);
        if(cardSelectMode == ECardSelectMode.Discard)
        {
            card.gameObject.SetActive(false);
            discardCardList.Add(card);
        }
        cardSelectNum--;
    }
    
    public void SelectCardDone()
    {
        if (cardSelectMode == ECardSelectMode.Duplicate)
        {
            foreach (var cardObj in selectedCardList)
            {
                CreateCardInHand(cardObj.GetComponent<CardUI_Select>().item);
                Destroy(cardObj);
            }
            selectedCardList.Clear();
            CardSelectModeTransit(ECardSelectMode.Hide, 0);
        }
        else if (cardSelectMode == ECardSelectMode.Discard)
        {
            for (int i = 0; i < selectedCardList.Count; i++)
            {
                discardCardList[i].MoveTransform(new PRS(selectedCardList[i].transform.position, Utils.QI, new Vector3(1, 1, 1)), false, 0f);
                discardCardList[i].gameObject.SetActive(true);
                StartCoroutine(DiscardSingleCard(discardCardList[i]));
                Destroy(selectedCardList[i]);
            }
            selectedCardList.Clear();
            discardCardList.Clear();
            SetOriginOrder();
            CardAlignment();
            CardSelectModeTransit(ECardSelectMode.Hide, 0);
        }
    }

    void SetOriginOrder()
    {
        int cnt = myCards.Count;
        for(int i = 0; i < cnt; i++)
        {
            var targetCard = myCards[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i+2);
        }
    }

    void CardAlignment()
    {
        List<PRS> originCardPRSs = new List<PRS>();
        originCardPRSs = RoundAlignment(myCardLeft, myCardRight, myCards.Count, 0.5f, cardPrefab.transform.localScale);

        var targetCards = myCards;
        for(int i = 0; i < targetCards.Count; i++)
        {
            var targetCard = targetCards[i];

            targetCard.originPRS = originCardPRSs[i];
            targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
        }
    }

    List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int objCount, float height, Vector3 scale)
    {
        float[] objLerps = new float[objCount];
        List<PRS> results = new List<PRS>(objCount);
        float interval;

        switch (objCount)
        {
            case 1: objLerps = new float[] { 0.5f }; break;
            case 2:
            case 3:
            case 4:
            case 5:
                interval = 0.6f / (objCount - 1);
                for (int i = 0; i < objCount; i++)
                {
                    objLerps[i] = 0.2f + interval * i;
                }
                break;
            default:
                interval = 1f / (objCount - 1);
                for(int i = 0; i < objCount; i++)
                {
                    objLerps[i] = interval * i;
                }
                break;
        }

        for(int i = 0; i < objCount; i++)
        {
            var targetPos = Vector3.Lerp(leftTr.position, rightTr.position, objLerps[i]);
            var targetRot = Utils.QI;
            float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
            curve = height >= 0 ? curve : -curve;
            targetPos.y += curve;
            targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
            results.Add(new PRS(targetPos, targetRot, scale));
        }

        return results;
    }

    public void CardMouseOver(Card card)
    {
        if (!onMyCardArea)
        {
            return;
        }
        if (isMyCardDrag)
        {
            return;
        }

        selectedCard = card;
        EnlargeCard(true, card);

        int wordIndex = TMP_TextUtilities.FindIntersectingWord(card.textTMP, Input.mousePosition, mainCam);

        if (wordIndex != -1)
        {
            TMP_WordInfo wordInfo = card.textTMP.textInfo.wordInfo[wordIndex];
            string hoveredWord = wordInfo.GetWord();
            Keyword keyword = Array.Find(keywordSO.keywords, x => x.word == hoveredWord);
            if (keyword != null)
            {
                if (tooltip == null)
                {
                    tooltip = Instantiate(tooltipPrefab, uiCanvas.transform);
                }

                Vector2 localPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    Input.mousePosition,
                    uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : uiCanvas.worldCamera,
                    out localPos);

                RectTransform tooltipRect = tooltip.GetComponent<RectTransform>();
                tooltipRect.anchoredPosition = localPos + new Vector2(20f, 20f);

                TMP_Text[] tooltipTMP = tooltip.GetComponentsInChildren<TMP_Text>();
                tooltipTMP[0].text = hoveredWord;
                tooltipTMP[1].text = keyword.explanation;
                return;
            }
        }
        
        if (tooltip != null)
        {
            Destroy(tooltip);
        }
    }

    public void CardMouseExit(Card card)
    {
        if (isMyCardDrag)
        {
            return;
        }

        EnlargeCard(false, card);
        
        if (tooltip != null)
        {
            Destroy(tooltip);
        }
    }

    public void CardMouseDown(Card card)
    {
        if (cardSelectMode != ECardSelectMode.Hide)
        {
            SelectCard(card);
            return;
        }
        
        if (eCardState != ECardState.CanMouseDrag)
        {
            return;
        }

        isMyCardDrag = true;
    }

    public void CardMouseUp(Card card)
    {
        isMyCardDrag = false;

        if (eCardState != ECardState.CanMouseDrag)
        {
            return;
        }
        if(selectedCard == null)
        {
            return;
        }

        if (!onMyCardArea)
        {
            int buffedCost = BuffManager.Inst.GetBuffedCardCost(selectedCard.item);
            if (buffedCost > TurnManager.Inst.nowCost)
            {
                EnlargeCard(false, selectedCard);
                selectedCard = null;
                return;
            }
            if (selectedCard.UseCard(true) == false)
            {
                EnlargeCard(false, selectedCard);
                selectedCard = null;
                return;
            }
            myCards.Remove(selectedCard);
            TurnManager.Inst.IncreaseCost(-buffedCost);
            useCount++;
            useCount_Turn++;
            TurnManager.OnUseCard?.Invoke();
            if (selectedCard.item.isVanish == true)
            {
                myCards.Remove(card);
                Destroy(card.gameObject);
            }
            else
            {
                StartCoroutine(DiscardSingleCard(selectedCard));
            }
            isMyCardDrag = false;
            selectedCard = null;

            SetOriginOrder();
            CardAlignment();
        }
    }

    void CardDrag()
    {
        if(selectedCard != null)
        {
            selectedCard.MoveTransform(new PRS(Utils.MousePos, Utils.QI, selectedCard.originPRS.scale), false);
        }
    }

    void DetectCardArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        int layer = LayerMask.NameToLayer("MyCardArea");
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    }

    public void EnlargeCard(bool isEnlarge, Card card)
    {
        if(isEnlarge)
        {
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, -7.6f, -10f);
            card.MoveTransform(new PRS(enlargePos, Utils.QI, new Vector3(8, 9.6f, 1)), false);
        }
        else
        {
            card.MoveTransform(card.originPRS, false);
        }

        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
    }

    void SetECardState()
    {
        if (!TurnManager.Inst)
        {
            return;
        }
        if (TurnManager.Inst.isLoading)
        {
            eCardState = ECardState.Nothing;
        }
        else
        {
            eCardState = ECardState.CanMouseDrag;
        }
    }
}
