using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PassiveManager : MonoBehaviour
{
    public static PassiveManager Inst { get; private set; }
    private void Awake() => Inst = this;
    
    public static Action<bool, int> PlayerSpecialRoulette1Activation;
    public static Action<bool, int> PlayerSpecialRoulette2Activation;
    public Sprite PlayerSpecialRoulette1Sprite;
    public string PlayerSpecialRoulette1Title;
    public string PlayerSpecialRoulette1Text;
    public Sprite PlayerSpecialRoulette2Sprite;
    public string PlayerSpecialRoulette2Title;
    public string PlayerSpecialRoulette2Text;

    public void SetPersona()
    {
        if (TurnManager.Inst.characterSO.personaPiece == null) return;
        PlayerSpecialRoulette1Sprite = TurnManager.Inst.characterSO.personaPiece.specialRouletteSprite;
        PlayerSpecialRoulette1Title = TurnManager.Inst.characterSO.personaPiece.specialRouletteTitle;
        PlayerSpecialRoulette1Text = TurnManager.Inst.characterSO.personaPiece.specialRouletteText;
        switch (TurnManager.Inst.characterSO.personaPiece.persona.name)
        {
            case "물보다 진한 피":
                TurnManager.Inst.playerTriggerMaxCnt = 12;
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial1.Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_PlayerSpecial1.Add(new List<Buff>());
                };
                PlayerSpecialRoulette1Activation = (isEnemy, value) =>
                {
                    if (isEnemy)
                    {
                        int trueDamage = TurnManager.Inst.EnemyTakeDmg(value);
                        int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_PlayerSpecial1[1], trueDamage / 3);
                        TurnManager.Inst.TakeDmg(-totalVal_Heal);
                    }
                    else
                    {
                        int trueDamage = TurnManager.Inst.TakeDmg(value);
                        int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_PlayerSpecial1[1], trueDamage / 3);
                        TurnManager.Inst.TakeDmg(-totalVal_Heal);
                    }
                };
                TurnManager.OnRouletteActivate += () =>
                {
                    if (TurnManager.Inst.characterSO.personaPiece.persona.name == "물보다 진한 피")
                    {
                        var playerPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat];
                        var enemyPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat];
                        if (enemyPiece.roulette.type == ERouletteType.Attack)
                        {
                            int damage = BuffManager.Inst.GetBuffedRouletteValue(enemyPiece);
                            TurnManager.Inst.TakeDmg(-damage / 3);
                            TurnManager.Inst.TriggerPlayerPassive(damage / 3);
                        }
                        if (playerPiece.roulette.type == ERouletteType.Attack)
                        {
                            int damage = BuffManager.Inst.GetBuffedRouletteValue(playerPiece);
                            TurnManager.Inst.TakeDmg(-damage / 3);
                            TurnManager.Inst.TriggerPlayerPassive(damage / 3);
                        }
                    }
                    /*else if (TurnManager.Inst.characterSO.personaPiece.persona.name == "저주받은 피")
                    {
                        if (RouletteManager.Inst.playerLookat == RouletteManager.Inst.triggerPos || RouletteManager.Inst.enemyLookat == RouletteManager.Inst.triggerPos)
                        {
                            TurnManager.Inst.characterSO.personaPiece.persona.name = "물보다 진한 피";
                            TurnManager.Inst.turnCost++;
                        }
                    }*/
                };
                /*TurnManager.OnRouletteTrigger += () =>
                {
                    TurnManager.Inst.characterSO.personaPiece.persona.name = "저주받은 피";
                    TurnManager.Inst.characterSO.personaPiece.persona.text = "플레이어 턴 시작 시 행동력을 1 잃습니다. 플레이어의 트리거 효과가 해제되면 효과가 제거됩니다.";
                    TurnManager.Inst.turnCost--;
                };*/
                break;
            case "카드 숨기기":
                TurnManager.Inst.playerTriggerMaxCnt = 99;
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial1.Add(new List<Buff>());
                };
                TurnManager.OnGameStart += () =>
                {
                    Item ace = new Item();
                    ace.name = "에이스";
                    ace.cost = 1;
                    ace.type = CardType.Effect;
                    ace.element = EPassiveType.Normal;
                    ace.dreamPieceNum = -1;
                    ace.isVolatile = false;
                    ace.isVanish = false;
                    ace.isRemain = false;
                    ace.text = "트리거 게이지를 최대로 얻습니다. 이번 턴이 종료될 때 12번 슬롯을 비활성화합니다.";
                    ace.cardValues = new List<int>();
                    ace.num = 1;
                    CardManager.Inst.itemDeck.Add(ace);
                    CardManager.Inst.itemDraw.Add(ace);
                    CardManager.Inst.ShuffleDeck();

                    TurnManager.OnRouletteSpin += (x) =>
                    {
                        if (RouletteManager.Inst.spinDirection == 1)
                        {
                            for (int i = 0; i < RouletteManager.rouletteNum; i++)
                            {
                                if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Player_Special_1)
                                {
                                    RouletteManager.Inst.roulettePieces[i].roulette.value--;
                                    if (RouletteManager.Inst.roulettePieces[i].roulette.value == 0)
                                    {
                                        RouletteItem tempItem = new RouletteItem();
                                        RouletteManager.Inst.roulettePieces[i].roulette.type = ERouletteType.None;
                                        RouletteManager.Inst.roulettePieces[i].Setup(RouletteManager.Inst.roulettePieces[i].roulette);
                                    }
                                }
                            }
                        }
                    };
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
        switch (TurnManager.Inst.characterSO.shadowPiece.shadow.name)
        {
            case "해방된 본능":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial2.Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_PlayerSpecial2.Add(new List<Buff>());
                };
                PlayerSpecialRoulette2Activation = (isEnemy, value) =>
                {
                    if (isEnemy)
                    {
                        int trueDamage = TurnManager.Inst.EnemyTakeDmg(value);
                        int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_PlayerSpecial1[1], trueDamage);
                        TurnManager.Inst.EnemyTakeDmg(-totalVal_Heal);
                    }
                    else
                    {
                        int trueDamage = TurnManager.Inst.TakeDmg(value);
                        int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_PlayerSpecial1[1], trueDamage);
                        TurnManager.Inst.EnemyTakeDmg(-totalVal_Heal);
                    }
                };
                rItem.type = ERouletteType.Attack;
                rItem.value = 8;
                RouletteManager.Inst.triggerPiece = rItem;
                RouletteManager.PlayerTriggerActivation = (isEnemy, totalVal) =>
                {
                    if (isEnemy)
                    {
                        TurnManager.Inst.EnemyTakeDmg(totalVal);
                    }
                    else
                    {
                        TurnManager.Inst.TakeDmg(totalVal);
                    }
                };
                TurnManager.OnPlayerTrigger += () =>
                {
                    BuffManager.Inst.rouletteBuff_Trigger.Clear();
                    BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, TurnManager.Inst.nowCost * 8, 1, 1);
                };
                TurnManager.OnCostChange += (x) =>
                {
                    if (RouletteManager.Inst.roulettePieces[RouletteManager.Inst.triggerPos].roulette == RouletteManager.Inst.triggerPiece)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, x * 8, 1, 1);
                    }
                };
                break;
            case "마술 해체":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial2.Add(new List<Buff>());
                };
                rItem.type = ERouletteType.Attack;
                rItem.value = 0;
                RouletteManager.Inst.triggerPiece = rItem;
                RouletteManager.PlayerTriggerActivation = (isEnemy, totalVal) =>
                {
                    if (isEnemy)
                    {
                        TurnManager.Inst.EnemyTakeDmg(totalVal);
                    }
                    else
                    {
                        TurnManager.Inst.TakeDmg(totalVal);
                    }
                };
                int counter = 0;
                TurnManager.OnPlayerTrigger += () =>
                {
                    BuffManager.Inst.rouletteBuff_Trigger.Clear();
                    counter = RouletteManager.rouletteNum - 1;
                    counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.None);
                    counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.Attack);
                    counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.Shield);
                    BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, counter * 7, 1, 1);
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if (RouletteManager.Inst.roulettePieces[RouletteManager.Inst.triggerPos].roulette == RouletteManager.Inst.triggerPiece)
                    {
                        counter = RouletteManager.rouletteNum - 1;
                        counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.None);
                        counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.Attack);
                        counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.Shield);
                        BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, counter * 7, 1, 1);
                    }
                };
                TurnManager.OnRouletteEnchant += () =>
                {
                    if (RouletteManager.Inst.roulettePieces[RouletteManager.Inst.triggerPos].roulette == RouletteManager.Inst.triggerPiece)
                    {
                        int newCnt = RouletteManager.rouletteNum - 1;
                        newCnt -= RouletteManager.Inst.CountRouletteType(ERouletteType.None);
                        newCnt -= RouletteManager.Inst.CountRouletteType(ERouletteType.Attack);
                        newCnt -= RouletteManager.Inst.CountRouletteType(ERouletteType.Shield);
                        if (newCnt != counter)
                        {
                            BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, (newCnt - counter) * 7, 1, 1);
                            counter = newCnt;
                        }
                    }
                };
                TurnManager.BeforeRouletteActivate += () =>
                {
                    if (RouletteManager.Inst.roulettePieces[RouletteManager.Inst.triggerPos].roulette == RouletteManager.Inst.triggerPiece)
                    {
                        if (RouletteManager.Inst.playerLookat == RouletteManager.Inst.triggerPos || RouletteManager.Inst.enemyLookat == RouletteManager.Inst.triggerPos)
                        {
                            for (int i = 0; i < RouletteManager.rouletteNum; i++)
                            {
                                RouletteManager.Inst.EnchantRoulettePiece(i, ERouletteType.None, 0);
                            }
                        }
                    }
                };
                TurnManager.OnRouletteSpin += (x) =>
                {
                    if (RouletteManager.Inst.spinDirection == 1)
                    {
                        for (int i = 0; i < RouletteManager.rouletteNum; i++)
                        {
                            if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Player_Special_2)
                            {
                                RouletteManager.Inst.roulettePieces[i].roulette.value--;
                                if (RouletteManager.Inst.roulettePieces[i].roulette.value == 0)
                                {
                                    RouletteItem tempItem = new RouletteItem();
                                    RouletteManager.Inst.roulettePieces[i].roulette.type = ERouletteType.None;
                                    RouletteManager.Inst.roulettePieces[i].Setup(RouletteManager.Inst.roulettePieces[i].roulette);
                                }
                            }
                        }
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
