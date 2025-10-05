using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoulettePiece : MonoBehaviour
{
    [SerializeField] SpriteRenderer roulettePiece;
    [SerializeField] TMP_Text rouletteValueTMP;
    [SerializeField] Sprite[] rouletteTypeSprites;
    
    public RouletteItem roulette;
    public bool isTriggered;

    public void Setup(RouletteItem rlt)
    {
        switch(rlt.type)
        {
            case ERouletteType.Attack:
                roulettePiece.sprite = rouletteTypeSprites[1]; break;
            case ERouletteType.Heal:
                roulettePiece.sprite = rouletteTypeSprites[2]; break;
            case ERouletteType.Shield:
                roulettePiece.sprite = rouletteTypeSprites[3]; break;
            case ERouletteType.Charge:
                roulettePiece.sprite = rouletteTypeSprites[4]; break;
            case ERouletteType.Lifesteal:
                roulettePiece.sprite = rouletteTypeSprites[5]; break;
            default:
                roulettePiece.sprite = rouletteTypeSprites[0]; break;
        }

        roulette.type = rlt.type;
        roulette.value = rlt.value;
        HideTotalValue();
        Trigger(false);
    }

    public void ShowTotalValue()
    {
        RouletteBuff target = new RouletteBuff();
        switch (roulette.type)
        {
            case ERouletteType.Attack:
                target = BuffManager.Inst.totalRouletteBuff_Attack; break;
            case ERouletteType.Heal:
                target = BuffManager.Inst.totalRouletteBuff_Heal; break;
            case ERouletteType.Shield:
                target = BuffManager.Inst.totalRouletteBuff_Shield; break;
            case ERouletteType.Charge:
                target = BuffManager.Inst.totalRouletteBuff_Charge; break;
            case ERouletteType.Lifesteal:
                target = BuffManager.Inst.totalRouletteBuff_Lifesteal_Dmg; break;
        }
        int totalVal = BuffManager.Inst.GetBuffedRouletteValue(target, roulette.value);
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
        if(roulette.type == ERouletteType.None && totalVal == 0)
        {
            rouletteValueTMP.text = "";
        }
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
        int totalVal = 0;
        switch (roulette.type)
        {
            case ERouletteType.Attack:
                totalVal = BuffManager.Inst.GetBuffedRouletteValue(BuffManager.Inst.totalRouletteBuff_Attack, roulette.value);
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
                totalVal = BuffManager.Inst.GetBuffedRouletteValue(BuffManager.Inst.totalRouletteBuff_Heal, roulette.value);
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
                totalVal = BuffManager.Inst.GetBuffedRouletteValue(BuffManager.Inst.totalRouletteBuff_Shield, roulette.value);
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
                totalVal = BuffManager.Inst.GetBuffedRouletteValue(BuffManager.Inst.totalRouletteBuff_Charge, roulette.value);
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
                int totalVal_Dmg = BuffManager.Inst.GetBuffedRouletteValue(BuffManager.Inst.totalRouletteBuff_Lifesteal_Dmg, roulette.value);
                if (isEnemy)
                {
                    int trueDamage = TurnManager.Inst.EnemyTakeDmg(totalVal_Dmg);
                    int totalVal_Heal = BuffManager.Inst.GetBuffedRouletteValue(BuffManager.Inst.totalRouletteBuff_Lifesteal_Heal, trueDamage / 3);
                    TurnManager.Inst.TakeDmg(-totalVal_Heal);
                }
                else
                {
                    int trueDamage = TurnManager.Inst.TakeDmg(totalVal_Dmg);
                    int totalVal_Heal = BuffManager.Inst.GetBuffedRouletteValue(BuffManager.Inst.totalRouletteBuff_Lifesteal_Heal, trueDamage / 3);
                    TurnManager.Inst.EnemyTakeDmg(-totalVal_Heal);
                }
                break;
        }
        if (isTriggered == true)
        {
            Trigger(false);
            var tempRoulettePiece = this.roulette;
            tempRoulettePiece.type = ERouletteType.None;
            this.Setup(tempRoulettePiece);
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
