using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Inst { get; private set; }
    void Awake()
    {
        Inst = this;

        subEnemyPos = new Transform[Enemy.maxSubEnemyNum];
        subEnemyActionPos = new Transform[Enemy.maxSubEnemyNum];
        subEnemyExecutePos = new Transform[Enemy.maxSubEnemyNum];
        subEnemyImg = new GameObject[Enemy.maxSubEnemyNum];

        subEnemies = new SubEnemy[Enemy.maxSubEnemyNum];
        subEnemyActionList = new List<EnemyAction>[Enemy.maxSubEnemyNum];
        subEnemySpecialRoulettes = new List<SpecialRoulette>[Enemy.maxSubEnemyNum];
        subEnemySpecialActions = new List<EnemySpecialAction>[Enemy.maxSubEnemyNum];
        phaseNum_SE = new int[Enemy.maxSubEnemyNum];
        patternNum_SE = new int[Enemy.maxSubEnemyNum];
        currentPattern_SE = new List<EnemyPattern>[Enemy.maxSubEnemyNum];
    }

    [Header("Prefabs")]
    [SerializeField][Tooltip("액션 심볼 Prefab")] GameObject actionPrefab;
    [SerializeField][Tooltip("액션 심볼 Prefab")] GameObject mainActionPrefab;
    [SerializeField][Tooltip("액션 심볼 Prefab")] GameObject subActionPrefab;
    [SerializeField][Tooltip("서브 적 Prefab")] GameObject subEnemyPrefab;
    [Header("Objects")]
    [Tooltip("메인 적 배경")] public GameObject mainEnemyRouletteBackground;
    [SerializeField][Tooltip("서브 적 캔버스")] Canvas subEnemyCanvas;
    [Tooltip("서브 적 룰렛 위치에 따른 캔버스 위치")] public int[] subEnemyCanvasPos_roulettePos = new int[Enemy.maxSubEnemyNum];
    [SerializeField][Tooltip("서브 적 룰렛 위치에 따른 캔버스 위치")] GameObject[] subEnemyCanvasPos_gameobject = new GameObject[Enemy.maxSubEnemyNum];
    [SerializeField][Tooltip("서브 적 룰렛 위치에 따른 룰렛 마커")] GameObject[] subEnemyCanvasPos_rouletteMarker = new GameObject[Enemy.maxSubEnemyNum];
    [Tooltip("서브 적 룰렛 위치에 따른 적 배경")] public GameObject[] subEnemyCanvasPos_enemyRouletteBackground = new GameObject[Enemy.maxSubEnemyNum];
    [Header("Positions")]
    [Tooltip("액션 심볼 간격")] public float actionMargin = -0.4f;
    [Tooltip("서브 액션 심볼 간격")] public float subActionMargin = -0.7f;
    [Tooltip("액션 심볼 스폰 지점")] public Transform enemyPos;
    [SerializeField][Tooltip("1번 액션 심볼 위치")] Transform enemyActionPos;
    [SerializeField][Tooltip("액션 심볼 소멸 지점")] Transform enemyExecutePos;
    [Tooltip("서브 적 액션 심볼 스폰 지점")] public Transform[] subEnemyPos;
    [SerializeField][Tooltip("서브 적 1번 액션 심볼 위치")] Transform[] subEnemyActionPos;
    [SerializeField][Tooltip("서브 적 액션 심볼 소멸 지점")] Transform[] subEnemyExecutePos;
    [Header("Data")]
    [Tooltip("액션별 최대 실행값\n(예: 2일 경우 회전 액션은 최대 2칸 회전)")] public int maxActionVal;

    public GameObject enemyImg;
    public GameObject[] subEnemyImg;
    [SerializeField] GameObject enemyThumbnail;
    [SerializeField] GameObject[] subEnemyThumbnail;
    [SerializeField] GameObject enemyTriggerBar;
    [SerializeField] TMP_Text enemyName;
    public List<EnemyAction> actionList;
    public List<EnemyAction>[] subEnemyActionList = new List<EnemyAction>[Enemy.maxSubEnemyNum];
    public List<EnemyAction> executeActionList;
    static float actionInterval = 0.5f;
    public EnemyAction lastAction;

    public Enemy enemy;
    public SubEnemy[] subEnemies = new SubEnemy[Enemy.maxSubEnemyNum];
    public int phaseNum;
    public float phaseScale;
    public int[] phaseNum_SE = new int[Enemy.maxSubEnemyNum];
    public float[] phaseScale_SE = new float[Enemy.maxSubEnemyNum];
    public int patternNum;
    public int[] patternNum_SE = new int[Enemy.maxSubEnemyNum];
    public bool isTriggerActivated;
    public int triggerPhaseNum;
    public float triggerPhaseScale;
    public int triggerPatternNum;
    public List<EnemyPattern> currentPattern;
    public List<EnemyPattern>[] currentPattern_SE = new List<EnemyPattern>[Enemy.maxSubEnemyNum];
    Action extendPattern;
    [Header("적 특수룰렛")]
    public List<SpecialRoulette> enemySpecialRoulettes;
    public static Action<RoulettePiece, bool, int, int, bool>[] enemySpecialRouletteActivation = new Action<RoulettePiece, bool, int, int, bool>[Enemy.enemySpecialRouletteNum];
    [Header("적 특수행동")]
    public List<EnemySpecialAction> enemySpecialActions;
    public static Action<int>[] enemySpecialActivation = new Action<int>[Enemy.enemySpecialActionNum];
    [Header("하위 적 특수룰렛")]
    public List<SpecialRoulette>[] subEnemySpecialRoulettes = new List<SpecialRoulette>[Enemy.maxSubEnemyNum];
    public static Action<RoulettePiece, bool, int, int, bool>[,] subEnemySpecialRouletteActivation = new Action<RoulettePiece, bool, int, int, bool>[Enemy.maxSubEnemyNum, SubEnemy.enemySpecialRouletteNum];
    [Header("하위 적 특수행동")]
    public List<EnemySpecialAction>[] subEnemySpecialActions = new List<EnemySpecialAction>[Enemy.maxSubEnemyNum];
    public static Action<int>[,] subEnemySpecialActivation = new Action<int>[Enemy.maxSubEnemyNum, SubEnemy.enemySpecialActionNum];

    public int FindEnemyIdxByPos(Transform enemyPos)
    {
        if(this.enemyPos == enemyPos) return 0;
        for(int i = 0; i < subEnemyPos.Length; i++)
        {
            if(subEnemyPos[i] == enemyPos) return i + 1;
        }
        return -1;
    }

    public void InitEnemy()
    {
        enemy = TurnManager.Inst.enemySO.enemies.Find(x => x.name == TurnManager.Inst.characterSO.enemyName);
        if(enemy == null) enemy = TurnManager.Inst.enemySO.enemies.Find(x => x.id == TurnManager.Inst.characterSO.enemyName);
        if(enemy == null) return;
        enemy = new Enemy(enemy);
        TurnManager.Inst.enemyMaxHealth[0] = enemy.health;
        TurnManager.Inst.enemyCurHealth[0] = enemy.health;
        TurnManager.Inst.enemyTriggerMaxCnt = enemy.triggerNum;
        TurnManager.Inst.enemyTriggerCnt = 0;
        phaseNum = 0;
        phaseScale = 1f;
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
        enemyImg.GetComponent<SpriteRenderer>().sprite = enemy.phase[0].sprite;
        enemyImg.GetComponent<Tooltip>().tooltipTitle = enemy.phase[0].name;
        enemyImg.GetComponent<Tooltip>().tooltipTxt = enemy.phase[0].text;
        enemyThumbnail.transform.Find("ImageMask/Image").GetComponent<Image>().sprite = enemy.phase[0].sprite;
        if(enemy.triggerNum == 0) enemyTriggerBar.SetActive(false);
        else enemyTriggerBar.SetActive(true);
        enemyTriggerBar.GetComponent<Tooltip>().tooltipTitle = enemy.phase[0].name;
        enemyTriggerBar.GetComponent<Tooltip>().tooltipTxt = enemy.phase[0].text;
        // enemyThumbnail.GetComponent<Tooltip>().tooltipTitle = enemy.phase[0].name;
        // enemyThumbnail.GetComponent<Tooltip>().tooltipTxt = enemy.phase[0].text;

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
                enemySpecialRouletteActivation[0] = (rPiece, isEnemy, value, enemyIdx, isEnhanced) =>
                {
                    int trueDamage = 0;
                    if (isEnemy)
                    {
                        trueDamage = TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette, enemyIdx);
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
                // damage scaling
                TurnManager.OnPlayerTurnStart += () =>
                {
                    phaseScale *= enemy.phase[phaseNum].scalingFactor;
                    BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], 0, Mathf.FloorToInt(phaseScale), 1);
                    BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0][0], 0, Mathf.FloorToInt(phaseScale), 1);
                    // if (TurnManager.Inst.turnNum % 2 == 0)
                    // {
                    //     BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], TurnManager.Inst.turnNum / 2, 1, 2);
                    //     BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0][0], TurnManager.Inst.turnNum / 2, 1, 2);
                    // }
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
                    phaseScale *= enemy.phase[phaseNum].scalingFactor;
                    BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], 0, Mathf.FloorToInt(phaseScale), 1);
                    BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 1], 0, Mathf.FloorToInt(phaseScale) * 1.5f, 2);
                    // if (TurnManager.Inst.turnNum % 2 == 0)
                    // {
                    //     BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], TurnManager.Inst.turnNum / 2, 1, 2);
                    //     BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 1], TurnManager.Inst.turnNum / 2, 1.5f, 2);
                    // }
                };

                bool wasDamaged = false;
                TurnManager.OnEnemyDamaged += (x, s, i) =>
                {
                    if(s == EDamageSource.Roulette && i == 0)
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
                enemySpecialRouletteActivation[0] = (rPiece, isEnemy, value, enemyIdx, isEnhanced) =>
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
                    phaseScale *= enemy.phase[phaseNum].scalingFactor;
                    BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], 0, Mathf.FloorToInt(phaseScale), 1);
                    // if (patternNum == 3)
                    // {
                    //     BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], TurnManager.Inst.turnNum, 1, 1);
                    // }
                };
                break;
            case "망령 1":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0].Add(new List<Buff>());
                };
                enemySpecialActivation[0] = (value) =>
                {
                    EnemyAction.EnchantAction(new RouletteType(ERouletteType.Attack), 5);
                };
                TurnManager.OnEnemyDamaged += (x, s, i) =>
                {
                    if(i == 0) TurnManager.Inst.TriggerEnemyPassive(1);
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    phaseScale *= enemy.phase[phaseNum].scalingFactor;
                    BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], 0, Mathf.FloorToInt(phaseScale), 1);
                    // if (patternNum == 3)
                    // {
                    //     BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], TurnManager.Inst.turnNum, 1, 1);
                    // }
                };
                break;
            case "망령 2":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0].Add(new List<Buff>());
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
                    phaseScale *= enemy.phase[phaseNum].scalingFactor;
                    BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], 0, Mathf.FloorToInt(phaseScale), 1);
                    // if (patternNum == 3)
                    // {
                    //     BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], TurnManager.Inst.turnNum * 2, 1, 1);
                    // }
                };
                break;
            case "마술사":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_EnemySpecial[0, 0].Add(new List<Buff>());
                };
                enemySpecialRouletteActivation[0] = (rPiece, isEnemy, value, enemyIdx, isEnhanced) =>
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
                    phaseScale *= enemy.phase[phaseNum].scalingFactor;
                    BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], 0, Mathf.FloorToInt(phaseScale) * 2, 1);
                    BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 3], 0, Mathf.FloorToInt(phaseScale), 1);
                    // BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], 2, 1, -1);
                    // BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 3], 1, 1, -1);
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
            case "귀신들린 인형":
                enemySpecialActivation[0] = (value) =>
                {
                    SubEnemy curseDoll = Array.Find(subEnemies, x => x != null && x.name == "저주 인형");
                    if(curseDoll == null)
                    {
                        TurnManager.Inst.EnemyTakeDmg(TurnManager.Inst.enemyCurHealth[0] + TurnManager.Inst.enemyShieldHealth[0] - 4, EDamageSource.Enemy);
                    }
                    else
                    {
                        int curseDollIdx = Array.FindIndex(subEnemies, x => x != null && x.name == "저주 인형");
                        TurnManager.Inst.EnemyTakeDmg(-TurnManager.Inst.enemyCurHealth[curseDollIdx], EDamageSource.Enemy);
                        DestroySubEnemy(curseDollIdx);
                    }
                };
                TurnManager.OnEnemyDamaged += (damage, source, enemyIdx) =>
                {
                    if(enemyIdx != 0) return;
                    int curseDoll = Array.FindIndex(subEnemies, x => x != null && x.name == "저주 인형");
                    if(curseDoll != -1)
                    {
                        TurnManager.Inst.TriggerEnemyPassive(1);
                        TurnManager.Inst.enemyShieldHealth[0] += damage;
                        TurnManager.Inst.EnemyTakeDmg(damage, source, curseDoll + 1);
                    }
                };
                TurnManager.OnSubEnemyDestroy += (subEnemyIdx) =>
                {
                    if(subEnemies[subEnemyIdx].name == "저주 인형")
                    {
                        ChangePhase(0, 0);
                    }
                };
                break;
            case "대장 컵":
                TurnManager.OnRouletteSpin += (isClockwise, pieces) =>
                {
                    if (isClockwise)
                    {
                        TurnManager.Inst.TriggerEnemyPassive(pieces);
                        TurnManager.Inst.GetShield(true, pieces, EDamageSource.Enemy);
                    }
                };
                break;
            case "흰 토끼":
                TurnManager.OnPlayerTurnStart += () =>
                {
                    TurnManager.Inst.TriggerEnemyPassive(1);
                    phaseScale *= enemy.phase[phaseNum].scalingFactor;
                    if(isTriggerActivated && triggerPatternNum == 0)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], 0, Mathf.FloorToInt(phaseScale), 1);
                    }
                    else if(!isTriggerActivated && patternNum == 0)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], 0, Mathf.FloorToInt(phaseScale), 1);
                    }
                    else if(!isTriggerActivated && patternNum == 2)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Shield[0], 0, Mathf.FloorToInt(phaseScale), 1);
                    }
                };
                break;
            case "말":
                enemySpecialActivation[0] = (value) =>
                {
                    TurnManager.Inst.TakeDmg(value, EDamageSource.Enemy);
                };
                TurnManager.OnEnemyAction += (enemyAction) =>
                {
                    if(enemyAction.actionType == EEnemyActionType.Shield)
                    {
                        TurnManager.Inst.TriggerEnemyPassive(1);
                    }
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if(isTriggerActivated && triggerPatternNum == 0)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], 0, Mathf.FloorToInt(phaseScale) * 3, 1);
                        if(TurnManager.Inst.shieldHealth > 0)
                        {
                            BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], 0, 0.5f, 1);
                        }
                    }
                    else if(!isTriggerActivated && patternNum == 0)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], 0, Mathf.FloorToInt(phaseScale), 1);
                    }
                    else if(!isTriggerActivated && patternNum == 2)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], 0, Mathf.FloorToInt(phaseScale), 1);
                    }
                };
                TurnManager.OnPlayerDamaged += (damage, source) =>
                {
                    if(TurnManager.Inst.shieldHealth > 0 && damage >= TurnManager.Inst.shieldHealth)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], 0, 2f, 1);
                    }
                };
                TurnManager.OnPlayerShielded += (shield, source) =>
                {
                    if(TurnManager.Inst.shieldHealth == shield && shield > 0)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], 0, 0.5f, 1);
                    }
                };
                break;
            case "식인 꽃":
                enemySpecialActivation[0] = (value) =>
                {
                    int trueDamage = TurnManager.Inst.TakeDmg(value, EDamageSource.Enemy);
                    TurnManager.Inst.EnemyTakeDmg(-trueDamage, EDamageSource.Enemy);
                    TurnManager.Inst.TriggerEnemyPassive(trueDamage);
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    phaseScale *= enemy.phase[phaseNum].scalingFactor;
                    if(!isTriggerActivated && patternNum == 0)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], 0, Mathf.FloorToInt(phaseScale), 1);
                    }
                    else if(!isTriggerActivated && patternNum == 1)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], 0, Mathf.FloorToInt(phaseScale) * 2, 1);
                    }
                    else if(!isTriggerActivated && patternNum == 3)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], 0, Mathf.FloorToInt(phaseScale) * 4, 1);
                    }
                };
                break;
            case "무서운 고양이":
                enemySpecialActivation[0] = (value) =>
                {
                    TurnManager.Inst.TakeDmg(value, EDamageSource.Enemy);
                };
                enemySpecialActivation[1] = (value) =>
                {
                    BuffManager.Inst.AddShowBuff("주저함", EBuffAffectType.Player, value, true);
                    BuffManager.Inst.AddShowBuff("주저함", EBuffAffectType.Roulette, value, true);
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if(isTriggerActivated && triggerPatternNum == 0)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], 0, Mathf.FloorToInt(phaseScale), 1);
                    }
                    else if(!isTriggerActivated && patternNum == 0)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], 0, Mathf.FloorToInt(phaseScale), 1);
                    }
                    else if(!isTriggerActivated && patternNum == 1)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Shield[0], 0, Mathf.FloorToInt(phaseScale), 1);
                    }
                    else if(!isTriggerActivated && patternNum == 2)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], 0, Mathf.FloorToInt(phaseScale), 1);
                    }
                };
                TurnManager.OnEnemyDamaged += (damage, source, enemyIdx) =>
                {
                    if(enemyIdx != 0) return;
                    if(source == EDamageSource.Roulette && damage > 0)
                    {
                        TurnManager.Inst.TriggerEnemyPassive(1);
                    }
                    Buff totalBuff = BuffManager.CalcTotalBuff(BuffManager.Inst.enemyBuff_Special[0, 0]);
                    int decreaseDamage = (totalBuff.add + 24 > damage)? damage : totalBuff.add + 24;
                    BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Special[0, 0], -decreaseDamage, 1, 1);
                };
                break;
            case "우는 와인":
                enemySpecialActivation[0] = (value) =>
                {
                    TurnManager.Inst.TakeDmg(value, EDamageSource.Enemy);
                    TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Enemy);
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    phaseScale *= enemy.phase[phaseNum].scalingFactor;
                    if (isTriggerActivated)
                    {
                        BuffManager.Inst.AddShowBuff("취약", EBuffAffectType.Enemy, 1, true);
                    }
                    else if(!isTriggerActivated && patternNum == 0)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Attack[0], 0, Mathf.FloorToInt(phaseScale), 1);
                    }
                };
                TurnManager.OnEnemyDamaged += (damage, source, enemyIdx) =>
                {
                    if(enemyIdx != 0) return;
                    if(damage > TurnManager.Inst.enemyShieldHealth[0])
                    {
                        TurnManager.Inst.TriggerEnemyPassive(damage - TurnManager.Inst.enemyShieldHealth[0]);
                    }
                };
                break;
            case "카드 병정 2":
                int decCost = 2;
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if (decCost > 0)
                    {
                        TurnManager.Inst.IncreaseCost(-TurnManager.Inst.turnNum);
                        decCost--;
                    }
                };
                break;
            case "카드 병정 3":
                TurnManager.OnPlayerTurnStart += () =>
                {
                    TurnManager.Inst.TriggerEnemyPassive(1);
                };
                break;
        }

        for(int i = 0; i < enemy.subEnemies.Count; i++)
        {
            SubEnemy subEnemy = TurnManager.Inst.enemySO.subEnemies.Find(x => x.name == enemy.subEnemies[i]);
            if(subEnemy == null || subEnemy.name == "") continue;
            InitSubEnemy(subEnemy);
        }
    }

    public void InitSubEnemy(SubEnemy subEnemy)
    {
        int subEnemyIdx = -1;
        for(int i = 0; i < Enemy.maxSubEnemyNum; i++)
        {
            if(subEnemies[i] == null || subEnemies[i].name == null)
            {
                subEnemies[i] = new SubEnemy(subEnemy);
                subEnemyIdx = i;
                break;
            }
        }
        if(subEnemyIdx == -1) return;
        TurnManager.Inst.enemyMaxHealth[subEnemyIdx + 1] = subEnemy.health;
        TurnManager.Inst.enemyCurHealth[subEnemyIdx + 1] = subEnemy.health;

        int tempIdx = Array.FindIndex(subEnemyCanvasPos_roulettePos, x => x == subEnemies[subEnemyIdx].roulettePos);
        if(tempIdx == -1) return;
        GameObject subEnemyObj = subEnemyCanvasPos_gameobject[tempIdx];
        subEnemyObj.transform.Find("SubEnemyUI/Values/Name").GetComponent<TMP_Text>().text = subEnemy.name;
        subEnemyObj.SetActive(true);
        subEnemyCanvasPos_rouletteMarker[tempIdx].SetActive(true);
        subEnemyCanvasPos_enemyRouletteBackground[tempIdx].SetActive(true);
        subEnemyPos[subEnemyIdx] = subEnemyObj.transform;
        subEnemyActionPos[subEnemyIdx] = subEnemyObj.transform.Find("EnemyActionPos");
        subEnemyExecutePos[subEnemyIdx] = subEnemyObj.transform.Find("EnemyExecutePos");
        subEnemyImg[subEnemyIdx] = subEnemyObj.transform.Find("EnemyImg").gameObject;
        subEnemyThumbnail[subEnemyIdx] = subEnemyObj.transform.Find("SubEnemyUI/EnemyThumbnail").gameObject;
        GameManager.Inst.SetSubEnemyUI(subEnemyIdx, subEnemyObj.transform);

        phaseNum_SE[subEnemyIdx] = 0;
        phaseScale_SE[subEnemyIdx] = 1f;
        patternNum_SE[subEnemyIdx] = 0;
        currentPattern_SE[subEnemyIdx] = subEnemy.phase[0].patterns[0].pattern;
        foreach(var p in subEnemies[subEnemyIdx].phase)
        {
            p.phaseClear = false;
        }
        subEnemyImg[subEnemyIdx].GetComponent<SpriteRenderer>().sprite = subEnemy.phase[0].sprite;
        subEnemyImg[subEnemyIdx].GetComponent<Tooltip>().tooltipTitle = subEnemy.phase[0].name;
        subEnemyImg[subEnemyIdx].GetComponent<Tooltip>().tooltipTxt = subEnemy.phase[0].text;
        subEnemyThumbnail[subEnemyIdx].transform.Find("ImageMask/Image").GetComponent<Image>().sprite = subEnemy.phase[0].sprite;
        // subEnemyThumbnail[subEnemyIdx].GetComponent<Tooltip>().tooltipTitle = subEnemy.phase[0].name;
        // subEnemyThumbnail[subEnemyIdx].GetComponent<Tooltip>().tooltipTxt = subEnemy.phase[0].text;
        subEnemyActionList[subEnemyIdx] = new List<EnemyAction>();

        List<SpecialRoulette> specialRoulettes = new List<SpecialRoulette>();
        for(int i = 0; i < subEnemy.enemySpecialRoulettes.Length; i++)
        {
            specialRoulettes.Add(new SpecialRoulette(subEnemy.enemySpecialRoulettes[i]));
        }
        subEnemySpecialRoulettes[subEnemyIdx] = specialRoulettes;

        List<EnemySpecialAction> specialActions = new List<EnemySpecialAction>();
        for(int i = 0; i < subEnemy.enemySpecialActions.Length; i++)
        {
            specialActions.Add(new EnemySpecialAction(subEnemy.enemySpecialActions[i]));
        }
        subEnemySpecialActions[subEnemyIdx] = specialActions;

        switch(subEnemy.name)
        {
            case "부하 컵 1":
            case "부하 컵 2":
                TurnManager.OnGameStart += () =>
                {
                    BuffManager.Inst.AddShowBuff("빙그르!", EBuffAffectType.Enemy, 5, true, null, subEnemyIdx + 1);
                };
                break;
            default:
                break;
        }
    }

    public void DestroySubEnemy(int subEnemyIdx)
    {
        Utils.AllignActions(ref TurnManager.OnSubEnemyDestroy, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnSubEnemyDestroy?.Invoke(subEnemyIdx);
        subEnemyCanvasPos_rouletteMarker[Array.FindIndex(subEnemyCanvasPos_roulettePos, x => x == subEnemies[subEnemyIdx].roulettePos)].SetActive(false);
        subEnemyCanvasPos_enemyRouletteBackground[Array.FindIndex(subEnemyCanvasPos_roulettePos, x => x == subEnemies[subEnemyIdx].roulettePos)].SetActive(false);
        subEnemies[subEnemyIdx] = null;
        subEnemyPos[subEnemyIdx].gameObject.SetActive(false);
        subEnemyPos[subEnemyIdx] = null;
        subEnemyActionPos[subEnemyIdx] = null;
        subEnemyExecutePos[subEnemyIdx] = null;
        subEnemyImg[subEnemyIdx] = null;
        GameManager.Inst.RemoveSubEnemyUI(subEnemyIdx);
        subEnemyActionList[subEnemyIdx] = new List<EnemyAction>();
        subEnemySpecialRoulettes[subEnemyIdx] = new List<SpecialRoulette>();
        subEnemySpecialActions[subEnemyIdx] = new List<EnemySpecialAction>();
    }

    public void CheckPhase()
    {
        // 메인 적 페이즈 전환
        if (!isTriggerActivated && enemy.phase[phaseNum].phaseClear)
        {
            phaseNum++;
            phaseScale = 1f;
            patternNum = 0;
            currentPattern = enemy.phase[phaseNum].patterns[0].pattern;
            enemyImg.GetComponent<SpriteRenderer>().sprite = enemy.phase[phaseNum].sprite;
            enemyImg.GetComponent<Tooltip>().tooltipTitle = enemy.phase[phaseNum].name;
            enemyImg.GetComponent<Tooltip>().tooltipTxt = enemy.phase[phaseNum].text;
            enemyThumbnail.transform.Find("ImageMask/Image").GetComponent<Image>().sprite = enemy.phase[phaseNum].sprite;
            enemyTriggerBar.GetComponent<Tooltip>().tooltipTitle = enemy.phase[phaseNum].name;
            enemyTriggerBar.GetComponent<Tooltip>().tooltipTxt = enemy.phase[phaseNum].text;
            // enemyThumbnail.GetComponent<Tooltip>().tooltipTitle = enemy.phase[phaseNum].name;
            // enemyThumbnail.GetComponent<Tooltip>().tooltipTxt = enemy.phase[phaseNum].text;
        }
        else if (isTriggerActivated && enemy.triggerPhase[triggerPhaseNum].phaseClear)
        {
            triggerPhaseNum++;
            triggerPatternNum = 0;
            currentPattern = enemy.triggerPhase[triggerPhaseNum].patterns[0].pattern;
            enemyImg.GetComponent<SpriteRenderer>().sprite = enemy.triggerPhase[triggerPhaseNum].sprite;
            enemyImg.GetComponent<Tooltip>().tooltipTitle = enemy.triggerPhase[triggerPhaseNum].name;
            enemyImg.GetComponent<Tooltip>().tooltipTxt = enemy.triggerPhase[triggerPhaseNum].text;
            enemyThumbnail.transform.Find("ImageMask/Image").GetComponent<Image>().sprite = enemy.triggerPhase[triggerPhaseNum].sprite;
            enemyTriggerBar.GetComponent<Tooltip>().tooltipTitle = enemy.triggerPhase[triggerPhaseNum].name;
            enemyTriggerBar.GetComponent<Tooltip>().tooltipTxt = enemy.triggerPhase[triggerPhaseNum].text;
            // enemyThumbnail.GetComponent<Tooltip>().tooltipTitle = enemy.triggerPhase[triggerPhaseNum].name;
            // enemyThumbnail.GetComponent<Tooltip>().tooltipTxt = enemy.triggerPhase[triggerPhaseNum].text;
        }

        // 서브 적 페이즈 전환
        for(int i = 0; i < subEnemies.Length; i++)
        {
            if(subEnemies[i] == null || subEnemies[i].name == null) continue;
            if (subEnemies[i].phase[phaseNum_SE[i]].phaseClear)
            {
                phaseNum_SE[i]++;
                phaseScale_SE[i] = 1f;
                patternNum_SE[i] = 0;
                currentPattern_SE[i] = subEnemies[i].phase[phaseNum_SE[i]].patterns[0].pattern;
                subEnemyImg[i].GetComponent<SpriteRenderer>().sprite = subEnemies[i].phase[phaseNum_SE[i]].sprite;
                subEnemyImg[i].GetComponent<Tooltip>().tooltipTitle = subEnemies[i].phase[phaseNum_SE[i]].name;
                subEnemyImg[i].GetComponent<Tooltip>().tooltipTxt = subEnemies[i].phase[phaseNum_SE[i]].text;
                subEnemyThumbnail[i].transform.Find("ImageMask/Image").GetComponent<Image>().sprite = subEnemies[i].phase[phaseNum_SE[i]].sprite;
                // subEnemyThumbnail[i].GetComponent<Tooltip>().tooltipTitle = subEnemies[i].phase[phaseNum_SE[i]].name;
                // subEnemyThumbnail[i].GetComponent<Tooltip>().tooltipTxt = subEnemies[i].phase[phaseNum_SE[i]].text;
            }
        }
    }

    public void ChangePhase(int phaseNum, int patternNum)
    {
        if(this.phaseNum != phaseNum)
        {
            phaseScale = 1f;
        }
        this.phaseNum = phaseNum;
        this.patternNum = patternNum;
        currentPattern = enemy.phase[phaseNum].patterns[patternNum].pattern;
        enemy.phase[phaseNum].phaseClear = false;
        enemyImg.GetComponent<SpriteRenderer>().sprite = enemy.phase[phaseNum].sprite;
        enemyImg.GetComponent<Tooltip>().tooltipTitle = enemy.phase[phaseNum].name;
        enemyImg.GetComponent<Tooltip>().tooltipTxt = enemy.phase[phaseNum].text;
        enemyThumbnail.transform.Find("ImageMask/Image").GetComponent<Image>().sprite = enemy.phase[phaseNum].sprite;
        enemyTriggerBar.GetComponent<Tooltip>().tooltipTitle = enemy.phase[phaseNum].name;
        enemyTriggerBar.GetComponent<Tooltip>().tooltipTxt = enemy.phase[phaseNum].text;
        // enemyThumbnail.GetComponent<Tooltip>().tooltipTitle = enemy.phase[phaseNum].name;
        // enemyThumbnail.GetComponent<Tooltip>().tooltipTxt = enemy.phase[phaseNum].text;
    }

    // 액션 리스트 초기화. 랜덤한 액션을 actionNum 개수만큼 생성
    public void InitActionList()
    {
        // 메인 적 패턴 결정
        if (!isTriggerActivated)
        {
            enemyImg.GetComponent<SpriteRenderer>().sprite = enemy.phase[phaseNum].sprite;
            enemyImg.GetComponent<Tooltip>().tooltipTitle = enemy.phase[phaseNum].name;
            enemyImg.GetComponent<Tooltip>().tooltipTxt = enemy.phase[phaseNum].text;
            enemyThumbnail.transform.Find("ImageMask/Image").GetComponent<Image>().sprite = enemy.phase[phaseNum].sprite;
            enemyTriggerBar.GetComponent<Tooltip>().tooltipTitle = enemy.phase[phaseNum].name;
            enemyTriggerBar.GetComponent<Tooltip>().tooltipTxt = enemy.phase[phaseNum].text;
            currentPattern = enemy.phase[phaseNum].patterns[patternNum++].pattern;
            if(patternNum == enemy.phase[phaseNum].patterns.Count)
            {
                patternNum = 0;
                if (!enemy.phase[phaseNum].phaseRepeat)
                {
                    enemy.phase[phaseNum].phaseClear = true;
                    phaseNum++;
                    phaseScale = 1f;
                    if(phaseNum >= enemy.phase.Count)
                    {
                        phaseNum = enemy.phase.Count - 1;
                    }
                }
            }
        }
        else
        {
            enemyImg.GetComponent<SpriteRenderer>().sprite = enemy.triggerPhase[triggerPhaseNum].sprite;
            enemyImg.GetComponent<Tooltip>().tooltipTitle = enemy.triggerPhase[triggerPhaseNum].name;
            enemyImg.GetComponent<Tooltip>().tooltipTxt = enemy.triggerPhase[triggerPhaseNum].text;
            enemyThumbnail.transform.Find("ImageMask/Image").GetComponent<Image>().sprite = enemy.triggerPhase[triggerPhaseNum].sprite;
            enemyTriggerBar.GetComponent<Tooltip>().tooltipTitle = enemy.triggerPhase[triggerPhaseNum].name;
            enemyTriggerBar.GetComponent<Tooltip>().tooltipTxt = enemy.triggerPhase[triggerPhaseNum].text;
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
        for(int i = 0; i < subEnemies.Length; i++)
        {
            if(subEnemies[i] == null || subEnemies[i].name == null) continue;
            currentPattern_SE[i] = subEnemies[i].phase[phaseNum_SE[i]].patterns[patternNum_SE[i]++].pattern;
            if(patternNum_SE[i] == subEnemies[i].phase[phaseNum_SE[i]].patterns.Count)
            {
                patternNum_SE[i] = 0;
                if(!subEnemies[i].phase[phaseNum_SE[i]].phaseRepeat)
                {
                    subEnemies[i].phase[phaseNum_SE[i]].phaseClear = true;
                    phaseNum_SE[i]++;
                    phaseScale_SE[i] = 1f;
                    if(phaseNum_SE[i] >= subEnemies[i].phase.Count)
                    {
                        phaseNum_SE[i] = subEnemies[i].phase.Count - 1;
                    }
                }
            }
        }
        // 액션 리스트 초기화
        actionList.Clear();
        for(int i = 0; i < subEnemies.Length; i++)
        {
            if(subEnemies[i] == null || subEnemies[i].name == null) continue;
            subEnemyActionList[i].Clear();
        }
        // 메인 적 액션 생성
        for (int i = 0; i < currentPattern.Count; i++)
        {
            var newActionObj = Instantiate(mainActionPrefab, enemyThumbnail.transform, false);
            newActionObj.transform.SetParent(enemyPos);
            newActionObj.transform.localScale = mainActionPrefab.transform.localScale;
            var newAction = newActionObj.GetComponent<EnemyAction>();

            newAction.SetAction(currentPattern[i], 0);
            newAction.tooltipPos = Camera.main.WorldToScreenPoint(enemyActionPos.position) - Camera.main.WorldToScreenPoint(Vector3.zero);

            actionList.Add(newAction);
        }
        extendPattern?.Invoke();
        // 서브 적 액션 생성
        for(int i = 0; i < subEnemies.Length; i++)
        {
            if(subEnemies[i] == null || subEnemies[i].name == null) continue;
            for (int j = 0; j < currentPattern_SE[i].Count; j++)
            {
                var newActionObj = Instantiate(subActionPrefab, subEnemyThumbnail[i].transform, false);
                newActionObj.transform.SetParent(subEnemyPos[i]);
                newActionObj.transform.localScale = subActionPrefab.transform.localScale;
                var newAction = newActionObj.GetComponent<EnemyAction>();

                newAction.SetAction(currentPattern_SE[i][j], i + 1);
                newAction.tooltipPos = Camera.main.WorldToScreenPoint(subEnemyActionPos[i].position) - Camera.main.WorldToScreenPoint(Vector3.zero);

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
        Vector3 mainActionPrefabWidthVec = new Vector3(mainActionPrefab.transform.localScale.x / 2 + actionMargin, 0, 0);
        float mainActionPrefabScreenWidth = Camera.main.WorldToScreenPoint(mainActionPrefabWidthVec).x - Camera.main.WorldToScreenPoint(Vector3.zero).x;
        for (int i = 0; i < actionList.Count; i++)
        {
            var targetPos = enemyActionPos.position;
            targetPos.x += i * mainActionPrefabWidthVec.x;
            if (actionList[i].transform.position != targetPos)
            {
                actionList[i].transform.DOMove(targetPos, 0.7f);
                actionList[i].tooltipPos.x += i * mainActionPrefabScreenWidth;
            }
        }

        Vector3 subActionPrefabWidthVec = new Vector3(subActionPrefab.transform.localScale.x / 2 + subActionMargin, 0, 0);
        float subActionPrefabScreenWidth = Camera.main.WorldToScreenPoint(subActionPrefabWidthVec).x - Camera.main.WorldToScreenPoint(Vector3.zero).x;

        for(int i = 0; i < subEnemies.Length; i++)
        {
            if(subEnemies[i] == null || subEnemies[i].name == null) continue;
            for(int j = 0; j < subEnemyActionList[i].Count; j++)
            {
                var targetPos = subEnemyActionPos[i].position;
                targetPos.x += j * (subActionPrefab.transform.localScale.x / 2 + subActionMargin);
                if (subEnemyActionList[i][j].transform.position != targetPos)
                {
                    subEnemyActionList[i][j].transform.DOMove(targetPos, 0.7f);
                    subEnemyActionList[i][j].tooltipPos.x += j * subActionPrefabScreenWidth;
                }
            }
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
            case "귀신들린 인형":
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
            case "대장 컵":
                int triggerSpinCount = 2;
                Action randomSpin = null;
                randomSpin = () =>
                {
                    RouletteManager.Inst.Spin(true, Random.Range(1, 13));
                    triggerSpinCount--;
                    if (triggerSpinCount == 0)
                    {
                        TurnManager.Inst.enemyTriggerCnt = 0;
                        TurnManager.OnPlayerTurnStart -= randomSpin;
                    }
                };
                TurnManager.OnPlayerTurnStart += randomSpin;
                break;
            case "흰 토끼":
                if(isTriggerActivated == false && phaseNum == 0)
                {
                    isTriggerActivated = true;
                    triggerPhaseNum = 0;
                    triggerPatternNum = 0;
                    Action detrigger = null;
                    detrigger = () =>
                    {
                        if(!isTriggerActivated)
                        {
                            phaseNum = 0;
                            TurnManager.Inst.enemyTriggerCnt = 0;
                            TurnManager.OnEnemyTurnEnd -= detrigger;
                        }
                    };
                    TurnManager.OnEnemyTurnEnd += detrigger;
                }
                break;
            case "말":
                if(isTriggerActivated == false && phaseNum == 0)
                {
                    isTriggerActivated = true;
                    triggerPhaseNum = 0;
                    triggerPatternNum = 0;
                    Action detrigger = null;
                    detrigger = () =>
                    {
                        if(!isTriggerActivated)
                        {
                            phaseNum = 0;
                            TurnManager.Inst.enemyTriggerCnt = 0;
                            TurnManager.OnEnemyTurnEnd -= detrigger;
                        }
                    };
                    TurnManager.OnEnemyTurnEnd += detrigger;
                }
                break;
            case "식인 꽃":
                if(isTriggerActivated == false && phaseNum == 0)
                {
                    isTriggerActivated = true;
                    triggerPhaseNum = 0;
                    triggerPatternNum = 0;
                    Action detrigger = null;
                    detrigger = () =>
                    {
                        if(!isTriggerActivated)
                        {
                            phaseNum = 0;
                            TurnManager.Inst.enemyTriggerCnt = 0;
                            TurnManager.OnEnemyTurnEnd -= detrigger;
                        }
                    };
                    TurnManager.OnEnemyTurnEnd += detrigger;
                }
                break;
            case "무서운 고양이":
                if(isTriggerActivated == false && phaseNum == 0)
                {
                    isTriggerActivated = true;
                    triggerPhaseNum = 0;
                    triggerPatternNum = 0;
                    Action detrigger = null;
                    detrigger = () =>
                    {
                        if(!isTriggerActivated)
                        {
                            phaseNum = 0;
                            TurnManager.Inst.enemyTriggerCnt = 0;
                            TurnManager.OnEnemyTurnEnd -= detrigger;
                        }
                    };
                    TurnManager.OnEnemyTurnEnd += detrigger;
                }
                break;
            case "우는 와인":
                if(isTriggerActivated == false && phaseNum == 0)
                {
                    isTriggerActivated = true;
                    triggerPhaseNum = 0;
                    triggerPatternNum = 0;
                    Action detrigger = null;
                    detrigger = () =>
                    {
                        if(!isTriggerActivated)
                        {
                            phaseNum = 0;
                            TurnManager.Inst.enemyTriggerCnt = 0;
                            TurnManager.OnEnemyTurnEnd -= detrigger;
                        }
                    };
                    TurnManager.OnEnemyTurnEnd += detrigger;
                }
                break;
            case "카드 병정 3":
                if(isTriggerActivated == false && phaseNum == 0)
                {
                    isTriggerActivated = true;
                    triggerPhaseNum = 0;
                    triggerPatternNum = 0;
                    Action detrigger = null;
                    detrigger = () =>
                    {
                        if(!isTriggerActivated)
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
        if (subEnemies[subEnemyIdx] == null || subEnemies[subEnemyIdx].name == null) return;
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

        for(int i = 0; i < subEnemies.Length; i++)
        {
            if(subEnemies[i] == null || subEnemies[i].name == null) continue;
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

        for(int i = 0; i < subEnemies.Length; i++)
        {
            if(subEnemies[i] == null || subEnemies[i].name == null) continue;
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
            if(actionList[i].isIgnore) continue;
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
        // 메인 적 액션 실행
        for (int i = 0; i < executeActionList.Count; i++)
        {
            int localIndex = i;
            var originalPos = executeActionList[localIndex].transform.position;
            var originalScale = executeActionList[localIndex].transform.localScale;
            var originalColor = new Color(120f / 255f, 120f / 255f, 120f / 255f);
            Sequence executionSubSeq = DOTween.Sequence();
            executionSubSeq.Append(executeActionList[localIndex].transform.DOScale(originalScale * 1.2f, actionInterval / 2));
            var subSeqSR = executeActionList[localIndex].GetComponentsInChildren<SpriteRenderer>();
            foreach (var sr in subSeqSR)
            {
                executionSubSeq.Join(sr.DOColor(Color.white, actionInterval / 2));
            }
            executionSubSeq.AppendCallback(() =>
            {
                lastAction = executeActionList[localIndex];
                executeActionList[localIndex].ExecuteAction();
            });
            executionSubSeq.AppendInterval(RouletteManager.spinDelay);
            executionSubSeq.Append(executeActionList[localIndex].transform.DOScale(originalScale, actionInterval / 2));
            foreach (var sr in subSeqSR)
            {
                if (executeActionList[localIndex].isIgnore)
                {
                    executionSubSeq.Join(sr.DOColor(new Color(1, 1, 1, 0), actionInterval / 2));
                }
                else
                {
                    executionSubSeq.Join(sr.DOColor(originalColor, actionInterval / 2));
                }
            }
            executionSeq.Append(executionSubSeq);
        }
        // 서브 적 액션 실행
        List<(SubEnemy SE, int enemyIdx)> sortedSubEnemies = new List<(SubEnemy, int)>();
        for(int i = 0; i < subEnemies.Length; i++)
        {
            if(subEnemies[i] == null || subEnemies[i].name == null) continue;
            sortedSubEnemies.Add((subEnemies[i], i));
        }
        sortedSubEnemies.OrderBy(x => x.SE.roulettePos).ToList();
        for(int i = 0; i < sortedSubEnemies.Count; i++)
        {
            for(int j = 0; j < subEnemyActionList[sortedSubEnemies[i].enemyIdx].Count; j++)
            {
                if(subEnemyActionList[sortedSubEnemies[i].enemyIdx][j].isIgnore) continue;
                int localIndex_i = sortedSubEnemies[i].enemyIdx;
                int localIndex_j = j;
                var originalScale = subEnemyActionList[sortedSubEnemies[i].enemyIdx][j].transform.localScale;
                var originalColor = new Color(120f/255f, 120f/255f, 120f/255f);
                Sequence executionSubSeq = DOTween.Sequence();
                executionSubSeq.Append(subEnemyActionList[localIndex_i][localIndex_j].transform.DOScale(originalScale * 1.2f, actionInterval / 2));
                var subSeqSR = subEnemyActionList[localIndex_i][localIndex_j].GetComponentsInChildren<SpriteRenderer>();
                foreach(var sr in subSeqSR)
                {
                    executionSubSeq.Join(sr.DOColor(Color.white, actionInterval / 2));
                }
                executionSubSeq.AppendCallback(() =>
                {
                    lastAction = subEnemyActionList[localIndex_i][localIndex_j];
                    subEnemyActionList[localIndex_i][localIndex_j].ExecuteAction();
                });
                executionSubSeq.AppendInterval(RouletteManager.spinDelay);
                executionSubSeq.Append(subEnemyActionList[localIndex_i][localIndex_j].transform.DOScale(originalScale, actionInterval / 2));
                foreach (var sr in subSeqSR)
                {
                    if (subEnemyActionList[localIndex_i][localIndex_j].isIgnore)
                    {
                        executionSubSeq.Join(sr.DOColor(new Color(1, 1, 1, 0), actionInterval / 2));
                    }
                    else
                    {
                        executionSubSeq.Join(sr.DOColor(originalColor, actionInterval / 2));
                    }
                }
                executionSeq.Append(executionSubSeq);
                
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

        for(int i = 0; i < subEnemies.Length; i++)
        {
            if(subEnemies[i] == null || subEnemies[i].name == null) continue;
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
        for(int i = 0; i < subEnemies.Length; i++)
        {
            if(subEnemies[i] == null || subEnemies[i].name == null) continue;
        }
        Utils.AllignActions(ref TurnManager.OnEnemyTurnEnd, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnEnemyTurnEnd?.Invoke();
        // RouletteManager.Inst.ActivateRoulette();
        if (GameManager.Inst.gameOverSignal == false)
        {
            TurnManager.Inst.StartPlayerTurn();
        }
    }

    Sequence enemyDamageSeq;

    private void Start()
    {
        TurnManager.OnPlayerTurnStart += InitActionList;
        TurnManager.OnPlayerTurnStart += AllignActionList;

        // TurnManager.OnEnemyDamaged += (damage, source, enemyIdx) =>
        // {
        //     Transform enemyDamageIcon = null;
        //     if (enemyIdx == 0)
        //     {
        //         enemyDamageIcon = enemyImg.transform.Find("DamageIcon");
        //     }
        //     else
        //     {
        //         enemyDamageIcon = subEnemyImg[enemyIdx - 1].transform.Find("DamageIcon");
        //     }
        //     if (enemyDamageIcon != null)
        //     {
        //         var enemyDamageFilter = enemyImg.transform.Find("DamageFilter");
        //         if(enemyDamageSeq != null && enemyDamageSeq.IsActive())
        //         {
        //             enemyDamageSeq.Kill();
        //             enemyDamageIcon.gameObject.SetActive(false);
        //             enemyDamageFilter?.gameObject.SetActive(false);
        //         }
        //         enemyDamageIcon.gameObject.SetActive(true);
        //         enemyDamageSeq = DOTween.Sequence();
        //         enemyDamageSeq.AppendInterval(0.7f);
        //         enemyDamageSeq.AppendCallback(() =>
        //         {
        //             // enemyDamageIcon.gameObject.SetActive(false);
        //             enemyDamageFilter?.gameObject.SetActive(true);
        //         });
        //         if(enemyDamageFilter != null)
        //         {
        //             enemyDamageSeq.Append(enemyDamageFilter.GetComponent<SpriteRenderer>().DOFade(0, 0.05f).SetLoops(6, LoopType.Yoyo).SetEase(Ease.Linear));
        //             enemyDamageSeq.AppendCallback(() =>
        //             {
        //                 enemyDamageIcon.gameObject.SetActive(false);
        //                 enemyDamageFilter.gameObject.SetActive(false);
        //             });
        //         }
        //         enemyDamageSeq.Play();
        //     }
        // };
        Vector3[] originalScales = new Vector3[Enemy.maxSubEnemyNum + 1];
        for(int i = 0; i < originalScales.Length; i++)
        {
            originalScales[i] = Vector3.zero;
        }

        TurnManager.OnEnemyDamaged += (damage, source, enemyIdx) =>
        {
            Transform enemyDamageSprite = null;
            if (enemyIdx == 0)
            {
                enemyDamageSprite = enemyImg.transform.Find("DamageSprite");
            }
            else
            {
                enemyDamageSprite = subEnemyImg[enemyIdx - 1]?.transform.Find("DamageSprite");
            }
            if (enemyDamageSprite != null)
            {
                if(originalScales[enemyIdx] == Vector3.zero) originalScales[enemyIdx] = enemyDamageSprite.localScale;
                Vector3 expandScale = enemyDamageSprite.localScale.x * 1.5f > originalScales[enemyIdx].x * 2.5f ? originalScales[enemyIdx] * 2.5f : enemyDamageSprite.localScale * 1.5f;
                enemyDamageSprite.localScale = Vector3.zero;
                enemyDamageSprite.Find("DamageTMP").GetComponent<TMP_Text>().text = "-" + damage.ToString();
                enemyDamageSprite.gameObject.SetActive(true);
                enemyDamageSprite.GetComponent<SpriteRenderer>().color = Color.white;
                enemyDamageSprite.Find("DamageTMP").GetComponent<TMP_Text>().color = Color.white;
                enemyDamageSeq?.Kill();
                enemyDamageSeq = DOTween.Sequence();
                enemyDamageSeq.Append(enemyDamageSprite.DOScale(expandScale, 0.2f))
                .Append(enemyDamageSprite.DOScale(originalScales[enemyIdx], 0.2f))
                .Append(enemyDamageSprite.GetComponent<SpriteRenderer>().DOFade(0, 0.2f).SetDelay(0.5f))
                .Join(enemyDamageSprite.Find("DamageTMP").GetComponent<TMP_Text>().DOFade(0, 0.2f))
                .AppendCallback(() =>
                {
                    enemyDamageSprite.gameObject.SetActive(false);
                    enemyDamageSprite.GetComponent<SpriteRenderer>().color = Color.white;
                    enemyDamageSprite.Find("DamageTMP").GetComponent<TMP_Text>().color = Color.white;
                });

                if(source == EDamageSource.TriggerRoulette)
                {
                    enemyDamageSprite.GetComponent<AudioSource>().PlayOneShot(SoundManager.Inst.enemyTriggerDamageSFX);
                }
                else
                {
                    enemyDamageSprite.GetComponent<AudioSource>().PlayOneShot(SoundManager.Inst.enemyDamageSFX);
                }
            }
        };
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
