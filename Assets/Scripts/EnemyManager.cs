using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
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

    [SerializeField] GameObject actionBox;
    [SerializeField] GameObject enemyImg;
    [SerializeField] TMP_Text enemyName;
    public List<EnemyAction> actionList;
    public List<EnemyAction> executeActionList;
    static float actionInterval = 0.5f;
    public EnemyAction lastAction;

    public Enemy enemy;
    public int phaseNum;
    public int patternNum;
    public List<EnemyPattern> currentPattern;
    Action extendPattern;
    public static Action<RoulettePiece, bool, int> EnemySpecialRoulette1Activation;
    public static Action<RoulettePiece, bool, int> EnemySpecialRoulette2Activation;
    public static Action<int> EnemySpecial1Activation;
    public static Action<int> EnemySpecial2Activation;
    [Header("적 특수룰렛 1")]
    public Sprite EnemySpecialRoulette1Sprite;
    public string EnemySpecialRoulette1Title;
    public string EnemySpecialRoulette1Text;
    [Header("적 특수룰렛 2")]
    public Sprite EnemySpecialRoulette2Sprite;
    public string EnemySpecialRoulette2Title;
    public string EnemySpecialRoulette2Text;
    [Header("적 특수행동 1")]
    public Sprite EnemySpecial1Sprite;
    public string EnemySpecial1Title;
    public string EnemySpecial1Text;
    [Header("적 특수행동 2")]
    public Sprite EnemySpecial2Sprite;
    public string EnemySpecial2Title;
    public string EnemySpecial2Text;

    public void InitEnemy()
    {
        enemy = TurnManager.Inst.enemySO.enemies.Find(x => x.name == TurnManager.Inst.characterSO.enemyName);
        TurnManager.Inst.enemyMaxHealth = enemy.health;
        TurnManager.Inst.enemyCurHealth = enemy.health;
        actionNum = enemy.actionNum;
        /*switch (enemy.passive)
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
        }*/
        foreach(var relic in enemy.relics)
        {
            RelicManager.Inst.ActivateRelic(relic);
        }
        TurnManager.Inst.enemyTriggerMaxCnt = enemy.triggerNum;
        TurnManager.Inst.enemyTriggerCnt = 0;
        phaseNum = 0;
        patternNum = 0;
        currentPattern = enemy.phase[0].patterns[0].pattern;
        foreach (var p in enemy.phase)
        {
            p.phaseClear = false;
        }
        enemyName.text = enemy.name;
        enemyImg.GetComponent<Tooltip>().tooltipTitle = enemy.phase[0].name;
        enemyImg.GetComponent<Tooltip>().tooltipTxt = enemy.phase[0].text;

        EnemySpecialRoulette1Sprite = enemy.specialRoulette1Sprite;
        EnemySpecialRoulette1Title = enemy.specialRoulette1Title;
        EnemySpecialRoulette1Text = enemy.specialRoulette1Text;
        EnemySpecialRoulette2Sprite = enemy.specialRoulette2Sprite;
        EnemySpecialRoulette2Title = enemy.specialRoulette2Title;
        EnemySpecialRoulette2Text = enemy.specialRoulette2Text;
        
        EnemySpecial1Sprite = enemy.specialAction1Sprite;
        EnemySpecial1Title = enemy.specialAction1Title;
        EnemySpecial1Text = enemy.specialAction1Text;
        EnemySpecial2Sprite = enemy.specialAction2Sprite;
        EnemySpecial2Title = enemy.specialAction2Title;
        EnemySpecial2Text = enemy.specialAction2Text;

        switch (enemy.name)
        {
            case "뱀파이어 폴":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_EnemySpecial1.Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_EnemySpecial1.Add(new List<Buff>());
                };
                EnemySpecialRoulette1Activation = (rPiece, isEnemy, value) =>
                {
                    int trueDamage = 0;
                    if (isEnemy)
                    {
                        trueDamage = TurnManager.Inst.EnemyTakeDmg(value);
                    }
                    else
                    {
                        trueDamage = TurnManager.Inst.TakeDmg(value);
                    }
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_EnemySpecial1[1], trueDamage);
                    TurnManager.Inst.EnemyTakeDmg(-totalVal_Heal);
                    if (totalVal_Heal > 0)
                    {
                        TurnManager.Inst.TriggerEnemyPassive(1);
                    }
                };
                EnemySpecial1Activation = (value) =>
                {
                    int trueDamage = TurnManager.Inst.TakeDmg(value);
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_EnemySpecial1[1], trueDamage);
                    TurnManager.Inst.EnemyTakeDmg(-totalVal_Heal);
                    if (totalVal_Heal > 0)
                    {
                        TurnManager.Inst.TriggerEnemyPassive(1);
                    }
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if (TurnManager.Inst.turnNum % 2 == 0)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special_1, TurnManager.Inst.turnNum / 2, 1, 2);
                        BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_EnemySpecial1[0], TurnManager.Inst.turnNum / 2, 1, 2);
                    }
                };

                RouletteItem rItem = new RouletteItem();
                rItem.type = ERouletteType.Enemy_Special_1;
                rItem.value = 15;
                RouletteManager.Inst.enemyTriggerPiece = rItem;
                TurnManager.OnEnemyTrigger += () =>
                {
                    if(phaseNum == 1)
                    {
                        BuffManager.Inst.rouletteBuff_Trigger.Clear();
                        BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, -10, 3, -1);
                    }
                };
                RouletteManager.EnemyTriggerActivation = (isEnemy, totalVal) =>
                {
                    EnemySpecialRoulette1Activation?.Invoke(null, isEnemy, totalVal);
                };
                break;
            case "마술사":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_EnemySpecial1.Add(new List<Buff>());
                };
                EnemySpecialRoulette1Activation = (rPiece, isEnemy, value) =>
                {
                    if (!isEnemy)
                    {
                        RouletteItem rItem = new RouletteItem();
                        rItem.type = ERouletteType.None;
                        rItem.value = 0;
                        rPiece.Setup(rItem);
                    }
                };
                EnemySpecial1Activation = (value) =>
                {
                    int magicHat = 0;
                    for (int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Enemy_Special_1)
                        {
                            int val = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                            magicHat += val;
                            RouletteItem rItem = new RouletteItem();
                            rItem.type = ERouletteType.None;
                            rItem.value = 0;
                            RouletteManager.Inst.roulettePieces[i].Setup(rItem);
                        }
                    }
                    if (magicHat == 0)
                    {
                        TurnManager.Inst.TriggerEnemyPassive(1);
                    }
                    else
                    {
                        TurnManager.Inst.TakeDmg(magicHat * value);
                    }
                };
                EnemySpecial2Activation = (value) =>
                {
                    int magicHat = 0;
                    for (int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Enemy_Special_1)
                        {
                            int val = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                            magicHat += val;
                            RouletteItem rItem = new RouletteItem();
                            rItem.type = ERouletteType.None;
                            rItem.value = 0;
                            RouletteManager.Inst.roulettePieces[i].Setup(rItem);
                        }
                    }
                    if (magicHat == 0)
                    {
                        TurnManager.Inst.TriggerEnemyPassive(1);
                    }
                    else
                    {
                        BuffManager.Inst.AddShowBuff("환영", EBuffAffectType.Enemy, magicHat * value);
                    }
                };
                TurnManager.BeforePlayerTurnStart += () =>
                {
                    if (phaseNum == 2)
                    {
                        TurnManager.Inst.TriggerEnemyPassive(1);
                    }
                };

                rItem = new RouletteItem();
                rItem.type = ERouletteType.Attack;
                rItem.value = 20;
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
            enemyImg.GetComponent<Tooltip>().tooltipTitle = enemy.phase[phaseNum].name;
            enemyImg.GetComponent<Tooltip>().tooltipTxt = enemy.phase[phaseNum].text;
        }
    }

    // 액션 리스트 초기화. 랜덤한 액션을 actionNum 개수만큼 생성
    public void InitActionList()
    {
        currentPattern = enemy.phase[phaseNum].patterns[patternNum++].pattern;
        if(patternNum == enemy.phase[phaseNum].patterns.Count)
        {
            patternNum = 0;
            if (!enemy.phase[phaseNum].phaseRepeat)
            {
                phaseNum++;
            }
        }
        actionList.Clear();
        actionBox.SetActive(true);
        for (int i = 0; i < currentPattern.Count; i++)
        {
            var newActionObj = Instantiate(actionPrefab, enemyPos.position, Utils.QI);
            newActionObj.transform.SetParent(enemyPos);
            var newAction = newActionObj.GetComponent<EnemyAction>();

            newAction.SetAction(currentPattern[i]);
            newAction.tooltipPos = enemyImg.GetComponent<Tooltip>().tooltipPos;

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
                Vector3 actionPrefabWidthVec = new Vector3(actionPrefab.GetComponent<SpriteRenderer>().bounds.size.x + actionMargin, 0, 0);
                float actionPrefabScreenWidth = Camera.main.WorldToScreenPoint(actionPrefabWidthVec).x - Camera.main.WorldToScreenPoint(Vector3.zero).x;
                actionList[i].tooltipPos.x += i * actionPrefabScreenWidth;
            }
        }
        Vector3 newScale = actionBox.transform.localScale;
        newScale.x *= (actionPrefab.GetComponent<SpriteRenderer>().bounds.size.x + actionMargin) * actionList.Count / actionBox.GetComponent<SpriteRenderer>().bounds.size.x;
        actionBox.transform.localScale = newScale;
    }

    // 적 트리거 발동.
    public void EnemyTriggerAction()
    {
        switch (enemy.name)
        {
            case "뱀파이어 폴":
                if (phaseNum == 0)
                {
                    enemy.phase[0].phaseClear = true;
                    EnemyAction.EnchantAction(ERouletteType.Enemy_Special_1, 5);
                    EnemyAction.EnchantAction(ERouletteType.Enemy_Special_1, 5);
                }
                else if (phaseNum == 1)
                {
                    RouletteManager.Inst.EnemyTriggerRoulette();
                }
                break;
            case "마술사":
                if (phaseNum == 0 || phaseNum == 1)
                {
                    enemy.phase[0].phaseClear = true;
                    enemy.phase[1].phaseClear = true;
                }
                else if (phaseNum == 2)
                {
                    RouletteManager.Inst.EnemyTriggerRoulette();
                    phaseNum = 3;
                    patternNum = 0;
                    Action changePhase = null;
                    changePhase = () =>
                    {
                        if (phaseNum == 3 && RouletteManager.Inst.isEnemyTrigger() == false)
                        {
                            phaseNum = 2;
                            TurnManager.OnRouletteActivate -= changePhase;
                            TurnManager.OnRouletteTrigger -= changePhase;
                            TurnManager.OnRouletteEnchant -= changePhase;
                        }
                    };
                    TurnManager.OnRouletteActivate += changePhase;
                    TurnManager.OnRouletteTrigger += changePhase;
                    TurnManager.OnRouletteEnchant += changePhase;
                }
                break;
        }
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
            List<int> turnSequence = turnAction.Value;
            RoulettePiece playerSlot_afterTurn = RouletteManager.Inst.roulettePieces[(RouletteManager.Inst.playerLookat - turnNum + RouletteManager.rouletteNum) % RouletteManager.rouletteNum];
            RoulettePiece enemySlot_afterTurn = RouletteManager.Inst.roulettePieces[(RouletteManager.Inst.enemyLookat - turnNum + RouletteManager.rouletteNum) % RouletteManager.rouletteNum];

            // 우선순위 정책
            if (enemy.name == "뱀파이어 폴")
            {
                if (playerSlot_afterTurn.isTriggered == true)
                {
                    bestTurnSequence = turnSequence;
                    selectedFlag = true;
                    break;
                }
                if (playerSlot_afterTurn.roulette.type == ERouletteType.Enemy_Special_1)
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
        Utils.AllignActions(ref TurnManager.OnEnemyTurnStart, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnEnemyTurnStart?.Invoke();
        ExecuteBestAction();
    }

    // 적 턴 종료
    public void EndEnemyTurn()
    {
        DestroyAllActionObjects();
        actionBox.SetActive(false);
        Utils.AllignActions(ref TurnManager.OnEnemyTurnEnd, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnEnemyTurnEnd?.Invoke();
        RouletteManager.Inst.ActivateRoulette();
        if (GameManager.Inst.gameOverSignal == false)
        {
            TurnManager.Inst.StartPlayerTurn();
        }
    }

    private void Start()
    {
        TurnManager.OnPlayerTurnStart += InitActionList;
        TurnManager.OnPlayerTurnStart += AllignActionList;

        actionBox.SetActive(false);
    }

    private void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart = null;
        EnemySpecialRoulette1Activation = null;
        EnemySpecialRoulette2Activation = null;
    }

    private void Update()
    {
        CheckPhase();
    }
}
