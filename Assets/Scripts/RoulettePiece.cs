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
            default:
                roulettePiece.sprite = rouletteTypeSprites[0]; break;
        }

        roulette.type = rlt.type;
        roulette.value = rlt.value;
        if(rlt.value != 0)
        {
            rouletteValueTMP.text = rlt.value.ToString();
        }
        else
        {
            rouletteValueTMP.text = "";
        }
        Trigger(false);
    }

    public void Trigger(bool triggerState)
    {
        isTriggered = triggerState;
        if(isTriggered)
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
        switch (roulette.type)
        {
            case ERouletteType.Attack:
                if (isEnemy)
                {
                    TurnManager.Inst.EnemyTakeDmg(roulette.value);
                }
                else
                {
                    TurnManager.Inst.TakeDmg(roulette.value);
                }
                break;
            case ERouletteType.Heal:
                if (isEnemy)
                {
                    TurnManager.Inst.EnemyTakeDmg(-roulette.value);
                }
                else
                {
                    TurnManager.Inst.TakeDmg(-roulette.value);
                }
                break;
            case ERouletteType.Shield:
                if (isEnemy)
                {
                    TurnManager.Inst.enemyShieldHealth += roulette.value;
                }
                else
                {
                    TurnManager.Inst.shieldHealth += roulette.value;
                }
                break;
            case ERouletteType.Charge:
                if (isEnemy)
                {
                    TurnManager.Inst.EnemyTakeDmg(-roulette.value);
                }
                else
                {
                    TurnManager.Inst.TakeDmg(roulette.value);
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
