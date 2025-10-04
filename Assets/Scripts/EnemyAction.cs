using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public enum EEnemyActionType { CW, CCW };

public class EnemyAction : MonoBehaviour
{
    [SerializeField] SpriteRenderer enemyAction;
    [SerializeField] TMP_Text enemyActionTMP;
    [SerializeField] Sprite[] enemyActionSprites;

    public EEnemyActionType actionType;
    public int maxActionVal;
    public int actionVal;
    public bool isIgnore = false;

    public void SetRandomAction()
    {
        EEnemyActionType[] typeVal = (EEnemyActionType[])System.Enum.GetValues(typeof(EEnemyActionType));
        int randSelect = Random.Range(0, typeVal.Length);
        actionType = typeVal[randSelect];
        enemyAction.sprite = enemyActionSprites[randSelect + 1];

        actionVal = Random.Range(1, maxActionVal + 1);
        enemyActionTMP.text = actionVal.ToString();
    }

    public void SetActionType(EEnemyActionType type)
    {
        actionType = type;
        enemyAction.sprite = enemyActionSprites[(int)type + 1];
    }

    public void SetActionVal(int value)
    {
        actionVal = value;
        enemyActionTMP.text = value.ToString();
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
        if (actionVal != 0 && isIgnore == false)
        {
            TurnManager.OnEnemyAction?.Invoke();
            switch (actionType)
            {
                case EEnemyActionType.CW:
                    RouletteManager.Inst.Spin(true, actionVal); break;
                case EEnemyActionType.CCW:
                    RouletteManager.Inst.Spin(false, actionVal); break;
            }
        }
    }
}
