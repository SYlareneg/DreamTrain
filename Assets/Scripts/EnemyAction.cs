using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
using Random = UnityEngine.Random;

public class EnemyAction : MonoBehaviour
{
    [SerializeField] SpriteRenderer enemyAction;
    [SerializeField] TMP_Text enemyActionTMP;
    [SerializeField] Sprite[] enemyActionSprites;

    public EEnemyActionType actionType;
    public int baseActionVal;
    public int actionVal;
    public bool isIgnore = false;
    Tooltip tooltip;

    public void SetActionType(EEnemyActionType type)
    {
        actionType = type;
        enemyAction.sprite = enemyActionSprites[(int)type + 1];
    }

    public void SetActionVal(int value)
    {
        baseActionVal = value;
        actionVal = value;
    }

    public void ShowAction()
    {
        int totalVal = actionVal;
        if (actionType == EEnemyActionType.Drain)
        {
            totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Drain, totalVal);
        }
        if(actionType == EEnemyActionType.Attack)
        {
            totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Attack, totalVal);
        }
        
        if (totalVal == 0)
        {
            enemyActionTMP.text = "";
        }
        else
        {
            enemyActionTMP.text = totalVal.ToString();
        }

        if (totalVal > actionVal)
        {
            enemyActionTMP.color = Color.green;
        }
        else if (totalVal == actionVal)
        {
            enemyActionTMP.color = Color.black;
        }
        else
        {
            enemyActionTMP.color = Color.red;
        }

        if (tooltip == null) return;
        switch(actionType)
        {
            case EEnemyActionType.Attack:
                tooltip.tooltipTitle = "공격";
                tooltip.tooltipTxt = "피해를 " + totalVal.ToString() + "만큼 줍니다.";
                break;
            case EEnemyActionType.Drain:
                tooltip.tooltipTitle = "흡혈";
                tooltip.tooltipTxt = "피해를 " + totalVal.ToString() + "만큼 주고 입힌 피해의 1/3만큼 체력을 회복합니다.";
                break;
            case EEnemyActionType.Enchant_Random:
                if (EnemyManager.Inst.enemy.name == "Vampire Paul")
                {
                    tooltip.tooltipTitle = "부여";
                    tooltip.tooltipTxt = "무작위 빈 룰렛 칸에 흡혈을 부여합니다. 빈 칸이 없을 경우 무작위 칸에 부여합니다.";
                }
                break;
            case EEnemyActionType.Heal:
                tooltip.tooltipTitle = "회복";
                tooltip.tooltipTxt = "체력을 " + totalVal.ToString() + "만큼 회복합니다.";
                break;
            case EEnemyActionType.Shield:
                tooltip.tooltipTitle = "실드";
                tooltip.tooltipTxt = "실드를 " + totalVal.ToString() + "만큼 얻습니다.";
                break;
            case EEnemyActionType.Turn:
                tooltip.tooltipTitle = "회전";
                tooltip.tooltipTxt = "룰렛을 " + ((totalVal >= 0)? "시계방향으로 " : "반시계방향으로 ") + Math.Abs(totalVal).ToString() + "칸 회전시킵니다.";
                break;
        }
    }

    public void SetAction(EnemyPattern p)
    {
        SetActionType(p.type);
        SetActionVal(p.val);
        isIgnore = false;

        if (actionType == EEnemyActionType.Turn)
        {
            if (baseActionVal < 0)
            {
                enemyAction.flipX = !enemyAction.flipX;
                actionVal = Random.Range(-baseActionVal, 0);
            }
            else
            {
                actionVal = Random.Range(1, baseActionVal + 1);
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
            TurnManager.OnEnemyAction?.Invoke();
            switch (actionType)
            {
                case EEnemyActionType.Turn:
                    RouletteManager.Inst.Spin(actionVal > 0, actionVal); break;
                case EEnemyActionType.Attack:
                    TurnManager.Inst.TakeDmg(actionVal); break;
                case EEnemyActionType.Heal:
                    TurnManager.Inst.EnemyTakeDmg(-actionVal); break;
                case EEnemyActionType.Shield:
                    TurnManager.Inst.GetShield(true, actionVal); break;
                case EEnemyActionType.Enchant_Random:
                    if (EnemyManager.Inst.enemy.name == "Vampire Paul")
                    {
                        EnchantAction(ERouletteType.Enemy_Special_1);
                    }
                    break;
                case EEnemyActionType.Drain:
                    DrainAction(actionVal);
                    break;
            }
        }
    }

    public static void DrainAction(int x)
    {
        int totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Drain, x);
        int damage = TurnManager.Inst.TakeDmg(totalVal);
        TurnManager.Inst.EnemyTakeDmg(-damage);
        if (damage > 0)
        {
            TurnManager.Inst.TriggerEnemyPassive(1);
        }
    }

    public static void EnchantAction(ERouletteType rType)
    {
        List<int> noneIdx = new List<int>();
        for (int i = 0; i < RouletteManager.rouletteNum; i++)
        {
            if (i != RouletteManager.Inst.triggerPos && RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.None)
            {
                noneIdx.Add(i);
            }
        }
        int randIdx;
        if (noneIdx.Count > 0)
        {
            randIdx = noneIdx[Random.Range(0, noneIdx.Count)];
            RouletteManager.Inst.EnchantRoulettePiece(randIdx, rType, 5);
        }
        else
        {
            for (int i = 0; i < RouletteManager.rouletteNum; i++)
            {
                if (i != RouletteManager.Inst.triggerPos && RouletteManager.Inst.roulettePieces[i].roulette.type != rType)
                {
                    noneIdx.Add(i);
                }
            }
            if (noneIdx.Count > 0)
            {
                randIdx = noneIdx[Random.Range(0, noneIdx.Count)];
                RouletteManager.Inst.EnchantRoulettePiece(randIdx, rType, 5);
            }
            else
            {
                DrainAction(5);
            }
        }
    }

    private void Update()
    {
        ShowAction();
    }
}
