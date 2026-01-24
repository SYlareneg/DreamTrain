using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Inst { get; private set; }
    void Awake() => Inst = this;

    [Header("게임 UI")]
    [SerializeField][Tooltip("화면 중심 안내 UI")] NotificationPanel notificationPanel;
    [SerializeField][Tooltip("게임 종료 UI")] ResultPanel resultPanel;
    [SerializeField][Tooltip("턴 종료 버튼")] GameObject endTurnBtn;
    [SerializeField][Tooltip("턴 텍스트")] TMP_Text turnNotificationTMP;
    [SerializeField][Tooltip("룰렛 버프 위치")] Vector2 rouletteBuffPos;
    [Tooltip("룰렛 버프")] public GameObject rouletteBuffUIView;
    [Header("카드 UI")]
    [SerializeField][Tooltip("카드 목록 UI")] GameObject cardListView;
    [SerializeField][Tooltip("카드 목록 UI")] GameObject cardScrollView_Deck;
    [SerializeField][Tooltip("카드 목록 UI")] GameObject cardScrollView_Draw;
    [SerializeField][Tooltip("카드 목록 UI")] GameObject cardScrollView_Discard;
    [Tooltip("카드 목록 content - 덱")] public GameObject cardListContent_Deck;
    [Tooltip("카드 목록 content - 드로우")] public GameObject cardListContent_Draw;
    [Tooltip("카드 목록 content - 무덤")] public GameObject cardListContent_Discard;
    [SerializeField][Tooltip("카드 목록 버튼 - 덱")] Button deckListBtn;
    [SerializeField][Tooltip("카드 목록 버튼 - 드로우")] Button drawListBtn;
    [SerializeField][Tooltip("카드 목록 버튼 - 무덤")] Button discardListBtn;
    [SerializeField][Tooltip("카드 목록 버튼 - pressed Image")] Sprite buttonPressedImg;
    [SerializeField][Tooltip("카드 목록 버튼 - normal Image")] Sprite buttonNormalImg;
    [HideInInspector] public List<CardUI> cardList_Deck;
    [HideInInspector] public List<CardUI> cardList_Draw;
    [HideInInspector] public List<CardUI> cardList_Discard;
    [Header("이드 UI")]
    [SerializeField][Tooltip("이드 목록 UI")] GameObject relicScrollView;
    [Tooltip("이드 목록 content")] public GameObject relicListScroll;
    [Header("플레이어 UI")]
    [SerializeField][Tooltip("플레이어 행동력 값 텍스트")] TMP_Text costTMP;
    [SerializeField][Tooltip("플레이어 체력 값 텍스트")] TMP_Text healthTMP;
    [SerializeField][Tooltip("플레이어 체력 바")] Image healthImg;
    [SerializeField][Tooltip("플레이어 실드 바")] Image shieldImg;
    [SerializeField][Tooltip("플레이어 실드 값 텍스트")] TMP_Text shieldTMP;
    [SerializeField][Tooltip("플레이어 트리거 조건 텍스트")] TMP_Text triggerCountTMP;
    [SerializeField][Tooltip("플레이어 트리거 조건 바")] Image triggerCntImg;
    [SerializeField][Tooltip("플레이어 버프 위치")] Vector2 playerBuffPos;
    [Tooltip("플레이어 버프")] public GameObject playerBuffUIView;
    [Tooltip("플레이어 페르소나")] public Image personaImg;
    [Tooltip("플레이어 그림자")] public Image shadowImg;
    [Tooltip("플레이어 데미지 이펙트")] public GameObject playerDamageEffect;
    [Tooltip("플레이어 데미지 이펙트 스프라이트")] public Sprite[] playerDamageEffectSprites;
    [Tooltip("적 공격 이펙트")] public GameObject enemyAttackEffect;
    [Header("적 UI")]
    [SerializeField][Tooltip("적 체력 값 텍스트")] TMP_Text[] enemyHealthTMP;
    [SerializeField][Tooltip("적 체력 바")] Image[] enemyHealthImg;
    [SerializeField][Tooltip("적 실드 바")] Image[] enemyShieldImg;
    [SerializeField][Tooltip("적 실드 값 텍스트")] TMP_Text[] enemyShieldTMP;
    [SerializeField][Tooltip("적 트리거 조건 텍스트")] TMP_Text enemyTriggerCountTMP;
    [SerializeField][Tooltip("적 트리거 조건 바")] Image enemyTriggerCntImg;
    [SerializeField][Tooltip("적 버프 위치")] Vector2[] enemyBuffPos;
    [Tooltip("적 버프")] public GameObject[] enemyBuffUIView;
    [HideInInspector] public List<RelicUI> relicList;
    [Header("카드 획득 UI")]
    [Tooltip("카드 보상 개수")] public int rewardCardCount = 3;
    [SerializeField][Tooltip("카드 획득 화면")] GameObject rewardCardView;
    [SerializeField][Tooltip("획득 카드 목록 위치")] Transform rewardCardList;
    [SerializeField][Tooltip("카드 보상 prefab")] GameObject rewardCardPrefab;
    [SerializeField][Tooltip("획득 카드 목록")] List<CardUI_Reward> rewardCards;
    [SerializeField][Tooltip("플레이어 정보")] CharacterSO characterSO;
    [SerializeField][Tooltip("카드풀 정보(공용)")] ItemSO normalItemListSO;
    [SerializeField][Tooltip("카드풀 정보(페르소나/그림자)")] DreamPieceSO dreamPieceListSO;
    [Tooltip("카드 등장 확률(가중치)\n{0: 공용, 1+: 꿈조각 전용}")] public float[] rewardCardWeights = new float[Enum.GetNames(typeof(CardRarity)).Length + 1];
    [SerializeField][Tooltip("카드 강화 확률")] float enhanceProbability;
    [Header("기타")]
    [SerializeField][Tooltip("스테이지 적 정보")] StageSO stageSO;
    [HideInInspector] public bool gameOverSignal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        DOTween.SetTweensCapacity(500, 50);
        gameOverSignal = false;
        StartGame();

        playerDamageEffect.SetActive(false);
        TurnManager.OnPlayerDamaged += (damage, source) =>
        {
            if(TurnManager.Inst.shieldHealth > 0) playerDamageEffect.GetComponent<Image>().sprite = playerDamageEffectSprites[1];
            else playerDamageEffect.GetComponent<Image>().sprite = playerDamageEffectSprites[0];
            playerDamageEffect.SetActive(true);
            playerDamageEffect.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0f);
            Sequence damageSeq = DOTween.Sequence();
            damageSeq.Append(playerDamageEffect.GetComponent<Image>().DOColor(new Color(1f, 1f, 1f, 0.6f), 0.6f))
            .Append(playerDamageEffect.GetComponent<Image>().DOColor(new Color(1f, 1f, 1f, 0f), 1.2f))
            .OnComplete(() =>
            {
                playerDamageEffect.SetActive(false);
            });

            Camera.main.transform.DOShakePosition(0.8f, 0.2f, 20, 90f);

            GetComponent<AudioSource>().PlayOneShot(SoundManager.Inst.playerDamageSFX);
        };
    }

    // Update is called once per frame
    private void Update()
    {
        InputCheatKey();
        UpdateUIState();

        Tooltip.showTooltipSignal = cardListView.activeSelf == false && rewardCardView.activeSelf == false;
    }

    // 개발자용 특수입력
    void InputCheatKey()
    {
        if (TurnManager.Inst.isLoading == false)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                Utils.AllignActions(ref TurnManager.OnAddCard, typeof(ShowBuff), typeof(RelicManager));
                TurnManager.OnAddCard?.Invoke();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                TurnManager.Inst.EndPlayerTurn();
            }
            if (Input.GetKeyDown(KeyCode.A))
            {
                RouletteManager.Inst.Spin(false, 1);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                RouletteManager.Inst.Spin(true, 1);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                RouletteManager.Inst.ActivateRoulette();
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                StartCoroutine(RouletteManager.Inst.TriggerRoulette());
            }
            if (Input.GetKeyDown(KeyCode.L))
            {
                Lever.Inst.ActivateLever();
            }
        }
    }

    [SerializeField] float barFillTime = 1f;
    Tween playerHealthbarSeq;
    float playerHealthbarTargetFill;
    Tween playerShieldbarSeq;
    float playerShieldbarTargetFill;
    Tween playerTriggerbarSeq;
    float playerTriggerbarTargetFill;
    Tween[] enemyHealthbarSeq = new Tween[5];
    float[] enemyHealthbarTargetFill = new float[5];
    Tween[] enemyShieldbarSeq = new Tween[5];
    float[] enemyShieldbarTargetFill = new float[5];
    Tween enemyTriggerbarSeq;
    float enemyTriggerbarTargetFill;
    // UI 텍스트, 숨김 여부 설정
    void UpdateUIState()
    {
        costTMP.text = TurnManager.Inst.nowCost.ToString() + "/" + TurnManager.Inst.turnCost.ToString();
        healthTMP.text = TurnManager.Inst.curHealth.ToString() + "/" + TurnManager.Inst.maxHealth.ToString();
        if(!Mathf.Approximately(playerHealthbarTargetFill, (float)TurnManager.Inst.curHealth / (TurnManager.Inst.maxHealth + TurnManager.Inst.shieldHealth)))
        {
            playerHealthbarTargetFill = (float)TurnManager.Inst.curHealth / (TurnManager.Inst.maxHealth + TurnManager.Inst.shieldHealth);
            playerHealthbarSeq?.Kill();
            playerHealthbarSeq = healthImg.DOFillAmount(playerHealthbarTargetFill, barFillTime).SetEase(Ease.OutCubic);
        }
        if(!Mathf.Approximately(playerShieldbarTargetFill, (float)(TurnManager.Inst.curHealth + TurnManager.Inst.shieldHealth) / (TurnManager.Inst.maxHealth + TurnManager.Inst.shieldHealth)))
        {
            playerShieldbarTargetFill = (float)(TurnManager.Inst.curHealth + TurnManager.Inst.shieldHealth) / (TurnManager.Inst.maxHealth + TurnManager.Inst.shieldHealth);
            playerShieldbarSeq?.Kill();
            playerShieldbarSeq = shieldImg.DOFillAmount(playerShieldbarTargetFill, barFillTime).SetEase(Ease.OutCubic);
        }
        if(TurnManager.Inst.shieldHealth > 0) shieldTMP.text = "+" + TurnManager.Inst.shieldHealth.ToString();
        else shieldTMP.text = "";
        triggerCountTMP.text = TurnManager.Inst.playerTriggerCnt.ToString() + "/" + TurnManager.Inst.playerTriggerMaxCnt.ToString();
        if (TurnManager.Inst.playerTriggerMaxCnt == 0)
        {
            triggerCntImg.fillAmount = 0;
        }
        else
        {
            if(!Mathf.Approximately(playerTriggerbarTargetFill, (float)TurnManager.Inst.playerTriggerCnt / TurnManager.Inst.playerTriggerMaxCnt))
            {
                playerTriggerbarTargetFill = (float)TurnManager.Inst.playerTriggerCnt / TurnManager.Inst.playerTriggerMaxCnt;
                playerTriggerbarSeq?.Kill();
                playerTriggerbarSeq = triggerCntImg.DOFillAmount(playerTriggerbarTargetFill, barFillTime).SetEase(Ease.OutCubic);
            }
        }
        for(int i = 0; i < enemyHealthTMP.Length; i++)
        {
            if(enemyHealthTMP[i] == null) continue;
            enemyHealthTMP[i].text = TurnManager.Inst.enemyCurHealth[i].ToString() + "/" + TurnManager.Inst.enemyMaxHealth[i].ToString();
            if(!Mathf.Approximately(enemyHealthbarTargetFill[i], (float)TurnManager.Inst.enemyCurHealth[i] / (TurnManager.Inst.enemyMaxHealth[i] + TurnManager.Inst.enemyShieldHealth[i])))
            {
                enemyHealthbarTargetFill[i] = (float)TurnManager.Inst.enemyCurHealth[i] / (TurnManager.Inst.enemyMaxHealth[i] + TurnManager.Inst.enemyShieldHealth[i]);
                enemyHealthbarSeq[i]?.Kill();
                enemyHealthbarSeq[i] = enemyHealthImg[i].DOFillAmount(enemyHealthbarTargetFill[i], barFillTime).SetEase(Ease.OutCubic);
            }
            if(!Mathf.Approximately(enemyShieldbarTargetFill[i], (float)(TurnManager.Inst.enemyCurHealth[i] + TurnManager.Inst.enemyShieldHealth[i]) / (TurnManager.Inst.enemyMaxHealth[i] + TurnManager.Inst.enemyShieldHealth[i])))
            {
                enemyShieldbarTargetFill[i] = (float)(TurnManager.Inst.enemyCurHealth[i] + TurnManager.Inst.enemyShieldHealth[i]) / (TurnManager.Inst.enemyMaxHealth[i] + TurnManager.Inst.enemyShieldHealth[i]);
                enemyShieldbarSeq[i]?.Kill();
                enemyShieldbarSeq[i] = enemyShieldImg[i].DOFillAmount(enemyShieldbarTargetFill[i], barFillTime).SetEase(Ease.OutCubic);
            }
            if(TurnManager.Inst.enemyShieldHealth[i] > 0) enemyShieldTMP[i].text = "+" + TurnManager.Inst.enemyShieldHealth[i].ToString();
            else enemyShieldTMP[i].text = "";
        }
        enemyTriggerCountTMP.text = TurnManager.Inst.enemyTriggerCnt.ToString() + "/" + TurnManager.Inst.enemyTriggerMaxCnt.ToString();
        if (TurnManager.Inst.enemyTriggerMaxCnt == 0)
        {
            enemyTriggerCntImg.fillAmount = 0;
        }
        else
        {
            if(!Mathf.Approximately(enemyTriggerbarTargetFill, (float)TurnManager.Inst.enemyTriggerCnt / TurnManager.Inst.enemyTriggerMaxCnt))
            {
                enemyTriggerbarTargetFill = (float)TurnManager.Inst.enemyTriggerCnt / TurnManager.Inst.enemyTriggerMaxCnt;
                enemyTriggerbarSeq?.Kill();
                enemyTriggerbarSeq = enemyTriggerCntImg.DOFillAmount(enemyTriggerbarTargetFill, barFillTime).SetEase(Ease.OutCubic);
            }
        }
    }

    public void SetSubEnemyUI(int subEnemyIdx, Transform subEnemyTransform)
    {
        enemyHealthTMP[subEnemyIdx + 1] = subEnemyTransform.Find("SubEnemyUI/Values/Health/HealthTMP").GetComponent<TMP_Text>();
        enemyHealthImg[subEnemyIdx + 1] = subEnemyTransform.Find("SubEnemyUI/Values/Health/HealthBar/HealthBarFront").GetComponent<Image>();
        enemyShieldTMP[subEnemyIdx + 1] = subEnemyTransform.Find("SubEnemyUI/Values/Health/ShieldTMP").GetComponent<TMP_Text>();
        enemyShieldImg[subEnemyIdx + 1] = subEnemyTransform.Find("SubEnemyUI/Values/Health/HealthBar/ShieldBar").GetComponent<Image>();
        RectTransform buffPosRect = subEnemyTransform.Find("SubEnemyUI/BuffPos").GetComponent<RectTransform>();
        enemyBuffPos[subEnemyIdx + 1] = RectTransformUtility.WorldToScreenPoint(Camera.main, buffPosRect.position) - new Vector2(Screen.width / 2, Screen.height / 2);
        enemyBuffUIView[subEnemyIdx + 1] = subEnemyTransform.Find("SubEnemyUI/Buffs").gameObject;
    }

    public void RemoveSubEnemyUI(int subEnemyIdx)
    {
        enemyHealthTMP[subEnemyIdx + 1] = null;
        enemyHealthImg[subEnemyIdx + 1] = null;
        enemyShieldImg[subEnemyIdx + 1] = null;
        enemyShieldTMP[subEnemyIdx + 1] = null;
        enemyBuffPos[subEnemyIdx + 1] = new Vector2(0, 0);
        enemyBuffUIView[subEnemyIdx + 1] = null;
    }

    public void StartGame()
    {
        if (TurnManager.Inst.characterSO.personaPiece != null)
        {
            Tooltip tooltip = personaImg.GetComponentInParent<Tooltip>();
            Tooltip triggerBarTooltip = triggerCntImg.transform.parent.GetComponent<Tooltip>();
            if (TurnManager.Inst.characterSO.personaPiece.persona.isEnhanced)
            {
                personaImg.sprite = TurnManager.Inst.characterSO.personaPiece.persona.enhancedPassive.sprite;
                if (tooltip)
                {
                    tooltip.tooltipTitle = TurnManager.Inst.characterSO.personaPiece.persona.enhancedPassive.name;
                    tooltip.tooltipTxt = TurnManager.Inst.characterSO.personaPiece.persona.enhancedPassive.text;
                }
                if(triggerBarTooltip)
                {
                    triggerBarTooltip.tooltipTitle = TurnManager.Inst.characterSO.personaPiece.persona.enhancedPassive.name;
                    triggerBarTooltip.tooltipTxt = TurnManager.Inst.characterSO.personaPiece.persona.enhancedPassive.text;
                }
            }
            else
            {
                personaImg.sprite = TurnManager.Inst.characterSO.personaPiece.persona.sprite;
                if (tooltip)
                {
                    tooltip.tooltipTitle = TurnManager.Inst.characterSO.personaPiece.persona.name;
                    tooltip.tooltipTxt = TurnManager.Inst.characterSO.personaPiece.persona.text;
                }
                if (triggerBarTooltip)
                {
                    triggerBarTooltip.tooltipTitle = TurnManager.Inst.characterSO.personaPiece.persona.name;
                    triggerBarTooltip.tooltipTxt = TurnManager.Inst.characterSO.personaPiece.persona.text;
                }
            }
        }
        if (TurnManager.Inst.characterSO.shadowPiece != null)
        {
            Tooltip tooltip = shadowImg.GetComponentInParent<Tooltip>();
            if (TurnManager.Inst.characterSO.shadowPiece.shadow.isEnhanced)
            {
                shadowImg.sprite = TurnManager.Inst.characterSO.shadowPiece.shadow.enhancedPassive.sprite;
                if (tooltip)
                {
                    tooltip.tooltipTitle = TurnManager.Inst.characterSO.shadowPiece.shadow.enhancedPassive.name;
                    tooltip.tooltipTxt = TurnManager.Inst.characterSO.shadowPiece.shadow.enhancedPassive.text;
                }
            }
            else
            {
                shadowImg.sprite = TurnManager.Inst.characterSO.shadowPiece.shadow.sprite;
                if (tooltip)
                {
                    tooltip.tooltipTitle = TurnManager.Inst.characterSO.shadowPiece.shadow.name;
                    tooltip.tooltipTxt = TurnManager.Inst.characterSO.shadowPiece.shadow.text;
                }
            }
        }
        TurnManager.Inst.InitializeGame();
        SceneChangeManager.Inst.SceneFadeIn(TurnManager.Inst.StartGameCo);
    }

    // 게임 종료
    public IEnumerator GameOver(bool isMyWin)
    {
        gameOverSignal = true;
        Utils.AllignActions(ref TurnManager.OnGameEnd, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnGameEnd?.Invoke(isMyWin);
        TurnManager.Inst.isLoading = true;
        Tooltip.showTooltipSignal = false;
        endTurnBtn.SetActive(false);
        yield return new WaitForSeconds(0.5f);

        if(isMyWin == false)
        {
            resultPanel.Show("패배");
        }
        else
        {
            Notification("승리", "", () =>
            {
                if(characterSO.isTutorial)
                {
                    characterSO.curHealth = TurnManager.Inst.maxHealth;
                    SceneChangeManager.Inst.SceneFadeOut("MapScene");
                }
                else
                {
                    ShowCardReward();
                }
            });
        }
    }
    
    // 화면 중심 안내 UI 호출
    public void Notification(string title, string message, Action onComplete)
    {
        Tooltip.showTooltipSignal = false;
        turnNotificationTMP.text = message;
        Action callback = () =>
        {
            Tooltip.showTooltipSignal = true;
        };
        callback += onComplete;
        notificationPanel.Show(title, callback);
    }

    public enum ListType { Deck, Draw, Discard };

    // 카드 목록 UI 호출

    public void ShowCardList()
    {
        if (cardListView.activeSelf == false)
        {
            TurnManager.Inst.isLoading = true;
            Tooltip.showTooltipSignal = false;
            foreach (CardUI card in cardList_Deck)
            {
                Destroy(card.gameObject);
            }
            cardList_Deck = CardManager.Inst.ItemBufferToCardUIList(CardManager.Inst.itemDeck, cardListContent_Deck.transform);
            foreach (CardUI card in cardList_Draw)
            {
                Destroy(card.gameObject);
            }
            cardList_Draw = CardManager.Inst.ItemBufferToCardUIList(CardManager.Inst.itemDraw, cardListContent_Draw.transform);
            foreach (CardUI card in cardList_Discard)
            {
                Destroy(card.gameObject);
            }
            cardList_Discard = CardManager.Inst.ItemBufferToCardUIList(CardManager.Inst.itemDiscard, cardListContent_Discard.transform);
            DeckCardList();
            Canvas.ForceUpdateCanvases();

            cardListView.SetActive(true);
        }
        else
        {
            TurnManager.Inst.isLoading = false;
            Tooltip.showTooltipSignal = true;
            cardListView.SetActive(false);
        }
    }

    // 덱 카드 목록 띄움
    public void DeckCardList()
    {
        cardScrollView_Deck.SetActive(true);
        cardScrollView_Draw.SetActive(false);
        cardScrollView_Discard.SetActive(false);

        deckListBtn.image.sprite = buttonPressedImg;
        drawListBtn.image.sprite = buttonNormalImg;
        discardListBtn.image.sprite = buttonNormalImg;
        
        deckListBtn.GetComponentInChildren<TMP_Text>().color = Color.white;
        drawListBtn.GetComponentInChildren<TMP_Text>().color = Color.black;
        discardListBtn.GetComponentInChildren<TMP_Text>().color = Color.black;
    }

    // 드로우 풀 카드 목록 띄움
    public void DrawCardList()
    {
        cardScrollView_Deck.SetActive(false);
        cardScrollView_Draw.SetActive(true);
        cardScrollView_Discard.SetActive(false);

        deckListBtn.image.sprite = buttonNormalImg;
        drawListBtn.image.sprite = buttonPressedImg;
        discardListBtn.image.sprite = buttonNormalImg;

        deckListBtn.GetComponentInChildren<TMP_Text>().color = Color.black;
        drawListBtn.GetComponentInChildren<TMP_Text>().color = Color.white;
        discardListBtn.GetComponentInChildren<TMP_Text>().color = Color.black;
    }

    // 무덤 카드 목록 띄움
    public void DiscardCardList()
    {
        cardScrollView_Deck.SetActive(false);
        cardScrollView_Draw.SetActive(false);
        cardScrollView_Discard.SetActive(true);

        deckListBtn.image.sprite = buttonNormalImg;
        drawListBtn.image.sprite = buttonNormalImg;
        discardListBtn.image.sprite = buttonPressedImg;

        deckListBtn.GetComponentInChildren<TMP_Text>().color = Color.black;
        drawListBtn.GetComponentInChildren<TMP_Text>().color = Color.black;
        discardListBtn.GetComponentInChildren<TMP_Text>().color = Color.white;
    }

    // 이드 목록 띄움
    public void RelicList()
    {
        TurnManager.Inst.isLoading = true;
        foreach (RelicUI relic in relicList)
        {
            Destroy(relic.gameObject);
        }

        relicList = RelicManager.Inst.RelicItemListToRelicUIList(RelicManager.Inst.relicList, relicListScroll.transform);
        Canvas.ForceUpdateCanvases();
    }

    public void SetPlayerBuffUI()
    {
        for (int i = playerBuffUIView.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(playerBuffUIView.transform.GetChild(i).gameObject);
        }
        BuffManager.Inst.BuffListToBuffUIList(BuffManager.Inst.playerShowBuffs, playerBuffUIView, playerBuffPos);
        var tooltipList = playerBuffUIView.GetComponentsInChildren<Tooltip>();
        foreach(var tooltip in tooltipList)
        {
            tooltip.tooltipPivot = new Vector2(0, 0);
        }
    }

    public void SetEnemyBuffUI(int enemyIdx = 0)
    {
        for (int i = enemyBuffUIView[enemyIdx].transform.childCount - 1; i >= 0; i--)
        {
            Destroy(enemyBuffUIView[enemyIdx].transform.GetChild(i).gameObject);
        }
        BuffManager.Inst.BuffListToBuffUIList(BuffManager.Inst.enemyShowBuffs[enemyIdx], enemyBuffUIView[enemyIdx], enemyBuffPos[enemyIdx]);
    }

    public void SetRouletteBuffUI()
    {
        for (int i = rouletteBuffUIView.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(rouletteBuffUIView.transform.GetChild(i).gameObject);
        }
        BuffManager.Inst.BuffListToBuffUIList(BuffManager.Inst.rouletteShowBuffs, rouletteBuffUIView, rouletteBuffPos);
    }

    public void ShowCardReward()
    {
        TurnManager.Inst.isLoading = true;
        Tooltip.showTooltipSignal = false;
        SetCardReward();
        rewardCardView.SetActive(true);
    }

    public void EndCardReward()
    {
        rewardCardView.SetActive(false);
        characterSO.maxHealth = TurnManager.Inst.maxHealth;
        characterSO.curHealth = TurnManager.Inst.curHealth;
        characterSO.dreamDust += 1;
        GetComponent<AudioSource>().PlayOneShot(SoundManager.Inst.coinGetSFX);
        // if(characterSO.enemyName == stageSO.stageList[stageSO.currentStage].bossEnemy.enemyName)
        // {
        //     stageSO.stageList[stageSO.currentStage].bossEnemy.isClear = true;
        //     characterSO.dreamDust += stageSO.stageList[stageSO.currentStage].bossEnemy.dreamDustReward;
        //     stageSO.stageList[stageSO.currentStage].stageClear = true;
        // }
        // StageEnemy stageEnemy = stageSO.stageList[stageSO.currentStage].stageEnemies.Find(x => x.enemyName == characterSO.enemyName);
        // if(stageEnemy != null)
        // {
        //     stageEnemy.isClear = true;
        //     characterSO.dreamDust += stageEnemy.dreamDustReward;
        // }
        SceneChangeManager.Inst.SceneFadeOut("EncounterScene");
    }

    public void SetCardReward()
    {
        List<Item> shareCards = normalItemListSO.items;
        List<Item> normalCards = new List<Item>();
        List<Item>[] dreamCards = new List<Item>[Enum.GetNames(typeof(CardRarity)).Length];
        for(int i = 0; i < dreamCards.Length; i++)
        {
            dreamCards[i] = new List<Item>();
        }
        List<Item>[] dreamCards_enhanced = new List<Item>[Enum.GetNames(typeof(CardRarity)).Length];
        for(int i = 0; i < dreamCards_enhanced.Length; i++)
        {
            dreamCards_enhanced[i] = new List<Item>();
        }
        DreamPiece_Reference persona_ref = dreamPieceListSO.dreamPieces.Find(x => x.name == characterSO.personaPiece.name);
        DreamPiece_Reference shadow_ref = dreamPieceListSO.dreamPieces.Find(x => x.name == characterSO.shadowPiece.name);
        foreach(Item item in normalItemListSO.items)
        {
            normalCards.Add(item);
        }
        foreach (Item_Enhanceable item in persona_ref.cards)
        {
            dreamCards[(int)item.rarity].Add((Item)item);
            dreamCards_enhanced[(int)item.rarity].Add(item.enhancedItem);
        }
        foreach (Item_Enhanceable item in shadow_ref.cards)
        {
            dreamCards[(int)item.rarity].Add((Item)item);
            dreamCards_enhanced[(int)item.rarity].Add(item.enhancedItem);
        }
        foreach(Transform child in rewardCardList)
        {
            Destroy(child.gameObject);
        }
        rewardCards = new List<CardUI_Reward>();
        for(int i = 0; i < rewardCardCount; i++)
        {
            var rewardCardObj = Instantiate(rewardCardPrefab, rewardCardList);
            CardUI_Reward rc = rewardCardObj.GetComponent<CardUI_Reward>();
            rewardCards.Add(rc);
        }
        foreach (CardUI_Reward rc in rewardCards)
        {
            float totalW = 0f;
            for (int i = 0; i < rewardCardWeights.Length; i++)
            {
                totalW += rewardCardWeights[i];
            }
            float rPoint = Random.value * totalW;
            int chooseCardPool = 0;
            for (int i = 0; i < rewardCardWeights.Length; i++)
            {
                if (rPoint < rewardCardWeights[i])
                {
                    chooseCardPool = i;
                    break;
                }
                rPoint -= rewardCardWeights[i];
            }
            Debug.Log("Chosen Card Pool: " + chooseCardPool.ToString());
            List<Item> lookat = new List<Item>();
            if(chooseCardPool == 0)
            {
                lookat = normalCards;
                Debug.Log(lookat.Count);
            }
            else if(chooseCardPool > 0 && chooseCardPool <= Enum.GetNames(typeof(CardRarity)).Length)
            {
                bool isE = Random.value < enhanceProbability;
                if(isE) lookat = dreamCards_enhanced[chooseCardPool - 1];
                else lookat = dreamCards[chooseCardPool - 1];
                Debug.Log(lookat.Count);
            }
            int cardIdx = Random.Range(0, lookat.Count);
            rc.Setup(lookat[cardIdx]);
            rc.gameObject.SetActive(true);
        }
    }

    public void AddCardReward(Item item)
    {
        Item newItem = new Item();
        newItem.SetItem(item);
        newItem.num = 1;
        if(item.dreamPieceNum < 0)
        {
            var existItem = characterSO.normalCards.Find(x => x.name == item.name);
            if(existItem == null) characterSO.normalCards.Add(newItem);
            else existItem.num++;
        }
        else if(dreamPieceListSO.dreamPieces[item.dreamPieceNum].name == characterSO.personaPiece.name)
        {
            var existItem = characterSO.personaPiece.cards.Find(x => x.name == item.name);
            if(existItem == null) characterSO.personaPiece.cards.Add(newItem);
            else existItem.num++;
        }
        else if(dreamPieceListSO.dreamPieces[item.dreamPieceNum].name == characterSO.shadowPiece.name)
        {
            var existItem = characterSO.shadowPiece.cards.Find(x => x.name == item.name);
            if(existItem == null) characterSO.shadowPiece.cards.Add(newItem);
            else existItem.num++;
        }
        else
        {
            Debug.LogError("undefined card added!");
        }
    }
}
