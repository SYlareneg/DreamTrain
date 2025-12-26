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
    [Tooltip("액션 심볼 간격")] public static float actionMargin = 0.5f;
    [SerializeField][Tooltip("액션 심볼 스폰 지점")] Transform enemyPos;
    [SerializeField][Tooltip("1번 액션 심볼 위치")] Transform enemyActionPos;
    [SerializeField][Tooltip("액션 심볼 소멸 지점")] Transform enemyExecutePos;
    [SerializeField][Tooltip("서브 적 액션 심볼 스폰 지점")] Transform[] subEnemyPos;
    [SerializeField][Tooltip("서브 적 1번 액션 심볼 위치")] Transform[] subEnemyActionPos;
    [SerializeField][Tooltip("서브 적 액션 심볼 소멸 지점")] Transform[] subEnemyExecutePos;
    [Header("Data")]
    [Tooltip("액션별 최대 실행값\n(예: 2일 경우 회전 액션은 최대 2칸 회전)")] public int maxActionVal;

    [Tooltip("액션 개수")] public int actionNum;

    [SerializeField] GameObject actionBox;
    [SerializeField] GameObject enemyImg;
    [SerializeField] GameObject[] subEnemyActionBox;
    [SerializeField] GameObject[] subEnemyImg;
    [SerializeField] TMP_Text enemyName;
    public List<EnemyAction> actionList;
    public List<List<EnemyAction>> subEnemyActionList;
    public List<EnemyAction> executeActionList;
    static float actionInterval = 0.5f;
    public EnemyAction lastAction;

    public Enemy enemy;
    public List<SubEnemy> subEnemies;
    public int phaseNum;
    public List<int> phaseNum_SE;
    public int patternNum;
    public List<int> patternNum_SE;
    public bool isTriggerActivated;
    public int triggerPhaseNum;
    public int triggerPatternNum;
    public List<EnemyPattern> currentPattern;
    public List<List<EnemyPattern>> currentPattern_SE;
    Action extendPattern;
    [Header("적 특수룰렛")]
    public List<SpecialRoulette> enemySpecialRoulettes;
    public static Action<RoulettePiece, bool, int>[] enemySpecialRouletteActivation = new Action<RoulettePiece, bool, int>[Enemy.enemySpecialRouletteNum];
    [Header("적 특수행동")]
    public List<EnemySpecialAction> enemySpecialActions;
    public static Action<int>[] enemySpecialActivation = new Action<int>[Enemy.enemySpecialActionNum];
    [Header("하위 적 특수룰렛")]
    public List<SpecialRoulette[]> subEnemySpecialRoulettes;
    public static Action<RoulettePiece, bool, int>[,] subEnemySpecialRouletteActivation = new Action<RoulettePiece, bool, int>[Enemy.maxSubEnemyNum, SubEnemy.enemySpecialRouletteNum];
    [Header("하위 적 특수행동")]
    public List<EnemySpecialAction[]> subEnemySpecialActions;
    public static Action<int>[,] subEnemySpecialActivation = new Action<int>[Enemy.maxSubEnemyNum, SubEnemy.enemySpecialActionNum];


    public void InitEnemy()
    {
        enemy = new Enemy(TurnManager.Inst.enemySO.enemies.Find(x => x.name == TurnManager.Inst.characterSO.enemyName));
        if(enemy == null) return;
        TurnManager.Inst.enemyMaxHealth[0] = enemy.health;
        TurnManager.Inst.enemyCurHealth[0] = enemy.health;
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
        isTriggerActivated = false;
        triggerPhaseNum = 0;
        triggerPatternNum = 0;
        currentPattern = enemy.phase[0].patterns[0].pattern;
        foreach (var p in enemy.phase)
        {
            p.phaseClear = false;
        }
        enemyName.text = enemy.name;
        enemyImg.GetComponent<Tooltip>().tooltipTitle = enemy.phase[0].name;
        enemyImg.GetComponent<Tooltip>().tooltipTxt = enemy.phase[0].text;

        enemySpecialRoulettes = new List<SpecialRoulette>();
        for(int i = 0; i < enemy.enemySpecialRoulettes.Length; i++)
        {
            enemySpecialRoulettes.Add(new SpecialRoulette(enemy.enemySpecialRoulettes[i]));
        }
        
        enemySpecialActions = new List<EnemySpecialAction>();
        for(int i = 0; i < enemy.enemySpecialActions.Length; i++)
        {
            enemySpecialActions.Add(new EnemySpecialAction(enemy.enemySpecialActions[i]));
        }

        switch (enemy.name)
        {
            case "뱀파이어 폴":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0].Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0].Add(new List<Buff>());
                };
                enemySpecialRouletteActivation[0] = (rPiece, isEnemy, value) =>
                {
                    int trueDamage = 0;
                    if (isEnemy)
                    {
                        trueDamage = TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette);
                    }
                    else
                    {
                        trueDamage = TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                    }
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0][1], trueDamage);
                    TurnManager.Inst.EnemyTakeDmg(-totalVal_Heal, EDamageSource.Roulette);
                    if (totalVal_Heal > 0)
                    {
                        TurnManager.Inst.TriggerEnemyPassive(1);
                    }
                };
                enemySpecialActivation[0] = (value) =>
                {
                    int trueDamage = TurnManager.Inst.TakeDmg(value, EDamageSource.Enemy);
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0][1], trueDamage);
                    TurnManager.Inst.EnemyTakeDmg(-totalVal_Heal, EDamageSource.Enemy);
                    if (totalVal_Heal > 0)
                    {
                        TurnManager.Inst.TriggerEnemyPassive(1);
                    }
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if (TurnManager.Inst.turnNum % 2 == 0)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], TurnManager.Inst.turnNum / 2, 1, 2);
                        BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0][0], TurnManager.Inst.turnNum / 2, 1, 2);
                    }
                };

                RouletteItem rItem = new RouletteItem();
                rItem.rtype = new RouletteType(ERouletteType.Enemy_Special, 0);
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
                    enemySpecialRouletteActivation[0]?.Invoke(null, isEnemy, totalVal);
                };
                break;
            case "박쥐":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0].Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0].Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_EnemySpecial[0, 1].Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_EnemySpecial[0, 1].Add(new List<Buff>());
                };
                enemySpecialActivation[0] = (value) =>
                {
                    int trueDamage = TurnManager.Inst.TakeDmg(value, EDamageSource.Enemy);
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0][1], trueDamage);
                    TurnManager.Inst.EnemyTakeDmg(-totalVal_Heal, EDamageSource.Enemy);
                    if (totalVal_Heal > 0)
                    {
                        TurnManager.Inst.TriggerEnemyPassive(1);
                    }
                };
                enemySpecialActivation[1] = (value) =>
                {
                    int trueDamage = TurnManager.Inst.TakeDmg(value, EDamageSource.Enemy);
                    if(trueDamage == 0 && isTriggerActivated == true)
                    {
                        isTriggerActivated = false;
                    }
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_EnemySpecial[0, 1][1], trueDamage);
                    TurnManager.Inst.EnemyTakeDmg(-totalVal_Heal, EDamageSource.Enemy);
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if (TurnManager.Inst.turnNum % 2 == 0)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], TurnManager.Inst.turnNum / 2, 1, 2);
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 1], TurnManager.Inst.turnNum / 2, 1.5f, 2);
                    }
                };

                bool wasDamaged = false;
                TurnManager.OnEnemyDamaged += (x, s) =>
                {
                    if(s == EDamageSource.Roulette)
                    {
                        wasDamaged = true;
                    }
                };
                TurnManager.OnEnemyTurnEnd += () =>
                {
                    TurnManager.Inst.TriggerEnemyPassive(1);
                    if (wasDamaged) TurnManager.Inst.TriggerEnemyPassive(3);
                    wasDamaged = false;
                };
                break;
            case "비둘기":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0].Add(new List<Buff>());
                };
                enemySpecialRouletteActivation[0] = (rPiece, isEnemy, value) =>
                {
                    if (!isEnemy)
                    {
                        rPiece.RouletteClear();
                    }
                };
                int doveCnt = 0;
                TurnManager.OnRouletteEnchant += (index) =>
                {
                    int tempDoveCnt = 0;
                    for (int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if (RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Enemy_Special, 0))
                        {
                            tempDoveCnt++;
                        }
                    }
                    if (tempDoveCnt != doveCnt)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], (tempDoveCnt - doveCnt) * 5, 1, -1);
                        doveCnt = tempDoveCnt;
                    }
                };
                enemySpecialActivation[0] = (value) =>
                {
                    for (int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if (RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Enemy_Special, 0))
                        {
                            RouletteManager.Inst.roulettePieces[i].RouletteClear();
                        }
                    }
                    TurnManager.Inst.TakeDmg(value, EDamageSource.Enemy);
                };

                TurnManager.OnRouletteSpin += (isClockwise, spin) =>
                {
                    for (int i = 0; i <= spin; i++)
                    {
                        int tempIdx = (RouletteManager.Inst.enemyLookat + RouletteManager.rouletteNum + (isClockwise? -1 : 1) * i) % RouletteManager.rouletteNum;
                        if (RouletteManager.Inst.roulettePieces[tempIdx].roulette.rtype == new RouletteType(ERouletteType.Enemy_Special))
                        {
                            if(!isTriggerActivated) TurnManager.Inst.TriggerEnemyPassive(5);
                            TurnManager.Inst.GetShield(true, 5, EDamageSource.Enemy);
                        }
                    }
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if (patternNum == 3)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], TurnManager.Inst.turnNum, 1, 1);
                    }
                };
                break;
            case "망령 1":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0].Add(new List<Buff>());
                };
                enemySpecialRouletteActivation[0] = (rPiece, isEnemy, value) =>
                {
                    if (isEnemy)
                    {
                        TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette);
                    }
                    else
                    {
                        TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                    }
                };
                TurnManager.OnEnemyDamaged += (x, s) =>
                {
                    TurnManager.Inst.TriggerEnemyPassive(1);
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if (patternNum == 3)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], TurnManager.Inst.turnNum, 1, 1);
                    }
                };
                break;
            case "망령 2":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0].Add(new List<Buff>());
                };
                enemySpecialRouletteActivation[0] = (rPiece, isEnemy, value) =>
                {
                    if (isEnemy)
                    {
                        TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette);
                    }
                    else
                    {
                        TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                    }
                };
                enemySpecialActivation[0] = (value) =>
                {
                    EnemyAction.EnchantAction(new RouletteType(ERouletteType.Attack), 5);
                };
                enemySpecialActivation[1] = (value) =>
                {
                    BuffManager.Inst.AddShowBuff("회전 봉인", EBuffAffectType.Player, value, true);
                };
                TurnManager.OnRouletteSpin += (x, s) =>
                {
                    TurnManager.Inst.TriggerEnemyPassive(1);
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if (patternNum == 3)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], TurnManager.Inst.turnNum * 2, 1, 1);
                    }
                };
                break;
            case "마술사":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0].Add(new List<Buff>());
                };
                enemySpecialRouletteActivation[0] = (rPiece, isEnemy, value) =>
                {
                    if (!isEnemy)
                    {
                        rPiece.RouletteClear();
                    }
                };
                enemySpecialActivation[0] = (value) =>
                {
                    int tempMagicHat = 0;
                    for (int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if (RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Enemy_Special, 0))
                        {
                            tempMagicHat++;
                            RouletteManager.Inst.roulettePieces[i].RouletteClear();
                        }
                    }
                    if (tempMagicHat == 0)
                    {
                        TurnManager.Inst.TriggerEnemyPassive(1);
                    }
                    TurnManager.Inst.TakeDmg(value, EDamageSource.Enemy);
                };
                enemySpecialActivation[1] = (value) =>
                {
                    if (value == 0)
                    {
                        TurnManager.Inst.TriggerEnemyPassive(1);
                    }
                    else
                    {
                        BuffManager.Inst.AddShowBuff("환영", EBuffAffectType.Enemy, value, true);
                    }
                };
                enemySpecialActivation[2] = (value) =>
                {
                    for (int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if (RouletteManager.Inst.roulettePieces[i].roulette.rtype.type != ERouletteType.Attack && RouletteManager.Inst.roulettePieces[i].roulette.rtype.type != ERouletteType.Shield)
                        {
                            RouletteManager.Inst.EnchantRoulettePiece(i, new RouletteType(ERouletteType.Enemy_Special, 0), 1);
                        }
                    }
                };
                enemySpecialActivation[3] = (value) =>
                {
                    TurnManager.Inst.TakeDmg(value, EDamageSource.Enemy);
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], 2, 1, -1);
                    BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 3], 1, 1, -1);
                };
                int magicHat = 0;
                TurnManager.OnRouletteEnchant += (value) =>
                {
                    int tempMagicHat = 0;
                    for (int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if (RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Enemy_Special, 0))
                        {
                            tempMagicHat++;
                        }
                    }
                    if(tempMagicHat != magicHat)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], (tempMagicHat - magicHat) * 5, 1, -1);
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 1], tempMagicHat - magicHat, 1, -1);
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 3], tempMagicHat - magicHat, 1, -1);
                        magicHat = tempMagicHat;
                    }
                };
                break;
        }

        subEnemies = new List<SubEnemy>();
        subEnemyActionList = new List<List<EnemyAction>>();
        subEnemySpecialRoulettes = new List<SpecialRoulette[]>();
        subEnemySpecialActions = new List<EnemySpecialAction[]>();
        phaseNum_SE = new List<int>();
        patternNum_SE = new List<int>();
        currentPattern_SE = new List<List<EnemyPattern>>();
        for(int i = 0; i < enemy.subEnemies.Count; i++)
        {
            SubEnemy subEnemy = TurnManager.Inst.enemySO.subEnemies.Find(x => x.name == enemy.subEnemies[i]);
            if(subEnemy == null) continue;
            TurnManager.Inst.enemyMaxHealth[i + 1] = subEnemy.health;
            TurnManager.Inst.enemyCurHealth[i + 1] = subEnemy.health;
            InitSubEnemy(subEnemy);
            subEnemyImg[i].GetComponent<Tooltip>().tooltipTitle = subEnemy.phase[0].name;
            subEnemyImg[i].GetComponent<Tooltip>().tooltipTxt = subEnemy.phase[0].text;
        }
    }

    public void InitSubEnemy(SubEnemy subEnemy)
    {
        subEnemies.Add(new SubEnemy(subEnemy));

        phaseNum_SE.Add(0);
        patternNum_SE.Add(0);
        currentPattern_SE.Add(subEnemy.phase[0].patterns[0].pattern);
        foreach(var p in subEnemy.phase)
        {
            p.phaseClear = false;
        }
        subEnemyActionList.Add(new List<EnemyAction>());

        SpecialRoulette[] specialRoulettes = new SpecialRoulette[SubEnemy.enemySpecialRouletteNum];
        for(int i = 0; i < subEnemy.enemySpecialRoulettes.Length; i++)
        {
            specialRoulettes[i] = new SpecialRoulette(subEnemy.enemySpecialRoulettes[i]);
        }
        subEnemySpecialRoulettes.Add(specialRoulettes);

        EnemySpecialAction[] specialActions = new EnemySpecialAction[SubEnemy.enemySpecialActionNum];
        for(int i = 0; i < subEnemy.enemySpecialActions.Length; i++)
        {
            specialActions[i] = new EnemySpecialAction(subEnemy.enemySpecialActions[i]);
        }
        subEnemySpecialActions.Add(specialActions);

        switch(subEnemy.name)
        {
            default:
                break;
        }
    }

    public void CheckPhase()
    {
        // 메인 적 페이즈 전환
        if (!isTriggerActivated && enemy.phase[phaseNum].phaseClear)
        {
            phaseNum++;
            patternNum = 0;
            currentPattern = enemy.phase[phaseNum].patterns[0].pattern;
            enemyImg.GetComponent<Tooltip>().tooltipTitle = enemy.phase[phaseNum].name;
            enemyImg.GetComponent<Tooltip>().tooltipTxt = enemy.phase[phaseNum].text;
        }
        else if (isTriggerActivated && enemy.triggerPhase[triggerPhaseNum].phaseClear)
        {
            triggerPhaseNum++;
            triggerPatternNum = 0;
            currentPattern = enemy.triggerPhase[triggerPhaseNum].patterns[0].pattern;
            enemyImg.GetComponent<Tooltip>().tooltipTitle = enemy.triggerPhase[triggerPhaseNum].name;
            enemyImg.GetComponent<Tooltip>().tooltipTxt = enemy.triggerPhase[triggerPhaseNum].text;
        }

        // 서브 적 페이즈 전환
        for(int i = 0; i < subEnemies.Count; i++)
        {
            if (subEnemies[i].phase[phaseNum_SE[i]].phaseClear)
            {
                phaseNum_SE[i]++;
                patternNum_SE[i] = 0;
                currentPattern_SE[i] = subEnemies[i].phase[phaseNum_SE[i]].patterns[0].pattern;
            }
        }
    }

    // 액션 리스트 초기화. 랜덤한 액션을 actionNum 개수만큼 생성
    public void InitActionList()
    {
        // 메인 적 패턴 결정
        if (!isTriggerActivated)
        {
            currentPattern = enemy.phase[phaseNum].patterns[patternNum++].pattern;
            if(patternNum == enemy.phase[phaseNum].patterns.Count)
            {
                patternNum = 0;
                if (!enemy.phase[phaseNum].phaseRepeat)
                {
                    enemy.phase[phaseNum].phaseClear = true;
                    phaseNum++;
                    if(phaseNum >= enemy.phase.Count)
                    {
                        phaseNum = enemy.phase.Count - 1;
                    }
                }
            }
        }
        else
        {
            currentPattern = enemy.triggerPhase[triggerPhaseNum].patterns[triggerPatternNum++].pattern;
            if (triggerPatternNum == enemy.triggerPhase[triggerPhaseNum].patterns.Count)
            {
                triggerPatternNum = 0;
                if (!enemy.triggerPhase[triggerPhaseNum].phaseRepeat)
                {
                    triggerPhaseNum++;
                    if (triggerPhaseNum >= enemy.triggerPhase.Count)
                    {
                        triggerPhaseNum = 0;
                        isTriggerActivated = false;
                    }
                }
            }
        }
        // 서브 적 패턴 결정
        for(int i = 0; i < subEnemies.Count; i++)
        {
            currentPattern_SE[i] = subEnemies[i].phase[phaseNum_SE[i]].patterns[patternNum_SE[i]++].pattern;
            if(patternNum_SE[i] == subEnemies[i].phase[phaseNum_SE[i]].patterns.Count)
            {
                patternNum_SE[i] = 0;
                if(!subEnemies[i].phase[phaseNum_SE[i]].phaseRepeat)
                {
                    subEnemies[i].phase[phaseNum_SE[i]].phaseClear = true;
                    phaseNum_SE[i]++;
                    if(phaseNum_SE[i] >= subEnemies[i].phase.Count)
                    {
                        phaseNum_SE[i] = subEnemies[i].phase.Count - 1;
                    }
                }
            }
        }
        // 액션 리스트 초기화
        actionList.Clear();
        actionBox.SetActive(true);
        for(int i = 0; i < subEnemyActionList.Count; i++)
        {
            subEnemyActionList[i].Clear();
            subEnemyActionBox[i].SetActive(true);
        }
        // 메인 적 액션 생성
        for (int i = 0; i < currentPattern.Count; i++)
        {
            var newActionObj = Instantiate(actionPrefab, enemyPos.position, Utils.QI);
            newActionObj.transform.SetParent(enemyPos);
            var newAction = newActionObj.GetComponent<EnemyAction>();

            newAction.SetAction(currentPattern[i], 0);
            newAction.tooltipPos = enemyImg.GetComponent<Tooltip>().tooltipPos;

            actionList.Add(newAction);
        }
        extendPattern?.Invoke();
        // 서브 적 액션 생성
        for(int i = 0; i < subEnemies.Count; i++)
        {
            for (int j = 0; j < currentPattern_SE[i].Count; j++)
            {
                var newActionObj = Instantiate(actionPrefab, subEnemyPos[i].position, Utils.QI);
                newActionObj.transform.SetParent(subEnemyPos[i]);
                var newAction = newActionObj.GetComponent<EnemyAction>();

                newAction.SetAction(currentPattern_SE[i][j], i + 1);
                newAction.tooltipPos = subEnemyImg[i].GetComponent<Tooltip>().tooltipPos;

                subEnemyActionList[i].Add(newAction);
            }
        }
        // 특정 액션 타입 블로킹
        foreach(EEnemyActionType eat in Enum.GetValues(typeof(EEnemyActionType)))
        {
            if(BuffManager.Inst.enemyBuff_ActionBlock[eat] == true)
            {
                RemoveActionType(eat);
            }
        }
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

        for(int i = 0; i < subEnemyActionList.Count; i++)
        {
            for (int j = 0; j < subEnemyActionList[i].Count; j++)
            {
                var targetPos = subEnemyActionPos[i].position;
                targetPos.x += j * (actionPrefab.GetComponent<SpriteRenderer>().bounds.size.x + actionMargin);
                if (subEnemyActionList[i][j].transform.position != targetPos)
                {
                    subEnemyActionList[i][j].transform.DOMove(targetPos, 0.7f);
                    Vector3 actionPrefabWidthVec = new Vector3(actionPrefab.GetComponent<SpriteRenderer>().bounds.size.x + actionMargin, 0, 0);
                    float actionPrefabScreenWidth = Camera.main.WorldToScreenPoint(actionPrefabWidthVec).x - Camera.main.WorldToScreenPoint(Vector3.zero).x;
                    subEnemyActionList[i][j].tooltipPos.x += j * actionPrefabScreenWidth;
                }
            }
            Vector3 tempScale = subEnemyActionBox[i].transform.localScale;
            tempScale.x *= (actionPrefab.GetComponent<SpriteRenderer>().bounds.size.x + actionMargin) * subEnemyActionList[i].Count / subEnemyActionBox[i].GetComponent<SpriteRenderer>().bounds.size.x;
            subEnemyActionBox[i].transform.localScale = tempScale;
        }
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
                    EnemyAction.EnchantAction(new RouletteType(ERouletteType.Enemy_Special, 0), 5);
                    EnemyAction.EnchantAction(new RouletteType(ERouletteType.Enemy_Special, 0), 5);
                    TurnManager.Inst.enemyTriggerCnt = 0;
                }
                else if (isTriggerActivated == false && phaseNum == 1)
                {
                    isTriggerActivated = true;
                    triggerPhaseNum = 0;
                    triggerPatternNum = 0;
                    Action detrigger = null;
                    detrigger = () =>
                    {
                        if (!isTriggerActivated)
                        {
                            phaseNum = 1;
                            TurnManager.Inst.enemyTriggerCnt = 0;
                            TurnManager.OnEnemyTurnEnd -= detrigger;
                        }
                    };
                    TurnManager.OnEnemyTurnEnd += detrigger;
                }
                break;
            case "박쥐":
                if(isTriggerActivated == false && phaseNum == 0)
                {
                    isTriggerActivated = true;
                    triggerPhaseNum = 0;
                    triggerPatternNum = patternNum;
                    Action detrigger = null;
                    detrigger = () =>
                    {
                        if(!isTriggerActivated)
                        {
                            phaseNum = 0;
                            patternNum = triggerPatternNum;
                            TurnManager.Inst.enemyTriggerCnt = 0;
                            TurnManager.OnEnemyTurnEnd -= detrigger;
                        }
                    };
                    TurnManager.OnEnemyTurnEnd += detrigger;
                }
                break;
            case "비둘기":
                if(isTriggerActivated == false && phaseNum == 0)
                {
                    isTriggerActivated = true;
                    triggerPhaseNum = 0;
                    triggerPatternNum = 0;
                    Action detrigger = null;
                    detrigger = () =>
                    {
                        if (!isTriggerActivated)
                        {
                            phaseNum = 0;
                            TurnManager.Inst.enemyTriggerCnt = 0;
                            TurnManager.OnEnemyTurnEnd -= detrigger;
                        }
                    };
                    TurnManager.OnEnemyTurnEnd += detrigger;
                }
                break;
            case "망령 1":
                if(isTriggerActivated == false && phaseNum == 0)
                {
                    isTriggerActivated = true;
                    triggerPhaseNum = 0;
                    triggerPatternNum = 0;
                    Action detrigger = null;
                    detrigger = () =>
                    {
                        if (!isTriggerActivated)
                        {
                            phaseNum = 0;
                            TurnManager.Inst.enemyTriggerCnt = 0;
                            TurnManager.OnEnemyTurnEnd -= detrigger;
                        }
                    };
                    TurnManager.OnEnemyTurnEnd += detrigger;
                }
                break;
            case "망령 2":
                if(isTriggerActivated == false && phaseNum == 0)
                {
                    isTriggerActivated = true;
                    triggerPhaseNum = 0;
                    triggerPatternNum = 0;
                    Action detrigger = null;
                    detrigger = () =>
                    {
                        if (!isTriggerActivated)
                        {
                            phaseNum = 0;
                            TurnManager.Inst.enemyTriggerCnt = 0;
                            TurnManager.OnEnemyTurnEnd -= detrigger;
                        }
                    };
                    TurnManager.OnEnemyTurnEnd += detrigger;
                }
                break;
            case "마술사":
                if (isTriggerActivated == false && (phaseNum == 0 || phaseNum == 1))
                {
                    isTriggerActivated = true;
                    triggerPhaseNum = 0;
                    triggerPatternNum = 0;
                    Action detrigger = null;
                    detrigger = () =>
                    {
                        if (!isTriggerActivated)
                        {
                            phaseNum = 0;
                            TurnManager.Inst.enemyTriggerCnt = 0;
                            TurnManager.OnEnemyTurnEnd -= detrigger;
                        }
                    };
                    TurnManager.OnEnemyTurnEnd += detrigger;
                }
                break;
            default:
                if(isTriggerActivated == false && phaseNum == 0)
                {
                    isTriggerActivated = true;
                    triggerPhaseNum = 0;
                    triggerPatternNum = patternNum;
                    Action detrigger = null;
                    detrigger = () =>
                    {
                        if(!isTriggerActivated)
                        {
                            phaseNum = 0;
                            patternNum = triggerPatternNum;
                            TurnManager.Inst.enemyTriggerCnt = 0;
                            TurnManager.OnEnemyTurnEnd -= detrigger;
                        }
                    };
                    TurnManager.OnEnemyTurnEnd += detrigger;
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

    public void RemoveSubEnemyAction(int subEnemyIdx, int actionIdx)
    {
        if (actionIdx >= 0 && actionIdx < subEnemyActionList[subEnemyIdx].Count)
        {
            subEnemyActionList[subEnemyIdx][actionIdx].IgnoreAction(true);
        }
    }

    public void HideAction(int index)
    {
        if (index >= 0 && index < actionList.Count)
        {
            actionList[index].HideAction(true);
        }
    }

    public void HideSubEnemyAction(int subEnemyIdx, int actionIdx)
    {
        if (actionIdx >= 0 && actionIdx < subEnemyActionList[subEnemyIdx].Count)
        {
            subEnemyActionList[subEnemyIdx][actionIdx].HideAction(true);
        }
    }

    public void RemoveActionType(EEnemyActionType actionType)
    {
        foreach (var action in actionList)
        {
            if (action.actionType == actionType)
            {
                action.IgnoreAction(true);
            }
        }

        for(int i = 0; i < subEnemyActionList.Count; i++)
        {
            foreach (var action in subEnemyActionList[i])
            {
                if (action.actionType == actionType)
                {
                    action.IgnoreAction(true);
                }
            }
        }
    }

    public void ReverseSpin()
    {
        foreach (var action in actionList)
        {
            if(action.actionType == EEnemyActionType.Turn)
            {
                action.actionVal = -action.actionVal;
            }
        }

        for(int i = 0; i < subEnemyActionList.Count; i++)
        {
            foreach (var action in subEnemyActionList[i])
            {
                if(action.actionType == EEnemyActionType.Turn)
                {
                    action.actionVal = -action.actionVal;
                }
            }
        }
    }

    // 메인 적 최선의 행동 계산, executeActionList에 최적 행동 리스트 저장.
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
        List<(int priority, List<int> list)> prioritizedTurnActionList = new List<(int, List<int>)>();
        foreach (var turnAction in turnActionSet)
        {
            int turnNum = turnAction.Key;
            List<int> turnSequence = turnAction.Value;
            RoulettePiece playerSlot_afterTurn = RouletteManager.Inst.roulettePieces[(RouletteManager.Inst.playerLookat - turnNum + RouletteManager.rouletteNum) % RouletteManager.rouletteNum];
            RoulettePiece enemySlot_afterTurn = RouletteManager.Inst.roulettePieces[(RouletteManager.Inst.enemyLookat - turnNum + RouletteManager.rouletteNum) % RouletteManager.rouletteNum];

            prioritizedTurnActionList.Add(GetTurnActionPriority(playerSlot_afterTurn, enemySlot_afterTurn, turnSequence));
        }
        List<(int priority, List<int> list)> sortedTurnActionList = prioritizedTurnActionList.OrderBy(x => x.priority).ToList();
        bestTurnSequence = sortedTurnActionList[0].list;
        executeIdx.AddRange(bestTurnSequence);
        executeIdx.Sort();

        foreach (int idx in executeIdx)
        {
            executeActionList.Add(actionList[idx]);
        }
    }

    (int priority, List<int> list) GetTurnActionPriority(RoulettePiece playerSlot, RoulettePiece enemySlot, List<int> turnSequence)
    {
        switch(enemy.name)
        {
            case "뱀파이어 폴":
                if (playerSlot.roulette.rtype == new RouletteType(ERouletteType.Enemy_Special, 0)) return (0, turnSequence);
                if (enemySlot.roulette.rtype.type == ERouletteType.Shield) return (1, turnSequence);
                if (playerSlot.roulette.rtype.type == ERouletteType.Attack) return (2, turnSequence);
                return (3, turnSequence);
            case "망령 1":
                if (playerSlot.roulette.rtype.type == ERouletteType.Attack || playerSlot.roulette.rtype == new RouletteType(ERouletteType.Enemy_Special, 0)) return (0, turnSequence);
                if (enemySlot.roulette.rtype.type == ERouletteType.Shield) return (1, turnSequence);
                if (enemySlot.roulette.rtype.type == ERouletteType.Heal) return (2, turnSequence);
                return (3, turnSequence);
            default:
                if (playerSlot.roulette.rtype == new RouletteType(ERouletteType.Enemy_Special, 0)) return (0, turnSequence);
                if (playerSlot.roulette.rtype.type == ERouletteType.Attack) return (1, turnSequence);
                if (enemySlot.roulette.rtype.type == ERouletteType.Shield) return (2, turnSequence);
                if (enemySlot.roulette.rtype.type == ERouletteType.Attack) return (4, turnSequence);
                return (3, turnSequence);
        }
    }

    // 적 최선의 행동 실행.
    public void ExecuteBestAction()
    {
        GetBestAction();
        Sequence executionSeq = DOTween.Sequence();
        for(int i = 0; i < subEnemyActionList.Count; i++)
        {
            for(int j = 0; j < subEnemyActionList[i].Count; j++)
            {
                int localIndex_i = i;
                int localIndex_j = j;
                var originalPos = subEnemyActionList[i][j].transform.position;
                executionSeq.Append(subEnemyActionList[i][localIndex_j].transform.DOMove(subEnemyExecutePos[localIndex_i].position, actionInterval).OnComplete(() =>
                {
                    Debug.Log(subEnemyActionList[localIndex_i].Count);
                    Debug.Log(localIndex_j);
                    lastAction = subEnemyActionList[localIndex_i][localIndex_j];
                    subEnemyActionList[localIndex_i][localIndex_j].ExecuteAction();
                }));
                if (subEnemyActionList[localIndex_i][localIndex_j].isIgnore)
                {
                    executionSeq.Append(subEnemyActionList[localIndex_i][localIndex_j].transform.DOMove(originalPos, RouletteManager.spinDelay));
                }
                else
                {
                    executionSeq.AppendInterval(RouletteManager.spinDelay);
                }
            }
        }

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

        for(int i = 0; i < subEnemyActionList.Count; i++)
        {
            foreach (var obj in subEnemyActionList[i])
            {
                Destroy(obj.gameObject);
            }
            subEnemyActionList[i].Clear();
        }
    }

    // 적 턴 시작
    public void StartEnemyTurn()
    {
        for(int i = 0; i < Enemy.maxSubEnemyNum + 1; i++)
        {
            TurnManager.Inst.enemyShieldHealth[i] = 0;
        }
        Utils.AllignActions(ref TurnManager.OnEnemyTurnStart, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnEnemyTurnStart?.Invoke();
        ExecuteBestAction();
    }

    // 적 턴 종료
    public void EndEnemyTurn()
    {
        DestroyAllActionObjects();
        actionBox.SetActive(false);
        foreach(var box in subEnemyActionBox)
        {
            box.SetActive(false);
        }
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
        for(int i = 0; i < Enemy.enemySpecialRouletteNum; i++)
        {
            enemySpecialRouletteActivation[i] = null;
        }
        for(int i = 0; i < Enemy.enemySpecialActionNum; i++)
        {
            enemySpecialActivation[i] = null;
        }

        for(int i = 0; i < Enemy.maxSubEnemyNum; i++)
        {
            for(int j = 0; j < SubEnemy.enemySpecialRouletteNum; j++)
            {
                subEnemySpecialRouletteActivation[i, j] = null;
            }
            for(int j = 0; j < SubEnemy.enemySpecialActionNum; j++)
            {
                subEnemySpecialActivation[i, j] = null;
            }
        }
    }

    private void Update()
    {
        CheckPhase();
    }
}
