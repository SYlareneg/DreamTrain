using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;

public class RoulettePiece : MonoBehaviour
{
    [SerializeField] SpriteRenderer roulettePiece;
    [SerializeField] TMP_Text rouletteValueTMP;
    [SerializeField] Sprite[] rouletteTypeSprites;
    
    public RouletteItem roulette;
    public bool isTriggered;
    Tooltip tooltip;

    public void Setup(RouletteItem rlt)
    {
        tooltip = GetComponent<Tooltip>();
        switch(rlt.type)
        {
            case ERouletteType.Attack:
                roulettePiece.sprite = rouletteTypeSprites[1];
                if (tooltip)
                {
                    tooltip.tooltipTitle = "공격 룰렛";
                    tooltip.tooltipTxt = "피해를 값만큼 줍니다.";
                }
                break;
            case ERouletteType.Heal:
                roulettePiece.sprite = rouletteTypeSprites[2]; 
                if (tooltip)
                {
                    tooltip.tooltipTitle = "회복 룰렛";
                    tooltip.tooltipTxt = "체력을 값만큼 회복합니다.";
                }
                break;
            case ERouletteType.Shield:
                roulettePiece.sprite = rouletteTypeSprites[3]; 
                if (tooltip)
                {
                    tooltip.tooltipTitle = "실드 룰렛";
                    tooltip.tooltipTxt = "실드를 값만큼 획득합니다.";
                }
                break;
            case ERouletteType.Charge:
                roulettePiece.sprite = rouletteTypeSprites[4]; 
                if (tooltip)
                {
                    tooltip.tooltipTitle = "전격 룰렛";
                    tooltip.tooltipTxt = "피해를 값만큼 줍니다.";
                }
                break;
            case ERouletteType.Lifesteal:
                roulettePiece.sprite = rouletteTypeSprites[5]; 
                if (tooltip)
                {
                    tooltip.tooltipTitle = "생명력 흡수 룰렛";
                    tooltip.tooltipTxt = "피해를 값만큼 줍니다. 입힌 피해의 1/3만큼 플레이어의 체력을 회복합니다.";
                }
                break;
            case ERouletteType.MagicBox:
                roulettePiece.sprite = rouletteTypeSprites[6]; 
                if (tooltip)
                {
                    tooltip.tooltipTitle = "마술상자 룰렛";
                    tooltip.tooltipTxt = "값의 카운터를 가지고 있습니다. 룰렛이 시계방향으로 회전할 때마다 카운트가 1 감소하고, 0이 되면 효과가 제거됩니다.";
                }
                break;
            case ERouletteType.Drain:
                roulettePiece.sprite = rouletteTypeSprites[7]; 
                if (tooltip)
                {
                    tooltip.tooltipTitle = "흡혈 룰렛";
                    tooltip.tooltipTxt = "피해를 값만큼 줍니다. 입힌 피해만큼 적의 체력을 회복합니다.";
                }
                break;
            default:
                roulettePiece.sprite = rouletteTypeSprites[0];
                if (tooltip)
                {
                    if (this == RouletteManager.Inst.roulettePieces[RouletteManager.Inst.triggerPos])
                    {
                        tooltip.tooltipTitle = "트리거 룰렛";
                        tooltip.tooltipTxt = "현재 활성화되어 있지 않습니다.";
                    }
                    else
                    {
                        tooltip.tooltipDisable = true;
                    }
                }
                break;
        }

        roulette.type = rlt.type;
        roulette.value = rlt.value;
        HideTotalValue();
        Trigger(false);
    }

    public void ShowTotalValue()
    {
        int totalVal = BuffManager.Inst.GetBuffedRouletteValue(this);

        if (totalVal > roulette.value)
        {
            rouletteValueTMP.color = Color.green;
        }
        else if (totalVal == roulette.value)
        {
            rouletteValueTMP.color = Color.black;
        }
        else
        {
            rouletteValueTMP.color = Color.red;
        }
        rouletteValueTMP.text = totalVal.ToString();
        if (roulette.type == ERouletteType.None && totalVal == 0)
        {
            rouletteValueTMP.text = "";
        }

        tooltip.tooltipTxt = Regex.Replace(tooltip.tooltipTxt, @"값", match =>
        {
            string replacement = $"{totalVal}";
            return replacement;
        });
    }

    public void HideTotalValue()
    {
        rouletteValueTMP.color = Color.black;
        if (roulette.value != 0)
        {
            rouletteValueTMP.text = roulette.value.ToString();
        }
        else
        {
            rouletteValueTMP.text = "";
        }
    }

    public void Trigger(bool triggerState)
    {
        isTriggered = triggerState;
        if (isTriggered)
        {
            roulettePiece.color = Color.green;
        }
        else
        {
            roulettePiece.color = Color.white;
        }
    }

    public void Activate(bool isEnemy)
    {
        int totalVal = BuffManager.Inst.GetBuffedRouletteValue(this);
        switch (roulette.type)
        {
            case ERouletteType.Attack:
                if (isEnemy)
                {
                    TurnManager.Inst.EnemyTakeDmg(totalVal);
                }
                else
                {
                    TurnManager.Inst.TakeDmg(totalVal);
                }
                break;
            case ERouletteType.Heal:
                if (isEnemy)
                {
                    TurnManager.Inst.EnemyTakeDmg(-totalVal);
                }
                else
                {
                    TurnManager.Inst.TakeDmg(-totalVal);
                }
                break;
            case ERouletteType.Shield:
                if (isEnemy)
                {
                    TurnManager.Inst.GetShield(true, totalVal);
                }
                else
                {
                    TurnManager.Inst.GetShield(false, totalVal);
                }
                break;
            case ERouletteType.Charge:
                if (isEnemy)
                {
                    TurnManager.Inst.EnemyTakeDmg(-totalVal);
                }
                else
                {
                    TurnManager.Inst.TakeDmg(totalVal);
                }
                break;
            case ERouletteType.Lifesteal:
                if (isEnemy)
                {
                    int trueDamage = TurnManager.Inst.EnemyTakeDmg(totalVal);
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_Lifesteal_Heal, totalVal / 3);
                    TurnManager.Inst.TakeDmg(-totalVal_Heal);
                    TurnManager.Inst.TriggerPlayerPassive(totalVal_Heal);
                }
                else
                {
                    int trueDamage = TurnManager.Inst.TakeDmg(totalVal);
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_Lifesteal_Heal, totalVal / 3);
                    TurnManager.Inst.EnemyTakeDmg(-totalVal_Heal);
                    TurnManager.Inst.TriggerPlayerPassive(totalVal_Heal);
                }
                break;
            case ERouletteType.Drain:
                if (isEnemy)
                {
                    int trueDamage = TurnManager.Inst.EnemyTakeDmg(totalVal);
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_Drain_Heal, trueDamage);
                    TurnManager.Inst.EnemyTakeDmg(-totalVal_Heal);
                }
                else
                {
                    int trueDamage = TurnManager.Inst.TakeDmg(totalVal);
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_Drain_Heal, trueDamage);
                    TurnManager.Inst.EnemyTakeDmg(-totalVal_Heal);
                }
                break;
        }
        if (isTriggered == true)
        {
            Trigger(false);
            this.Setup(RouletteManager.Inst.triggerPiece_None);
            TurnManager.Inst.playerTriggerCnt = 0;
        }
    }

    private void OnMouseDown()
    {
        RouletteManager.Inst.RouletteMouseDown();
    }

    private void OnMouseUp()
    {
        RouletteManager.Inst.RouletteMouseUp();
    }
}
