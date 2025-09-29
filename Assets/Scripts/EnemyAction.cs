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

    public void SetRandomAction()
    {
        EEnemyActionType[] typeVal = (EEnemyActionType[])System.Enum.GetValues(typeof(EEnemyActionType));
        int randSelect = Random.Range(0, typeVal.Length);
        actionType = typeVal[randSelect];
        enemyAction.sprite = enemyActionSprites[randSelect];

        actionVal = Random.Range(1, maxActionVal + 1);
        enemyActionTMP.text = actionVal.ToString();
    }

    public void ExecuteAction()
    {
        if(actionVal != 0)
        {
            switch(actionType)
            {
                case EEnemyActionType.CW:
                    RouletteManager.Inst.Spin(true, actionVal); break;
                case EEnemyActionType.CCW:
                    RouletteManager.Inst.Spin(false, actionVal); break;
            }
            RouletteManager.Inst.ActivateRoulette();
        }
    }
}
