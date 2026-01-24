using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
using System.Text.RegularExpressions;
using Random = UnityEngine.Random;
using UnityEngine.UI;

public class EnemyAction : MonoBehaviour
{
    [SerializeField] SpriteRenderer enemyAction;
    [SerializeField] TMP_Text enemyActionTMP;
    [SerializeField] Sprite[] enemyActionSprites;

    public EEnemyActionType actionType;
    public int actionTypeNum;
    public int enemyIdx;
    public int baseActionVal;
    public int actionVal;
    public bool isIgnore = false;
    Tooltip tooltip;
    public Vector2 tooltipPos;

    public void SetActionType(EEnemyActionType type, int typeNum)
    {
        actionType = type;
        actionTypeNum = typeNum;
        if (type == EEnemyActionType.Special_Activate)
        {
            enemyAction.sprite = EnemyManager.Inst.enemySpecialActions[typeNum].sprite;
        }
        else if(type == EEnemyActionType.Turn)
        {
            enemyAction.sprite = enemyActionSprites[(int)type + 1];
        }
        else
        {
            enemyAction.sprite = enemyActionSprites[(int)type + 2];
        }
    }

    public void SetActionVal(int value)
    {
        baseActionVal = value;
        actionVal = value;
    }

    public void ShowAction()
    {
        int totalVal = actionVal;
        if (actionType == EEnemyActionType.Special_Activate)
        {
            totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Special[enemyIdx, actionTypeNum], totalVal);
        }
        if(actionType == EEnemyActionType.Attack)
        {
            totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Attack[enemyIdx], totalVal);
        }
        if(actionType == EEnemyActionType.Heal)
        {
            totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Heal[enemyIdx], totalVal);
        }
        if(actionType == EEnemyActionType.Shield)
        {
            totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Shield[enemyIdx], totalVal);
        }
        
        if (totalVal == 0)
        {
            enemyActionTMP.text = "";
        }
        else
        {
            if (actionType == EEnemyActionType.Turn && totalVal < 0)
            {
                enemyActionTMP.text = (-totalVal).ToString();
            }
            else
            {
                enemyActionTMP.text = totalVal.ToString();
            }
        }

        if (totalVal > actionVal)
        {
            enemyActionTMP.color = Color.green;
        }
        else if (totalVal == actionVal)
        {
            enemyActionTMP.color = Color.white;
        }
        else
        {
            enemyActionTMP.color = Color.red;
        }

        if (tooltip == null) return;
        switch (actionType)
        {
            case EEnemyActionType.Attack:
                tooltip.tooltipTitle = "공격";
                tooltip.tooltipTxt = "피해를 값만큼 줍니다.";
                break;
            case EEnemyActionType.Special_Activate:
                tooltip.tooltipTitle = EnemyManager.Inst.enemySpecialActions[actionTypeNum].title;
                tooltip.tooltipTxt = EnemyManager.Inst.enemySpecialActions[actionTypeNum].text;
                break;
            case EEnemyActionType.Enchant_Random:
                tooltip.tooltipTitle = "부여";
                tooltip.tooltipTxt = "무작위 빈 룰렛 칸에 " + EnemyManager.Inst.enemySpecialRoulettes[actionTypeNum].title + "을(를) 부여합니다. 빈 칸이 없을 경우 무작위 칸에 부여합니다.";
                break;
            case EEnemyActionType.Spawn_SubEnemy:
                tooltip.tooltipTitle = "소환";
                tooltip.tooltipTxt = "하위 적을 값명 소환합니다.";
                break;
            case EEnemyActionType.Heal:
                tooltip.tooltipTitle = "회복";
                tooltip.tooltipTxt = "체력을 값만큼 회복합니다.";
                break;
            case EEnemyActionType.Shield:
                tooltip.tooltipTitle = "실드";
                tooltip.tooltipTxt = "실드를 값만큼 얻습니다.";
                break;
            case EEnemyActionType.Turn:
                tooltip.tooltipTitle = "회전";
                tooltip.tooltipTxt = "룰렛을 " + ((totalVal >= 0) ? "시계방향으로 " : "반시계방향으로 ") + "<" + Math.Abs(totalVal).ToString() + ">칸 회전시킵니다.";
                break;
        }
        tooltip.tooltipTxt = Regex.Replace(tooltip.tooltipTxt, @"값|<\d+>", match =>
        {
            string replacement = $"<{totalVal}>";
            return replacement;
        });
    }

    public void SetAction(EnemyPattern p, int enemyIdx)
    {
        SetActionType(p.type, p.typeNum);
        this.enemyIdx = enemyIdx;
        SetActionVal(p.val);
        isIgnore = false;

        if (actionType == EEnemyActionType.Turn)
        {
            if (baseActionVal < 0)
            {
                enemyAction.sprite = enemyActionSprites[(int)actionType + 2];
                actionVal = Random.Range(baseActionVal, p.typeNum);
            }
            else
            {
                actionVal = Random.Range(p.typeNum + 1, baseActionVal + 1);
            }
        }
        tooltip = GetComponent<Tooltip>();
        ShowAction();
    }

    public void IgnoreAction(bool bIg)
    {
        isIgnore = bIg;
        if (isIgnore)
        {
            enemyAction.color = Color.red;
        }
        else
        {
            enemyAction.color = Color.white;
        }
    }

    public void HideAction(bool bHide)
    {
        if (bHide)
        {
            enemyAction.sprite = enemyActionSprites[0];
            enemyActionTMP.gameObject.SetActive(false);
        }
        else
        {
            enemyAction.sprite = enemyActionSprites[(int)actionType + 1];
            enemyActionTMP.gameObject.SetActive(true);
        }
    }

    public void ExecuteAction()
    {
        if (isIgnore == false)
        {
            Utils.AllignActions(ref TurnManager.OnEnemyAction, typeof(ShowBuff), typeof(RelicManager));
            TurnManager.OnEnemyAction?.Invoke(this);
            int totalVal = actionVal;
            switch (actionType)
            {
                case EEnemyActionType.Turn:
                    RouletteManager.Inst.Spin(totalVal > 0, Math.Abs(totalVal)); break;
                case EEnemyActionType.Attack:
                    GameManager.Inst.enemyAttackEffect.SetActive(true);
                    GameManager.Inst.enemyAttackEffect.transform.localScale = Vector3.zero;
                    GameManager.Inst.enemyAttackEffect.GetComponent<Image>().color = Color.white;
                    Sequence attackSeq = DOTween.Sequence();
                    attackSeq.Append(GameManager.Inst.enemyAttackEffect.transform.DOScale(Vector3.one, 0.2f))
                    .AppendInterval(0.2f)
                    .Append(GameManager.Inst.enemyAttackEffect.GetComponent<Image>().DOFade(0f, 0.2f))
                    .OnComplete(() =>
                    {
                        GameManager.Inst.enemyAttackEffect.SetActive(false);
                    });
                    totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Attack[enemyIdx], totalVal);
                    TurnManager.Inst.TakeDmg(totalVal, EDamageSource.Enemy); break;
                case EEnemyActionType.Heal:
                    totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Heal[enemyIdx], totalVal);
                    TurnManager.Inst.EnemyTakeDmg(-totalVal, EDamageSource.Enemy, enemyIdx); break;
                case EEnemyActionType.Shield:
                    totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Shield[enemyIdx], totalVal);
                    TurnManager.Inst.GetShield(true, totalVal, EDamageSource.Enemy, enemyIdx); break;
                case EEnemyActionType.Enchant_Random:
                    for(int i = 0; i < totalVal; i++)
                    {
                        int baseVal = 0;
                        if(enemyIdx == 0) baseVal = EnemyManager.Inst.enemySpecialRoulettes[actionTypeNum].baseVal;
                        else baseVal = EnemyManager.Inst.subEnemySpecialRoulettes[enemyIdx - 1][actionTypeNum].baseVal;
                        EnchantAction(new RouletteType(ERouletteType.Enemy_Special, actionTypeNum, enemyIdx), baseVal);
                    }
                    break;
                case EEnemyActionType.Special_Activate:
                    totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Special[enemyIdx, actionTypeNum], totalVal);
                    SpecialAction(actionTypeNum, totalVal, enemyIdx); break;
                case EEnemyActionType.Spawn_SubEnemy:
                    SubEnemy SE = TurnManager.Inst.enemySO.subEnemies.Find(x => x.name == EnemyManager.Inst.enemy.subEnemies_Spawn[actionTypeNum]);
                    if(SE == null || SE.name == null) break;
                    for(int i = 0; i < totalVal; i++)
                    {
                        EnemyManager.Inst.InitSubEnemy(SE);
                    }
                    break;
            }
        }
    }

    public static void SpecialAction(int num, int val, int enemyIdx = 0)
    {
        if(enemyIdx == 0) EnemyManager.enemySpecialActivation[num]?.Invoke(val);
        else EnemyManager.subEnemySpecialActivation[enemyIdx - 1, num]?.Invoke(val);
    }

    public static void EnchantAction(RouletteType rType, int rVal)
    {
        List<int> noneIdx = new List<int>();
        for (int i = 0; i < RouletteManager.rouletteNum; i++)
        {
            if (RouletteManager.Inst.roulettePieces[i].roulette.rtype.type == ERouletteType.None)
            {
                noneIdx.Add(i);
            }
        }
        int randIdx;
        if (noneIdx.Count > 0)
        {
            randIdx = noneIdx[Random.Range(0, noneIdx.Count)];
            RouletteManager.Inst.EnchantRoulettePiece(randIdx, rType, rVal);
        }
        else
        {
            for (int i = 0; i < RouletteManager.rouletteNum; i++)
            {
                if (RouletteManager.Inst.roulettePieces[i].roulette.rtype != rType)
                {
                    noneIdx.Add(i);
                }
            }
            if (noneIdx.Count > 0)
            {
                randIdx = noneIdx[Random.Range(0, noneIdx.Count)];
                RouletteManager.Inst.EnchantRoulettePiece(randIdx, rType, rVal);
            }
            else
            {
                switch (rType.type)
                {
                    case ERouletteType.Attack:
                        TurnManager.Inst.TakeDmg(rVal, EDamageSource.Enemy); break;
                    case ERouletteType.Heal:
                        TurnManager.Inst.EnemyTakeDmg(-rVal, EDamageSource.Enemy); break;
                    case ERouletteType.Shield:
                        TurnManager.Inst.GetShield(true, rVal, EDamageSource.Enemy); break;
                    case ERouletteType.Enemy_Special:
                        if (rType.specialTypeIdx == 0 && rType.enemyIdx == 0 && EnemyManager.Inst.enemy.name == "마술사") break;
                        SpecialAction(rType.specialTypeIdx, rVal, rType.enemyIdx); break;
                    default:
                        break;
                }
            }
        }
    }

    private void Update()
    {
        ShowAction();
        tooltip.tooltipPos = this.tooltipPos;
    }
}
