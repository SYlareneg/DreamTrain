using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;
using DG.Tweening;

// 현재 카드 선택 화면 모드 (숨김, 카드 복제, 카드 버리기)
public enum ECardSelectMode
{
    Hide, Duplicate, Discard
};
public class CardManager : MonoBehaviour
{
    // 카드 매니저 선언. scene에는 카드 매니저가 유일하게 존재하며, Inst를 통해 접근.
    public static CardManager Inst { get; private set; }
    void Awake() => Inst = this;

    [Header("카드 매니저")]
    [Tooltip("플레이어 덱 정보")] public CharacterSO characterSO;
    [Tooltip("카드 프리팹")][SerializeField] static GameObject cardPrefab;
    [Tooltip("카드 UI 프리팹")][SerializeField] static GameObject cardUIPrefab;
    [Tooltip("현재 카드 매니저 상태(카드를 드래그 할 수 있는지)")][ReadOnly, SerializeField] ECardState eCardState;
    bool isMyCardDrag; // 카드 드래그 여부
    bool onMyCardArea; // 현재 드래그 중인 카드가 나의 핸드 범위에 위치하는지 확인. false일 경우 카드 사용, true일 경우 카드 핸드로 복귀

    // 카드 매니저 상태 (카드 상호작용 불가, 카드 마우스 호버 가능/사용 불가, 카드 사용 가능)
    enum ECardState { Nothing, CanMouseOver, CanMouseDrag }

    [Header("핸드, 덱, 드로우, 무덤")]
    [Tooltip("플레이어 핸드")] public List<Card> myCards;
    [Tooltip("핸드 맨 왼쪽 카드 위치")][SerializeField] Transform myCardLeft;
    [Tooltip("핸드 맨 오른쪽 카드 위치")][SerializeField] Transform myCardRight;
    [Tooltip("덱에 있는 카드 목록")] public List<Item> itemDeck;
    [Tooltip("뽑을 카드 더미")] public List<Item> itemDraw;
    [Tooltip("버린 카드 더미")] public List<Item> itemDiscard;
    [Tooltip("카드 뽑는 위치")][SerializeField] Transform cardSpawnPoint;
    [Tooltip("카드 버리는 위치")][SerializeField] Transform cardDiscardPoint;
    [Tooltip("현재 선택 중인 카드(마우스와 닿아 있는 카드)")] public Card selectedCard;

    [Header("카드 선택")]
    [Tooltip("카드 선택 화면")][SerializeField] GameObject cardSelectScreen;
    [Tooltip("카드 선택 화면(버리기, 복제) UI 프리팹")][SerializeField] GameObject cardUISelectPrefab;
    public ECardSelectMode cardSelectMode; // 현재 카드 선택 화면 모드 (숨김, 카드 복제, 카드 버리기)
    public int cardSelectNum; // 카드 선택 화면에서 선택해야 하는 카드 개수
    public List<GameObject> selectedCardList; // 카드 선택 화면에서 선택한 카드 목록
    public List<Card> discardCardList; // 카드 선택 화면 모드가 '카드 버리기'일 시, 버릴 카드 목록
    [Tooltip("선택된 카드 배치 레이아웃")][SerializeField] GameObject selectedCards;
    [Tooltip("카드 선택 화면 모드 텍스트")][SerializeField] TMP_Text selectModeText;
    [Tooltip("카드 선택 버튼")][SerializeField] Button cardSelectButton;

    // 이드 전달 정보
    public int useCount; // 총 사용한 카드 개수
    public int useCount_Turn; // 이번 턴 사용한 카드 개수
    
    [Header("툴팁")]
    [Tooltip("툴팁 배치 캔버스")][SerializeField] Canvas uiCanvas;
    [Tooltip("툴팁 프리팹")][SerializeField] GameObject tooltipPrefab;
    [Tooltip("툴팁 키워드 목록")]public KeywordSO keywordSO;
    private Camera mainCam; // 메인 카메라
    private GameObject tooltip; // 툴팁
    private RectTransform canvasRect; // 툴팁 배치 캔버스 위치

    // items로 주어진 카드 item들에 대해 attachTransform에 CardUI 오브젝트를 생성하고, 생성한 CardUI 리스트를 반환한다.
    // GameManager에서 덱, 드로우, 무덤의 카드 목록을 UI로 제시하기 위해 사용
    public static List<CardUI> ItemBufferToCardUIList(List<Item> items, Transform attachTransform)
    {
        List<CardUI> cardList = new List<CardUI>(); // 반환할 CardUI 리스트
        List<Item> sortedItemList = items.OrderBy(x => x.name).ToList(); // 이름 순 정렬

        // CardUI 오브젝트 생성
        foreach(Item item in sortedItemList)
        {
            var cardObject = Instantiate(cardUIPrefab, attachTransform.position, Utils.QI);
            cardObject.transform.SetParent(attachTransform);
            var card = cardObject.GetComponent<CardUI>();

            card.Setup(item);
            cardList.Add(card);
        }

        // CardUI 리스트 반환
        return cardList;
    }

    // 드로우 카드 목록에서 맨 앞의 item을 뽑는다. 만약 드로우 카드 목록이 비어 있을 경우 무덤의 카드를 드로우 카드 목록에 넣고 섞는다.
    // 카드를 뽑을 때 사용
    private Item PopItem()
    {
        // 드로우 카드 목록이 비어 있을 경우
        if(itemDraw.Count == 0)
        {
            // 무덤의 모든 카드를 드로우 카드 목록에 넣는다.
            while(itemDiscard.Count > 0)
            {
                itemDraw.Add(itemDiscard[0]);
                itemDiscard.RemoveAt(0);
            }
            // 드로우 카드 목록을 섞는다.
            ShuffleDeck();
        }
        // 여전히 드로우 카드 목록이 비어 있다면 null 반환 (덱에 카드가 없음)
        if(itemDraw.Count == 0)
        {
            return null;
        }
        // 드로우 카드 목록 맨 앞의 카드를 목록에서 제거하고 반환한다.
        Item item = itemDraw[0];
        itemDraw.RemoveAt(0);
        return item;
    }

    // characterSO로 주어진 플레이어 덱 정보에 따라 덱, 드로우, 무덤을 초기화한다.
    // TurnManager에서 게임을 시작할 때 호출된다.
    public void InitializeItemBuffer()
    {
        itemDeck = new List<Item>();
        itemDraw = new List<Item>();
        itemDiscard = new List<Item>();
        // 일반 카드 덱, 드로우 카드 목록에 추가
        foreach(Item itemInDeck in characterSO.normalCards)
        {
            for(int j = 0; j < itemInDeck.num; j++)
            {
                itemDeck.Add(itemInDeck);
                itemDraw.Add(itemInDeck);
            }
        }
        // 페르소나 카드 덱, 드로우 카드 목록에 추가
        foreach(Item itemInDeck in characterSO.personaPiece.cards)
        {
            for(int j = 0; j < itemInDeck.num; j++)
            {
                itemDeck.Add(itemInDeck);
                itemDraw.Add(itemInDeck);
            }
        }
        // 그림자 카드 덱, 드로우 카드 목록에 추가
        foreach(Item itemInDeck in characterSO.shadowPiece.cards)
        {
            for(int j = 0; j < itemInDeck.num; j++)
            {
                itemDeck.Add(itemInDeck);
                itemDraw.Add(itemInDeck);
            }
        }
    }

    // 드로우 카드 목록(itemDraw)을 섞는 함수
    public void ShuffleDeck()
    {
        // 맨 앞에서부터 랜덤한 카드를 하나 선정하여 배치한다.
        for(int i = 0; i < itemDraw.Count; i++)
        {
            int rand = Random.Range(i, itemDraw.Count);
            Item temp = itemDraw[i];
            itemDraw[i] = itemDraw[rand];
            itemDraw[rand] = temp; 
        }
    }

    // Start
    // 메인 카메라, 툴팁 캔버스 설정
    // 카드 드로우, 카드 버리기 함수 설정
    // 플레이어 턴 시작 시 '이번 턴 사용한 카드 개수(useCount_Turn)' 초기화
    private void Start()
    {
        mainCam = Camera.main;
        canvasRect = uiCanvas.GetComponent<RectTransform>();

        TurnManager.OnAddCard += AddCard;
        TurnManager.OnDiscardCard += DiscardCard;
        TurnManager.OnPlayerTurnStart += () => { useCount_Turn = 0; };
    }

    // OnDestroy
    // 변화시킨 action 초기화 (오류 방지)
    private void OnDestroy()
    {
        TurnManager.OnAddCard = null;
        TurnManager.OnDiscardCard = null;
        TurnManager.OnPlayerTurnStart = null;
    }

    // Update
    // 카드 드래그 중일 경우 선택한 카드가 마우스 커서 따라가도록 함
    // onMyCardArea, eCardState 갱신
    private void Update()
    {
        if(isMyCardDrag)
        {
            CardDrag();
        }

        DetectCardArea();
        SetECardState();
    }

    // 핸드에 item을 카드 아이템으로 갖는 카드 생성
    public void CreateCardInHand(Item item)
    {
        // 카드 생성
        var cardObject = Instantiate(cardPrefab, cardSpawnPoint.position, Utils.QI);
        var card = cardObject.GetComponent<Card>();
        card.Setup(item); // 카드 아이템 item으로 설정
        if(card.item != null)
        {
            // 카드 아이템이 정상적으로 설정되었을 경우 핸드에 카드 추가
            myCards.Add(card);
        }
        else
        {
            // 설정한 카드 아이템이 잘못되었을 경우 카드 소멸
            Destroy(card.gameObject);
        }
    
        SetOriginOrder(); // 핸드 내 카드 order 정렬 (오른쪽 카드가 더 위에 오도록)
        CardAlignment(); // 핸드 내 카드 위치 정렬 (myCardLeft, myCardRight 기준)

        // 만약 핸드의 카드 개수가 최대 핸드 카드 개수를 초과했을 경우, 새로 생성한 카드를 버린다.
        if(TurnManager.Inst.maxCardCount < myCards.Count)
        {
            StartCoroutine(DiscardSingleCard(card));
        }
    }

    // 카드 드로우
    // 드로우 카드 목록의 맨 앞 원소를 카드 아이템으로 갖는 카드 생성
    void AddCard()
    {
        CreateCardInHand(PopItem());
    }

    // 핸드에서 card 카드 버림
    IEnumerator DiscardSingleCard(Card card)
    {
        // 잔류 카드가 아닐 경우 핸드에서 제거
        bool isRemain = card.item.isRemain;
        if (isRemain == false)
        {
            myCards.Remove(card);
        }
        
        // 잔류 카드가 아니고, 휘발성 카드가 아닐 경우 무덤에 카드 추가 (소멸 카드의 경우 사용 즉시 소멸됨)
        if (card.item.isVolatile == false && isRemain == false)
        {
            itemDiscard.Add(card.item);
            card.MoveTransform(new PRS(cardDiscardPoint.position, Utils.QI, new Vector3(1, 1, 1)), true, 0.7f);
        }

        // 핸드 재정렬
        SetOriginOrder();
        CardAlignment();
        yield return new WaitForSeconds(0.7f);
        // 카드 이동 완료 후 카드 오브젝트 파괴
        if (isRemain == false)
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
            if (selectedCard.UseCard(true) == false)
            {
                EnlargeCard(false, selectedCard);
                selectedCard = null;
                return;
            }
            useCount++;
            useCount_Turn++;
            bool flag = selectedCard.item.isRemain;
            selectedCard.item.isRemain = false;
            if (selectedCard.item.isVanish == true)
            {
                myCards.Remove(card);
                Destroy(card.gameObject);
            }
            else
            {
                StartCoroutine(DiscardSingleCard(selectedCard));
            }
            selectedCard.item.isRemain = flag;
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
