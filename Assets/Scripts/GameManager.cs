using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    void Awake() => Inst = this;

    [Header("게임 UI")]
    [SerializeField][Tooltip("화면 중심 안내 UI")] NotificationPanel notificationPanel;
    [SerializeField][Tooltip("게임 종료 UI")] ResultPanel resultPanel;
    [SerializeField][Tooltip("턴 종료 버튼")] GameObject endTurnBtn;
    [SerializeField][Tooltip("턴 텍스트")] TMP_Text turnNotificationTMP;
    [SerializeField][Tooltip("룰렛 버프 위치")] Vector2 rouletteBuffPos;
    [Tooltip("룰렛 버프")] public GameObject rouletteBuffUIView;
    [Header("카드 UI")]
    [SerializeField][Tooltip("카드 목록 UI")] GameObject cardScrollView;
    [Tooltip("카드 목록 content")] public GameObject cardListScroll;
    [HideInInspector] public List<CardUI> cardList;
    [SerializeField][Tooltip("드로우풀 카드 수 UI")] TMP_Text drawNum;
    [SerializeField][Tooltip("무덤 카드 수 UI")] TMP_Text discardNum;
    [SerializeField][Tooltip("덱 카드 수 UI")] TMP_Text deckNum;
    [Header("이드 UI")]
    [SerializeField][Tooltip("이드 목록 UI")] GameObject relicScrollView;
    [Tooltip("이드 목록 content")] public GameObject relicListScroll;
    [Header("플레이어 UI")]
    [SerializeField][Tooltip("플레이어 행동력 값 텍스트")] TMP_Text costTMP;
    [SerializeField][Tooltip("플레이어 체력 값 텍스트")] TMP_Text healthTMP;
    [SerializeField][Tooltip("플레이어 체력 바")] Image healthImg;
    [SerializeField][Tooltip("플레이어 실드 UI")] GameObject shieldObj;
    [SerializeField][Tooltip("플레이어 실드 값 텍스트")] TMP_Text shieldTMP;
    [SerializeField][Tooltip("플레이어 트리거 조건 텍스트")] TMP_Text triggerCountTMP;
    [SerializeField][Tooltip("플레이어 트리거 조건 바")] Image triggerCntImg;
    [SerializeField][Tooltip("플레이어 버프 위치")] Vector2 playerBuffPos;
    [Tooltip("플레이어 버프")] public GameObject playerBuffUIView;
    [Tooltip("플레이어 페르소나")] public Image personaImg;
    [Tooltip("플레이어 그림자")] public Image shadowImg;
    [Header("적 UI")]
    [SerializeField][Tooltip("적 체력 값 텍스트")] TMP_Text enemyHealthTMP;
    [SerializeField][Tooltip("적 체력 바")] Image enemyHealthImg;
    [SerializeField][Tooltip("적 실드 UI")] GameObject enemyShieldObj;
    [SerializeField][Tooltip("적 실드 값 텍스트")] TMP_Text enemyShieldTMP;
    [SerializeField][Tooltip("적 트리거 조건 텍스트")] TMP_Text enemyTriggerCountTMP;
    [SerializeField][Tooltip("적 트리거 조건 바")] Image enemyTriggerCntImg;
    [SerializeField][Tooltip("적 버프 위치")] Vector2 enemyBuffPos;
    [Tooltip("적 버프")] public GameObject enemyBuffUIView;
    [HideInInspector] public List<RelicUI> relicList;
    [Header("카드 획득 UI")]
    [SerializeField][Tooltip("카드 획득 화면")] GameObject rewardCardView;
    [SerializeField][Tooltip("획득 카드 목록")] CardUI_Reward[] rewardCards;
    [SerializeField][Tooltip("플레이어 정보")] CharacterSO characterSO;
    [SerializeField][Tooltip("카드풀 정보(공용)")] ItemSO normalItemListSO;
    [SerializeField][Tooltip("카드풀 정보(페르소나/그림자)")] DreamPieceSO dreamPieceListSO;
    [SerializeField][Tooltip("카드 등장 확률(가중치)\n{0: 공용, 1: 일반, 2: 페르소나-전용, 3: 그림자-전용}")] float[] rewardCardWeights = new float[4];
    [SerializeField][Tooltip("카드 강화 확률")] float enhanceProbability;
    [Header("기타")]
    [SerializeField][Tooltip("스테이지 적 정보")] StageSO stageSO;
    [HideInInspector] public bool gameOverSignal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        DOTween.SetTweensCapacity(500, 50);
        gameOverSignal = false;
        StartGame();
    }

    // Update is called once per frame
    private void Update()
    {
        InputCheatKey();
        UpdateUIState();
    }

    // 개발자용 특수입력
    void InputCheatKey()
    {
        if (TurnManager.Inst.isLoading == false)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Utils.AllignActions(ref TurnManager.OnAddCard, typeof(ShowBuff), typeof(RelicManager));
                TurnManager.OnAddCard?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                TurnManager.Inst.EndPlayerTurn();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                RouletteManager.Inst.Spin(false, 1);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                RouletteManager.Inst.Spin(true, 1);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                RouletteManager.Inst.ActivateRoulette();
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                RouletteManager.Inst.TriggerRoulette();
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                Lever.Inst.ActivateLever();
            }
        }
    }

    // UI 텍스트, 숨김 여부 설정
    void UpdateUIState()
    {
        drawNum.text = CardManager.Inst.itemDraw.Count.ToString();
        discardNum.text = CardManager.Inst.itemDiscard.Count.ToString();
        deckNum.text = CardManager.Inst.itemDeck.Count.ToString();
        costTMP.text = TurnManager.Inst.nowCost.ToString() + "/" + TurnManager.Inst.turnCost.ToString();
        healthTMP.text = TurnManager.Inst.curHealth.ToString() + "/" + TurnManager.Inst.maxHealth.ToString();
        healthImg.fillAmount = (float)TurnManager.Inst.curHealth / TurnManager.Inst.maxHealth;
        if (TurnManager.Inst.shieldHealth > 0)
        {
            shieldObj.SetActive(true);
        }
        else
        {
            shieldObj.SetActive(false);
        }
        shieldTMP.text = TurnManager.Inst.shieldHealth.ToString();
        triggerCountTMP.text = TurnManager.Inst.playerTriggerCnt.ToString() + "/" + TurnManager.Inst.playerTriggerMaxCnt.ToString();
        if (TurnManager.Inst.playerTriggerMaxCnt == 0)
        {
            triggerCntImg.fillAmount = 0;
        }
        else
        {
            triggerCntImg.fillAmount = (float)TurnManager.Inst.playerTriggerCnt / TurnManager.Inst.playerTriggerMaxCnt;
        }
        enemyHealthTMP.text = TurnManager.Inst.enemyCurHealth.ToString() + "/" + TurnManager.Inst.enemyMaxHealth.ToString();
        enemyHealthImg.fillAmount = (float)TurnManager.Inst.enemyCurHealth / TurnManager.Inst.enemyMaxHealth;
        if(TurnManager.Inst.enemyShieldHealth > 0)
        {
            enemyShieldObj.SetActive(true);
        }
        else
        {
            enemyShieldObj.SetActive(false);
        }
        enemyShieldTMP.text = TurnManager.Inst.enemyShieldHealth.ToString();
        enemyTriggerCountTMP.text = TurnManager.Inst.enemyTriggerCnt.ToString() + "/" + TurnManager.Inst.enemyTriggerMaxCnt.ToString();
        if (TurnManager.Inst.enemyTriggerMaxCnt == 0)
        {
            enemyTriggerCntImg.fillAmount = 0;
        }
        else
        {
            enemyTriggerCntImg.fillAmount = (float)TurnManager.Inst.enemyTriggerCnt / TurnManager.Inst.enemyTriggerMaxCnt;
        }
    }

    public void StartGame()
    {
        if (TurnManager.Inst.characterSO.personaPiece != null)
        {
            Tooltip tooltip = personaImg.GetComponentInParent<Tooltip>();
            if (TurnManager.Inst.characterSO.personaPiece.persona.isEnhanced)
            {
                personaImg.sprite = TurnManager.Inst.characterSO.personaPiece.persona.enhancedPassive.sprite;
                if (tooltip)
                {
                    tooltip.tooltipTitle = TurnManager.Inst.characterSO.personaPiece.persona.enhancedPassive.name;
                    tooltip.tooltipTxt = TurnManager.Inst.characterSO.personaPiece.persona.enhancedPassive.text;
                }
            }
            else
            {
                personaImg.sprite = TurnManager.Inst.characterSO.personaPiece.persona.sprite;
                if (tooltip)
                {
                    tooltip.tooltipTitle = TurnManager.Inst.characterSO.personaPiece.persona.name;
                    tooltip.tooltipTxt = TurnManager.Inst.characterSO.personaPiece.persona.text;
                }
            }
        }
        if (TurnManager.Inst.characterSO.shadowPiece != null)
        {
            Tooltip tooltip = shadowImg.GetComponentInParent<Tooltip>();
            if (TurnManager.Inst.characterSO.shadowPiece.shadow.isEnhanced)
            {
                shadowImg.sprite = TurnManager.Inst.characterSO.shadowPiece.shadow.enhancedPassive.sprite;
                if (tooltip)
                {
                    tooltip.tooltipTitle = TurnManager.Inst.characterSO.shadowPiece.shadow.enhancedPassive.name;
                    tooltip.tooltipTxt = TurnManager.Inst.characterSO.shadowPiece.shadow.enhancedPassive.text;
                }
            }
            else
            {
                shadowImg.sprite = TurnManager.Inst.characterSO.shadowPiece.shadow.sprite;
                if (tooltip)
                {
                    tooltip.tooltipTitle = TurnManager.Inst.characterSO.shadowPiece.shadow.name;
                    tooltip.tooltipTxt = TurnManager.Inst.characterSO.shadowPiece.shadow.text;
                }
            }
        }
        TurnManager.Inst.InitializeGame();
        SceneChangeManager.Inst.SceneFadeIn(TurnManager.Inst.StartGameCo);
    }

    // 게임 종료
    public IEnumerator GameOver(bool isMyWin)
    {
        gameOverSignal = true;
        Utils.AllignActions(ref TurnManager.OnGameEnd, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnGameEnd?.Invoke();
        TurnManager.Inst.isLoading = true;
        endTurnBtn.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        if(isMyWin == false)
        {
            resultPanel.Show("패배");
        }
        else
        {
            Notification("승리", "", () =>
            {
                ShowCardReward();
            });
        }
    }
    
    // 화면 중심 안내 UI 호출
    public void Notification(string title, string message, Action onComplete)
    {
        turnNotificationTMP.text = message;
        notificationPanel.Show(title, onComplete);
    }

    public enum ListType { Deck, Draw, Discard };

    // 카드 목록 UI 호출
    public void CardList(ListType listType)
    {
        if (cardScrollView.activeSelf == false)
        {
            TurnManager.Inst.isLoading = true;
            foreach (CardUI card in cardList)
            {
                Destroy(card.gameObject);
            }

            switch (listType)
            {
                case ListType.Deck:
                    cardList = CardManager.Inst.ItemBufferToCardUIList(CardManager.Inst.itemDeck);
                    break;
                case ListType.Draw:
                    cardList = CardManager.Inst.ItemBufferToCardUIList(CardManager.Inst.itemDraw);
                    break;
                case ListType.Discard:
                    cardList = CardManager.Inst.ItemBufferToCardUIList(CardManager.Inst.itemDiscard);
                    break;
            }
            Canvas.ForceUpdateCanvases();

            cardScrollView.SetActive(true);
        }
        else
        {
            TurnManager.Inst.isLoading = false;
            cardScrollView.SetActive(false);
        }
    }

    // 덱 카드 목록 띄움
    public void DeckCardList()
    {
        CardList(ListType.Deck);
    }

    // 드로우 풀 카드 목록 띄움
    public void DrawCardList()
    {
        CardList(ListType.Draw);
    }

    // 무덤 카드 목록 띄움
    public void DiscardCardList()
    {
        CardList(ListType.Discard);
    }

    // 이드 목록 띄움
    public void RelicList()
    {
        TurnManager.Inst.isLoading = true;
        foreach (RelicUI relic in relicList)
        {
            Destroy(relic.gameObject);
        }

        relicList = RelicManager.Inst.RelicItemListToRelicUIList(RelicManager.Inst.relicList, relicListScroll.transform);
        Canvas.ForceUpdateCanvases();
    }

    public void SetPlayerBuffUI()
    {
        for (int i = playerBuffUIView.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(playerBuffUIView.transform.GetChild(i).gameObject);
        }
        BuffManager.Inst.BuffListToBuffUIList(BuffManager.Inst.playerShowBuffs, playerBuffUIView, playerBuffPos);
        var tooltipList = playerBuffUIView.GetComponentsInChildren<Tooltip>();
        foreach(var tooltip in tooltipList)
        {
            tooltip.tooltipPivot = new Vector2(0, 0);
        }
    }

    public void SetEnemyBuffUI()
    {
        for (int i = enemyBuffUIView.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(enemyBuffUIView.transform.GetChild(i).gameObject);
        }
        BuffManager.Inst.BuffListToBuffUIList(BuffManager.Inst.enemyShowBuffs, enemyBuffUIView, enemyBuffPos);
    }

    public void SetRouletteBuffUI()
    {
        for (int i = rouletteBuffUIView.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(rouletteBuffUIView.transform.GetChild(i).gameObject);
        }
        BuffManager.Inst.BuffListToBuffUIList(BuffManager.Inst.rouletteShowBuffs, rouletteBuffUIView, rouletteBuffPos);
    }

    public void ShowCardReward()
    {
        SetCardReward();
        rewardCardView.SetActive(true);
    }

    public void EndCardReward()
    {
        rewardCardView.SetActive(false);
        if(characterSO.enemyName == stageSO.stageList[stageSO.currentStage].bossName)
        {
            stageSO.stageList[stageSO.currentStage].stageClear = true;
        }
        StageEnemy stageEnemy = stageSO.stageList[stageSO.currentStage].stageEnemies.Find(x => x.enemyName == characterSO.enemyName);
        if(stageEnemy != null)
        {
            stageEnemy.isClear = true;
        }
        SceneChangeManager.Inst.SceneFadeOut("PassengerScene");
    }

    public void SetCardReward()
    {
        List<Item> shareCards = normalItemListSO.items;
        List<Item> normalCards = new List<Item>();
        List<Item> personaCards = new List<Item>();
        List<Item> shadowCards = new List<Item>();
        List<Item> normalCards_enhanced = new List<Item>();
        List<Item> personaCards_enhanced = new List<Item>();
        List<Item> shadowCards_enhanced = new List<Item>();
        DreamPiece_Reference persona_ref = dreamPieceListSO.dreamPieces.Find(x => x.name == characterSO.personaPiece.name);
        DreamPiece_Reference shadow_ref = dreamPieceListSO.dreamPieces.Find(x => x.name == characterSO.shadowPiece.name);
        foreach (Item_Enhanceable item in persona_ref.cards)
        {
            if (item.element == EPassiveType.Normal)
            {
                normalCards.Add((Item)item);
                normalCards_enhanced.Add(item.enhancedItem);
            }
            else if (item.element == EPassiveType.Persona)
            {
                personaCards.Add((Item)item);
                personaCards_enhanced.Add(item.enhancedItem);
            }
        }
        foreach (Item_Enhanceable item in shadow_ref.cards)
        {
            if (item.element == EPassiveType.Normal)
            {
                normalCards.Add((Item)item);
                normalCards_enhanced.Add(item.enhancedItem);
            }
            else if (item.element == EPassiveType.Shadow)
            {
                shadowCards.Add((Item)item);
                shadowCards_enhanced.Add(item.enhancedItem);
            }
        }
        foreach (CardUI_Reward rc in rewardCards)
        {
            bool isE = Random.value < enhanceProbability;
            float totalW = 0f;
            for (int i = isE ? 1 : 0; i < rewardCardWeights.Length; i++)
            {
                totalW += rewardCardWeights[i];
            }
            float rPoint = Random.value * totalW;
            int chooseCardPool = isE ? 1 : 0;
            for (int i = isE ? 1 : 0; i < rewardCardWeights.Length; i++)
            {
                if (rPoint < rewardCardWeights[i])
                {
                    chooseCardPool = i;
                    break;
                }
                rPoint -= rewardCardWeights[i];
            }
            List<Item> lookat;
            switch (chooseCardPool)
            {
                case 0:
                    lookat = shareCards;
                    break;
                case 1:
                    if (isE) lookat = normalCards_enhanced;
                    else lookat = normalCards;
                    break;
                case 2:
                    if (isE) lookat = personaCards_enhanced;
                    else lookat = personaCards;
                    break;
                case 3:
                    if (isE) lookat = shadowCards_enhanced;
                    else lookat = shadowCards;
                    break;
                default:
                    lookat = new List<Item>();
                    break;
            }
            int cardIdx = Random.Range(0, lookat.Count);
            rc.Setup(lookat[cardIdx]);
            rc.gameObject.SetActive(true);
        }
    }

    public void AddCardReward(Item item)
    {
        Item newItem = new Item();
        newItem.SetItem(item);
        newItem.num = 1;
        if(item.dreamPieceNum < 0)
        {
            var existItem = characterSO.normalCards.Find(x => x.name == item.name);
            if(existItem == null) characterSO.normalCards.Add(newItem);
            else existItem.num++;
        }
        else if(dreamPieceListSO.dreamPieces[item.dreamPieceNum].name == characterSO.personaPiece.name)
        {
            var existItem = characterSO.personaPiece.cards.Find(x => x.name == item.name);
            if(existItem == null) characterSO.personaPiece.cards.Add(newItem);
            else existItem.num++;
        }
        else if(dreamPieceListSO.dreamPieces[item.dreamPieceNum].name == characterSO.shadowPiece.name)
        {
            var existItem = characterSO.shadowPiece.cards.Find(x => x.name == item.name);
            if(existItem == null) characterSO.shadowPiece.cards.Add(newItem);
            else existItem.num++;
        }
        else
        {
            Debug.LogError("undefined card added!");
        }
    }
}
