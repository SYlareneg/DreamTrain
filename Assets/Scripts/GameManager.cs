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

    [SerializeField] NotificationPanel notificationPanel;
    [SerializeField] ResultPanel resultPanel;
    [SerializeField] GameObject endTurnBtn;
    [SerializeField] TMP_Text turnNotificationTMP;
    [SerializeField] TMP_Text drawNum;
    [SerializeField] TMP_Text discardNum;
    [SerializeField] TMP_Text deckNum;
    [SerializeField] TMP_Text costTMP;
    [SerializeField] TMP_Text healthTMP;
    [SerializeField] TMP_Text triggerCountTMP;
    [SerializeField] TMP_Text enemyHealthTMP;
    [SerializeField] TMP_Text enemyTriggerCountTMP;

    public CharacterSO characterSO;

    [SerializeField] GameObject cardScrollView;
    public GameObject cardListScroll;
    public List<CardUI> cardList;
    [SerializeField] GameObject relicScrollView;
    public GameObject relicListScroll;
    public List<RelicUI> relicList;

    public bool gameOverSignal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DOTween.SetTweensCapacity(500, 50);
        gameOverSignal = false;
        StartGame();
    }

    // Update is called once per frame
    private void Update()
    {
        InputCheatKey();
        drawNum.text = CardManager.Inst.itemDraw.Count.ToString();
        discardNum.text = CardManager.Inst.itemDiscard.Count.ToString();
        deckNum.text = CardManager.Inst.itemDeck.Count.ToString();
        costTMP.text = TurnManager.Inst.nowCost.ToString() + "/" + TurnManager.Inst.turnCost.ToString();
        healthTMP.text = TurnManager.Inst.curHealth.ToString() + "/" + TurnManager.Inst.maxHealth.ToString();
        triggerCountTMP.text = TurnManager.Inst.playerTriggerCnt.ToString() + "/" + TurnManager.Inst.playerTriggerMaxCnt.ToString();
        enemyHealthTMP.text = TurnManager.Inst.enemyCurHealth.ToString() + "/" + TurnManager.Inst.enemyMaxHealth.ToString();
        enemyTriggerCountTMP.text = TurnManager.Inst.enemyTriggerCnt.ToString() + "/" + TurnManager.Inst.enemyTriggerMaxCnt.ToString();
    }

    void InputCheatKey()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TurnManager.OnAddCard?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            TurnManager.Inst.EndTurn();
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

    public void StartGame()
    {
        TurnManager.Inst.StartGameCo();
    }

    public void Notification(string title, string message, Action onComplete)
    {
        turnNotificationTMP.text = message;
        notificationPanel.Show(title, onComplete);
    }

    public IEnumerator GameOver(bool isMyWin)
    {
        gameOverSignal = true;
        TurnManager.Inst.isLoading = true;
        endTurnBtn.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        resultPanel.Show(isMyWin ? "Win" : "Lose");
    }

    public enum ListType { Deck, Draw, Discard };

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

    public void DeckCardList()
    {
        CardList(ListType.Deck);
    }

    public void DrawCardList()
    {
        CardList(ListType.Draw);
    }

    public void DiscardCardList()
    {
        CardList(ListType.Discard);
    }

    public void RelicList()
    {
        if (relicScrollView.activeSelf == false)
        {
            TurnManager.Inst.isLoading = true;
            foreach (RelicUI relic in relicList)
            {
                Destroy(relic.gameObject);
            }

            relicList = RelicManager.Inst.RelicItemListToRelicUIList(RelicManager.Inst.relicList);
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
