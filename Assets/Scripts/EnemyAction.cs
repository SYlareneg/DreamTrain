using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
using System.Text.RegularExpressions;
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
    public Vector2 tooltipPos;

    public void SetActionType(EEnemyActionType type)
    {
        actionType = type;
        if (type == EEnemyActionType.Enchant_Random_1 || type == EEnemyActionType.Enchant_Random_2)
        {
            enemyAction.sprite = enemyActionSprites[5];
        }
        else if (type == EEnemyActionType.Special_Activate_1)
        {
            enemyAction.sprite = EnemyManager.Inst.EnemySpecial1Sprite;
        }
        else if (type == EEnemyActionType.Special_Activate_2)
        {
            enemyAction.sprite = EnemyManager.Inst.EnemySpecial2Sprite;
        }
        else
        {
            enemyAction.sprite = enemyActionSprites[(int)type + 1];
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
        if (actionType == EEnemyActionType.Special_Activate_1)
        {
            totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Special_1, totalVal);
        }
        if (actionType == EEnemyActionType.Special_Activate_2)
        {
            totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Special_2, totalVal);
        }
        if(actionType == EEnemyActionType.Attack)
        {
            totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Attack, totalVal);
        }
        if(actionType == EEnemyActionType.Heal)
        {
            totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Heal, totalVal);
        }
        if(actionType == EEnemyActionType.Shield)
        {
            totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Shield, totalVal);
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
            enemyActionTMP.color = Color.black;
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
            case EEnemyActionType.Special_Activate_1:
                tooltip.tooltipTitle = EnemyManager.Inst.EnemySpecial1Title;
                tooltip.tooltipTxt = EnemyManager.Inst.EnemySpecial1Text;
                break;
            case EEnemyActionType.Special_Activate_2:
                tooltip.tooltipTitle = EnemyManager.Inst.EnemySpecial2Title;
                tooltip.tooltipTxt = EnemyManager.Inst.EnemySpecial2Text;
                break;
            case EEnemyActionType.Enchant_Random_1:
                tooltip.tooltipTitle = "부여";
                tooltip.tooltipTxt = "무작위 빈 룰렛 칸에 " + EnemyManager.Inst.EnemySpecialRoulette1Title + "을(를) 부여합니다. 빈 칸이 없을 경우 무작위 칸에 부여합니다.";
                break;
            case EEnemyActionType.Enchant_Random_2:
                tooltip.tooltipTitle = "부여";
                tooltip.tooltipTxt = "무작위 빈 룰렛 칸에 " + EnemyManager.Inst.EnemySpecialRoulette2Title + "을(를) 부여합니다. 빈 칸이 없을 경우 무작위 칸에 부여합니다.";
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
                actionVal = Random.Range(baseActionVal, 0);
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
            Utils.AllignActions(ref TurnManager.OnEnemyAction, typeof(ShowBuff), typeof(RelicManager));
            TurnManager.OnEnemyAction?.Invoke();
            int totalVal = actionVal;
            switch (actionType)
            {
                case EEnemyActionType.Turn:
                    RouletteManager.Inst.Spin(totalVal > 0, Math.Abs(totalVal)); break;
                case EEnemyActionType.Attack:
                    totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Attack, totalVal);
                    TurnManager.Inst.TakeDmg(totalVal); break;
                case EEnemyActionType.Heal:
                    totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Heal, totalVal);
                    TurnManager.Inst.EnemyTakeDmg(-totalVal); break;
                case EEnemyActionType.Shield:
                    totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Shield, totalVal);
                    TurnManager.Inst.GetShield(true, totalVal); break;
                case EEnemyActionType.Enchant_Random_1:
                    EnchantAction(ERouletteType.Enemy_Special_1, totalVal); break;
                case EEnemyActionType.Enchant_Random_2:
                    EnchantAction(ERouletteType.Enemy_Special_2, totalVal); break;
                case EEnemyActionType.Special_Activate_1:
                    totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Special_1, totalVal);
                    SpecialAction1(totalVal); break;
                case EEnemyActionType.Special_Activate_2:
                    totalVal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.enemyBuff_Special_2, totalVal);
                    SpecialAction2(totalVal); break;
            }
        }
    }

    public static void SpecialAction1(int x)
    {
        EnemyManager.EnemySpecial1Activation?.Invoke(x);
    }

    public static void SpecialAction2(int x)
    {
        EnemyManager.EnemySpecial2Activation?.Invoke(x);
    }

    public static void EnchantAction(ERouletteType rType, int rVal)
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
            RouletteManager.Inst.EnchantRoulettePiece(randIdx, rType, rVal);
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
                RouletteManager.Inst.EnchantRoulettePiece(randIdx, rType, rVal);
            }
            else
            {
                switch (rType)
                {
                    case ERouletteType.Attack:
                        TurnManager.Inst.TakeDmg(rVal); break;
                    case ERouletteType.Heal:
                        TurnManager.Inst.EnemyTakeDmg(-rVal); break;
                    case ERouletteType.Shield:
                        TurnManager.Inst.GetShield(true, rVal); break;
                    case ERouletteType.Enemy_Special_1:
                        if (EnemyManager.Inst.enemy.name == "마술사") break;
                        SpecialAction1(rVal); break;
                    case ERouletteType.Enemy_Special_2:
                        SpecialAction2(rVal); break;
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
