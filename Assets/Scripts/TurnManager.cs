using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public enum EDamageSource
{
    Enemy, Roulette, UseableItem, Buff, Relic, Card, Passive, Stats, TriggerRoulette
};
public class TurnManager : MonoBehaviour
{
    public static TurnManager Inst { get; private set; }
    private void Awake()
    {
        Inst = this;
        enemyMaxHealth = new int[Enemy.maxSubEnemyNum + 1];
        enemyCurHealth = new int[Enemy.maxSubEnemyNum + 1];
        enemyShieldHealth = new int[Enemy.maxSubEnemyNum + 1];
    }

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
    [Tooltip("추가 행동력")] public int extraCost;
    [Header("플레이어")]
    [Tooltip("최대 체력")] public int maxHealth;
    [Tooltip("현재 체력")] public int curHealth;
    [Tooltip("현재 실드량")] public int shieldHealth;
    [Tooltip("트리거 조건")] public int playerTriggerMaxCnt;
    [Tooltip("트리거 조건 현재 카운트")] public int playerTriggerCnt;
    [Header("적")]
    [Tooltip("최대 체력")] public int[] enemyMaxHealth = new int[Enemy.maxSubEnemyNum + 1];
    [Tooltip("현재 체력")] public int[] enemyCurHealth = new int[Enemy.maxSubEnemyNum + 1];
    [Tooltip("현재 실드량")] public int[] enemyShieldHealth = new int[Enemy.maxSubEnemyNum + 1];
    [Tooltip("트리거 조건")] public int enemyTriggerMaxCnt;
    [Tooltip("트리거 조건 현재 카운트")] public int enemyTriggerCnt;
    [Header("SO")]
    [Tooltip("플레이어/적 정보")] public CharacterSO characterSO;
    [Tooltip("적 리스트")] public EnemySO enemySO;

    // 로딩 여부. 로딩중일 경우 인터랙션 불가.
    public bool isLoading;

    WaitForSeconds delay05 = new WaitForSeconds(0.5f);

    [HideInInspector] public static Action BeforePlayerTurnStart;
    [HideInInspector] public static Action OnPlayerTurnStart;
    [HideInInspector] public static Action OnPlayerTurnEnd;
    [HideInInspector] public static Action OnEnemyTurnStart;
    [HideInInspector] public static Action OnEnemyTurnEnd;
    [HideInInspector] public static Action OnGameStart;
    [HideInInspector] public static Action<bool> OnGameEnd;
    [HideInInspector] public static Action<Card, int> OnUseCard;
    [HideInInspector] public static Action OnAddCard;
    [HideInInspector] public static Action OnDiscardCard;
    [HideInInspector] public static Action OnVanishCard;
    [HideInInspector] public static Action OnSelectCardDone;
    [HideInInspector] public static Action<int, EDamageSource> OnPlayerDamaged;
    [HideInInspector] public static Action<int, EDamageSource> OnPlayerHealed;
    [HideInInspector] public static Action<int, EDamageSource> OnPlayerShielded;
    [HideInInspector] public static Action<int> OnPlayerHealthChange;
    [HideInInspector] public static Action OnPlayerTrigger;
    [HideInInspector] public static Action<int> OnPlayerTriggerIncrease;
    [HideInInspector] public static Action<int> OnPlayerTriggerDecrease;
    [HideInInspector] public static Action<int, EDamageSource, int> OnEnemyDamaged;
    [HideInInspector] public static Action<int, EDamageSource, int> OnEnemyHealed;
    [HideInInspector] public static Action<int, EDamageSource, int> OnEnemyShielded;
    [HideInInspector] public static Action OnEnemyTrigger;
    [HideInInspector] public static Action<int> OnEnemyTriggerIncrease;
    [HideInInspector] public static Action<int> OnEnemyTriggerDecrease;
    [HideInInspector] public static Action<EnemyAction> OnEnemyAction;
    [HideInInspector] public static Action<int> OnSubEnemyDestroy;
    [HideInInspector] public static Action<bool, int> OnRouletteSpin;
    [HideInInspector] public static Action<int> AfterRouletteSpin;
    [HideInInspector] public static Action OnRouletteTrigger;
    [HideInInspector] public static Func<int, RouletteType, bool> CheckRouletteEnchantable;
    [HideInInspector] public static Action<int> OnRouletteEnchant;
    [HideInInspector] public static Action<int> AfterRouletteEnchant;
    [HideInInspector] public static Action<int> OnRouletteErase;
    [HideInInspector] public static Action<int> OnRouletteEnhance;
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
        playerTriggerCnt = 0;
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
        // BuffManager.Inst.AddShowBuff("강화", EBuffAffectType.Enemy, 2, false);
        // BuffManager.Inst.AddShowBuff("보호", EBuffAffectType.Enemy, 2, false);
        // BuffManager.Inst.AddShowBuff("활력", EBuffAffectType.Enemy, 2, false);
        // BuffManager.Inst.AddShowBuff("주저함", EBuffAffectType.Enemy, 2, false);
        // BuffManager.Inst.AddShowBuff("환영", EBuffAffectType.Enemy, 2, false);
        // BuffManager.Inst.AddShowBuff("놀이 시간", EBuffAffectType.Player, 2, false);
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
        IncreaseCost(-nowCost, true);
        extraCost = 0;
        SetFullCost();
        // 플레이어 턴 시작 UI를 띄우고, StartPlayerTurn_AfterNotify 호출
        GameManager.Inst.Notification("나의 턴", "턴 " + turnNum.ToString(), StartPlayerTurn_AfterNotify);
        GetComponent<AudioSource>().PlayOneShot(SoundManager.Inst.playerTurnStartSFX);
    }

    // 플레이어 턴 시작 - UI 호출 이후
    void StartPlayerTurn_AfterNotify()
    {
        shieldHealth = 0;
        // 플레이어 턴 시작 시 호출해야 할 액션(함수) 목록 모두 호출
        Utils.AllignActions(ref OnPlayerTurnStart, typeof(ShowBuff), typeof(RelicManager));
        OnPlayerTurnStart?.Invoke();

        if(characterSO.isTutorial)
        {
            if(CardManager.Inst.itemDraw.Count < turnDraw)
            {
                List<Item> tempCardList = new List<Item>();
                for(int i = 0; i < CardManager.Inst.itemDraw.Count; i++)
                {
                    tempCardList.Add(CardManager.Inst.itemDraw[i]);
                }
                CardManager.Inst.itemDraw.Clear();
                while(CardManager.Inst.itemDiscard.Count > 0)
                {
                    CardManager.Inst.itemDraw.Add(CardManager.Inst.itemDiscard[0]);
                    CardManager.Inst.itemDiscard.RemoveAt(0);
                }
                CardManager.Inst.ShuffleDeck();
                for(int i = 0; i < tempCardList.Count; i++)
                {
                    CardManager.Inst.itemDraw.Insert(i, tempCardList[i]);
                }
            }
            switch (characterSO.enemyName)
            {
                case "카드 병정 2":
                case "CardSoldier2":
                    if(turnNum == 1)
                    {
                        Item item_shield = CardManager.Inst.itemDeck.Find(card => card.name == "수비 부여");
                        Item item_hide = CardManager.Inst.itemDeck.Find(card => card.name == "숨기");
                        CardManager.Inst.itemDraw.Remove(item_shield);
                        CardManager.Inst.itemDraw.Remove(item_hide);
                        CardManager.Inst.itemDraw.Insert(turnDraw * 2 - 2, item_shield);
                        CardManager.Inst.itemDraw.Insert(turnDraw * 2 - 1, item_hide);
                        StartCoroutine(Draw(turnDraw, () =>
                        {
                            TutorialManager.Inst.ShowTutorialBox(2, 1, 1);
                        }));
                    }
                    else if(turnNum == 2)
                    {
                        StartCoroutine(Draw(turnDraw, () =>
                        {
                            TutorialManager.Inst.ShowTutorialBox(2, 2, 1);
                        }));
                    }
                    else if(turnNum == 3)
                    {
                        StartCoroutine(Draw(turnDraw, () =>
                        {
                            TutorialManager.Inst.ShowTutorialBox(2, 3, 1);
                        }));
                    }
                    else
                    {
                        isLoading = false;
                        TutorialManager.Inst.tutorialStage = 0;
                        StartCoroutine(Draw(turnDraw, () =>
                        {
                            isLoading = false;
                        }));
                    }
                    break;
                case "카드 병정":
                case "CardSoldier":
                    if(turnNum == 1)
                    {
                        Item item_turn2 = new Item(CardManager.Inst.itemDeck.Find(card => card.name == "2칸 회전"));
                        Item item_turn3 = new Item(CardManager.Inst.itemDeck.Find(card => card.name == "3칸 회전"));
                        CardManager.Inst.itemDeck.Add(item_turn2);
                        CardManager.Inst.itemDeck.Add(item_turn3);
                        CardManager.Inst.itemDraw.Insert(turnDraw - 2, item_turn3);
                        CardManager.Inst.itemDraw.Insert(turnDraw - 1, item_turn2);
                        StartCoroutine(Draw(turnDraw, () =>
                        {
                            TutorialManager.Inst.ShowTutorialBox(1, 1, 1);
                        }));
                    }
                    else if(turnNum == 2)
                    {
                        StartCoroutine(Draw(turnDraw, () =>
                        {
                            TutorialManager.Inst.ShowTutorialBox(0, 0, 0);
                        }));
                    }
                    else
                    {
                        isLoading = false;
                        TutorialManager.Inst.tutorialStage = 0;
                        StartCoroutine(Draw(turnDraw, () =>
                        {
                            isLoading = false;
                        }));
                    }
                    break;
            }
        }
        else
        {
            // turnDraw만큼 카드를 뽑고, 로딩을 종료 (플레이어 인터랙션 가능)
            StartCoroutine(Draw(turnDraw, () =>
            {
                isLoading = false;
            }));
        }
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
        GetComponent<AudioSource>().PlayOneShot(SoundManager.Inst.enemyTurnStartSFX);
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

    public void StartDraw(int drawNum, Action onComplete)
    {
        StartCoroutine(Draw(drawNum, onComplete));
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
            if(damageSource != EDamageSource.Card)
            {
                damage = BuffManager.Inst.GetBuffedPlayerDamage(damageSource, damage);
            }
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
            if(damageSource != EDamageSource.Card)
            {
                damage = -BuffManager.Inst.GetBuffedPlayerHeal(damageSource, -damage);
            }
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
            if(characterSO.isTutorial)
            {
                damage = curHealth - 1;
                curHealth = 1;
                shieldHealth = 0;
                Utils.AllignActions(ref OnPlayerHealthChange, typeof(ShowBuff), typeof(RelicManager));
                OnPlayerHealthChange?.Invoke(damage);
                return damage;
            }
            damage = curHealth;
            curHealth = 0;
            StartCoroutine(GameManager.Inst.GameOver(false));
            Utils.AllignActions(ref OnPlayerHealthChange, typeof(ShowBuff), typeof(RelicManager));
            OnPlayerHealthChange?.Invoke(damage);
            return damage;
        }
    }

    // 적 체력 변동 (데미지 or 힐, 실드 고려). 적 생존 여부 반환
    public int EnemyTakeDmg(int damage, EDamageSource damageSource, int enemyIdx = 0)
    {
        if (damage > 0)
        {
            if(damageSource != EDamageSource.Card)
            {
                damage = BuffManager.Inst.GetBuffedEnemyDamage(damageSource, damage);
            }
            if (damage < 0)
            {
                damage = 0;
                return 0;
            }
            Utils.AllignActions(ref OnEnemyDamaged, typeof(ShowBuff), typeof(RelicManager));
            OnEnemyDamaged?.Invoke(damage, damageSource, enemyIdx);
        }
        else
        {
            if(damageSource != EDamageSource.Card)
            {
                damage = -BuffManager.Inst.GetBuffedEnemyHeal(damageSource, -damage);
            }
            if (damage > 0)
            {
                damage = 0;
                return 0;
            }
            Utils.AllignActions(ref OnEnemyHealed, typeof(ShowBuff), typeof(RelicManager));
            OnEnemyHealed?.Invoke(-damage, damageSource, enemyIdx);
        }
        if (enemyCurHealth[enemyIdx] + enemyShieldHealth[enemyIdx] > damage)
        {
            if (damage > 0)
            {
                if (enemyShieldHealth[enemyIdx] >= damage)
                {
                    enemyShieldHealth[enemyIdx] -= damage;
                    return 0;
                }
                else
                {
                    damage -= enemyShieldHealth[enemyIdx];
                    enemyShieldHealth[enemyIdx] = 0;
                }
            }
            if (enemyCurHealth[enemyIdx] - damage > enemyMaxHealth[enemyIdx])
            {
                damage = enemyCurHealth[enemyIdx] - enemyMaxHealth[enemyIdx];
            }
            enemyCurHealth[enemyIdx] -= damage;
            return damage;
        }
        else
        {
            damage = enemyCurHealth[enemyIdx];
            enemyCurHealth[enemyIdx] = 0;
            if(enemyIdx == 0) StartCoroutine(GameManager.Inst.GameOver(true));
            else EnemyManager.Inst.DestroySubEnemy(enemyIdx - 1);
            return damage;
        }
    }

    public void IncreaseCost(int value, bool isRestore = false)
    {
        if(nowCost + value < 0) value = -nowCost;
        if(isRestore && nowCost + value > turnCost) value = turnCost - nowCost;
        nowCost += value;
        // if(!isRestore && extraCost + value >= 0) extraCost += value;
        Utils.AllignActions(ref OnCostChange, typeof(ShowBuff), typeof(RelicManager));
        OnCostChange?.Invoke(value);
    }

    public void SetFullCost()
    {
        IncreaseCost(turnCost - nowCost, true);
    }

    public void GetShield(bool isEnemy, int value, EDamageSource source, int enemyIdx = 0)
    {
        if (isEnemy)
        {
            value = BuffManager.Inst.GetBuffedEnemyShield(source, value);
            enemyShieldHealth[enemyIdx] += value;
            if (enemyShieldHealth[enemyIdx] < 0) enemyShieldHealth[enemyIdx] = 0;
            Utils.AllignActions(ref OnEnemyShielded, typeof(ShowBuff), typeof(RelicManager));
            OnEnemyShielded?.Invoke(value, source, enemyIdx);
        }
        else
        {
            if(source != EDamageSource.Card)
            {
                value = BuffManager.Inst.GetBuffedPlayerShield(source, value);
            }
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
            StartCoroutine(RouletteManager.Inst.TriggerRoulette());
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

    void LateUpdate()
    {
        if(GameManager.Inst.gameOverSignal == true)
        {
            isLoading = true;
        }
    }
}
