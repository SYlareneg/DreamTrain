using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class CardManager : MonoBehaviour
{
    public static CardManager Inst { get; private set; }
    void Awake() => Inst = this;

    public ItemSO itemSO;
    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject cardUIPrefab;
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

    bool isMyCardDrag;
    bool onMyCardArea;

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
        
        foreach(Item itemInDeck in itemSO.items)
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
        TurnManager.OnAddCard += AddCard;
        TurnManager.OnDiscardCard += DiscardCard;
    }

    private void OnDestroy()
    {
        TurnManager.OnAddCard -= AddCard;
        TurnManager.OnDiscardCard -= DiscardCard;
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

    void AddCard()
    {
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        var card = cardObject.GetComponent<Card>();
        card.Setup(PopItem());
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

    IEnumerator DiscardSingleCard(Card card)
    {
        itemDiscard.Add(card.item);
        myCards.Remove(card);
        card.MoveTransform(new PRS(cardDiscardPoint.position, Utils.QI, new Vector3(1, 1, 1)), true, 0.7f);

        SetOriginOrder();
        CardAlignment();
        yield return new WaitForSeconds(0.7f);
        Destroy(card.gameObject);
    }

    void DiscardCard()
    {
        int cnt = myCards.Count;
        for(int i = 0; i < cnt; i++)
        {
            StartCoroutine(DiscardSingleCard(myCards[0]));
        }
    }

    void SetOriginOrder()
    {
        int cnt = myCards.Count;
        for(int i = 0; i < cnt; i++)
        {
            var targetCard = myCards[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i);
        }
    }

    void CardAlignment()
    {
        List<PRS> originCardPRSs = new List<PRS>();
        originCardPRSs = RoundAlignment(myCardLeft, myCardRight, myCards.Count, 0.5f, new Vector3(6, 7.2f, 1));

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
        if(eCardState == ECardState.Nothing)
        {
            return;
        }
        if(!onMyCardArea)
        {
            return;
        }

        selectedCard = card;
        EnlargeCard(true, card);
    }

    public void CardMouseExit(Card card)
    {
        if(!onMyCardArea)
        {
            return;
        }

        EnlargeCard(false, card);
    }

    public void CardMouseDown(Card card)
    {
        if(eCardState != ECardState.CanMouseDrag)
        {
            return;
        }

        isMyCardDrag = true;
    }

    public void CardMouseUp(Card card)
    {
        isMyCardDrag = false;

        if(eCardState != ECardState.CanMouseDrag)
        {
            return;
        }

        if(!onMyCardArea)
        {
            if(selectedCard.item.cost > TurnManager.Inst.nowCost)
            {
                EnlargeCard(false, selectedCard);
                selectedCard = null;
                return;
            }
            myCards.Remove(selectedCard);
            TurnManager.Inst.nowCost -= selectedCard.item.cost;
            selectedCard.UseCard(true);
            StartCoroutine(DiscardSingleCard(selectedCard));
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

    void EnlargeCard(bool isEnlarge, Card card)
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
        if(TurnManager.Inst.isLoading)
        {
            eCardState = ECardState.Nothing;
        }
        else
        {
            eCardState = ECardState.CanMouseDrag;
        }
    }
}
