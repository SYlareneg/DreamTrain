using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveManager : MonoBehaviour
{
    public static PassiveManager Inst { get; private set; }
    private void Awake() => Inst = this;
    public void SetPersona()
    {
        if (TurnManager.Inst.characterSO.personaPiece == null) return;
        switch (TurnManager.Inst.characterSO.personaPiece.persona.name)
        {
            case "Thicker than Water":
                TurnManager.Inst.playerTriggerMaxCnt = 3;
                TurnManager.OnRouletteActivate += () =>
                {
                    if (TurnManager.Inst.characterSO.personaPiece.persona.name == "Thicker than Water")
                    {
                        var playerPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat];
                        var enemyPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat];
                        if (enemyPiece.roulette.type == ERouletteType.Attack)
                        {
                            int damage = BuffManager.Inst.GetBuffedRouletteValue(enemyPiece);
                            damage = BuffManager.Inst.GetPlayerBuffValue(BuffManager.Inst.damageBuff, damage);
                            TurnManager.Inst.TakeDmg(-damage / 3);
                            TurnManager.Inst.TriggerPlayerPassive(1);
                        }
                        if (playerPiece.roulette.type == ERouletteType.Attack)
                        {
                            int damage = BuffManager.Inst.GetBuffedRouletteValue(playerPiece);
                            damage = BuffManager.Inst.GetPlayerBuffValue(BuffManager.Inst.damageBuff, damage);
                            TurnManager.Inst.TakeDmg(-damage / 3);
                            TurnManager.Inst.TriggerPlayerPassive(1);
                        }
                    }
                    else if (TurnManager.Inst.characterSO.personaPiece.persona.name == "Cursed Blood")
                    {
                        if (RouletteManager.Inst.playerLookat == RouletteManager.Inst.triggerPos || RouletteManager.Inst.enemyLookat == RouletteManager.Inst.triggerPos)
                        {
                            TurnManager.Inst.characterSO.personaPiece.persona.name = "Thicker than Water";
                            TurnManager.Inst.turnCost++;
                        }
                    }
                };
                TurnManager.OnRouletteTrigger += () =>
                {
                    TurnManager.Inst.characterSO.personaPiece.persona.name = "Cursed Blood";
                    TurnManager.Inst.turnCost--;
                };
                break;
            case "Palming Cards":
                TurnManager.OnGameStart += () =>
                {
                    Item ace = new Item();
                    ace.name = "Ace";
                    ace.cost = 1;
                    ace.type = CardType.Effect;
                    ace.element = EPassiveType.Normal;
                    ace.dreamPieceNum = -1;
                    ace.isVolatile = false;
                    ace.isVanish = false;
                    ace.isRemain = false;
                    ace.text = "Trigger player slot. This trigger always ends at end of turn.";
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
                                if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.MagicBox)
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
        RouletteItem rItem = new RouletteItem();
        switch (TurnManager.Inst.characterSO.shadowPiece.shadow.name)
        {
            case "Unleashed Instinct":
                rItem.type = ERouletteType.Attack;
                rItem.value = 8;
                RouletteManager.Inst.triggerPiece = rItem;
                TurnManager.OnPlayerTrigger += () =>
                {
                    Debug.Log(TurnManager.Inst.nowCost);
                    BuffManager.Inst.AddRouletteBuff(BuffManager.Inst.singleRouletteBuff_Trigger, TurnManager.Inst.nowCost * 8, 1, 1);
                };
                TurnManager.OnCostChange += (x) =>
                {
                    if (RouletteManager.Inst.roulettePieces[RouletteManager.Inst.triggerPos].isTriggered == true)
                    {
                        Debug.Log("Trigger buff");
                        BuffManager.Inst.AddRouletteBuff(BuffManager.Inst.singleRouletteBuff_Trigger, x * 8, 1, 1);
                    }
                };
                break;
            case "Exposure":
                rItem.type = ERouletteType.Attack;
                rItem.value = 0;
                RouletteManager.Inst.triggerPiece = rItem;
                int counter = 0;
                TurnManager.OnPlayerTurnStart += () =>
                {
                    counter = RouletteManager.rouletteNum - 1;
                    counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.None);
                    counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.Attack);
                    counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.Shield);
                    BuffManager.Inst.AddRouletteBuff(BuffManager.Inst.singleRouletteBuff_Trigger, counter * 7, 1, 1);
                };
                TurnManager.OnRouletteEnchant += () =>
                {
                    int newCnt = RouletteManager.rouletteNum - 1;
                    newCnt -= RouletteManager.Inst.CountRouletteType(ERouletteType.None);
                    newCnt -= RouletteManager.Inst.CountRouletteType(ERouletteType.Attack);
                    newCnt -= RouletteManager.Inst.CountRouletteType(ERouletteType.Shield);
                    BuffManager.Inst.AddRouletteBuff(BuffManager.Inst.singleRouletteBuff_Trigger, (newCnt - counter) * 7, 1, 1);
                };
                TurnManager.BeforeRouletteActivate += () =>
                {
                    if (RouletteManager.Inst.roulettePieces[RouletteManager.Inst.triggerPos].isTriggered == true)
                    {
                        if (RouletteManager.Inst.playerLookat == RouletteManager.Inst.triggerPos || RouletteManager.Inst.enemyLookat == RouletteManager.Inst.triggerPos)
                        {
                            for (int i = 0; i < RouletteManager.rouletteNum; i++)
                            {
                                if (i == RouletteManager.Inst.triggerPos)
                                {
                                    continue;
                                }
                                if (RouletteManager.Inst.roulettePieces[i].roulette.type != ERouletteType.None && RouletteManager.Inst.roulettePieces[i].roulette.type != ERouletteType.Attack && RouletteManager.Inst.roulettePieces[i].roulette.type != ERouletteType.Shield)
                                {
                                    RouletteItem rItem = new RouletteItem();
                                    rItem.type = ERouletteType.None;
                                    rItem.value = 0;
                                    RouletteManager.Inst.roulettePieces[i].Setup(rItem);
                                }
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
                            if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.MagicBox)
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
}
