using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] GameObject actionPrefab;
    [SerializeField] Transform enemyPos;
    [SerializeField] Transform enemyActionPos;
    [SerializeField] Transform enemyExecutePos;

    public int maxActionVal;

    public int actionNum;
    public List<EnemyAction> actionList;

    public static float actionMargin = 0.5f;

    public void InitActionList()
    {
        actionList = new List<EnemyAction>();
        for (int i = 0; i < actionNum; i++)
        {
            var newActionObj = Instantiate(actionPrefab, enemyPos.position, Utils.QI);
            var newAction = newActionObj.GetComponent<EnemyAction>();
            newAction.maxActionVal = maxActionVal;
            newAction.SetRandomAction();
            actionList.Add(newAction);
        }
    }

    public void ShowAllActions()
    {
        InitActionList();
        for (int i = 0; i < actionList.Count; i++)
        {
            var targetPos = enemyActionPos.position;
            targetPos.x += i * (actionPrefab.GetComponent<SpriteRenderer>().bounds.size.x + actionMargin);
            actionList[i].transform.DOMove(targetPos, 0.7f);
        }
    }

    public void ExecuteBestAction()
    {
        if (actionList.Count > 0)
        {
            actionList[0].transform.DOMove(enemyExecutePos.position, 0.7f).OnComplete(DeleteActionList);
        }
    }

    public void EnemyTriggerAction()
    {
        TurnManager.Inst.TakeDmg(2);
        TurnManager.Inst.enemyTriggerCnt = 0;
    }

    void DeleteActionList()
    {
        actionList[0].ExecuteAction();
        while (actionList.Count > 0)
        {
            Destroy(actionList[0].gameObject);
            actionList.RemoveAt(0);
        }
    }

    private void Start()
    {
        TurnManager.OnTurnEnd += ExecuteBestAction;
    }

    private void OnDestroy()
    {
        TurnManager.OnTurnEnd -= ExecuteBestAction;
    }
}
