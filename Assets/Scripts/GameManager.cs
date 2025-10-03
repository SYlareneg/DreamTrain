using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    void Awake() => Inst = this;

    [Header("게임 UI")]
    [SerializeField][Tooltip("화면 중심 안내 UI")] NotificationPanel notificationPanel;
    [SerializeField][Tooltip("게임 종료 UI")] ResultPanel resultPanel;
    [SerializeField][Tooltip("턴 종료 버튼")] GameObject endTurnBtn;
    [SerializeField][Tooltip("턴 텍스트")] TMP_Text turnNotificationTMP;
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
    [SerializeField][Tooltip("플레이어 실드 UI")] GameObject shieldObj;
    [SerializeField][Tooltip("플레이어 실드 값 텍스트")] TMP_Text shieldTMP;
    [SerializeField][Tooltip("플레이어 트리거 조건 텍스트")] TMP_Text triggerCountTMP;
    [Header("적 UI")]
    [SerializeField][Tooltip("적 체력 값 텍스트")] TMP_Text enemyHealthTMP;
    [SerializeField][Tooltip("적 실드 UI")] GameObject enemyShieldObj;
    [SerializeField][Tooltip("적 실드 값 텍스트")] TMP_Text enemyShieldTMP;
    [SerializeField][Tooltip("적 트리거 조건 텍스트")] TMP_Text enemyTriggerCountTMP;
    [HideInInspector] public List<RelicUI> relicList;

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
        if (Input.GetKeyDown(KeyCode.Q))
        {
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
    }

    // UI 텍스트, 숨김 여부 설정
    void UpdateUIState()
    {
        drawNum.text = CardManager.Inst.itemDraw.Count.ToString();
        discardNum.text = CardManager.Inst.itemDiscard.Count.ToString();
        deckNum.text = CardManager.Inst.itemDeck.Count.ToString();
        costTMP.text = TurnManager.Inst.nowCost.ToString() + "/" + TurnManager.Inst.turnCost.ToString();
        healthTMP.text = TurnManager.Inst.curHealth.ToString() + "/" + TurnManager.Inst.maxHealth.ToString();
        if(TurnManager.Inst.shieldHealth > 0)
        {
            shieldObj.SetActive(true);
        }
        else
        {
            shieldObj.SetActive(false);
        }
        shieldTMP.text = TurnManager.Inst.shieldHealth.ToString();
        triggerCountTMP.text = TurnManager.Inst.playerTriggerCnt.ToString() + "/" + TurnManager.Inst.playerTriggerMaxCnt.ToString();
        enemyHealthTMP.text = TurnManager.Inst.enemyCurHealth.ToString() + "/" + TurnManager.Inst.enemyMaxHealth.ToString();
        enemyTriggerCountTMP.text = TurnManager.Inst.enemyTriggerCnt.ToString() + "/" + TurnManager.Inst.enemyTriggerMaxCnt.ToString();
        if(TurnManager.Inst.enemyShieldHealth > 0)
        {
            enemyShieldObj.SetActive(true);
        }
        else
        {
            enemyShieldObj.SetActive(false);
        }
        enemyShieldTMP.text = TurnManager.Inst.enemyShieldHealth.ToString();
    }

    public void StartGame()
    {
        TurnManager.Inst.StartGameCo();
    }

    // 게임 종료
    public IEnumerator GameOver(bool isMyWin)
    {
        gameOverSignal = true;
        TurnManager.OnGameEnd?.Invoke();
        TurnManager.Inst.isLoading = true;
        endTurnBtn.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        resultPanel.Show(isMyWin ? "Win" : "Lose");
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
        if (relicScrollView.activeSelf == false)
        {
            TurnManager.Inst.isLoading = true;
            foreach (RelicUI relic in relicList)
            {
                Destroy(relic.gameObject);
            }

            relicList = RelicManager.Inst.RelicItemListToRelicUIList(RelicManager.Inst.relicList, relicListScroll.transform);
            Canvas.ForceUpdateCanvases();

            relicScrollView.SetActive(true);
        }
        else
        {
            TurnManager.Inst.isLoading = false;
            relicScrollView.SetActive(false);
        }
    }
}
