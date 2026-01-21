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
    [Tooltip("카드 프리팹")][SerializeField] GameObject cardPrefab;
    [Tooltip("카드 UI 프리팹")][SerializeField] GameObject cardUIPrefab;
    [Tooltip("카드 툴팁 프리팹")][SerializeField] GameObject cardTooltipPrefab;
    [Tooltip("현재 카드 매니저 상태(카드를 드래그 할 수 있는지)")][ReadOnly, SerializeField] ECardState eCardState;
    [Tooltip("카드 드래그 여부")][ReadOnly] public bool isMyCardDrag;
    [Tooltip("현재 드래그 중인 카드가 나의 핸드 범위에 위치하는지 확인")][ReadOnly, SerializeField] bool onMyCardArea; // false일 경우 카드 사용, true일 경우 카드 핸드로 복귀
    [Tooltip("현재 드래그 중인 카드가 적의 카드 적용 범위에 위치하는지 확인")][ReadOnly, SerializeField] int onEnemyCardArea; // 0 이상일 경우 카드 사용, -1일 경우 카드 핸드로 복귀

    // 카드 매니저 상태 (카드 상호작용 불가, 카드 마우스 호버 가능/사용 불가, 카드 사용 가능)
    enum ECardState { Nothing, CanMouseOver, CanMouseDrag }

    [Header("핸드, 덱, 드로우, 무덤")]
    [Tooltip("플레이어 핸드")][ReadOnly, SerializeField] public List<Card> myCards;
    [Tooltip("핸드 맨 왼쪽 카드 위치")][SerializeField] Transform myCardLeft;
    [Tooltip("핸드 맨 오른쪽 카드 위치")][SerializeField] Transform myCardRight;
    [Tooltip("덱에 있는 카드 목록")] public List<Item> itemDeck;
    [Tooltip("뽑을 카드 더미")] public List<Item> itemDraw;
    [Tooltip("버린 카드 더미")] public List<Item> itemDiscard;
    [Tooltip("카드 뽑는 위치")][SerializeField] Transform cardSpawnPoint;
    [Tooltip("카드 버리는 위치")][SerializeField] Transform cardDiscardPoint;
    [Tooltip("현재 선택 중인 카드(마우스와 닿아 있는 카드)")][ReadOnly, SerializeField] Card selectedCard;

    [Header("카드 선택")]
    [Tooltip("카드 선택 화면")][SerializeField] GameObject cardSelectScreen;
    [Tooltip("카드 선택 화면(버리기, 복제) UI 프리팹")][SerializeField] GameObject cardUISelectPrefab;
    [Tooltip("현재 카드 선택 화면 모드 (숨김, 카드 복제, 카드 버리기)")][ReadOnly, SerializeField] ECardSelectMode cardSelectMode;
    [Tooltip("선택해야 하는 카드 개수")][ReadOnly, SerializeField] int cardSelectNum;
    [Tooltip("선택한 카드 목록")][ReadOnly, SerializeField] List<GameObject> selectedCardList;
    [Tooltip("모드가 '카드 버리기'일 시, 버릴 카드 목록")][ReadOnly, SerializeField] List<Card> discardCardList;
    [Tooltip("선택된 카드 배치 레이아웃")][SerializeField] GameObject selectedCards;
    [Tooltip("카드 선택 화면 모드 텍스트")][SerializeField] TMP_Text selectModeText;
    [Tooltip("카드 선택 버튼")][SerializeField] Button cardSelectButton;
    [Tooltip("보드 보기 버튼")][SerializeField] Button showBoardButton;
    [Header("적 선택")]
    [Tooltip("적 선택 화면")][SerializeField] GameObject enemySelectScreen;

    [Header("이드")]
    [Tooltip("총 사용한 카드 개수")] public int useCount; 
    [Tooltip("이번 턴 사용한 카드 개수")] public int useCount_Turn;
    
    [Header("툴팁")]
    [Tooltip("툴팁 배치 캔버스")][SerializeField] Canvas uiCanvas;
    [Tooltip("툴팁 프리팹")][SerializeField] GameObject tooltipPrefab;
    [Tooltip("툴팁 키워드 목록")][SerializeField] KeywordSO keywordSO;
    private bool tooltipCreated = false; // 툴팁 생성 여부
    private RectTransform canvasRect; // 툴팁 배치 캔버스 위치

    #region Item Management
    // items로 주어진 카드 item들에 대해 attachTransform에 CardUI 오브젝트를 생성하고, 생성한 CardUI 리스트를 반환한다.
    // GameManager에서 덱, 드로우, 무덤의 카드 목록을 UI로 제시하기 위해 사용
    public List<CardUI> ItemBufferToCardUIList(List<Item> items, Transform attachTransform)
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

    #endregion

    #region Card Managenent
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
            // 카드 버리는 모션
            card.gameObject.SetActive(false);
            //card.MoveTransform(new PRS(cardDiscardPoint.position, Utils.QI, new Vector3(1, 1, 1)), true, 0.7f);
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

    // 핸드의 모든 카드 버림
    void DiscardCard()
    {
        int cnt = myCards.Count;
        for (int i = 0; i < cnt; i++)
        {
            StartCoroutine(DiscardSingleCard(myCards[0]));
        }
    }
    
    // 카드 order 정렬 (오른쪽 카드가 가장 위에 오게끔 정렬)
    void SetOriginOrder()
    {
        // myCards는 핸드의 카드를 왼쪽에서 오른쪽 순으로 나열해 놓은 리스트. index가 작을 경우 낮은 order, index가 높을 경우 높은 order를 갖는다.
        int cnt = myCards.Count;
        for(int i = 0; i < cnt; i++)
        {
            var targetCard = myCards[i];
            targetCard?.GetComponent<Order>().SetOriginOrder(i+2);
        }
    }

    // 카드 PRS 정렬 (myCardLeft, myCardRight 기준 정렬)
    void CardAlignment()
    {
        // 정렬된 카드 PRS(position, rotation, scale)
        List<PRS> originCardPRSs = new List<PRS>();
        originCardPRSs = RoundAlignment(myCardLeft, myCardRight, myCards.Count, 0f, cardPrefab.transform.localScale);

        // 카드를 정렬된 카드 PRS로 이동
        var targetCards = myCards;
        for(int i = 0; i < targetCards.Count; i++)
        {
            var targetCard = targetCards[i];
            // 카드의 base PRS 설정
            targetCard.originPRS = originCardPRSs[i];
            // 카드 이동
            targetCard.MoveTransform(targetCard.originPRS, true, 0.7f);
        }
    }

    // 정렬된 카드 PRS 리스트 생성
    // leftTr: 맨 왼쪽 카드의 transform, rightTr: 맨 오른쪽 카드의 transform
    // objCount: 총 카드 개수, height: 카드 곡률 높이 (카드가 호 형태로 배치될텐데, 이때 호의 높이), scale: 카드 크기
    List<PRS> RoundAlignment(Transform leftTr, Transform rightTr, int objCount, float height, Vector3 scale)
    {
        float[] objLerps = new float[objCount];
        List<PRS> results = new List<PRS>(objCount);
        float interval;

        // 카드 개수에 따라 배치를 변경
        // 카드 1장: 가운데 배치
        // 카드 2~5장: leftTr, rightTr 기준 0.2~0.8 지점에 카드 배치
        // 카드 6장 이상: leftTr, rightTr 기준 0~1 지점에 카드 배치
        // switch (objCount)
        // {
        //     case 1: objLerps = new float[] { 0.5f }; break;
        //     case 2:
        //     case 3:
        //     case 4:
        //     case 5:
        //         interval = 1f / (objCount - 1);
        //         for (int i = 0; i < objCount; i++)
        //         {
        //             objLerps[i] = interval * i;
        //         }
        //         break;
        //     default:
        //         interval = 1.5f / (objCount - 1);
        //         for(int i = 0; i < objCount; i++)
        //         {
        //             objLerps[i] = interval * i;
        //         }
        //         break;
        // }
        if(objCount == 1)
        {
            objLerps = new float[] { 0.5f };
        }
        else
        {
            interval = 1f / (objCount - 1);
            for(int i = 0; i < objCount; i++)
            {
                objLerps[i] = interval * i;
            }
        }
        Vector3 newRightTr = new Vector3(rightTr.position.x, rightTr.position.y, rightTr.position.z);
        if(objCount > 5)
        {
            newRightTr.x += (objCount - 5) * cardPrefab.transform.localScale.x * 0.3f;
        }

        for(int i = 0; i < objCount; i++)
        {
            var targetPos = Vector3.Lerp(leftTr.position, newRightTr, objLerps[i]);
            var targetRot = Utils.QI;
            // // 곡률에 따른 카드 position, rotation 보정
            // float curve = Mathf.Sqrt(Mathf.Pow(height, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
            // curve = height >= 0 ? curve : -curve;
            // // 카드 position 설정
            // targetPos.y += curve;
            // // 카드 rotation 설정
            // targetRot = Quaternion.Slerp(leftTr.rotation, rightTr.rotation, objLerps[i]);
            results.Add(new PRS(targetPos, targetRot, scale));
        }

        return results;
    }

    // 카드에 마우스가 올라갔을 때 호출
    // 카드 확대, selectCard 설정, 키워드에 따른 툴팁 설정
    public void CardMouseOver(Card card)
    {
        // 카드가 핸드 영역을 벗어나 있을 경우 오류 발생, 즉시 종료
        if (!onMyCardArea)
        {
            return;
        }
        // 카드가 드래그 중일 경우 확대 및 selectedCard 설정 안함
        if (isMyCardDrag)
        {
            return;
        }

        // selectedCard = 마우스를 올려놓은 카드
        selectedCard = card;
        selectedCard.highlight.enabled = false;
        // 카드 확대
        EnlargeCard(true, card);
        if (tooltipCreated)
        {
            return;
        }
    }

    // 카드에 마우스가 빠져나갔을 때 호출
    // 카드 축소, 툴팁 제거
    public void CardMouseExit(Card card)
    {
        // 카드가 드래그 중일 경우 축소 설정 안함
        if (isMyCardDrag)
        {
            return;
        }

        if(selectedCard != null)
        {
            selectedCard.highlight.enabled = false;
        }
        // 확대했던 카드 축소
        EnlargeCard(false, card);
    }

    // 카드에 마우스를 놓고 눌렀을 때 호출
    // 카드 선택, 카드 드래그
    public void CardMouseDown(Card card)
    {
        // 현재 카드 선택 모드에 진입했을 경우
        if (cardSelectMode != ECardSelectMode.Hide)
        {
            // 누른 카드 선택
            SelectCard(card);
            return;
        }
        // 카드를 드래그할 수 없는 경우, 종료
        if (eCardState != ECardState.CanMouseDrag)
        {
            return;
        }
        // 카드 드래그 시작
        isMyCardDrag = true;
        if(selectedCard != null && selectedCard.item.isSingleTarget == true) onEnemyCardArea = -1;
    }

    // 카드에 마우스를 놓고 누른 후 뗐을 때 호출
    // 카드 사용
    public void CardMouseUp(Card card)
    {
        // 카드 드래그 종료
        isMyCardDrag = false;
        // 카드가 드래그 불가한 상황, 드래그한 카드가 없는 상황일 경우 오류 발생, 즉시 종료
        if (eCardState != ECardState.CanMouseDrag)
        {
            return;
        }
        if(selectedCard == null)
        {
            return;
        }
        // 카드를 놓은 지점이 MyCardArea 외부(복수 적일 경우 추가로 적 카드 적용 범위 내부)일 경우, 카드 사용
        if (!onMyCardArea)
        {
            if(TurnManager.Inst.characterSO.isTutorial == true && TutorialManager.Inst.cardActivate == true && TutorialManager.Inst.activateCardName != "tutorial_allcards" && TutorialManager.Inst.activateCardName != card.item.name)
            {
                EnlargeCard(false, selectedCard);
                selectedCard = null;
                return;
            }
            if(selectedCard.item.isSingleTarget == true && onEnemyCardArea == -1)
            {
                // EnlargeCard(false, selectedCard);
                // selectedCard = null;
                TurnManager.Inst.isLoading = true;
                selectedCard.gameObject.SetActive(false);
                StartCoroutine(CardEnemySelect());
                return;
            }
            // 카드 사용
            selectedCard.highlight.enabled = false;
            if (selectedCard.UseCard(onEnemyCardArea) == false)
            {
                // 카드 사용이 불가능할 경우, 카드 축소 및 selectCard 초기화
                EnlargeCard(false, selectedCard);
                selectedCard = null;
                return;
            }
            // 카드 사용 이후
            // 카드 사용 횟수 증가
            useCount++;
            useCount_Turn++;
            // 만약 현재 카드가 '잔류' 카드일 경우, '잔류' 효과에 의해 카드가 버려지지 않는다.
            // 이를 해결하기 위해 '잔류' 여부를 flag에 저장해 놓고, 잠시 카드의 '잔류' 효과를 해제한 후, 버려진 다음 카드의 '잔류' 여부를 재설정한다.
            bool flag = selectedCard.item.isRemain;
            selectedCard.item.isRemain = false;
            // 만약 현재 카드가 '소멸' 카드일 경우, 카드를 파괴한다.
            if (selectedCard.item.isVanish == true)
            {
                myCards.Remove(card);
                Destroy(card.gameObject);
            }
            // '소멸' 카드가 아닐 경우, 카드를 버린다.
            else
            {
                StartCoroutine(DiscardSingleCard(selectedCard));
            }
            // 카드를 버린 후 저장해 놓은 '잔류' 여부에 따라 카드의 '잔류' 여부를 재설정한다.
            selectedCard.item.isRemain = flag;
            // selectCard 초기화
            selectedCard = null;
            // 카드 재정렬
            SetOriginOrder();
            CardAlignment();
        }
        else
        {
            EnlargeCard(false, selectedCard);
            selectedCard.gameObject.SetActive(true);
            selectedCard = null;
        }
    }

    public IEnumerator CardEnemySelect()
    {
        enemySelectScreen.SetActive(true);
        List<(GameObject enemy, int enemyIdx)> enemyPosList = new List<(GameObject enemy, int enemyIdx)>();
        enemyPosList.Add((EnemyManager.Inst.enemyPos.gameObject, 0));
        for(int i = 0; i < Enemy.maxSubEnemyNum; i++)
        {
            if(EnemyManager.Inst.subEnemies[i] != null && EnemyManager.Inst.subEnemies[i].name != null)
            {
                enemyPosList.Add((EnemyManager.Inst.subEnemyPos[i].gameObject, i + 1));
            }
        }
        foreach(var (enemy, enemyIdx) in enemyPosList)
        {
            var enemyPos = Instantiate(enemy, enemySelectScreen.transform);
            enemyPos.transform.SetParent(enemySelectScreen.transform, true);
            enemyPos.transform.SetAsLastSibling();
            foreach(Transform child in enemyPos.transform)
            {
                if(child.name == "EnemyImg")
                {
                    foreach(Transform grandChild in child)
                    {
                        Destroy(grandChild.gameObject);
                    }
                    var childSR = child.GetComponent<SpriteRenderer>();
                    if(childSR != null) childSR.sortingOrder += enemySelectScreen.GetComponent<Canvas>().sortingOrder + 10;

                    child.gameObject.AddComponent<Button>().onClick.AddListener(() =>
                    {
                        onEnemyCardArea = enemyIdx;
                    });
                }
                else if(child.name == "EnemyHighlight")
                {
                    var childSR = child.GetComponent<SpriteRenderer>();
                    if(childSR != null)
                    {
                        childSR.sortingOrder += enemySelectScreen.GetComponent<Canvas>().sortingOrder + 10;
                        Sequence highlightBlinkSeq = DOTween.Sequence();
                        highlightBlinkSeq.Append(childSR.DOFade(0f, 0.5f));
                        highlightBlinkSeq.Append(childSR.DOFade(1f, 0.5f));
                        highlightBlinkSeq.SetLoops(-1);
                        highlightBlinkSeq.SetTarget(enemyPos);
                    }
                    child.gameObject.SetActive(true);
                }
                else if(child.name == "EnemyFrame")
                {
                    var childSRMask = child.GetComponent<SpriteMask>();
                    if(childSRMask != null)
                    {
                        childSRMask.frontSortingOrder += enemySelectScreen.GetComponent<Canvas>().sortingOrder + 10;
                        childSRMask.backSortingOrder += enemySelectScreen.GetComponent<Canvas>().sortingOrder + 10;
                    }
                }
                else
                {
                    Destroy(child.gameObject);
                }
            }

            GameObject enemyRoulette = null;
            if(enemyIdx == 0)
            {
                enemyRoulette = Instantiate(EnemyManager.Inst.mainEnemyRouletteBackground, enemySelectScreen.transform);
                enemyRoulette.transform.SetParent(enemySelectScreen.transform, true);
                enemyRoulette.transform.SetAsLastSibling();
                enemyRoulette.transform.position = EnemyManager.Inst.mainEnemyRouletteBackground.transform.position;
                enemyRoulette.transform.localScale = EnemyManager.Inst.mainEnemyRouletteBackground.transform.localScale * 30f * 1.085f;
            }
            else
            {
                int tempIdx = Array.FindIndex(EnemyManager.Inst.subEnemyCanvasPos_roulettePos, x => x == EnemyManager.Inst.subEnemies[enemyIdx - 1].roulettePos);
                if(tempIdx >= 0) enemyRoulette = Instantiate(EnemyManager.Inst.subEnemyCanvasPos_enemyRouletteBackground[tempIdx], enemySelectScreen.transform);
                enemyRoulette.transform.SetParent(enemySelectScreen.transform, true);
                enemyRoulette.transform.SetAsLastSibling();
                enemyRoulette.transform.position = EnemyManager.Inst.subEnemyCanvasPos_enemyRouletteBackground[tempIdx].transform.position;
                enemyRoulette.transform.localScale = EnemyManager.Inst.subEnemyCanvasPos_enemyRouletteBackground[tempIdx].transform.localScale * 30f * 1.085f;
            }
            
            foreach(Transform child in enemyRoulette.transform)
            {
                var childSR = child.GetComponent<SpriteRenderer>();
                if(childSR != null) childSR.sortingOrder += enemySelectScreen.GetComponent<Canvas>().sortingOrder + 10;
            }
        }

        var currentCard = Instantiate(selectedCard, enemySelectScreen.transform);
        currentCard.transform.SetParent(enemySelectScreen.transform, true);
        currentCard.transform.SetAsLastSibling();
        currentCard.highlight.enabled = true;
        Destroy(currentCard.GetComponent<Card>());
        Destroy(currentCard.GetComponent<Order>());
        currentCard.gameObject.SetActive(true);
        currentCard.transform.position = Vector3.zero;
        currentCard.transform.localScale = selectedCard.transform.localScale / 0.02777778f;
        
        yield return new WaitUntil(() => onEnemyCardArea >= 0);
        eCardState = ECardState.CanMouseDrag;
        enemySelectScreen.SetActive(false);
        foreach(Transform child in enemySelectScreen.transform)
        {
            if(child.name == "FadeoutScreen" || child.name == "Cancel") continue;
            DOTween.Kill(child.gameObject);
            Destroy(child.gameObject);
        }
        TurnManager.Inst.isLoading = false;
        CardMouseUp(selectedCard);
    }

    public void CancelEnemySelect()
    {
        onEnemyCardArea = -1;
        eCardState = ECardState.CanMouseDrag;
        enemySelectScreen.SetActive(false);
        foreach(Transform child in enemySelectScreen.transform)
        {
            if(child.name == "FadeoutScreen" || child.name == "Cancel") continue;
            DOTween.Kill(child.gameObject);
            Destroy(child.gameObject);
        }
        TurnManager.Inst.isLoading = false;
        selectedCard.gameObject.SetActive(true);
        EnlargeCard(false, selectedCard);
        selectedCard = null;
    }

    // 카드 드래그. 카드 위치를 마우스 위치로 이동
    void CardDrag()
    {
        if(selectedCard != null)
        {
            selectedCard.MoveTransform(new PRS(Utils.MousePos, Utils.QI, selectedCard.originPRS.scale), false);
            if(!onMyCardArea && (onEnemyCardArea >= 0 || selectedCard.item.isSingleTarget == false))
            {
                selectedCard.highlight.enabled = true;
            }
            else
            {
                selectedCard.highlight.enabled = false;
            }
        }
    }

    // 현재 카드가 MyCardArea에 올라와 있는지 확인, 이에 따라 onMyCardArea 설정
    void DetectCardArea()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int mylayer = LayerMask.NameToLayer("MyCardArea");
        int enemylayer = LayerMask.NameToLayer("EnemyCardArea");
        int layerMask = LayerMask.GetMask("MyCardArea", "EnemyCardArea");
        RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, layerMask);
        onMyCardArea = Array.Exists(hits, x => x.collider.gameObject.layer == mylayer);
        // var enemyHits = Array.Find(hits, x => x.collider.gameObject.layer == enemylayer);
        // if(enemyHits.collider != null)
        // {
        //     Transform enemyPos = enemyHits.collider.transform;
        //     onEnemyCardArea = EnemyManager.Inst.FindEnemyIdxByPos(enemyPos);

        //     if(isMyCardDrag && !onMyCardArea && selectedCard != null && selectedCard.item.isSingleTarget == true)
        //     {
        //         enemyPos.Find("EnemyHighlight").gameObject.SetActive(true);
        //     }
        // }
        // else
        // {
        //     onEnemyCardArea = 0;
        //     EnemyManager.Inst.enemyPos.Find("EnemyHighlight").gameObject.SetActive(false);
        //     for(int i = 0; i < Enemy.maxSubEnemyNum; i++)
        //     {
        //         if(EnemyManager.Inst.subEnemies[i] != null && EnemyManager.Inst.subEnemies[i].name != null)
        //         {
        //             onEnemyCardArea = -1;
        //             EnemyManager.Inst.subEnemyPos[i].Find("EnemyHighlight").gameObject.SetActive(false);
        //         }
        //     }

        //     if(isMyCardDrag && !onMyCardArea && selectedCard != null && selectedCard.item.isSingleTarget == true && onEnemyCardArea == 0)
        //     {
        //         EnemyManager.Inst.enemyPos.Find("EnemyHighlight").gameObject.SetActive(true);
        //     }
        // }
    }

    // 카드 확대/축소
    // isEnlarge=true: 확대
    // isEnlarge=false: 원래대로 축소
    public void EnlargeCard(bool isEnlarge, Card card)
    {
        if(isEnlarge)
        {
            Vector3 enlargePos = new Vector3(card.originPRS.pos.x, Camera.main.ScreenToWorldPoint(new Vector3(0f, 0f, Camera.main.nearClipPlane)).y + cardPrefab.transform.localScale.y * 0.75f, -10f);
            card.MoveTransform(new PRS(enlargePos, Utils.QI, cardPrefab.transform.localScale * 1.5f), false);
        }
        else
        {
            card.MoveTransform(card.originPRS, false);
            card.highlight.enabled = false;
        }

        // 확대했을 경우 카드의 order를 핸드에서 맨 앞에 오도록 설정
        card.GetComponent<Order>().SetMostFrontOrder(isEnlarge);
    }

    // 카드 매니저 state 설정. TurnManager.Inst.isLoading일 경우 카드 상호작용 불가, 이외의 경우 카드 드래그 가능
    void SetECardState()
    {
        if (!TurnManager.Inst)
        {
            return;
        }
        if (TurnManager.Inst.isLoading && (TurnManager.Inst.characterSO.isTutorial == false || TutorialManager.Inst.cardActivate == false))
        {
            eCardState = ECardState.Nothing;
        }
        else
        {
            eCardState = ECardState.CanMouseDrag;
        }
    }

    // 핸드 개수 반환
    public int myCardNum()
    {
        return myCards.Count;
    }

    #endregion
    
    #region Card Select Management
    // 카드 선택 모드 변경(Hide: 카드 선택 화면 숨김, Duplicate: 복제할 카드 선택, Discard: 버릴 카드 선택), selectNum: 선택할 카드 개수
    public void CardSelectModeTransit(ECardSelectMode mode, int selectNum)
    {
        // 카드 선택 모드 설정
        cardSelectMode = mode;
        cardSelectNum = selectNum;
        // Hide일 경우 카드 선택 화면 숨김, 아닐 경우 카드 선택 화면 보임
        cardSelectScreen.SetActive(mode != ECardSelectMode.Hide);
        // 카드 선택 화면을 띄울 경우 플레이어의 UI 이외 상호작용 막음(카드 드래그, 룰렛 등)
        TurnManager.Inst.isLoading = mode != ECardSelectMode.Hide;
        cardSelectButton.interactable = false;
        showBoardButton.gameObject.SetActive(mode != ECardSelectMode.Hide);

        // 카드 선택 화면 설명
        if (mode == ECardSelectMode.Duplicate)
        {
            selectModeText.text = "복제할 카드를 " + selectNum.ToString() + "장 선택하십시오.";
        }
        else if (mode == ECardSelectMode.Discard)
        {
            selectModeText.text = "버릴 카드를" + selectNum.ToString() + "장 선택하십시오.";
        }
        
        // 만약 선택해야 하는 카드 개수가 핸드 카드 개수 이상일 경우, 어짜피 핸드의 모든 카드를 선택해야 하므로 바로 모든 카드를 선택하고, 선택 완료 함수 SelectCardDone을 호출한다.
        if (selectNum >= myCards.Count)
        {
            foreach (Card card in myCards)
            {
                SelectCard(card);
            }
            SelectCardDone();
        }
    }

    public void ShowBoard()
    {
        if(cardSelectScreen.activeSelf == true) cardSelectScreen.SetActive(false);
        else cardSelectScreen.SetActive(true);
    }

    // 카드 card 선택 
    public void SelectCard(Card card)
    {
        // 선택한 카드 개수가 선택해야 하는 총 카드 개수보다 크면 오류 발생
        if (cardSelectNum < 0) return;
        // 선택한 카드 개수가 선택해야 하는 총 카드 개수와 같으면
        // 가장 먼저 선택한 카드를 제거하고, 이후 새로운 카드 card를 선택한다.
        if (cardSelectNum == 0)
        {
            UnSelectCard(selectedCardList[0]);
        }
        // 선택한 카드 개수가 선택해야 하는 총 카드 개수보다 작으면
        // 바로 새로운 카드 card를 선택한다.

        // 새로운 카드 card 생성 후 선택
        GameObject selectedCardUI = Instantiate(cardUISelectPrefab, selectedCards.transform.position, Utils.QI);
        selectedCardUI.transform.SetParent(selectedCards.transform, false);
        CardUI_Select cUI = selectedCardUI.GetComponent<CardUI_Select>();
        cUI.Setup(card.item);
        selectedCardList.Add(selectedCardUI);
        // Discard 모드일 경우 카드를 숨긴다. (버리기 모드에서는 선택 완료 시 핸드에서 카드가 버려지는 연출을 수행해야 하므로)
        if(cardSelectMode == ECardSelectMode.Discard)
        {
            card.gameObject.SetActive(false);
            discardCardList.Add(card);
        }
        cardSelectNum--;
    }

    // 카드 오브젝트 cUI_gameObject 선택 해제
    public void UnSelectCard(GameObject cUI_gameObject)
    {
        // 선택 해제할 카드의 selectedCardList 내 index
        int idx = selectedCardList.IndexOf(cUI_gameObject);
        // 주어진 카드를 제거
        selectedCardList.Remove(cUI_gameObject);
        Destroy(cUI_gameObject);
        // 버리기 모드일 경우 제거한 카드가 다시 핸드에서 보이게끔 설정한다.
        if (cardSelectMode == ECardSelectMode.Discard)
        {
            discardCardList[idx].gameObject.SetActive(true);
            EnlargeCard(false, discardCardList[idx]);
            discardCardList.RemoveAt(idx);
        }
        // 선택 가능한 카드 개수 증가
        cardSelectNum++;
    }

    // 카드 선택 종료 가능 여부 반환
    public bool IsSelectable()
    {
        return cardSelectNum == 0;
    }
    
    // 카드 선택 완료
    void SelectCardDone()
    {
        // 카드 복제 모드
        if (cardSelectMode == ECardSelectMode.Duplicate)
        {
            foreach (var cardObj in selectedCardList)
            {
                // 선택한 카드의 item을 갖는 새로운 카드 핸드에 생성
                CreateCardInHand(cardObj.GetComponent<CardUI_Select>().item);
                // 선택 카드 UI 오브젝트 파괴
                Destroy(cardObj);
            }
            selectedCardList.Clear();
            SetOriginOrder();
            CardAlignment();
            // 카드 선택 모드 Hide로 변경(카드 선택 화면 숨김)
            CardSelectModeTransit(ECardSelectMode.Hide, 0);
        }
        // 카드 버리기 모드
        else if (cardSelectMode == ECardSelectMode.Discard)
        {
            for (int i = 0; i < selectedCardList.Count; i++)
            {
                // 선택한 카드 버림
                discardCardList[i].MoveTransform(new PRS(selectedCardList[i].transform.position, Utils.QI, new Vector3(1, 1, 1)), false, 0f);
                discardCardList[i].gameObject.SetActive(true);
                StartCoroutine(DiscardSingleCard(discardCardList[i]));
                // 선택 카드 UI 오브젝트 파괴
                Destroy(selectedCardList[i]);
            }
            selectedCardList.Clear();
            discardCardList.Clear();
            SetOriginOrder();
            CardAlignment();
            // 카드 선택 모드 Hide로 변경(카드 선택 화면 숨김)
            CardSelectModeTransit(ECardSelectMode.Hide, 0);
        }
    }

    #endregion
    // Start
    // 메인 카메라, 툴팁 캔버스 설정
    // 카드 드로우, 카드 버리기 함수 설정
    // 플레이어 턴 시작 시 '이번 턴 사용한 카드 개수(useCount_Turn)' 초기화
    private void Start()
    {
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
}
