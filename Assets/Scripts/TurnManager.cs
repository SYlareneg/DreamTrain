using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public enum EDamageSource
{
    Enemy, Roulette, UseableItem, Buff, Relic, Card, Passive
};
public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }
    private void Awake() => Inst = this;

    [Header("개발 설정")]
    [SerializeField][Tooltip("카드 배분이 매우 빨라집니다")] bool fastMode;
    [Header("턴")]
    [Tooltip("턴 카운트")] public int turnNum;

    [Header("카드")]
    [Tooltip("최대 카드 개수")] public int maxCardCount;
    [Tooltip("매 턴 드로우할 기본 카드 개수")] public int drawCardCount;
    [Tooltip("시작 카드 개수")] public int startCardCount;
    [Tooltip("매 턴 드로우하는 카드 개수")] public int turnDraw;
    [Header("행동력")]
    [Tooltip("최대 행동력")] public int turnCost;
    [Tooltip("현재 행동력")] public int nowCost;
    [Header("플레이어")]
    [Tooltip("최대 체력")] public int maxHealth;
    [Tooltip("현재 체력")] public int curHealth;
    [Tooltip("현재 실드량")] public int shieldHealth;
    [Tooltip("트리거 조건")] public int playerTriggerMaxCnt;
    [Tooltip("트리거 조건 현재 카운트")] public int playerTriggerCnt;
    [Header("적")]
    [Tooltip("최대 체력")] public int enemyMaxHealth;
    [Tooltip("현재 체력")] public int enemyCurHealth;
    [Tooltip("현재 실드량")] public int enemyShieldHealth;
    [Tooltip("트리거 조건")] public int enemyTriggerMaxCnt;
    [Tooltip("트리거 조건 현재 카운트")] public int enemyTriggerCnt;
    [Header("SO")]
    [Tooltip("플레이어/적 정보")] public CharacterSO characterSO;
    [Tooltip("적 리스트")] public EnemySO enemySO;

    // 로딩 여부. 로딩중일 경우 인터랙션 불가.
    [HideInInspector] public bool isLoading;

    WaitForSeconds delay05 = new WaitForSeconds(0.5f);
    WaitForSeconds delay07 = new WaitForSeconds(0.7f);
    WaitForSeconds delay10 = new WaitForSeconds(1.0f);
    WaitForSeconds delay15 = new WaitForSeconds(1.5f);

    [HideInInspector] public static Action BeforePlayerTurnStart;
    [HideInInspector] public static Action OnPlayerTurnStart;
    [HideInInspector] public static Action OnPlayerTurnEnd;
    [HideInInspector] public static Action OnEnemyTurnStart;
    [HideInInspector] public static Action OnEnemyTurnEnd;
    [HideInInspector] public static Action OnGameStart;
    [HideInInspector] public static Action OnGameEnd;
    [HideInInspector] public static Action<Card> OnUseCard;
    [HideInInspector] public static Action OnAddCard;
    [HideInInspector] public static Action OnDiscardCard;
    [HideInInspector] public static Action<int, EDamageSource> OnPlayerDamaged;
    [HideInInspector] public static Action<int, EDamageSource> OnPlayerHealed;
    [HideInInspector] public static Action<int, EDamageSource> OnPlayerShielded;
    [HideInInspector] public static Action<int> OnPlayerHealthChange;
    [HideInInspector] public static Action OnPlayerTrigger;
    [HideInInspector] public static Action<int> OnPlayerTriggerIncrease;
    [HideInInspector] public static Action<int> OnPlayerTriggerDecrease;
    [HideInInspector] public static Action<int, EDamageSource> OnEnemyDamaged;
    [HideInInspector] public static Action<int, EDamageSource> OnEnemyHealed;
    [HideInInspector] public static Action<int, EDamageSource> OnEnemyShielded;
    [HideInInspector] public static Action OnEnemyTrigger;
    [HideInInspector] public static Action<int> OnEnemyTriggerIncrease;
    [HideInInspector] public static Action<int> OnEnemyTriggerDecrease;
    [HideInInspector] public static Action OnEnemyAction;
    [HideInInspector] public static Action<bool, int> OnRouletteSpin;
    [HideInInspector] public static Action<int> AfterRouletteSpin;
    [HideInInspector] public static Action OnRouletteTrigger;
    [HideInInspector] public static Func<int, RouletteType, bool> CheckRouletteEnchantable;
    [HideInInspector] public static Action<int> OnRouletteEnchant;
    [HideInInspector] public static Action OnRouletteActivate;
    [HideInInspector] public static Action<int> OnCostChange;
    [HideInInspector] public static Action OnUseableItemUse;

    public static void PrintAllActions(Action<int> action)
    {
        if (action == null)
        {
            Debug.Log("아무 액션도 등록되어 있지 않습니다.");
            return;
        }

        foreach (var d in action.GetInvocationList())
        {
            Debug.Log($"액션: {d.Method.Name},  소속 객체: {Utils.GetOwningType(d)}");
        }
    }

    // 개발자 설정 적용
    void GameDeveloperSetup()
    {
        // 카드 배분 속도 조정
        if (fastMode)
        {
            delay05 = new WaitForSeconds(0.05f);
        }
    }

    // characterSO 정보 적용
    void InitializeCharacters()
    {
        // 플레이어 정보 적용
        maxHealth = characterSO.maxHealth;
        curHealth = characterSO.curHealth;
        // 적 정보 적용
        EnemyManager.Inst.InitEnemy();
    }

    // 게임 매니저 초기화
    void InitializeManagers()
    {
        RelicManager.Inst.ActivateRelics();
        RouletteManager.Inst.InitRoulette();
        CardManager.Inst.InitializeItemBuffer();
        CardManager.Inst.ShuffleDeck();
        BuffManager.Inst.InitAllBuffs();
    }

    // 게임 시작 전 초기화
    public void InitializeGame()
    {
        GameDeveloperSetup();
        isLoading = true;
        turnNum = 0;
        InitializeCharacters();
        PassiveManager.Inst.SetPersona();
        PassiveManager.Inst.SetShadow();
        InitializeManagers();
    }

    // 게임 시작
    public void StartGameCo()
    {
        Utils.AllignActions(ref OnGameStart, typeof(ShowBuff), typeof(RelicManager));
        OnGameStart?.Invoke();
        //BuffManager.Inst.AddShowBuff("회전 봉인", EBuffAffectType.Enemy, 2, false);
        turnDraw = drawCardCount;
        // startCardCount만큼 카드를 뽑고, StartPlayerTurn 호출
        StartCoroutine(Draw(startCardCount, StartPlayerTurn));
    }

    // 플레이어 턴 시작
    public void StartPlayerTurn()
    {
        Utils.AllignActions(ref BeforePlayerTurnStart, typeof(ShowBuff), typeof(RelicManager));
        BeforePlayerTurnStart?.Invoke();
        isLoading = true;
        turnNum++;
        IncreaseCost(-nowCost);
        SetFullCost();
        // 플레이어 턴 시작 UI를 띄우고, StartPlayerTurn_AfterNotify 호출
        GameManager.Inst.Notification("나의 턴", "턴 " + turnNum.ToString(), StartPlayerTurn_AfterNotify);
    }

    // 플레이어 턴 시작 - UI 호출 이후
    void StartPlayerTurn_AfterNotify()
    {
        shieldHealth = 0;
        // 플레이어 턴 시작 시 호출해야 할 액션(함수) 목록 모두 호출
        Utils.AllignActions(ref OnPlayerTurnStart, typeof(ShowBuff), typeof(RelicManager));
        OnPlayerTurnStart?.Invoke();
        // turnDraw만큼 카드를 뽑고, 로딩을 종료 (플레이어 인터랙션 가능)
        StartCoroutine(Draw(turnDraw, () => isLoading = false));
    }

    // 플레이어 턴 종료
    public void EndPlayerTurn()
    {
        isLoading = true;
        Utils.AllignActions(ref OnPlayerTurnEnd, typeof(ShowBuff), typeof(RelicManager));
        OnPlayerTurnEnd?.Invoke();
        Discard();
        // 적 턴 시작 UI를 띄우고, StartPlayerTurn_AfterNotify 호출
        GameManager.Inst.Notification("적 턴", "턴 " + turnNum.ToString(), EnemyManager.Inst.StartEnemyTurn);
    }

    // 카드 드로우
    public IEnumerator Draw(int drawNum, Action onComplete)
    {
        for (int i = 0; i < drawNum; i++)
        {
            yield return delay05;
            Utils.AllignActions(ref OnAddCard, typeof(ShowBuff), typeof(RelicManager));
            OnAddCard?.Invoke();
        }
        onComplete?.Invoke();
    }

    // 카드 버림 (카드 사용시 카드는 버려짐. 버려진 카드는 무덤으로 감.)
    public void Discard()
    {
        Utils.AllignActions(ref OnDiscardCard, typeof(ShowBuff), typeof(RelicManager));
        OnDiscardCard?.Invoke();
    }

    // 플레이어 체력 변동 (데미지 or 힐, 실드 고려). 플레이어 생존 여부 반환
    public int TakeDmg(int damage, EDamageSource damageSource)
    {
        if (damage > 0)
        {
            damage = BuffManager.Inst.GetBuffedPlayerDamage(damageSource, damage);
            if (damage < 0)
            {
                damage = 0;
                return 0;
            }
            Utils.AllignActions(ref OnPlayerDamaged, typeof(ShowBuff), typeof(RelicManager));
            OnPlayerDamaged?.Invoke(damage, damageSource);
        }
        else
        {
            damage = -BuffManager.Inst.GetBuffedPlayerHeal(damageSource, -damage);
            if (damage > 0)
            {
                damage = 0;
                return 0;
            }
            Utils.AllignActions(ref OnPlayerHealed, typeof(ShowBuff), typeof(RelicManager));
            OnPlayerHealed?.Invoke(-damage, damageSource);
        }
        if (curHealth + shieldHealth > damage)
        {
            if (damage > 0)
            {
                if (shieldHealth >= damage)
                {
                    shieldHealth -= damage;
                    return 0;
                }
                else
                {
                    damage -= shieldHealth;
                    shieldHealth = 0;
                }
            }
            if (curHealth - damage > maxHealth)
            {
                damage = curHealth - maxHealth;
            }
            
            curHealth -= damage;
            Utils.AllignActions(ref OnPlayerHealthChange, typeof(ShowBuff), typeof(RelicManager));
            OnPlayerHealthChange?.Invoke(damage);
            return damage;
        }
        else
        {
            damage = curHealth;
            curHealth = 0;
            StartCoroutine(GameManager.Inst.GameOver(false));
            Utils.AllignActions(ref OnPlayerHealthChange, typeof(ShowBuff), typeof(RelicManager));
            OnPlayerHealthChange?.Invoke(damage);
            return damage;
        }
    }

    // 적 체력 변동 (데미지 or 힐, 실드 고려). 적 생존 여부 반환
    public int EnemyTakeDmg(int damage, EDamageSource damageSource)
    {
        if (damage > 0)
        {
            damage = BuffManager.Inst.GetBuffedEnemyDamage(damageSource, damage);
            if (damage < 0)
            {
                damage = 0;
                return 0;
            }
            Utils.AllignActions(ref OnEnemyDamaged, typeof(ShowBuff), typeof(RelicManager));
            OnEnemyDamaged?.Invoke(damage, damageSource);
        }
        else
        {
            damage = -BuffManager.Inst.GetBuffedPlayerHeal(damageSource, -damage);
            if (damage > 0)
            {
                damage = 0;
                return 0;
            }
            Utils.AllignActions(ref OnEnemyHealed, typeof(ShowBuff), typeof(RelicManager));
            OnEnemyHealed?.Invoke(-damage, damageSource);
        }
        if (enemyCurHealth + enemyShieldHealth > damage)
        {
            if (damage > 0)
            {
                if (enemyShieldHealth >= damage)
                {
                    enemyShieldHealth -= damage;
                    return 0;
                }
                else
                {
                    damage -= enemyShieldHealth;
                    enemyShieldHealth = 0;
                }
            }
            if (enemyCurHealth - damage > enemyMaxHealth)
            {
                damage = enemyCurHealth - enemyMaxHealth;
            }
            enemyCurHealth -= damage;
            return damage;
        }
        else
        {
            damage = enemyCurHealth;
            enemyCurHealth = 0;
            StartCoroutine(GameManager.Inst.GameOver(true));
            return damage;
        }
    }

    public void IncreaseCost(int value)
    {
        nowCost += value;
        Utils.AllignActions(ref OnCostChange, typeof(ShowBuff), typeof(RelicManager));
        OnCostChange?.Invoke(value);
    }

    public void SetFullCost()
    {
        IncreaseCost(turnCost - nowCost);
    }

    public void GetShield(bool isEnemy, int value, EDamageSource source)
    {
        if (isEnemy)
        {
            value = BuffManager.Inst.GetBuffedEnemyShield(source, value);
            enemyShieldHealth += value;
            if (enemyShieldHealth < 0) enemyShieldHealth = 0;
            Utils.AllignActions(ref OnEnemyShielded, typeof(ShowBuff), typeof(RelicManager));
            OnEnemyShielded?.Invoke(value, source);
        }
        else
        {
            value = BuffManager.Inst.GetBuffedPlayerShield(source, value);
            shieldHealth += value;
            if (shieldHealth < 0) shieldHealth = 0;
            Utils.AllignActions(ref OnPlayerShielded, typeof(ShowBuff), typeof(RelicManager));
            OnPlayerShielded?.Invoke(value, source);
        }
    }

    // 플레이어 트리거 카운터 증가. 카운터 모두 채워졌을 시 발동
    public void TriggerPlayerPassive(int value)
    {
        if (RouletteManager.Inst.isTriggerActivated && RouletteManager.Inst.isPlayerTrigger()) return;
        if (value > 0)
        {
            Utils.AllignActions(ref OnPlayerTriggerIncrease, typeof(ShowBuff), typeof(RelicManager));
            OnPlayerTriggerIncrease?.Invoke(value);
        }
        else
        {
            Utils.AllignActions(ref OnPlayerTriggerDecrease, typeof(ShowBuff), typeof(RelicManager));
            OnPlayerTriggerDecrease?.Invoke(value);
        }
        if (playerTriggerCnt < playerTriggerMaxCnt)
        {
            playerTriggerCnt += value;
            if (playerTriggerCnt > playerTriggerMaxCnt)
            {
                playerTriggerCnt = playerTriggerMaxCnt;
            }
            if (playerTriggerCnt < 0)
            {
                playerTriggerCnt = 0;
            }
        }
        if (playerTriggerCnt != 0 && playerTriggerCnt == playerTriggerMaxCnt)
        {
            RouletteManager.Inst.TriggerRoulette();
            playerTriggerCnt = 0;
        }
    }

    // 적 트리거 카운터 증가. 카운터 모두 채워졌을 시 발동
    public void TriggerEnemyPassive(int value)
    {
        if (RouletteManager.Inst.isTriggerActivated && RouletteManager.Inst.isEnemyTrigger()) return;
        if (value > 0)
        {
            Utils.AllignActions(ref OnEnemyTriggerIncrease, typeof(ShowBuff), typeof(RelicManager));
            OnEnemyTriggerIncrease?.Invoke(value);
        }
        else
        {
            Utils.AllignActions(ref OnEnemyTriggerDecrease, typeof(ShowBuff), typeof(RelicManager));
            OnEnemyTriggerDecrease?.Invoke(value);
        }
        if (enemyTriggerCnt < enemyTriggerMaxCnt)
        {
            enemyTriggerCnt += value;
            if (enemyTriggerCnt > enemyTriggerMaxCnt)
            {
                enemyTriggerCnt = enemyTriggerMaxCnt;
            }
            if(enemyTriggerCnt < 0)
            {
                enemyTriggerCnt = 0;
            }

            if (enemyTriggerCnt != 0 && enemyTriggerCnt == enemyTriggerMaxCnt)
            {
                EnemyManager.Inst.EnemyTriggerAction();
            }
        }
    }
}
