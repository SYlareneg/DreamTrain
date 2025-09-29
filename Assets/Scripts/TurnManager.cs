using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }
    private void Awake() => Inst = this;

    [Header("Develop")]
    [SerializeField][Tooltip("카드 배분이 매우 빨라집니다")] bool fastMode;
    [SerializeField][Tooltip("최대 카드 개수를 정합니다")] public int maxCardCount;
    [SerializeField][Tooltip("시작 카드 개수를 정합니다")] public int startCardCount;
    [SerializeField][Tooltip("매 턴 드로우할 카드 개수를 정합니다")] public int drawCardCount;
    [SerializeField][Tooltip("매 턴 얻는 코스트를 정합니다")] public int turnCost;

    [Header("Properties")]
    public bool isLoading;
    public int turnNum;
    public int nowCost;
    public int maxHealth;
    public int curHealth;
    public int enemyMaxHealth;
    public int enemyCurHealth;
    public int playerTriggerMaxCnt;
    public int playerTriggerCnt;
    public int enemyTriggerMaxCnt;
    public int enemyTriggerCnt;

    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
    WaitForSeconds delay07 = new WaitForSeconds(0.7f);
    WaitForSeconds delay10 = new WaitForSeconds(1.0f);
    WaitForSeconds delay15 = new WaitForSeconds(1.5f);

    public static Action OnAddCard;
    public static Action OnDiscardCard;
    public static Action OnTurnStart;
    public static Action OnTurnEnd;

    void GameSetup()
    {
        if(fastMode)
        {
            delay05 = new WaitForSeconds(0.05f);
        }
    }

    public IEnumerator StartGameCo()
    {
        GameSetup();
        isLoading = true;
        turnNum = 0;
        nowCost = 0;
        maxHealth = GameManager.Inst.characterSO.maxHealth;
        curHealth = GameManager.Inst.characterSO.curHealth;
        enemyMaxHealth = GameManager.Inst.characterSO.enemyMaxHealth;
        enemyCurHealth = GameManager.Inst.characterSO.enemyCurHealth;
        playerTriggerMaxCnt = GameManager.Inst.characterSO.playerTriggerMaxCnt;
        playerTriggerCnt = 0;
        enemyTriggerMaxCnt = GameManager.Inst.characterSO.enemyTriggerMaxCnt;
        enemyTriggerCnt = 0;
        RelicManager.Inst.ActivateRelics();
        EnemyManager.Inst.maxActionVal = GameManager.Inst.characterSO.enemyMaxActionVal;
        EnemyManager.Inst.actionNum = GameManager.Inst.characterSO.enemyActionNum;
        RouletteManager.Inst.InitRoulette();
        CardManager.Inst.InitializeItemBuffer();
        CardManager.Inst.ShuffleDeck();

        StartCoroutine(Draw(startCardCount));
        yield return delay05;
        StartCoroutine(StartTurnCo());
    }

    public IEnumerator StartTurnCo()
    {
        isLoading = true;
        turnNum++;
        nowCost = turnCost;
        OnTurnStart?.Invoke();

        GameManager.Inst.Notification("My Turn", "Turn "+turnNum.ToString());

        yield return delay07;
        EnemyManager.Inst.ShowAllActions();
        StartCoroutine(Draw(drawCardCount));
        yield return delay10;
        isLoading = false;
    }

    public void EndTurn()
    {
        StartCoroutine(EndTurnCo());
        isLoading = true;
        Discard();
    }

    public IEnumerator EndTurnCo()
    {
        bool allCoFinished = false;
        OnTurnEnd += () => allCoFinished = true;
        OnTurnEnd?.Invoke();
        yield return new WaitUntil(() => allCoFinished);
        yield return delay15;
        if (GameManager.Inst.gameOverSignal == false)
        {
            StartCoroutine(StartTurnCo());
        }
    }

    public IEnumerator Draw(int drawNum)
    {
        for (int i = 0; i < drawNum; i++)
        {
            yield return delay05;
            OnAddCard?.Invoke();
        }
    }

    public void Discard()
    {
        OnDiscardCard?.Invoke();
    }

    public bool TakeDmg(int damage)
    {
        if(curHealth > damage)
        {
            curHealth -= damage;
            if(curHealth > maxHealth)
            {
                curHealth = maxHealth;
            }
            return true;
        }
        else
        {
            curHealth = 0;
            StartCoroutine(GameManager.Inst.GameOver(false));
            return false;
        }
    }

    public bool EnemyTakeDmg(int damage)
    {
        if(enemyCurHealth > damage)
        {
            enemyCurHealth -= damage;
            if(enemyCurHealth > enemyMaxHealth)
            {
                enemyCurHealth = enemyMaxHealth;
            }
            return true;
        }
        else
        {
            enemyCurHealth = 0;
            StartCoroutine(GameManager.Inst.GameOver(true));
            return false;
        }
    }

    public void IncreaseCost(int value)
    {
        nowCost += value;
    }

    public void TriggerPlayerPassive(int value)
    {
        playerTriggerCnt += value;
        if(playerTriggerCnt >= playerTriggerMaxCnt)
        {
            playerTriggerCnt -= playerTriggerMaxCnt;
            RouletteManager.Inst.TriggerRoulette();
        }
    }

    public void TriggerEnemyPassive(int value)
    {
        enemyTriggerCnt += value;
        if(enemyTriggerCnt >= enemyTriggerMaxCnt)
        {
            enemyTriggerCnt -= enemyTriggerMaxCnt;
            EnemyManager.Inst.EnemyTriggerAction();
        }
    }
}
