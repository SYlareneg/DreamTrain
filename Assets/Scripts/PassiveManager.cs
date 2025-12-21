using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PassiveManager : MonoBehaviour
{
    public static PassiveManager Inst { get; private set; }
    private void Awake() => Inst = this;
    
    public static Action<bool, int> PlayerSpecialRoulette1Activation;
    public static Action<int> PlayerSpecialRoulette1Clear;
    public static Action<bool, int> PlayerSpecialRoulette2Activation;
    public static Action<int> PlayerSpecialRoulette2Clear;
    public Sprite PlayerSpecialRoulette1Sprite;
    public string PlayerSpecialRoulette1Title;
    public string PlayerSpecialRoulette1Text;
    public Sprite PlayerSpecialRoulette2Sprite;
    public string PlayerSpecialRoulette2Title;
    public string PlayerSpecialRoulette2Text;

    public void SetPersona()
    {
        if (TurnManager.Inst.characterSO.personaPiece == null) return;
        RouletteManager.Inst.playerTriggerSprite = TurnManager.Inst.characterSO.personaPiece.triggerSprite;
        PlayerSpecialRoulette1Sprite = TurnManager.Inst.characterSO.personaPiece.specialRouletteSprite;
        PlayerSpecialRoulette1Title = TurnManager.Inst.characterSO.personaPiece.specialRouletteTitle;
        PlayerSpecialRoulette1Text = TurnManager.Inst.characterSO.personaPiece.specialRouletteText;
        RouletteItem rItem = new RouletteItem();
        string personaName = "";
        if (TurnManager.Inst.characterSO.personaPiece.persona.isEnhanced) personaName = TurnManager.Inst.characterSO.personaPiece.persona.enhancedPassive.name;
        else personaName = TurnManager.Inst.characterSO.personaPiece.persona.name;
        switch (personaName)
        {
            case "붉은 달":
            case "붉은 달+":
                // 특수 룰렛 설정
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial1.Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_PlayerSpecial1.Add(new List<Buff>());
                };
                PlayerSpecialRoulette1Activation = (isEnemy, value) =>
                {
                    int trueDamage = 0;
                    bool isEnhanced = false;
                    if (isEnemy)
                    {
                        trueDamage = TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette);
                        isEnhanced = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].isEnhanced;
                    }
                    else
                    {
                        trueDamage = TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                        isEnhanced = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].isEnhanced;
                    }
                    int healVal = 0;
                    if (isEnhanced) healVal = trueDamage / 2;
                    else healVal = trueDamage / 3;
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_PlayerSpecial1[1], healVal);
                    TurnManager.Inst.TakeDmg(-totalVal_Heal, EDamageSource.Roulette);
                };
                PlayerSpecialRoulette1Clear = (index) =>
                {
                    RouletteManager.Inst.EnchantRoulettePiece(index, ERouletteType.None, 0);
                };
                // 트리거 게이지 최대치 설정
                TurnManager.Inst.playerTriggerMaxCnt = 15;
                // 트리거 조각 설정
                rItem.type = ERouletteType.Attack;
                rItem.value = 0;
                RouletteManager.Inst.playerTriggerPiece = rItem;
                // 트리거 조건 설정
                TurnManager.OnPlayerHealed += (healamount, healsource) =>
                {
                    TurnManager.Inst.TriggerPlayerPassive(healamount);
                };
                // 트리거 효과 설정
                RouletteManager.PlayerTriggerActivation = (isEnemy, totalVal) =>
                {
                    TurnManager.Inst.TakeDmg(TurnManager.Inst.curHealth / 10, EDamageSource.Passive);
                    TurnManager.Inst.EnemyTakeDmg(totalVal, EDamageSource.Roulette);
                };
                // 트리거 데미지 계산
                int useHealth = 0;
                TurnManager.OnPlayerTrigger += () =>
                {
                    BuffManager.Inst.rouletteBuff_Trigger.Clear();
                    useHealth = TurnManager.Inst.curHealth / 10;
                    if (personaName == "붉은 달") BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, useHealth * 3, 1, -1);
                    else if (personaName == "붉은 달+")  BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, useHealth * 4, 1, -1);
                };
                TurnManager.OnPlayerHealthChange += (damage) =>
                {
                    if (RouletteManager.Inst.isTriggerActivated && RouletteManager.Inst.isPlayerTrigger())
                    {
                        int healthDiff = TurnManager.Inst.curHealth / 10 - useHealth;
                        if(healthDiff != 0)
                        {
                            if (personaName == "붉은 달") BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, healthDiff * 3, 1, -1);
                            else if (personaName == "붉은 달+")  BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, healthDiff * 4, 1, -1);
                        }
                        useHealth = TurnManager.Inst.curHealth / 10;
                    }
                };
                break;
            case "마술 해체":
            case "마술 해체+":
                // 특수 룰렛 설정
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial1.Add(new List<Buff>());
                };
                TurnManager.OnRouletteSpin += (x, y) =>
                {
                    if (RouletteManager.Inst.spinDirection == 1)
                    {
                        for (int i = 0; i < RouletteManager.rouletteNum; i++)
                        {
                            if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Player_Special_1)
                            {
                                RouletteManager.Inst.roulettePieces[i].roulette.value--;
                                int val = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                                if (val == 0)
                                {
                                    PlayerSpecialRoulette1Clear?.Invoke(i);
                                }
                            }
                        }
                    }
                };
                PlayerSpecialRoulette1Clear = (index) =>
                {
                    RouletteManager.Inst.EnchantRoulettePiece(index, ERouletteType.None, 0);
                };
                // 트리거 게이지 최대치 설정
                TurnManager.Inst.playerTriggerMaxCnt = 2;
                // 트리거 조각 설정
                rItem.type = ERouletteType.Attack;
                rItem.value = 0;
                RouletteManager.Inst.playerTriggerPiece = rItem;
                // 트리거 조건 설정
                TurnManager.OnGameStart += () =>
                {
                    Item ace = new Item();
                    ace.name = "에이스";
                    if (personaName == "카드 숨기기+") ace.name += "+";
                    ace.cost = 1;
                    ace.type = CardType.Effect;
                    ace.element = EPassiveType.Normal;
                    ace.dreamPieceNum = -1;
                    ace.isVolatile = false;
                    ace.isVanish = false;
                    if (personaName == "카드 숨기기+") ace.isRemain = true;
                    else if (personaName == "카드 숨기기") ace.isRemain = false;
                    ace.text = "트리거 게이지를 1 얻습니다.";
                    if (personaName == "카드 숨기기+") ace.text += " <color=red>잔류</color>";
                    ace.cardValues = new List<int>();
                    ace.cardValueTypes = new List<ECardValueType>();
                    ace.num = 1;
                    CardManager.Inst.itemDeck.Add(ace);
                    CardManager.Inst.itemDraw.Add(ace);
                    CardManager.Inst.ShuffleDeck();
                };
                // 트리거 효과 설정
                RouletteManager.PlayerTriggerActivation = (isEnemy, totalVal) =>
                {
                    for (int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if(RouletteManager.Inst.roulettePieces[i].roulette.type != ERouletteType.None && RouletteManager.Inst.roulettePieces[i].roulette.type != ERouletteType.Attack && RouletteManager.Inst.roulettePieces[i].roulette.type != ERouletteType.Shield)
                        {
                            RouletteManager.Inst.roulettePieces[i].RouletteClear();
                        }
                    }
                    TurnManager.Inst.EnemyTakeDmg(totalVal, EDamageSource.Roulette);
                };
                // 트리거 데미지 계산
                int counter = 0;
                TurnManager.OnPlayerTrigger += () =>
                {
                    BuffManager.Inst.rouletteBuff_Trigger.Clear();
                    counter = RouletteManager.rouletteNum;
                    counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.None);
                    counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.Attack);
                    counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.Shield);
                    if (personaName == "마술 해체+" && counter >= 6) BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, counter * 7, 1.5f, -1);
                    else BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, counter * 7, 1, -1);
                };
                TurnManager.OnRouletteEnchant += (x) =>
                {
                    if (RouletteManager.Inst.isTriggerActivated)
                    {
                        int newCnt = RouletteManager.rouletteNum;
                        newCnt -= RouletteManager.Inst.CountRouletteType(ERouletteType.None);
                        newCnt -= RouletteManager.Inst.CountRouletteType(ERouletteType.Attack);
                        newCnt -= RouletteManager.Inst.CountRouletteType(ERouletteType.Shield);
                        if (newCnt != counter)
                        {
                            if (personaName == "마술 해체+" && counter >= 6 && newCnt < 6) BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, (newCnt - counter) * 7, 2.0f / 3, -1);
                            else if (personaName == "마술 해체+" && counter < 6 && newCnt >= 6) BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, (newCnt - counter) * 7, 1.5f, -1);
                            else BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, (newCnt - counter) * 7, 1, -1);
                            counter = newCnt;
                        }
                    }
                };
                break;
            case "겨울 바람":
            case "겨울 바람+":
                // 특수 룰렛 설정
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial1.Add(new List<Buff>());
                };
                List<FrozenRoulette> frozenRoulettes = new List<FrozenRoulette>();
                TurnManager.CheckRouletteEnchantable += (index, type) =>
                {
                    var frozenChk = frozenRoulettes.Find(x => x.rIdx == index);
                    if (frozenChk != null && RouletteManager.Inst.roulettePieces[index].roulette.type == ERouletteType.Player_Special_1)
                    {
                        return false;
                    }
                    if(frozenChk == null && type == ERouletteType.Player_Special_1)
                    {
                        GameObject frozenSprite = new GameObject("FrozenIcon");
                        FrozenRoulette frzRlt = new FrozenRoulette();
                        frzRlt.rIdx = index;
                        frzRlt.rItem = RouletteManager.Inst.roulettePieces[index].roulette;
                        frzRlt.frozenIcon = frozenSprite;
                        frozenRoulettes.Add(frzRlt);
                        frozenSprite.transform.SetParent(RouletteManager.Inst.roulettePieces[index].transform);
                        frozenSprite.transform.localPosition = Vector3.zero;
                        frozenSprite.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
                        SpriteRenderer frozenSpriteRenderer = frozenSprite.AddComponent<SpriteRenderer>();
                        frozenSpriteRenderer.sortingOrder = RouletteManager.Inst.roulettePieces[index].GetComponent<SpriteRenderer>().sortingOrder + 1;
                        frozenSpriteRenderer.sprite = TurnManager.Inst.characterSO.personaPiece.specialRouletteSprite;
                        frozenSpriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                        PlayerSpecialRoulette1Sprite = RouletteManager.Inst.roulettePieces[index].originalSprite;
                        if(RouletteManager.Inst.isTriggerActivated)
                        {
                            frozenSprite.SetActive(false);
                        }
                        else
                        {
                            frozenSprite.SetActive(true);
                        }
                    }
                    return true;
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    for (int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Player_Special_1)
                        {
                            RouletteManager.Inst.roulettePieces[i].roulette.value--;
                            int val = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                            if (val == 0)
                            {
                                PlayerSpecialRoulette1Clear?.Invoke(i);
                            }
                        }
                    }
                };
                PlayerSpecialRoulette1Clear = (index) =>
                {
                    var frozenChk = frozenRoulettes.Find(x => x.rIdx == index);
                    if (frozenChk != null)
                    {
                        RouletteManager.Inst.EnchantRoulettePiece(index, frozenChk.rItem.type, frozenChk.rItem.value);
                        Destroy(frozenChk.frozenIcon);
                        frozenRoulettes.Remove(frozenChk);
                    }
                    else
                    {
                        RouletteManager.Inst.EnchantRoulettePiece(index, ERouletteType.None, 0);
                    }
                };
                // 트리거 게이지 최대치 설정
                TurnManager.Inst.playerTriggerMaxCnt = 12;
                // 트리거 조각 설정
                rItem.type = ERouletteType.None;
                rItem.value = 0;
                if(personaName == "겨울 바람+") 
                {
                    rItem.type = ERouletteType.Attack;
                    rItem.value = 12;
                }
                RouletteManager.Inst.playerTriggerPiece = rItem;
                // 트리거 조건 설정
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if (personaName == "겨울 바람") TurnManager.Inst.TriggerPlayerPassive(3);
                    else if (personaName == "겨울 바람+") TurnManager.Inst.TriggerPlayerPassive(4);
                };
                // 트리거 효과 설정
                RouletteManager.PlayerTriggerActivation = (isEnemy, totalVal) =>
                {
                    for(int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        RouletteManager.Inst.EnchantRoulettePiece(i, ERouletteType.Player_Special_1, 2);
                    }
                    for(int i = 0; i < EnemyManager.Inst.actionList.Count; i++)
                    {
                        EnemyManager.Inst.RemoveAction(i);
                    }
                };
                break;
        }
    }
    public void SetShadow()
    {
        if (TurnManager.Inst.characterSO.shadowPiece == null) return;
        PlayerSpecialRoulette2Sprite = TurnManager.Inst.characterSO.shadowPiece.specialRouletteSprite;
        PlayerSpecialRoulette2Title = TurnManager.Inst.characterSO.shadowPiece.specialRouletteTitle;
        PlayerSpecialRoulette2Text = TurnManager.Inst.characterSO.shadowPiece.specialRouletteText;
        RouletteItem rItem = new RouletteItem();
        string shadowName = "";
        if (TurnManager.Inst.characterSO.shadowPiece.shadow.isEnhanced) shadowName = TurnManager.Inst.characterSO.shadowPiece.shadow.enhancedPassive.name;
        else shadowName = TurnManager.Inst.characterSO.shadowPiece.shadow.name;
        switch (shadowName)
        {
            case "붉은 송곳니":
            case "붉은 송곳니+":
                // 특수 룰렛 설정
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial2.Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_PlayerSpecial2.Add(new List<Buff>());
                };
                PlayerSpecialRoulette2Activation = (isEnemy, value) =>
                {
                    int trueDamage = 0;
                    bool isEnhanced = false;
                    if (isEnemy)
                    {
                        trueDamage = TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette);
                        isEnhanced = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].isEnhanced;
                    }
                    else
                    {
                        trueDamage = TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                        isEnhanced = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].isEnhanced;
                    }
                    int healVal = 0;
                    if (isEnhanced) healVal = trueDamage / 2;
                    else healVal = trueDamage / 3;
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_PlayerSpecial2[1], healVal);
                    TurnManager.Inst.TakeDmg(-totalVal_Heal, EDamageSource.Roulette);
                };
                PlayerSpecialRoulette2Clear = (index) =>
                {
                    RouletteManager.Inst.EnchantRoulettePiece(index, ERouletteType.None, 0);
                };
                // 패시브 효과 설정
                TurnManager.OnRouletteActivate += () =>
                {
                    var playerPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat];
                    var enemyPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat];
                    if (enemyPiece.roulette.type == ERouletteType.Attack)
                    {
                        int damage = BuffManager.Inst.GetBuffedRouletteValue(enemyPiece);
                        int heal = 0;
                        if (shadowName == "붉은 송곳니") heal = damage / 3;
                        else if (shadowName == "붉은 송곳니+") heal = damage / 2;
                        TurnManager.Inst.TakeDmg(-heal, EDamageSource.Passive);
                    }
                    if (playerPiece.roulette.type == ERouletteType.Attack)
                    {
                        int damage = BuffManager.Inst.GetBuffedRouletteValue(playerPiece);
                        int heal = 0;
                        if (shadowName == "붉은 송곳니") heal = damage / 3;
                        else if (shadowName == "붉은 송곳니+") heal = damage / 2;
                        Debug.Log(heal);
                        TurnManager.Inst.TakeDmg(-heal, EDamageSource.Passive);
                    }
                };
                break;
            case "손기술":
            case "손기술+":
                // 특수 룰렛 설정
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial2.Add(new List<Buff>());
                };
                TurnManager.OnRouletteSpin += (x, y) =>
                {
                    if (RouletteManager.Inst.spinDirection == 1)
                    {
                        for (int i = 0; i < RouletteManager.rouletteNum; i++)
                        {
                            if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Player_Special_2)
                            {
                                RouletteManager.Inst.roulettePieces[i].roulette.value--;
                                int val = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                                if (val == 0)
                                {
                                    PlayerSpecialRoulette2Clear?.Invoke(i);
                                }
                            }
                        }
                    }
                };
                PlayerSpecialRoulette2Clear = (index) =>
                {
                    RouletteManager.Inst.EnchantRoulettePiece(index, ERouletteType.None, 0);
                };
                // 패시브 효과 설정
                int cardCnt = 0;
                int maxCardCnt = 0;
                if (shadowName == "손기술") maxCardCnt = 3;
                else if (shadowName == "손기술+") maxCardCnt = 2;
                TurnManager.OnUseCard += () =>
                {
                    cardCnt++;
                    if(cardCnt >= maxCardCnt)
                    {
                        StartCoroutine(TurnManager.Inst.Draw(1, null));
                        cardCnt = 0;
                    }
                };
                break;
            case "순환하는 계절":
            case "순환하는 계절+":
                // 특수 룰렛 설정
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial2.Add(new List<Buff>());
                };
                List<FrozenRoulette> frozenRoulettes = new List<FrozenRoulette>();
                TurnManager.CheckRouletteEnchantable += (index, type) =>
                {
                    var frozenChk = frozenRoulettes.Find(x => x.rIdx == index);
                    if (frozenChk != null && RouletteManager.Inst.roulettePieces[index].roulette.type == ERouletteType.Player_Special_2)
                    {
                        return false;
                    }
                    if(frozenChk == null && type == ERouletteType.Player_Special_2)
                    {
                        GameObject frozenSprite = new GameObject("FrozenIcon");
                        FrozenRoulette frzRlt = new FrozenRoulette();
                        frzRlt.rIdx = index;
                        frzRlt.rItem = RouletteManager.Inst.roulettePieces[index].roulette;
                        frzRlt.frozenIcon = frozenSprite;
                        frozenRoulettes.Add(frzRlt);
                        frozenSprite.transform.SetParent(RouletteManager.Inst.roulettePieces[index].transform);
                        frozenSprite.transform.localPosition = Vector3.zero;
                        frozenSprite.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
                        SpriteRenderer frozenSpriteRenderer = frozenSprite.AddComponent<SpriteRenderer>();
                        frozenSpriteRenderer.sortingOrder = RouletteManager.Inst.roulettePieces[index].GetComponent<SpriteRenderer>().sortingOrder + 1;
                        frozenSpriteRenderer.sprite = TurnManager.Inst.characterSO.shadowPiece.specialRouletteSprite;
                        frozenSpriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                        PlayerSpecialRoulette2Sprite = RouletteManager.Inst.roulettePieces[index].originalSprite;
                        if(RouletteManager.Inst.isTriggerActivated)
                        {
                            frozenSprite.SetActive(false);
                        }
                        else
                        {
                            frozenSprite.SetActive(true);
                        }
                    }
                    return true;
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    for (int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Player_Special_2)
                        {
                            RouletteManager.Inst.roulettePieces[i].roulette.value--;
                            int val = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                            if (val == 0)
                            {
                                PlayerSpecialRoulette2Clear?.Invoke(i);
                            }
                        }
                    }
                };
                PlayerSpecialRoulette2Clear = (index) =>
                {
                    var frozenChk = frozenRoulettes.Find(x => x.rIdx == index);
                    if (frozenChk != null)
                    {
                        RouletteManager.Inst.EnchantRoulettePiece(index, frozenChk.rItem.type, frozenChk.rItem.value);
                        Destroy(frozenChk.frozenIcon);
                        frozenRoulettes.Remove(frozenChk);
                    }
                    else
                    {
                        RouletteManager.Inst.EnchantRoulettePiece(index, ERouletteType.None, 0);
                    }
                };
                // 패시브 효과 설정
                int turnCnt = 0;
                int shieldVal = 0;
                if (shadowName == "순환하는 계절") shieldVal = 4;
                else if (shadowName == "순환하는 계절+") shieldVal = 8;
                TurnManager.OnPlayerTurnStart += () =>
                {
                    turnCnt++;
                    if (turnCnt >= 2)
                    {
                        TurnManager.Inst.GetShield(false, shieldVal, EDamageSource.Passive);
                        turnCnt = 0;
                    }
                };
                break;
        }
    }

    private void OnDestroy()
    {
        PlayerSpecialRoulette1Activation = null;
        PlayerSpecialRoulette2Activation = null;
    }
}

class FrozenRoulette
{
    public int rIdx;
    public RouletteItem rItem;
    public GameObject frozenIcon;
}
