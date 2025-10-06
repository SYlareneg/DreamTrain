using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Inst { get; private set; }
    private void Awake() => Inst = this;

    public RelicSO relicSO;
    public GameObject relicUIPrefab;
    public List<RelicItem> relicList;

    public List<RelicUI> RelicItemListToRelicUIList(List<RelicItem> rItemList, Transform attachUI)
    {
        List<RelicUI> rUIList = new List<RelicUI>();
        List<RelicItem> sortedRelicList = rItemList.OrderBy(x => x.relicName).ToList();

        foreach (RelicItem rItem in sortedRelicList)
        {
            var relicObject = Instantiate(relicUIPrefab, Vector3.zero, Utils.QI);
            relicObject.transform.SetParent(attachUI);
            var relic = relicObject.GetComponent<RelicUI>();

            relic.Setup(rItem);
            rUIList.Add(relic);
        }

        return rUIList;
    }
    public void InitRelicList()
    {
        relicList.Clear();
        foreach (RelicItem rItem in relicSO.relicItems)
        {
            relicList.Add(rItem);
        }
    }

    public void ActivateRelics()
    {
        InitRelicList();
        for (int i = 0; i < relicList.Count; i++)
        {
            Action relicAction = null;
            foreach (var effect in relicList[i].relicEffects)
            {
                var localEffect = effect;
                switch (localEffect.type)
                {
                    case ERelicActivateEffectType.Player_Shield:
                        relicAction += () => { TurnManager.Inst.shieldHealth += localEffect.value; }; break;
                    case ERelicActivateEffectType.Player_Heal:
                        relicAction += () => { TurnManager.Inst.TakeDmg(-localEffect.value); }; break;
                    case ERelicActivateEffectType.Player_Cost_Increase:
                        relicAction += () => { TurnManager.Inst.IncreaseCost(localEffect.value); }; break;
                    case ERelicActivateEffectType.Player_Max_Cost_Increase:
                        relicAction += () => { TurnManager.Inst.turnCost += localEffect.value; }; break;
                    case ERelicActivateEffectType.Card_Draw:
                        relicAction += () => { StartCoroutine(TurnManager.Inst.Draw(localEffect.value, null)); }; break;
                    case ERelicActivateEffectType.Card_Cost_Change:
                        relicAction += () => { BuffManager.Inst.AddCardBuff(BuffManager.Inst.allCardCostBuff, localEffect.value, 1, localEffect.value2); }; break;
                    case ERelicActivateEffectType.Card_Value_Change:
                        relicAction += () => { }; break;
                    case ERelicActivateEffectType.Card_Duplicate_Hand:
                        relicAction += () => { }; break;
                    case ERelicActivateEffectType.Card_Duplicate_Deck:
                        relicAction += () => { }; break;
                    case ERelicActivateEffectType.Card_Add_Hand:
                        relicAction += () =>
                        {
                            CardManager.Inst.itemDeck.Add(localEffect.ivalue);
                            CardManager.Inst.CreateCardInHand(localEffect.ivalue);
                        }; break;
                    case ERelicActivateEffectType.Card_Add_Draw:
                        relicAction += () =>
                        {
                            CardManager.Inst.itemDeck.Add(localEffect.ivalue);
                            CardManager.Inst.itemDraw.Add(localEffect.ivalue);
                        }; break;
                    case ERelicActivateEffectType.Card_Add_Discard:
                        relicAction += () =>
                        {
                            CardManager.Inst.itemDeck.Add(localEffect.ivalue);
                            CardManager.Inst.itemDiscard.Add(localEffect.ivalue);
                        }; break;
                    case ERelicActivateEffectType.Roulette_Value_Change_ADD:
                        relicAction += () => {
                            RouletteManager.Inst.roulettePieces[localEffect.value].roulette.value += localEffect.value2;
                            RouletteManager.Inst.roulettePieces[localEffect.value].Setup(RouletteManager.Inst.roulettePieces[localEffect.value].roulette);
                        }; break;
                    case ERelicActivateEffectType.Roulette_Value_Change_MUL:
                        relicAction += () => {
                            RouletteManager.Inst.roulettePieces[localEffect.value].roulette.value *= localEffect.value2;
                            RouletteManager.Inst.roulettePieces[localEffect.value].Setup(RouletteManager.Inst.roulettePieces[localEffect.value].roulette);
                        }; break;
                    case ERelicActivateEffectType.Roulette_Spin_CW:
                        relicAction += () => { RouletteManager.Inst.Spin(true, localEffect.value); }; break;
                    case ERelicActivateEffectType.Roulette_Spin_CCW:
                        relicAction += () => { RouletteManager.Inst.Spin(false, localEffect.value); }; break;
                    case ERelicActivateEffectType.Roulette_Enchant_Type:
                        relicAction += () => { RouletteManager.Inst.EnchantRoulettePiece(localEffect.value, localEffect.rlvalue.type, RouletteManager.Inst.roulettePieces[localEffect.value].roulette.value); }; break;
                    case ERelicActivateEffectType.Roulette_Enchant_Val:
                        relicAction += () => { RouletteManager.Inst.EnchantRoulettePiece(localEffect.value, RouletteManager.Inst.roulettePieces[localEffect.value].roulette.type, localEffect.rlvalue.value); }; break;
                    case ERelicActivateEffectType.Roulette_Trigger:
                        relicAction += () => { RouletteManager.Inst.TriggerRoulette(); }; break;
                    case ERelicActivateEffectType.Enemy_Action_Hide:
                        relicAction += () => { EnemyManager.Inst.HideAction(localEffect.value); }; break;
                    case ERelicActivateEffectType.Enemy_Action_Delete:
                        relicAction += () => { EnemyManager.Inst.RemoveAction(localEffect.value); }; break;
                    case ERelicActivateEffectType.Enemy_Spin_Reverse:
                        relicAction += () => { EnemyManager.Inst.ReverseSpin(); }; break;
                    case ERelicActivateEffectType.Enemy_Spin_Ignore:
                        relicAction += () => { EnemyManager.Inst.RemoveAllSpin(); }; break;
                    case ERelicActivateEffectType.Enemy_Damage:
                        relicAction += () => { TurnManager.Inst.EnemyTakeDmg(localEffect.value); }; break;
                    case ERelicActivateEffectType.Develop_Test:
                        relicAction += () => { Debug.LogWarning("Develop relic effect activated"); }; break;
                    default:
                        Debug.LogWarning("Error in relic effect"); break;
                }
            }
            Action relicActivation = null;
            foreach (var conditionAND in relicList[i].relicConditions)
            {
                Action totalCondition = relicAction;
                foreach (var condition in conditionAND.conditions)
                {
                    var localCondition = condition;
                    Action temp = totalCondition;
                    switch (localCondition.type)
                    {
                        case ERelicActivateConditionType.None:
                            break;
                        case ERelicActivateConditionType.Turn_Count:
                            totalCondition = () =>
                            {
                                if (TurnManager.Inst.turnNum % localCondition.value == 0)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Turn_GE:
                            totalCondition = () =>
                            {
                                if (TurnManager.Inst.turnNum >= localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Turn_EQ:
                            totalCondition = () =>
                            {
                                if (TurnManager.Inst.turnNum == localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_Count:
                            totalCondition = () =>
                            {
                                if (RouletteManager.Inst.spinCount % localCondition.value == 0)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_Count_Turn:
                            totalCondition = () =>
                            {
                                if (RouletteManager.Inst.spinCount_Turn % localCondition.value == 0)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_Count_GE:
                            totalCondition = () =>
                            {
                                if (RouletteManager.Inst.spinCount >= localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_Count_GE_Turn:
                            totalCondition = () =>
                            {
                                if (RouletteManager.Inst.spinCount_Turn >= localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_Count_EQ:
                            totalCondition = () =>
                            {
                                if (RouletteManager.Inst.spinCount == localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_Count_EQ_Turn:
                            totalCondition = () =>
                            {
                                if (RouletteManager.Inst.spinCount_Turn == localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_Direction:
                            totalCondition = () =>
                            {
                                if (RouletteManager.Inst.spinDirection == localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_Distance:
                            totalCondition = () =>
                            {
                                if (RouletteManager.Inst.spinDistance % localCondition.value == 0)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_Distance_Turn:
                            totalCondition = () =>
                            {
                                if (RouletteManager.Inst.spinDistance_Turn % localCondition.value == 0)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_Distance_GE:
                            totalCondition = () =>
                            {
                                if (RouletteManager.Inst.spinDistance >= localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_Distance_GE_Turn:
                            totalCondition = () =>
                            {
                                if (RouletteManager.Inst.spinDistance_Turn >= localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_Distance_EQ:
                            totalCondition = () =>
                            {
                                if (RouletteManager.Inst.spinDistance == localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_Distance_EQ_Turn:
                            totalCondition = () =>
                            {
                                if (RouletteManager.Inst.spinDistance_Turn == localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_IsSpinned:
                            totalCondition = () =>
                            {
                                if ((RouletteManager.Inst.spinCount > 0) == (localCondition.value > 0))
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Roulette_IsSpinned_Turn:
                            totalCondition = () =>
                            {
                                if ((RouletteManager.Inst.spinCount_Turn > 0) == (localCondition.value > 0))
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Card_Cost:
                            totalCondition = () =>
                            {
                                if (CardManager.Inst.selectedCard.item.cost == localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Card_Count:
                            totalCondition = () =>
                            {
                                if (CardManager.Inst.useCount % localCondition.value == 0)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Card_Count_Turn:
                            totalCondition = () =>
                            {
                                if (CardManager.Inst.useCount_Turn % localCondition.value == 0)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Card_Count_GE:
                            totalCondition = () =>
                            {
                                if (CardManager.Inst.useCount >= localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Card_Count_GE_Turn:
                            totalCondition = () =>
                            {
                                if (CardManager.Inst.useCount_Turn >= localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Card_Count_EQ:
                            totalCondition = () =>
                            {
                                if (CardManager.Inst.useCount == localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Card_Count_EQ_Turn:
                            totalCondition = () =>
                            {
                                if (CardManager.Inst.useCount_Turn == localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Card_IsUsed:
                            totalCondition = () =>
                            {
                                if ((CardManager.Inst.useCount > 0) == (localCondition.value > 0))
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Card_IsUsed_Turn:
                            totalCondition = () =>
                            {
                                if ((CardManager.Inst.useCount_Turn > 0) == (localCondition.value > 0))
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Card_Type:
                            totalCondition = () =>
                            {
                                if (CardManager.Inst.selectedCard.item.type == localCondition.ivalue.type)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Card_Element:
                            totalCondition = () =>
                            {
                                if (CardManager.Inst.selectedCard.item.element == localCondition.ivalue.element)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Enemy_Health_GE:
                            totalCondition = () =>
                            {
                                if (((float)TurnManager.Inst.enemyCurHealth / TurnManager.Inst.enemyMaxHealth) >= localCondition.fvalue)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Enemy_Health_LE:
                            totalCondition = () =>
                            {
                                if (((float)TurnManager.Inst.enemyCurHealth / TurnManager.Inst.enemyMaxHealth) <= localCondition.fvalue)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Enemy_Shield_GE:
                            totalCondition = () =>
                            {
                                if (TurnManager.Inst.enemyShieldHealth >= localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Enemy_Action_Type:
                            totalCondition = () =>
                            {
                                if (EnemyManager.Inst.lastAction.actionType == localCondition.actionType)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Player_Health_GE:
                            totalCondition = () =>
                            {
                                if (((float)TurnManager.Inst.curHealth / TurnManager.Inst.maxHealth) >= localCondition.fvalue)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Player_Health_LE:
                            totalCondition = () =>
                            {
                                if (((float)TurnManager.Inst.curHealth / TurnManager.Inst.maxHealth) <= localCondition.fvalue)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Player_Shield_GE:
                            totalCondition = () =>
                            {
                                if (TurnManager.Inst.shieldHealth >= localCondition.value)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        case ERelicActivateConditionType.Activate_Trigger:
                            totalCondition = () =>
                            {
                                if (RouletteManager.Inst.playerLookat == RouletteManager.Inst.triggerPos || RouletteManager.Inst.enemyLookat == RouletteManager.Inst.triggerPos)
                                {
                                    temp?.Invoke();
                                }
                            }; break;
                        default:
                            Debug.LogWarning("Error in relic condition"); break;
                    }
                }
                relicActivation += totalCondition; break;
            }
            foreach (var timing in relicList[i].relicTimings)
            {
                var localTiming = timing;
                switch (localTiming)
                {
                    case ERelicActivateTimingType.Player_Turn_Start:
                        TurnManager.OnPlayerTurnStart += relicActivation; break;
                    case ERelicActivateTimingType.Player_Turn_End:
                        TurnManager.OnPlayerTurnEnd += relicActivation; break;
                    case ERelicActivateTimingType.Enemy_Turn_Start:
                        TurnManager.OnEnemyTurnStart += relicActivation; break;
                    case ERelicActivateTimingType.Enemy_Turn_End:
                        TurnManager.OnEnemyTurnEnd += relicActivation; break;
                    case ERelicActivateTimingType.Game_Start:
                        TurnManager.OnGameStart += relicActivation; break;
                    case ERelicActivateTimingType.Game_End:
                        TurnManager.OnGameEnd += relicActivation; break;
                    case ERelicActivateTimingType.Roulette_Spin:
                        TurnManager.OnRouletteSpin += (x) => relicActivation(); break;
                    case ERelicActivateTimingType.Roulette_Trigger:
                        TurnManager.OnRouletteTrigger += relicActivation; break;
                    case ERelicActivateTimingType.Roulette_Enchant:
                        TurnManager.OnRouletteEnchant += relicActivation; break;
                    case ERelicActivateTimingType.Roulette_Activate:
                        TurnManager.OnRouletteActivate += relicActivation; break;
                    case ERelicActivateTimingType.Card_Use:
                        TurnManager.OnUseCard += relicActivation; break;
                    case ERelicActivateTimingType.Card_Draw:
                        TurnManager.OnAddCard += relicActivation; break;
                    case ERelicActivateTimingType.Enemy_Damage:
                        TurnManager.OnEnemyDamaged += (x) => relicActivation(); break;
                    case ERelicActivateTimingType.Enemy_Heal:
                        TurnManager.OnEnemyHealed += (x) => relicActivation(); break;
                    case ERelicActivateTimingType.Enemy_Trigger:
                        TurnManager.OnEnemyTrigger += relicActivation; break;
                    case ERelicActivateTimingType.Enemy_Shield:
                        TurnManager.OnEnemyShielded += (x) => relicActivation(); break;
                    case ERelicActivateTimingType.Enemy_Action:
                        TurnManager.OnEnemyAction += relicActivation; break;
                    case ERelicActivateTimingType.Player_Damage:
                        TurnManager.OnPlayerDamaged += (x) => relicActivation(); break;
                    case ERelicActivateTimingType.Player_Heal:
                        TurnManager.OnPlayerHealed += (x) => relicActivation(); break;
                    case ERelicActivateTimingType.Player_Trigger:
                        TurnManager.OnPlayerTrigger += relicActivation; break;
                    case ERelicActivateTimingType.Player_Shield:
                        TurnManager.OnPlayerShielded += (x) => relicActivation(); break;
                    case ERelicActivateTimingType.Cost_Change:
                        TurnManager.OnCostChange += (x) => relicActivation(); break;
                    default:
                        Debug.LogWarning("Error in relic timing"); break;
                }
            }
        }
    }

    private void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart = null;
        TurnManager.OnPlayerTurnEnd = null;
        TurnManager.OnEnemyTurnStart = null;
        TurnManager.OnEnemyTurnEnd = null;
        TurnManager.OnGameStart = null;
        TurnManager.OnGameEnd = null;
        TurnManager.OnUseCard = null;
        TurnManager.OnAddCard = null;
        TurnManager.OnDiscardCard = null;
        TurnManager.OnPlayerDamaged = null;
        TurnManager.OnPlayerHealed = null;
        TurnManager.OnPlayerShielded = null;
        TurnManager.OnPlayerTrigger = null;
        TurnManager.OnEnemyDamaged = null;
        TurnManager.OnEnemyHealed = null;
        TurnManager.OnEnemyShielded = null;
        TurnManager.OnEnemyTrigger = null;
        TurnManager.OnEnemyAction = null;
        TurnManager.OnRouletteSpin = null;
        TurnManager.OnRouletteTrigger = null;
        TurnManager.OnRouletteEnchant = null;
        TurnManager.OnRouletteActivate = null;
    }
}
