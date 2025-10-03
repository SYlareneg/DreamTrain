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

    [Header("Prefabs")]
    [SerializeField][Tooltip("액션 심볼 Prefab")] GameObject actionPrefab;
    [Header("Positions")]
    [SerializeField][Tooltip("액션 심볼 스폰 지점")] Transform enemyPos;
    [SerializeField][Tooltip("1번 액션 심볼 위치")] Transform enemyActionPos;
    [Tooltip("액션 심볼 간격")] public static float actionMargin = 0.5f;
    [SerializeField][Tooltip("액션 심볼 소멸 지점")] Transform enemyExecutePos;
    [Header("Data")]
    [Tooltip("액션별 최대 실행값\n(예: 2일 경우 회전 액션은 최대 2칸 회전)")] public int maxActionVal;

    [Tooltip("액션 개수")] public int actionNum;
    [HideInInspector] public List<EnemyAction> actionList;
    [HideInInspector] public List<EnemyAction> executeActionList;
    static float actionInterval = 0.5f;

    // 액션 리스트 초기화. 랜덤한 액션을 actionNum 개수만큼 생성
    public void InitActionList()
    {
        actionList.Clear();
        for (int i = 0; i < actionNum; i++)
        {
            var newActionObj = Instantiate(actionPrefab, enemyPos.position, Utils.QI);
            var newAction = newActionObj.GetComponent<EnemyAction>();
            newAction.maxActionVal = maxActionVal;
            newAction.SetRandomAction();
            actionList.Add(newAction);
        }
    }

    // 액션 심볼 스폰. 액션 리스트 생성, 해당 리스트에 따라 액션 심볼 오브젝트 소환
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

    // 적 트리거 발동. TODO
    public void EnemyTriggerAction()
    {
        TurnManager.Inst.TakeDmg(2);
        TurnManager.Inst.enemyTriggerCnt = 0;
    }

    // 적 최선의 행동 계산, executeActionList에 최적 행동 리스트 저장. TODO
    public void GetBestAction()
    {
        executeActionList.Add(actionList[0]);
    }

    // 적 최선의 행동 실행.
    public void ExecuteBestAction()
    {
        GetBestAction();
        Sequence executionSeq = DOTween.Sequence();
        for (int i = 0; i < executeActionList.Count; i++)
        {
            int localIndex = i;
            executionSeq.Append(executeActionList[localIndex].gameObject.transform.DOMove(enemyExecutePos.position, actionInterval).OnComplete(() =>
            {
                executeActionList[localIndex].ExecuteAction();
            }));
            executionSeq.AppendInterval(RouletteManager.spinDelay);
        }
        executionSeq.AppendCallback(EndEnemyTurn);
        executionSeq.Play();
    }

    // 모든 액션 심볼 오브젝트 제거
    void DestroyAllActionObjects()
    {
        foreach (var obj in actionList)
        {
            Destroy(obj.gameObject);
        }
        actionList.Clear();
        executeActionList.Clear();
    }

    // 적 턴 시작
    public void StartEnemyTurn()
    {
        TurnManager.Inst.enemyShieldHealth = 0;
        TurnManager.OnEnemyTurnStart?.Invoke();
        ExecuteBestAction();
    }

    // 적 턴 종료
    public void EndEnemyTurn()
    {
        DestroyAllActionObjects();
        TurnManager.OnEnemyTurnEnd?.Invoke();
        RouletteManager.Inst.ActivateRoulette();
        if (GameManager.Inst.gameOverSignal == false)
        {
            TurnManager.Inst.StartPlayerTurn();
        }
    }

    private void Start()
    {
        TurnManager.OnPlayerTurnStart += ShowAllActions;
    }

    private void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart -= ShowAllActions;
    }
}
