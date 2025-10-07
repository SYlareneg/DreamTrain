using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Inst { get; private set; }
    void Awake() => Inst = this;

    [Header("Prefabs")]
    [SerializeField][Tooltip("액션 심볼 Prefab")] GameObject actionPrefab;
    [Header("Positions")]
    [SerializeField][Tooltip("액션 심볼 스폰 지점")] Transform enemyPos;
    [SerializeField][Tooltip("1번 액션 심볼 위치")] Transform enemyActionPos;
    [Tooltip("액션 심볼 간격")] public static float actionMargin = 0.5f;
    [SerializeField][Tooltip("액션 심볼 소멸 지점")] Transform enemyExecutePos;
    [Header("Data")]
    [Tooltip("액션별 최대 실행값\n(예: 2일 경우 회전 액션은 최대 2칸 회전)")] public int maxActionVal;

    [Tooltip("액션 개수")] public int actionNum;
    public List<EnemyAction> actionList;
    public List<EnemyAction> executeActionList;
    static float actionInterval = 0.5f;
    public EnemyAction lastAction;

    public Enemy enemy;
    public int phaseNum;
    public int patternNum;
    public List<EnemyPattern> currentPattern;
    Action extendPattern;

    public void InitEnemy()
    {
        enemy = TurnManager.Inst.characterSO.enemy;
        TurnManager.Inst.enemyMaxHealth = enemy.health;
        TurnManager.Inst.enemyCurHealth = enemy.health;
        actionNum = enemy.actionNum;
        switch (enemy.passive)
        {
            case "Humanist":
                BuffManager.Inst.AddRouletteBuff(BuffManager.Inst.totalRouletteBuff_Attack, -3, 1, -1);
                break;
            case "Small Wings":
                actionNum++;
                extendPattern += () =>
                {
                    EnemyPattern ePat = new EnemyPattern(EEnemyActionType.Turn, 3);
                    var newActionObj = Instantiate(actionPrefab, enemyPos.position, Utils.QI);
                    newActionObj.transform.SetParent(enemyPos);
                    var newAction = newActionObj.GetComponent<EnemyAction>();
                    newAction.SetAction(ePat);
                    actionList.Add(newAction);
                };
                break;
            case "Sharp Teeth":
                BuffManager.Inst.AddRouletteBuff(BuffManager.Inst.totalRouletteBuff_Drain_Dmg, 1, 1, -1);
                BuffManager.Inst.AddEnemyBuff(BuffManager.Inst.enemyDrainBuff, 1, 1, -1);
                break;
        }
        TurnManager.Inst.enemyTriggerMaxCnt = enemy.triggerNum;
        TurnManager.Inst.enemyTriggerCnt = 0;
        phaseNum = 0;
        patternNum = 0;
        currentPattern = enemy.phase[0].patterns[0].pattern;

        switch (enemy.name)
        {
            case "Vampire Paul":
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if (TurnManager.Inst.turnNum % 2 == 0)
                    {
                        BuffManager.Inst.AddEnemyBuff(BuffManager.Inst.enemyDrainBuff, TurnManager.Inst.turnNum / 2, 1, 2);
                        BuffManager.Inst.AddRouletteBuff(BuffManager.Inst.totalRouletteBuff_Drain_Dmg, TurnManager.Inst.turnNum / 2, 1, 2);
                    }
                };

                RouletteItem rItem = new RouletteItem();
                rItem.type = ERouletteType.Drain;
                rItem.value = 5;
                RouletteManager.Inst.enemyTriggerPiece = rItem;
                break;
        }
    }

    public void CheckPhase()
    {
        if (enemy.phase[phaseNum].phaseClear)
        {
            phaseNum++;
            patternNum = 0;
            currentPattern = enemy.phase[phaseNum].patterns[0].pattern;
        }
    }

    // 액션 리스트 초기화. 랜덤한 액션을 actionNum 개수만큼 생성
    public void InitActionList()
    {
        currentPattern = enemy.phase[phaseNum].patterns[patternNum].pattern;
        patternNum = (patternNum + 1) % enemy.phase[phaseNum].patterns.Count;
        actionList.Clear();
        for (int i = 0; i < currentPattern.Count; i++)
        {
            var newActionObj = Instantiate(actionPrefab, enemyPos.position, Utils.QI);
            newActionObj.transform.SetParent(enemyPos);
            var newAction = newActionObj.GetComponent<EnemyAction>();

            newAction.SetAction(currentPattern[i]);

            actionList.Add(newAction);
        }
        extendPattern?.Invoke();
    }

    public void AllignActionList()
    {
        for (int i = 0; i < actionList.Count; i++)
        {
            var targetPos = enemyActionPos.position;
            targetPos.x += i * (actionPrefab.GetComponent<SpriteRenderer>().bounds.size.x + actionMargin);
            if (actionList[i].transform.position != targetPos)
            {
                actionList[i].transform.DOMove(targetPos, 0.7f);
            }
        }
    }

    // 적 트리거 발동.
    public void EnemyTriggerAction()
    {
        switch (enemy.name)
        {
            case "Vampire Paul":
                if (phaseNum == 0)
                {
                    phaseNum++;
                    EnemyAction.DrainEnchantAction();
                    EnemyAction.DrainEnchantAction();
                }
                else if (phaseNum == 1)
                {
                    RouletteManager.Inst.EnemyTriggerRoulette();
                }
                break;
        }
        TurnManager.Inst.enemyTriggerCnt = 0;
    }

    public void RemoveAction(int index)
    {
        if (index >= 0 && index < actionList.Count)
        {
            actionList[index].IgnoreAction(true);
        }
    }

    public void HideAction(int index)
    {
        if (index >= 0 && index < actionList.Count)
        {
            actionList[index].HideAction(true);
        }
    }

    public void RemoveAllSpin()
    {
        foreach (var action in actionList)
        {
            if (action.actionType == EEnemyActionType.Turn)
            {
                action.IgnoreAction(true);
            }
        }
    }

    public void ReverseSpin()
    {
        foreach (var action in actionList)
        {
            action.actionVal = -action.actionVal;
        }
    }

    // 적 최선의 행동 계산, executeActionList에 최적 행동 리스트 저장.
    public void GetBestAction()
    {
        List<int> turnActions = new List<int>();
        List<int> executeIdx = new List<int>();
        for (int i = 0; i < actionList.Count; i++)
        {
            if (actionList[i].actionType != EEnemyActionType.Turn)
            {
                executeIdx.Add(i);
            }
            else
            {
                turnActions.Add(i);
            }
        }
        Dictionary<int, List<int>> turnActionSet = new Dictionary<int, List<int>>();
        turnActionSet[0] = new List<int>();
        foreach (var action in turnActions)
        {
            int newTurn = actionList[action].actionVal;
            Dictionary<int, List<int>> tempTurnActionSet = new Dictionary<int, List<int>>();
            foreach (var actionSet in turnActionSet)
            {
                int newKey = actionSet.Key + newTurn;
                List<int> newSet = new List<int>(actionSet.Value);
                newSet.Add(action);
                if (tempTurnActionSet.ContainsKey(newKey) == false)
                {
                    tempTurnActionSet[newKey] = newSet;
                }
            }
            foreach (var actionSet in tempTurnActionSet)
            {
                turnActionSet[actionSet.Key] = actionSet.Value;
            }
        }
        List<int> bestTurnSequence = new List<int>();
        bool selectedFlag = false;
        foreach (var turnAction in turnActionSet)
        {
            int turnNum = turnAction.Key;
            Debug.Log(turnNum);
            List<int> turnSequence = turnAction.Value;
            RoulettePiece playerSlot_afterTurn = RouletteManager.Inst.roulettePieces[(RouletteManager.Inst.playerLookat - turnNum + RouletteManager.rouletteNum) % RouletteManager.rouletteNum];
            RoulettePiece enemySlot_afterTurn = RouletteManager.Inst.roulettePieces[(RouletteManager.Inst.enemyLookat - turnNum + RouletteManager.rouletteNum) % RouletteManager.rouletteNum];
            Debug.Log(playerSlot_afterTurn.roulette.type);

            // 우선순위 정책
            if (enemy.name == "Vampire Paul")
            {
                if (playerSlot_afterTurn.isTriggered == true)
                {
                    bestTurnSequence = turnSequence;
                    selectedFlag = true;
                    break;
                }
                if (playerSlot_afterTurn.roulette.type == ERouletteType.Drain)
                {
                    bestTurnSequence = turnSequence;
                    selectedFlag = true;
                    break;
                }
                if (enemySlot_afterTurn.roulette.type == ERouletteType.Shield)
                {
                    bestTurnSequence = turnSequence;
                    selectedFlag = true;
                    break;
                }
                if (playerSlot_afterTurn.roulette.type == ERouletteType.Attack)
                {
                    bestTurnSequence = turnSequence;
                    selectedFlag = true;
                    break;
                }
            }
        }
        if (selectedFlag == false)
        {
            List<int> keys = new List<int>(turnActionSet.Keys);
            int randKey = keys[Random.Range(0, keys.Count)];
            bestTurnSequence = turnActionSet[randKey];
        }
        executeIdx.AddRange(bestTurnSequence);
        executeIdx.Sort();

        foreach (int idx in executeIdx)
        {
            executeActionList.Add(actionList[idx]);
        }
    }

    // 적 최선의 행동 실행.
    public void ExecuteBestAction()
    {
        GetBestAction();
        Sequence executionSeq = DOTween.Sequence();
        for (int i = 0; i < executeActionList.Count; i++)
        {
            int localIndex = i;
            var originalPos = executeActionList[localIndex].transform.position;
            executionSeq.Append(executeActionList[localIndex].transform.DOMove(enemyExecutePos.position, actionInterval).OnComplete(() =>
            {
                lastAction = executeActionList[localIndex];
                executeActionList[localIndex].ExecuteAction();
            }));
            if (executeActionList[localIndex].isIgnore)
            {
                executionSeq.Append(executeActionList[localIndex].transform.DOMove(originalPos, RouletteManager.spinDelay));
            }
            else
            {
                executionSeq.AppendInterval(RouletteManager.spinDelay);
            }
        }
        executionSeq.AppendCallback(EndEnemyTurn);
        executionSeq.Play();
    }

    // 모든 액션 심볼 오브젝트 제거
    void DestroyAllActionObjects()
    {
        foreach (var obj in actionList)
        {
            Destroy(obj.gameObject);
        }
        actionList.Clear();
        executeActionList.Clear();
    }

    // 적 턴 시작
    public void StartEnemyTurn()
    {
        TurnManager.Inst.enemyShieldHealth = 0;
        TurnManager.OnEnemyTurnStart?.Invoke();
        ExecuteBestAction();
    }

    // 적 턴 종료
    public void EndEnemyTurn()
    {
        DestroyAllActionObjects();
        TurnManager.OnEnemyTurnEnd?.Invoke();
        RouletteManager.Inst.ActivateRoulette();
        if (GameManager.Inst.gameOverSignal == false)
        {
            TurnManager.Inst.StartPlayerTurn();
        }
    }

    private void Start()
    {
        TurnManager.OnPlayerTurnStart = InitActionList + TurnManager.OnPlayerTurnStart;
        TurnManager.OnPlayerTurnStart += AllignActionList;
    }

    private void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart = null;
    }

    private void Update()
    {
        CheckPhase();
    }
}
