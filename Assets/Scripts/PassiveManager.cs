using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PassiveManager : MonoBehaviour
{
    public static PassiveManager Inst { get; private set; }
    private void Awake() => Inst = this;

    public SpecialRoulette[] playerSpecialRoulettes = new SpecialRoulette[DreamPiece_Base.playerSpecialRouletteNum * 2];
    public static Action<bool, int, int, bool>[] playerSpecialRouletteActivation = new Action<bool, int, int, bool>[DreamPiece_Base.playerSpecialRouletteNum * 2];
    public static Action<int>[] playerSpecialRouletteClear = new Action<int>[DreamPiece_Base.playerSpecialRouletteNum * 2];


    // 마술사 꿈조각용
    public CardType[] playerSpecialRoulette_lastCardType = new CardType[RouletteManager.rouletteNum];
    public CardType lastCardType = CardType.Skill;

    public static int GetSpecialRouletteIdx(bool isDP1, int idx)
    {
        if(idx >= DreamPiece_Base.playerSpecialRouletteNum) return -1;
        if (isDP1)
        {
            return idx;
        }
        else
        {
            return idx + DreamPiece_Base.playerSpecialRouletteNum;
        }
    }

    public void SetPersona()
    {
        if (TurnManager.Inst.characterSO.personaPiece == null) return;
        RouletteManager.Inst.playerTriggerSprite = TurnManager.Inst.characterSO.personaPiece.triggerSprite;
        playerSpecialRoulettes = new SpecialRoulette[DreamPiece_Base.playerSpecialRouletteNum * 2];
        for(int i = 0; i < TurnManager.Inst.characterSO.personaPiece.playerSpecialRoulettes.Length; i++)
        {
            playerSpecialRoulettes[GetSpecialRouletteIdx(true, i)] = new SpecialRoulette(TurnManager.Inst.characterSO.personaPiece.playerSpecialRoulettes[i]);
        }
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
                    BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(true, 0)].Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(true, 0)].Add(new List<Buff>());
                    // BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(true, 1)].Add(new List<Buff>());
                    // BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(true, 1)].Add(new List<Buff>());
                };
                playerSpecialRouletteActivation[GetSpecialRouletteIdx(true, 0)] = (isEnemy, value, enemyIdx, isEnhanced) =>
                {
                    int trueDamage = 0;
                    if (isEnemy)
                    {
                        trueDamage = TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette, enemyIdx);
                    }
                    else
                    {
                        trueDamage = TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                    }
                    int healVal = 0;
                    if(isEnhanced) healVal = trueDamage / 2;
                    else healVal = trueDamage / 3;
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(true, 0)][1], healVal);
                    TurnManager.Inst.TakeDmg(-totalVal_Heal, EDamageSource.Roulette);
                };
                playerSpecialRouletteClear[GetSpecialRouletteIdx(true, 0)] = (index) =>
                {
                    RouletteItem rItem = new RouletteItem();
                    rItem.rtype = new RouletteType(ERouletteType.None, 0);
                    rItem.value = 0;
                    RouletteManager.Inst.roulettePieces[index].Setup(rItem);
                };
                // playerSpecialRouletteActivation[GetSpecialRouletteIdx(true, 1)] = (isEnemy, value, enemyIdx) =>
                // {
                //     int trueDamage = 0;
                //     if (isEnemy)
                //     {
                //         trueDamage = TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette, enemyIdx);
                //     }
                //     else
                //     {
                //         trueDamage = TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                //     }
                //     int healVal = 0;
                //     healVal = trueDamage / 2;
                //     int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(true, 0)][1], healVal);
                //     TurnManager.Inst.TakeDmg(-totalVal_Heal, EDamageSource.Roulette);
                // };
                // playerSpecialRouletteClear[GetSpecialRouletteIdx(true, 1)] = (index) =>
                // {
                //     RouletteManager.Inst.EnchantRoulettePiece(index, new RouletteType(ERouletteType.None), 0);
                // };
                // 트리거 게이지 최대치 설정
                TurnManager.Inst.playerTriggerMaxCnt = 15;
                // 트리거 조각 설정
                rItem.rtype = new RouletteType(ERouletteType.Attack);
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
                    TurnManager.Inst.EnemyTakeDmg(totalVal, EDamageSource.TriggerRoulette);
                    for(int i = 0; i < Enemy.maxSubEnemyNum; i++)
                    {
                        if(EnemyManager.Inst.subEnemies[i] == null || EnemyManager.Inst.subEnemies[i].name == null) continue;
                        TurnManager.Inst.EnemyTakeDmg(totalVal, EDamageSource.TriggerRoulette, i + 1);
                    }
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
                    BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(true, 0)].Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(true, 1)].Add(new List<Buff>());
                };
                TurnManager.OnRouletteSpin += (x, y) =>
                {
                    if (RouletteManager.Inst.spinDirection == 1)
                    {
                        for (int i = 0; i < RouletteManager.rouletteNum; i++)
                        {
                            if (RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(true, 0)))
                            {
                                RouletteManager.Inst.roulettePieces[i].roulette.value--;
                                int val = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                                if (val == 0)
                                {
                                    playerSpecialRouletteClear[GetSpecialRouletteIdx(true, 0)]?.Invoke(i);
                                }
                            }
                        }
                    }
                };
                TurnManager.OnUseCard += (card, enemyIdx) =>
                {
                    for(int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if(RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(true, 1))
                        && playerSpecialRoulette_lastCardType[i] == card.item.type)
                        {
                            switch(card.item.type)
                            {
                                case CardType.Turn:
                                    BuffManager.AddBuffToTarget(BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[i]], 4, 1, -1);
                                    break;
                                case CardType.Skill:
                                    BuffManager.AddBuffToTarget(BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[i]], 4, 1, -1);
                                    break;
                                case CardType.Enchant:
                                    BuffManager.AddBuffToTarget(BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[i]], 1, 1, -1);
                                    break;
                                case CardType.Dream:
                                    BuffManager.AddBuffToTarget(BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[i]], 1, 1, -1);
                                    break;
                            }
                        }
                    }

                    foreach(Card c in CardManager.Inst.myCards)
                    {
                        if(c.item.name == "마술 카드" || c.item.name == "마술 카드+")
                        {
                            switch(card.item.type)
                            {
                                case CardType.Turn:
                                    c.character.sprite = Utils.LoadSpriteByName("Cards", "마술카드_클로버");
                                    break;
                                case CardType.Skill:
                                    c.character.sprite = Utils.LoadSpriteByName("Cards", "마술카드_다이아");
                                    break;
                                case CardType.Enchant:
                                    c.character.sprite = Utils.LoadSpriteByName("Cards", "마술카드_하트");
                                    break;
                                case CardType.Dream:
                                    c.character.sprite = Utils.LoadSpriteByName("Cards", "마술카드_스페이드");
                                    break;
                            }
                        }
                    }
                };
                TurnManager.OnAddCard += () =>
                {
                    foreach(Card c in CardManager.Inst.myCards)
                    {
                        if(c.item.name == "마술 카드" || c.item.name == "마술 카드+")
                        {
                            switch(lastCardType)
                            {
                                case CardType.Turn:
                                    c.character.sprite = Utils.LoadSpriteByName("Cards", "마술카드_클로버");
                                    break;
                                case CardType.Skill:
                                    c.character.sprite = Utils.LoadSpriteByName("Cards", "마술카드_다이아");
                                    break;
                                case CardType.Enchant:
                                    c.character.sprite = Utils.LoadSpriteByName("Cards", "마술카드_하트");
                                    break;
                                case CardType.Dream:
                                    c.character.sprite = Utils.LoadSpriteByName("Cards", "마술카드_스페이드");
                                    break;
                            }
                        }
                    }
                };
                TurnManager.OnRouletteEnchant += (index) =>
                {
                    if(RouletteManager.Inst.roulettePieces[index].roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(true, 1)))
                    {
                        playerSpecialRoulette_lastCardType[index] = lastCardType;
                    }
                };
                TurnManager.AfterRouletteEnchant += (index) =>
                {
                    if(RouletteManager.Inst.roulettePieces[index].roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(true, 1)))
                    {
                        switch(playerSpecialRoulette_lastCardType[index])
                        {
                            case CardType.Turn:
                                RouletteManager.Inst.roulettePieces[index].roulette.value = 4;
                                RouletteManager.Inst.roulettePieces[index].originalSprite = Utils.LoadSpriteByName("SpecialRoulette", "룰렛_마술카드_클로버");
                                if (RouletteManager.Inst.roulettePieces[index].isEnhanced)
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "클로버 룰렛+";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "값 피해를 줍니다. 스킬 카드가 사용될 때마다 위력이 4 증가합니다.";
                                }
                                else
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "클로버 룰렛";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "값 피해를 줍니다. 스킬 카드가 사용될 때마다 위력이 4 증가합니다.";
                                }
                                break;
                            case CardType.Skill:
                                RouletteManager.Inst.roulettePieces[index].roulette.value = 4;
                                RouletteManager.Inst.roulettePieces[index].originalSprite = Utils.LoadSpriteByName("SpecialRoulette", "룰렛_마술카드_다이아");
                                if (RouletteManager.Inst.roulettePieces[index].isEnhanced)
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "다이아몬드 룰렛+";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "방어도를 값만큼 부여합니다. 부여 카드가 사용될 때마다 위력이 4 증가합니다.";
                                }
                                else
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "다이아몬드 룰렛";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "방어도를 값만큼 부여합니다. 부여 카드가 사용될 때마다 위력이 4 증가합니다.";
                                }
                                break;
                            case CardType.Enchant:
                                RouletteManager.Inst.roulettePieces[index].roulette.value = 1;
                                RouletteManager.Inst.roulettePieces[index].originalSprite = Utils.LoadSpriteByName("SpecialRoulette", "룰렛_마술카드_하트");
                                if (RouletteManager.Inst.roulettePieces[index].isEnhanced)
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "하트 룰렛+";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "체력을 값만큼 회복합니다. 회전 카드가 사용될 때마다 위력이 1 증가합니다.";
                                }
                                else
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "하트 룰렛";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "체력을 값만큼 회복합니다. 회전 카드가 사용될 때마다 위력이 1 증가합니다.";
                                }
                                break;
                            case CardType.Dream:
                                RouletteManager.Inst.roulettePieces[index].roulette.value = 1;
                                RouletteManager.Inst.roulettePieces[index].originalSprite = Utils.LoadSpriteByName("SpecialRoulette", "룰렛_마술카드_스페이드");
                                if (RouletteManager.Inst.roulettePieces[index].isEnhanced)
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "스페이드 룰렛+";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "행동력을 값만큼 회복합니다. 몽상 카드가 사용될 때마다 위력이 1 증가합니다.";
                                }
                                else
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "스페이드 룰렛";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "행동력을 값만큼 회복합니다. 몽상 카드가 사용될 때마다 위력이 1 증가합니다.";
                                }
                                break;
                        }
                        RouletteManager.Inst.roulettePieces[index].SetRoulettePieceSprite(RouletteManager.Inst.roulettePieces[index].originalSprite);
                        RouletteManager.Inst.roulettePieces[index].SetRoulettePieceTooltip(RouletteManager.Inst.roulettePieces[index].originalTooltipTitle, RouletteManager.Inst.roulettePieces[index].originalTooltipText);
                    }
                };
                playerSpecialRouletteActivation[GetSpecialRouletteIdx(true, 1)] = (isEnemy, value, enemyIdx, isEnhanced) =>
                {
                    int activateIndex = RouletteManager.Inst.playerLookat;
                    if(isEnemy) activateIndex = RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx);
                    switch(playerSpecialRoulette_lastCardType[activateIndex])
                    {
                        case CardType.Turn:
                            if(isEnemy) TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette, enemyIdx);
                            else TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                            break;
                        case CardType.Skill:
                            TurnManager.Inst.GetShield(isEnemy, value, EDamageSource.Roulette, enemyIdx);
                            break;
                        case CardType.Enchant:
                            if(isEnemy) TurnManager.Inst.EnemyTakeDmg(-value, EDamageSource.Roulette, enemyIdx);
                            else TurnManager.Inst.TakeDmg(-value, EDamageSource.Roulette);
                            break;
                        case CardType.Dream:
                            if(!isEnemy) TurnManager.Inst.IncreaseCost(value);
                            break;
                    }
                    RouletteManager.Inst.roulettePieces[activateIndex].RouletteClear();
                };
                playerSpecialRouletteClear[GetSpecialRouletteIdx(true, 0)] = (index) =>
                {
                    RouletteItem rItem = new RouletteItem();
                    rItem.rtype = new RouletteType(ERouletteType.None, 0);
                    rItem.value = 0;
                    RouletteManager.Inst.roulettePieces[index].Setup(rItem);
                };
                playerSpecialRouletteClear[GetSpecialRouletteIdx(true, 1)] = (index) =>
                {
                    BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[index]].Clear();
                    RouletteItem rItem = new RouletteItem();
                    rItem.rtype = new RouletteType(ERouletteType.None, 0);
                    rItem.value = 0;
                    RouletteManager.Inst.roulettePieces[index].Setup(rItem);
                };
                // 트리거 게이지 최대치 설정
                TurnManager.Inst.playerTriggerMaxCnt = 2;
                // 트리거 조각 설정
                rItem.rtype = new RouletteType(ERouletteType.Attack, 0);
                rItem.value = 12;
                RouletteManager.Inst.playerTriggerPiece = rItem;
                // 트리거 조건 설정
                TurnManager.OnGameStart += () =>
                {
                    Item ace = new Item();
                    ace.name = "에이스";
                    ace.cost = 0;
                    ace.type = CardType.Skill;
                    ace.rarity = CardRarity.Normal;
                    ace.dreamPieceNum = 1;
                    ace.isVolatile = false;
                    ace.isVanish = false;
                    ace.isRemain = false;
                    ace.text = "트리거 게이지를 1 얻습니다.";
                    ace.cardValues = new List<int>(1) { 1 };
                    ace.cardValueTypes = new List<ECardValueType>(1) { ECardValueType.Special };
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
                        if(i == RouletteManager.Inst.playerLookat || i == RouletteManager.Inst.enemyLookat) continue;
                        if(RouletteManager.Inst.roulettePieces[i].roulette.rtype.type != ERouletteType.None)
                        {
                            Debug.Log("마술 해체 트리거 클리어: " + RouletteManager.Inst.roulettePieces[i].roulette.rtype.type.ToString());
                            RouletteManager.Inst.roulettePieces[i].RouletteClear();
                        }
                    }
                    TurnManager.Inst.EnemyTakeDmg(totalVal, EDamageSource.TriggerRoulette);
                    for(int i = 0; i < Enemy.maxSubEnemyNum; i++)
                    {
                        if(EnemyManager.Inst.subEnemies[i] == null || EnemyManager.Inst.subEnemies[i].name == null) continue;
                        TurnManager.Inst.EnemyTakeDmg(totalVal, EDamageSource.TriggerRoulette, i + 1);
                    }
                };
                // 트리거 데미지 계산
                int counter = 0;
                TurnManager.OnPlayerTrigger += () =>
                {
                    BuffManager.Inst.rouletteBuff_Trigger.Clear();
                    counter = RouletteManager.rouletteNum;
                    counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.None);
                    if(RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype.type != ERouletteType.None) counter--;
                    if(RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].roulette.rtype.type != ERouletteType.None) counter--;
                    Debug.Log("마술 해체 트리거 카운터: " + counter);
                    if (personaName == "마술 해체+" && counter >= 6) BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, counter * 6, 1.5f, -1);
                    else BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, counter * 6, 1, -1);
                };
                TurnManager.OnRouletteEnchant += (x) =>
                {
                    if (RouletteManager.Inst.isTriggerActivated)
                    {
                        int newCnt = RouletteManager.rouletteNum;
                        newCnt -= RouletteManager.Inst.CountRouletteType(ERouletteType.None);
                        if(RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype.type != ERouletteType.None) newCnt--;
                        if(RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].roulette.rtype.type != ERouletteType.None) newCnt--;
                        Debug.Log("마술 해체 트리거 카운터: " + newCnt);
                        if (newCnt != counter)
                        {
                            if (personaName == "마술 해체+" && counter >= 6 && newCnt < 6) BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, (newCnt - counter) * 6, 2.0f / 3, -1);
                            else if (personaName == "마술 해체+" && counter < 6 && newCnt >= 6) BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, (newCnt - counter) * 6, 1.5f, -1);
                            else BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, (newCnt - counter) * 6, 1, -1);
                            counter = newCnt;
                        }
                    }
                };
                TurnManager.OnRouletteSpin += (isClockwise, pieces) =>
                {
                    if (RouletteManager.Inst.isTriggerActivated)
                    {
                        int newCnt = RouletteManager.rouletteNum;
                        newCnt -= RouletteManager.Inst.CountRouletteType(ERouletteType.None);
                        if (RouletteManager.Inst.roulettePieces[(RouletteManager.Inst.playerLookat + pieces * (isClockwise ? -1 : 1) + RouletteManager.rouletteNum) % RouletteManager.rouletteNum].roulette.rtype.type != ERouletteType.None) newCnt--;
                        if (RouletteManager.Inst.roulettePieces[(RouletteManager.Inst.enemyLookat + pieces * (isClockwise ? -1 : 1) + RouletteManager.rouletteNum) % RouletteManager.rouletteNum].roulette.rtype.type != ERouletteType.None) newCnt--;
                        Debug.Log("마술 해체 트리거 카운터: " + newCnt);
                        if (newCnt != counter)
                        {
                            if (personaName == "마술 해체+" && counter >= 6 && newCnt < 6) BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, (newCnt - counter) * 6, 2.0f / 3, -1);
                            else if (personaName == "마술 해체+" && counter < 6 && newCnt >= 6) BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, (newCnt - counter) * 6, 1.5f, -1);
                            else BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, (newCnt - counter) * 6, 1, -1);
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
                    BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(true, 0)].Add(new List<Buff>());
                };
                List<FrozenRoulette> frozenRoulettes = new List<FrozenRoulette>();
                TurnManager.CheckRouletteEnchantable += (index, type) =>
                {
                    var frozenChk = frozenRoulettes.Find(x => x.rIdx == index);
                    if (frozenChk != null && RouletteManager.Inst.roulettePieces[index].roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(true, 0)))
                    {
                        return false;
                    }
                    else if(frozenChk == null && type == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(true, 0)))
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
                        frozenSpriteRenderer.sprite = TurnManager.Inst.characterSO.personaPiece.playerSpecialRoulettes[0].sprite;
                        frozenSpriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                        playerSpecialRoulettes[GetSpecialRouletteIdx(true, 0)].sprite = RouletteManager.Inst.roulettePieces[index].originalSprite;
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
                        if (RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(true, 0)))
                        {
                            RouletteManager.Inst.roulettePieces[i].roulette.value--;
                            int val = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                            if (val == 0)
                            {
                                playerSpecialRouletteClear[GetSpecialRouletteIdx(true, 0)]?.Invoke(i);
                            }
                        }
                    }
                };
                playerSpecialRouletteClear[GetSpecialRouletteIdx(true, 0)] = (index) =>
                {
                    var frozenChk = frozenRoulettes.Find(x => x.rIdx == index);
                    if (frozenChk != null)
                    {
                        RouletteManager.Inst.roulettePieces[index].roulette.rtype = new RouletteType(ERouletteType.None);
                        RouletteManager.Inst.EnchantRoulettePiece(index, frozenChk.rItem.rtype, frozenChk.rItem.value);
                        Destroy(frozenChk.frozenIcon);
                        frozenRoulettes.Remove(frozenChk);
                    }
                    else
                    {
                        RouletteManager.Inst.roulettePieces[index].roulette.rtype = new RouletteType(ERouletteType.None);
                        RouletteItem rItem = new RouletteItem();
                        rItem.rtype = new RouletteType(ERouletteType.None, 0);
                        rItem.value = 0;
                        RouletteManager.Inst.roulettePieces[index].Setup(rItem);
                    }
                };
                // 트리거 게이지 최대치 설정
                TurnManager.Inst.playerTriggerMaxCnt = 12;
                // 트리거 조각 설정
                rItem.rtype = new RouletteType(ERouletteType.None);
                rItem.value = 0;
                if(personaName == "겨울 바람+") 
                {
                    rItem.rtype = new RouletteType(ERouletteType.Attack);
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
                        RouletteManager.Inst.EnchantRoulettePiece(i, new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(true, 0)), 2);
                    }
                    for(int i = 0; i < EnemyManager.Inst.actionList.Count; i++)
                    {
                        EnemyManager.Inst.RemoveAction(i);
                    }
                    for(int i = 0; i < Enemy.maxSubEnemyNum; i++)
                    {
                        if(EnemyManager.Inst.subEnemies[i] == null || EnemyManager.Inst.subEnemies[i].name == null) continue;
                        for(int j = 0; j < EnemyManager.Inst.subEnemyActionList[i].Count; j++)
                        {
                            EnemyManager.Inst.RemoveSubEnemyAction(i, j);
                        }
                    }
                };
                break;
            case "사냥 본능":
            case "사냥 본능+":
                // 특수 룰렛 설정
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(true, 0)].Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(true, 1)].Add(new List<Buff>());
                };
                playerSpecialRouletteActivation[GetSpecialRouletteIdx(true, 0)] = (isEnemy, value, enemyIdx, isEnhanced) =>
                {
                    if (isEnemy)
                    {
                        TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette, enemyIdx);
                    }
                    else
                    {
                        TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                    }
                };
                playerSpecialRouletteClear[GetSpecialRouletteIdx(true, 0)] = (index) =>
                {
                    RouletteItem rItem = new RouletteItem();
                    rItem.rtype = new RouletteType(ERouletteType.None, 0);
                    rItem.value = 0;
                    RouletteManager.Inst.roulettePieces[index].Setup(rItem);
                };
                playerSpecialRouletteActivation[GetSpecialRouletteIdx(true, 1)] = (isEnemy, value, enemyIdx, isEnhanced) =>
                {
                    if (isEnemy)
                    {
                        TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette, enemyIdx);
                    }
                    else
                    {
                        TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                    }
                };
                playerSpecialRouletteClear[GetSpecialRouletteIdx(true, 1)] = (index) =>
                {
                    RouletteItem rItem = new RouletteItem();
                    rItem.rtype = new RouletteType(ERouletteType.None, 0);
                    rItem.value = 0;
                    RouletteManager.Inst.roulettePieces[index].Setup(rItem);
                };
                TurnManager.OnRouletteActivate += () =>
                {
                    if(RouletteManager.Inst.isTriggerActivated) return;
                    var playerPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat];
                    for(int i = 0; i <= Enemy.maxSubEnemyNum; i++)
                    {
                        if(i != 0 && (EnemyManager.Inst.subEnemies[i - 1] == null || EnemyManager.Inst.subEnemies[i - 1].name == null)) continue;
                        var enemyPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(i)];
                        if (enemyPiece.roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(true, 0)))
                        {
                            enemyPiece.roulette.value -= 3;
                            if(enemyPiece.roulette.value <= 0)
                            {
                                enemyPiece.RouletteClear();
                            }
                        }
                        else if(enemyPiece.roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(true, 1)))
                        {
                            enemyPiece.RouletteClear();
                            BuffManager.Inst.roulettePieceBuff[enemyPiece].Clear();
                        }
                    }
                    if (playerPiece.roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(true, 0)))
                    {
                        playerPiece.roulette.value -= 3;
                        if(playerPiece.roulette.value <= 0)
                        {
                            playerPiece.RouletteClear();
                        }
                    }
                    else if(playerPiece.roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(true, 1)))
                    {
                        playerPiece.RouletteClear();
                        BuffManager.Inst.roulettePieceBuff[playerPiece].Clear();
                    }
                };
                TurnManager.OnRouletteSpin += (isClockwise, spinValue) =>
                {
                    for(int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if(RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(true, 1)))
                        {
                            BuffManager.AddBuffToTarget(BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[i]], spinValue, 1, -1);
                        }
                    }
                };
                // 트리거 게이지 최대치 설정
                TurnManager.Inst.playerTriggerMaxCnt = 10;
                // 트리거 조각 설정
                rItem.rtype = new RouletteType(ERouletteType.Attack);
                rItem.value = 24;
                if(DataManager.Inst.characterSO.isTutorial) rItem.value = 99;
                if(personaName == "사냥 본능+") 
                {
                    rItem.value = 36;
                }
                RouletteManager.Inst.playerTriggerPiece = rItem;
                // 트리거 조건 설정
                TurnManager.OnUseCard += (x, enemyIdx) =>
                {
                    TurnManager.Inst.TriggerPlayerPassive(1);
                };
                // 트리거 효과 설정
                RouletteManager.PlayerTriggerActivation = (isEnemy, totalVal) =>
                {
                    TurnManager.Inst.EnemyTakeDmg(totalVal, EDamageSource.TriggerRoulette);
                };
                break;
        }
    }
    public void SetShadow()
    {
        if (TurnManager.Inst.characterSO.shadowPiece == null) return;
        for(int i = 0; i < TurnManager.Inst.characterSO.shadowPiece.playerSpecialRoulettes.Length; i++)
        {
            playerSpecialRoulettes[GetSpecialRouletteIdx(false, i)] = new SpecialRoulette(TurnManager.Inst.characterSO.shadowPiece.playerSpecialRoulettes[i]);
        }
        RouletteItem rItem = new RouletteItem();
        string shadowName = "";
        if (TurnManager.Inst.characterSO.shadowPiece.shadow.isEnhanced) shadowName = TurnManager.Inst.characterSO.shadowPiece.shadow.enhancedPassive.name;
        else shadowName = TurnManager.Inst.characterSO.shadowPiece.shadow.name;
        switch (shadowName)
        {
            case "붉은 송곳니":
            case "붉은 송곳니+":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(false, 0)].Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(false, 0)].Add(new List<Buff>());
                    // BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(false, 1)].Add(new List<Buff>());
                    // BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(false, 1)].Add(new List<Buff>());
                };
                playerSpecialRouletteActivation[GetSpecialRouletteIdx(false, 0)] = (isEnemy, value, enemyIdx, isEnhanced) =>
                {
                    int trueDamage = 0;
                    if (isEnemy)
                    {
                        trueDamage = TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette, enemyIdx);
                    }
                    else
                    {
                        trueDamage = TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                    }
                    int healVal = 0;
                    if(isEnhanced) healVal = trueDamage / 2;
                    else healVal = trueDamage / 3;
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(false, 0)][1], healVal);
                    TurnManager.Inst.TakeDmg(-totalVal_Heal, EDamageSource.Roulette);
                };
                playerSpecialRouletteClear[GetSpecialRouletteIdx(false, 0)] = (index) =>
                {
                    RouletteItem rItem = new RouletteItem();
                    rItem.rtype = new RouletteType(ERouletteType.None, 0);
                    rItem.value = 0;
                    RouletteManager.Inst.roulettePieces[index].Setup(rItem);
                };
                // playerSpecialRouletteActivation[GetSpecialRouletteIdx(false, 1)] = (isEnemy, value, enemyIdx) =>
                // {
                //     int trueDamage = 0;
                //     if (isEnemy)
                //     {
                //         trueDamage = TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette, enemyIdx);
                //     }
                //     else
                //     {
                //         trueDamage = TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                //     }
                //     int healVal = 0;
                //     healVal = trueDamage / 2;
                //     int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(false, 0)][1], healVal);
                //     TurnManager.Inst.TakeDmg(-totalVal_Heal, EDamageSource.Roulette);
                // };
                // playerSpecialRouletteClear[GetSpecialRouletteIdx(false, 1)] = (index) =>
                // {
                //     RouletteManager.Inst.EnchantRoulettePiece(index, new RouletteType(ERouletteType.None), 0);
                // };
                // 패시브 효과 설정
                TurnManager.OnRouletteActivate += () =>
                {
                    if(RouletteManager.Inst.isTriggerActivated) return;
                    var playerPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat];
                    for(int i = 0; i <= Enemy.maxSubEnemyNum; i++)
                    {
                        if(i != 0 && (EnemyManager.Inst.subEnemies[i - 1] == null || EnemyManager.Inst.subEnemies[i - 1].name == null)) continue;
                        var enemyPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(i)];
                        if (enemyPiece.roulette.rtype.type == ERouletteType.Attack)
                        {
                            int damage = BuffManager.Inst.GetBuffedRouletteValue(enemyPiece);
                            int heal = 0;
                            if (shadowName == "붉은 송곳니") heal = damage / 3;
                            else if (shadowName == "붉은 송곳니+") heal = damage / 2;
                            TurnManager.Inst.TakeDmg(-heal, EDamageSource.Passive);
                        }
                    }
                    if (playerPiece.roulette.rtype.type == ERouletteType.Attack)
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
                // 특수 룰렛 설정
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(false, 0)].Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(false, 1)].Add(new List<Buff>());
                };
                TurnManager.OnRouletteSpin += (x, y) =>
                {
                    if (RouletteManager.Inst.spinDirection == 1)
                    {
                        for (int i = 0; i < RouletteManager.rouletteNum; i++)
                        {
                            if (RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(false, 0)))
                            {
                                RouletteManager.Inst.roulettePieces[i].roulette.value--;
                                int val = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                                if (val == 0)
                                {
                                    playerSpecialRouletteClear[GetSpecialRouletteIdx(false, 0)]?.Invoke(i);
                                }
                            }
                        }
                    }
                };
                TurnManager.OnUseCard += (card, enemyIdx) =>
                {
                    for(int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if(RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(false, 1))
                        && playerSpecialRoulette_lastCardType[i] == card.item.type)
                        {
                            switch(card.item.type)
                            {
                                case CardType.Turn:
                                    BuffManager.AddBuffToTarget(BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[i]], 4, 1, -1);
                                    break;
                                case CardType.Skill:
                                    BuffManager.AddBuffToTarget(BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[i]], 4, 1, -1);
                                    break;
                                case CardType.Enchant:
                                    BuffManager.AddBuffToTarget(BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[i]], 1, 1, -1);
                                    break;
                                case CardType.Dream:
                                    BuffManager.AddBuffToTarget(BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[i]], 1, 1, -1);
                                    break;
                            }
                        }
                    }
                };
                TurnManager.OnRouletteEnchant += (index) =>
                {
                    if(RouletteManager.Inst.roulettePieces[index].roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(false, 1)))
                    {
                        playerSpecialRoulette_lastCardType[index] = lastCardType;
                    }
                };
                TurnManager.AfterRouletteEnchant += (index) =>
                {
                    if(RouletteManager.Inst.roulettePieces[index].roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(false, 1)))
                    {
                        switch(playerSpecialRoulette_lastCardType[index])
                        {
                            case CardType.Turn:
                                RouletteManager.Inst.roulettePieces[index].roulette.value = 4;
                                RouletteManager.Inst.roulettePieces[index].originalSprite = Utils.LoadSpriteByName("SpecialRoulette", "룰렛_마술카드_클로버");
                                if (RouletteManager.Inst.roulettePieces[index].isEnhanced)
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "클로버 룰렛+";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "값 피해를 줍니다. 스킬 카드가 사용될 때마다 위력이 6 증가합니다.";
                                }
                                else
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "클로버 룰렛";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "값 피해를 줍니다. 스킬 카드가 사용될 때마다 위력이 4 증가합니다.";
                                }
                                break;
                            case CardType.Skill:
                                RouletteManager.Inst.roulettePieces[index].roulette.value = 4;
                                RouletteManager.Inst.roulettePieces[index].originalSprite = Utils.LoadSpriteByName("SpecialRoulette", "룰렛_마술카드_다이아");
                                if (RouletteManager.Inst.roulettePieces[index].isEnhanced)
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "다이아몬드 룰렛+";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "방어도를 값만큼 부여합니다. 부여 카드가 사용될 때마다 위력이 6 증가합니다.";
                                }
                                else
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "다이아몬드 룰렛";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "방어도를 값만큼 부여합니다. 부여 카드가 사용될 때마다 위력이 4 증가합니다.";
                                }
                                break;
                            case CardType.Enchant:
                                RouletteManager.Inst.roulettePieces[index].roulette.value = 1;
                                RouletteManager.Inst.roulettePieces[index].originalSprite = Utils.LoadSpriteByName("SpecialRoulette", "룰렛_마술카드_하트");
                                if (RouletteManager.Inst.roulettePieces[index].isEnhanced)
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "하트 룰렛+";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "체력을 값만큼 회복합니다. 회전 카드가 사용될 때마다 위력이 1 증가합니다.";
                                }
                                else
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "하트 룰렛";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "체력을 값만큼 회복합니다. 회전 카드가 사용될 때마다 위력이 1 증가합니다.";
                                }
                                break;
                            case CardType.Dream:
                                RouletteManager.Inst.roulettePieces[index].roulette.value = 1;
                                RouletteManager.Inst.roulettePieces[index].originalSprite = Utils.LoadSpriteByName("SpecialRoulette", "룰렛_마술카드_스페이드");
                                if (RouletteManager.Inst.roulettePieces[index].isEnhanced)
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "스페이드 룰렛+";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "행동력을 값만큼 회복합니다. 몽상 카드가 사용될 때마다 위력이 1 증가합니다.";
                                }
                                else
                                {
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipTitle = "스페이드 룰렛";
                                    RouletteManager.Inst.roulettePieces[index].originalTooltipText = "행동력을 값만큼 회복합니다. 몽상 카드가 사용될 때마다 위력이 1 증가합니다.";
                                }
                                break;
                        }
                        RouletteManager.Inst.roulettePieces[index].SetRoulettePieceSprite(RouletteManager.Inst.roulettePieces[index].originalSprite);
                        RouletteManager.Inst.roulettePieces[index].SetRoulettePieceTooltip(RouletteManager.Inst.roulettePieces[index].originalTooltipTitle, RouletteManager.Inst.roulettePieces[index].originalTooltipText);
                    }
                };
                playerSpecialRouletteActivation[GetSpecialRouletteIdx(false, 1)] = (isEnemy, value, enemyIdx, isEnhanced) =>
                {
                    int activateIndex = RouletteManager.Inst.playerLookat;
                    if(isEnemy) activateIndex = RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx);
                    switch(playerSpecialRoulette_lastCardType[activateIndex])
                    {
                        case CardType.Turn:
                            if(isEnemy) TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette, enemyIdx);
                            else TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                            break;
                        case CardType.Skill:
                            TurnManager.Inst.GetShield(isEnemy, value, EDamageSource.Roulette, enemyIdx);
                            break;
                        case CardType.Enchant:
                            if(isEnemy) TurnManager.Inst.EnemyTakeDmg(-value, EDamageSource.Roulette, enemyIdx);
                            else TurnManager.Inst.TakeDmg(-value, EDamageSource.Roulette);
                            break;
                        case CardType.Dream:
                            if(!isEnemy) TurnManager.Inst.IncreaseCost(value);
                            break;
                    }
                    RouletteManager.Inst.roulettePieces[activateIndex].RouletteClear();
                };
                playerSpecialRouletteClear[GetSpecialRouletteIdx(false, 0)] = (index) =>
                {
                    RouletteItem rItem = new RouletteItem();
                    rItem.rtype = new RouletteType(ERouletteType.None, 0);
                    rItem.value = 0;
                    RouletteManager.Inst.roulettePieces[index].Setup(rItem);
                };
                playerSpecialRouletteClear[GetSpecialRouletteIdx(false, 1)] = (index) =>
                {
                    BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[index]].Clear();
                    RouletteItem rItem = new RouletteItem();
                    rItem.rtype = new RouletteType(ERouletteType.None, 0);
                    rItem.value = 0;
                    RouletteManager.Inst.roulettePieces[index].Setup(rItem);
                };
                // 패시브 효과 설정
                int cardCnt = 0;
                int maxCardCnt = 0;
                if (shadowName == "손기술") maxCardCnt = 3;
                else if (shadowName == "손기술+") maxCardCnt = 2;
                TurnManager.OnUseCard += (x, enemyIdx) =>
                {
                    cardCnt++;
                    if(cardCnt >= maxCardCnt)
                    {
                        StartCoroutine(TurnManager.Inst.Draw(1, null));
                        cardCnt = 0;
                    }
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    cardCnt = 0;
                };
                break;
            case "순환하는 계절":
            case "순환하는 계절+":
                // 특수 룰렛 설정
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(false, 0)].Add(new List<Buff>());
                };
                List<FrozenRoulette> frozenRoulettes = new List<FrozenRoulette>();
                TurnManager.CheckRouletteEnchantable += (index, type) =>
                {
                    var frozenChk = frozenRoulettes.Find(x => x.rIdx == index);
                    if (frozenChk != null && RouletteManager.Inst.roulettePieces[index].roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(false, 0)))
                    {
                        return false;
                    }
                    else if(frozenChk == null && type == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(false, 0)))
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
                        frozenSpriteRenderer.sprite = TurnManager.Inst.characterSO.shadowPiece.playerSpecialRoulettes[0].sprite;
                        frozenSpriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                        playerSpecialRoulettes[GetSpecialRouletteIdx(false, 0)].sprite = RouletteManager.Inst.roulettePieces[index].originalSprite;
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
                        if (RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(false, 0)))
                        {
                            RouletteManager.Inst.roulettePieces[i].roulette.value--;
                            int val = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                            if (val == 0)
                            {
                                playerSpecialRouletteClear[GetSpecialRouletteIdx(false, 0)]?.Invoke(i);
                            }
                        }
                    }
                };
                playerSpecialRouletteClear[GetSpecialRouletteIdx(false, 0)] = (index) =>
                {
                    var frozenChk = frozenRoulettes.Find(x => x.rIdx == index);
                    if (frozenChk != null)
                    {
                        RouletteManager.Inst.EnchantRoulettePiece(index, frozenChk.rItem.rtype, frozenChk.rItem.value);
                        Destroy(frozenChk.frozenIcon);
                        frozenRoulettes.Remove(frozenChk);
                    }
                    else
                    {
                        RouletteItem rItem = new RouletteItem();
                        rItem.rtype = new RouletteType(ERouletteType.None, 0);
                        rItem.value = 0;
                        RouletteManager.Inst.roulettePieces[index].Setup(rItem);
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
            case "영역 본능":
            case "영역 본능+":
                // 특수 룰렛 설정
                // 특수 룰렛 설정
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(false, 0)].Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_PlayerSpecial[GetSpecialRouletteIdx(false, 1)].Add(new List<Buff>());
                };
                playerSpecialRouletteActivation[GetSpecialRouletteIdx(false, 0)] = (isEnemy, value, enemyIdx, isEnhanced) =>
                {
                    if (isEnemy)
                    {
                        TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette, enemyIdx);
                    }
                    else
                    {
                        TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                    }
                };
                playerSpecialRouletteClear[GetSpecialRouletteIdx(false, 0)] = (index) =>
                {
                    RouletteItem rItem = new RouletteItem();
                    rItem.rtype = new RouletteType(ERouletteType.None, 0);
                    rItem.value = 0;
                    RouletteManager.Inst.roulettePieces[index].Setup(rItem);
                };
                playerSpecialRouletteActivation[GetSpecialRouletteIdx(false, 1)] = (isEnemy, value, enemyIdx, isEnhanced) =>
                {
                    if (isEnemy)
                    {
                        TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette, enemyIdx);
                    }
                    else
                    {
                        TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                    }
                };
                playerSpecialRouletteClear[GetSpecialRouletteIdx(false, 1)] = (index) =>
                {
                    RouletteItem rItem = new RouletteItem();
                    rItem.rtype = new RouletteType(ERouletteType.None, 0);
                    rItem.value = 0;
                    RouletteManager.Inst.roulettePieces[index].Setup(rItem);
                };
                TurnManager.OnRouletteActivate += () =>
                {
                    if(RouletteManager.Inst.isTriggerActivated) return;
                    var playerPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat];
                    for(int i = 0; i <= Enemy.maxSubEnemyNum; i++)
                    {
                        if(i != 0 && (EnemyManager.Inst.subEnemies[i - 1] == null || EnemyManager.Inst.subEnemies[i - 1].name == null)) continue;
                        var enemyPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(i)];
                        if (enemyPiece.roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(false, 0)))
                        {
                            enemyPiece.roulette.value -= 3;
                            if(enemyPiece.roulette.value <= 0)
                            {
                                enemyPiece.RouletteClear();
                            }
                        }
                        else if(enemyPiece.roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(false, 1)))
                        {
                            enemyPiece.RouletteClear();
                            BuffManager.Inst.roulettePieceBuff[enemyPiece].Clear();
                        }
                    }
                    if (playerPiece.roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(false, 0)))
                    {
                        playerPiece.roulette.value -= 3;
                        if(playerPiece.roulette.value <= 0)
                        {
                            playerPiece.RouletteClear();
                        }
                    }
                    else if(playerPiece.roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(false, 1)))
                    {
                        playerPiece.RouletteClear();
                        BuffManager.Inst.roulettePieceBuff[playerPiece].Clear();
                    }
                };
                TurnManager.OnRouletteSpin += (isClockwise, spinValue) =>
                {
                    for(int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if(RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(false, 1)))
                        {
                            BuffManager.AddBuffToTarget(BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[i]], spinValue, 1, -1);
                        }
                    }
                };
                // 패시브 효과 설정
                TurnManager.OnGameStart += () =>
                {
                    if (!DataManager.Inst.characterSO.isTutorial)
                    {
                        EnemyAction.EnchantAction(new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(false, 0)), playerSpecialRoulettes[GetSpecialRouletteIdx(false, 0)].baseVal);
                        if(shadowName == "영역 본능+") EnemyAction.EnchantAction(new RouletteType(ERouletteType.Player_Special, GetSpecialRouletteIdx(false, 0)), playerSpecialRoulettes[GetSpecialRouletteIdx(false, 0)].baseVal);
                    }
                };
                break;
        }
    }

    private void OnDestroy()
    {
        for(int i = 0; i < DreamPiece_Base.playerSpecialRouletteNum * 2; i++)
        {
            playerSpecialRouletteActivation[i] = null;
            playerSpecialRouletteClear[i] = null;
        }
    }
}

class FrozenRoulette
{
    public int rIdx;
    public RouletteItem rItem;
    public GameObject frozenIcon;
}
