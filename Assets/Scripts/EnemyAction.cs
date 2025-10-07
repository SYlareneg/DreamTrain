using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class EnemyAction : MonoBehaviour
{
    [SerializeField] SpriteRenderer enemyAction;
    [SerializeField] TMP_Text enemyActionTMP;
    [SerializeField] Sprite[] enemyActionSprites;

    public EEnemyActionType actionType;
    public int baseActionVal;
    public int actionVal;
    public bool isIgnore = false;

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
            totalVal = BuffManager.Inst.GetEnemyBuffValue(BuffManager.Inst.enemyDrainBuff, actionVal);
        }
        
        if (actionVal == 0)
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
    }

    public void SetAction(EnemyPattern p)
    {
        SetActionType(p.type);
        SetActionVal(p.val);
        isIgnore = false;

        if (actionType == EEnemyActionType.Turn)
        {
            actionVal = Random.Range(1, baseActionVal + 1);
            if (baseActionVal < 0)
            {
                enemyAction.flipX = !enemyAction.flipX;
                actionVal = -actionVal;
            }
        }
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
                        DrainEnchantAction();
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
        int totalVal = BuffManager.Inst.GetEnemyBuffValue(BuffManager.Inst.enemyDrainBuff, x);
        int damage = TurnManager.Inst.TakeDmg(totalVal);
        TurnManager.Inst.EnemyTakeDmg(-damage);
        if (damage > 0)
        {
            TurnManager.Inst.TriggerEnemyPassive(1);
        }
    }

    public static void DrainEnchantAction()
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
            RouletteManager.Inst.EnchantRoulettePiece(randIdx, ERouletteType.Drain, 5);
        }
        else
        {
            for (int i = 0; i < RouletteManager.rouletteNum; i++)
            {
                if (i != RouletteManager.Inst.triggerPos && RouletteManager.Inst.roulettePieces[i].roulette.type != ERouletteType.Drain)
                {
                    noneIdx.Add(i);
                }
            }
            if (noneIdx.Count > 0)
            {
                randIdx = noneIdx[Random.Range(0, noneIdx.Count)];
                RouletteManager.Inst.EnchantRoulettePiece(randIdx, ERouletteType.Drain, 5);
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
