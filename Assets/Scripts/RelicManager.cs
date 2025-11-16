using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Inst { get; private set; }
    private void Awake() => Inst = this;

    public RelicSO relicSO;
    public GameObject relicUIPrefab;
    public List<RelicItem_Enhanceable> relicList;

    public List<RelicUI> RelicItemListToRelicUIList(List<RelicItem_Enhanceable> rItemList, Transform attachUI)
    {
        List<RelicUI> rUIList = new List<RelicUI>();
        List<RelicItem_Enhanceable> sortedRelicList = rItemList.OrderBy(x => x.relicOwner).ToList();

        for(int i = 0; i < sortedRelicList.Count; i++)
        {
            var relicObject = Instantiate(relicUIPrefab, Vector3.zero, Utils.QI);
            relicObject.transform.SetParent(attachUI);
            var relic = relicObject.GetComponent<RelicUI>();

            if (i < sortedRelicList.Count - 1 && sortedRelicList[i + 1].relicOwner == sortedRelicList[i].relicOwner)
            {
                var relic1 = sortedRelicList[i].isEnhanced ? sortedRelicList[i].enhancedRelicItem : sortedRelicList[i];
                var relic2 = sortedRelicList[i].isEnhanced ? sortedRelicList[i + 1].enhancedRelicItem : sortedRelicList[i + 1];
                relic.Setup(relic1, relic2);
                rUIList.Add(relic);
                i++;
            }
            else
            {
                var relic1 = sortedRelicList[i].isEnhanced ? sortedRelicList[i].enhancedRelicItem : sortedRelicList[i];
                relic.Setup(relic1, null);
                rUIList.Add(relic);
            }
        }
        return rUIList;
    }
    public void InitRelicList()
    {
        relicList.Clear();
        foreach (RelicItem_Enhanceable rItem in relicSO.relicItems)
        {
            relicList.Add(rItem);
        }
        if (GameManager.Inst != null)
        {
            GameManager.Inst.RelicList();
        }
    }
    
    public void ActivateRelic(RelicItem relicItem)
    {
        // 특수 이드
        switch(relicItem.relicName)
        {
            case "흔적":
            case "흔적+":
                TurnManager.BeforePlayerTurnStart += () =>
                {
                    int leftShield = TurnManager.Inst.shieldHealth;
                    if (relicItem.relicName == "흔적") leftShield = (int)(leftShield * 0.25f);
                    else leftShield = (int)(leftShield * 0.4f);
                };
                return;
            case "갈증":
            case "갈증+":
                int threshold = (int)(TurnManager.Inst.maxHealth * 0.5f);
                if (relicItem.relicName == "갈증+") threshold = (int)(TurnManager.Inst.maxHealth * 0.75f);
                Action<int, EDamageSource> buffAction = (x, s) =>
                {
                    if (TurnManager.Inst.curHealth <= threshold)
                    {
                        if (BuffManager.Inst.GetShowBuff("활력", EBuffAffectType.Player) != null) BuffManager.Inst.AddShowBuff("활력", EBuffAffectType.Player, 1);
                    }
                    else
                    {
                        BuffManager.Inst.RemoveShowBuff("활력", EBuffAffectType.Player);
                    }
                };
                TurnManager.OnPlayerDamaged += buffAction;
                TurnManager.OnPlayerHealed += buffAction;
                return;
            case "호기심":
            case "호기심+":
                TurnManager.OnUseableItemUse += () =>
                {
                    if (relicItem.relicName == "호기심") TurnManager.Inst.IncreaseCost(1);
                    else TurnManager.Inst.IncreaseCost(2);
                };
                return;
            case "순진무구":
            case "순진무구+":
                float damageMul = 1.5f;
                if(relicItem.relicName == "순진무구+") damageMul = 2f;
                bool cardUsed = false;
                TurnManager.OnPlayerTurnStart += () =>
                {
                    cardUsed = false;
                    BuffManager.AddBuffToTarget(BuffManager.Inst.playerBuff_Damage_Type[(int)EDamageSource.Roulette], 0, damageMul, -1);
                    BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Damage_Type[(int)EDamageSource.Roulette], 0, damageMul, -1);
                };
                TurnManager.OnUseCard += () =>
                {
                    if(cardUsed == false)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.playerBuff_Damage_Type[(int)EDamageSource.Roulette], 0, 1f / damageMul, -1);
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Damage_Type[(int)EDamageSource.Roulette], 0, 1f / damageMul, -1);
                        cardUsed = true;
                    }
                };
                break;
            case "송곳니":
            case "송곳니+":
                int addVal = 3;
                if (relicItem.relicName == "송곳니+") addVal = 5;
                TurnManager.OnGameStart += () =>
                {
                    if (TurnManager.Inst.characterSO.personaPiece.name == "뱀파이어 폴")
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_PlayerSpecial1[0], addVal, 1, -1);
                    }
                    else if(TurnManager.Inst.characterSO.shadowPiece.name == "뱀파이어 폴")
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_PlayerSpecial2[0], addVal, 1, -1);
                    }
                };
                return;
            case "작은 날개":
            case "작은 날개+":
                int mulVal = 3;
                if (relicItem.relicName == "작은 날개+") mulVal = 4;
                TurnManager.OnPlayerTurnEnd += () =>
                {
                    TurnManager.Inst.GetShield(false, TurnManager.Inst.nowCost * mulVal, EDamageSource.Relic);
                };
                return;
            case "평화주의":
            case "평화주의+":
                int shieldVal = 8;
                if (relicItem.relicName == "평화주의+") shieldVal = 10;
                bool chkEnemyDamaged = false;
                TurnManager.OnPlayerTurnStart += () =>
                {
                    chkEnemyDamaged = false;
                };
                TurnManager.OnEnemyDamaged += (x, s) =>
                {
                    chkEnemyDamaged = true;
                };
                TurnManager.OnPlayerTurnEnd += () =>
                {
                    if (chkEnemyDamaged == false)
                    {
                        TurnManager.Inst.GetShield(false, shieldVal, EDamageSource.Relic);
                    }
                };
                return;
        }
        // 일반적인 이드
        Action relicAction = null;
        foreach (var effect in relicItem.relicEffects)
        {
            var localEffect = effect;
            switch (localEffect.type)
            {
                case ERelicActivateEffectType.Player_Shield:
                    relicAction += () => { TurnManager.Inst.shieldHealth += localEffect.value; }; break;
                case ERelicActivateEffectType.Player_Heal:
                    relicAction += () => { TurnManager.Inst.TakeDmg(-localEffect.value, EDamageSource.Relic); }; break;
                case ERelicActivateEffectType.Player_Damage:
                    relicAction += () => { TurnManager.Inst.TakeDmg(localEffect.value, EDamageSource.Relic); }; break;
                case ERelicActivateEffectType.Player_Cost_Increase:
                    relicAction += () => { TurnManager.Inst.IncreaseCost(localEffect.value); }; break;
                case ERelicActivateEffectType.Player_Cost_Decrease:
                    relicAction += () => { TurnManager.Inst.IncreaseCost(-localEffect.value); }; break;
                case ERelicActivateEffectType.Player_Max_Cost_Increase:
                    relicAction += () => { TurnManager.Inst.turnCost += localEffect.value; }; break;
                case ERelicActivateEffectType.Player_Max_Hand_Change:
                    relicAction += () => { TurnManager.Inst.drawCardCount += localEffect.value; }; break;
                case ERelicActivateEffectType.Player_Trigger_Increase:
                    relicAction += () => { TurnManager.Inst.TriggerPlayerPassive(localEffect.value); }; break;
                case ERelicActivateEffectType.Player_Trigger_Decrease:
                    relicAction += () => { TurnManager.Inst.TriggerPlayerPassive(-localEffect.value); }; break;
                case ERelicActivateEffectType.Card_Draw:
                    relicAction += () => { StartCoroutine(TurnManager.Inst.Draw(localEffect.value, null)); }; break;
                case ERelicActivateEffectType.Card_Cost_Change:
                    relicAction += () => { BuffManager.AddBuffToTarget(BuffManager.Inst.allCardCostBuff, localEffect.value, 1, localEffect.value2); }; break;
                case ERelicActivateEffectType.Card_Value_Change: // 구현필요
                    relicAction += () => { }; break;
                case ERelicActivateEffectType.Card_Duplicate_Hand:
                    relicAction += () => { CardManager.Inst.CardSelectModeTransit(ECardSelectMode.Duplicate, localEffect.value); }; break;
                case ERelicActivateEffectType.Card_Duplicate_Deck:  // 구현필요
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
                case ERelicActivateEffectType.Card_Block: // 구현필요
                    relicAction += () => { }; break;
                case ERelicActivateEffectType.Roulette_Value_Change_ADD:
                    relicAction += () => {
                        List<Buff> buffTarget = null;
                        switch (localEffect.rlvalue.type)
                        {
                            case ERouletteType.Attack:
                                buffTarget = BuffManager.Inst.rouletteBuff_Attack; break;
                            case ERouletteType.Heal:
                                buffTarget = BuffManager.Inst.rouletteBuff_Heal; break;
                            case ERouletteType.Shield:
                                buffTarget = BuffManager.Inst.rouletteBuff_Shield; break;
                        }
                        if (buffTarget != null)
                        {
                            BuffManager.AddBuffToTarget(buffTarget, localEffect.value, 1, localEffect.value2);
                        }
                    }; break;
                case ERelicActivateEffectType.Roulette_Value_Change_MUL:
                    relicAction += () => {
                        List<Buff> buffTarget = null;
                        switch (localEffect.rlvalue.type)
                        {
                            case ERouletteType.Attack:
                                buffTarget = BuffManager.Inst.rouletteBuff_Attack; break;
                            case ERouletteType.Heal:
                                buffTarget = BuffManager.Inst.rouletteBuff_Heal; break;
                            case ERouletteType.Shield:
                                buffTarget = BuffManager.Inst.rouletteBuff_Shield; break;
                        }
                        if (buffTarget != null)
                        {
                            BuffManager.AddBuffToTarget(buffTarget, 0, localEffect.value, localEffect.value2);
                        }
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
                case ERelicActivateEffectType.Roulette_Trigger_Cancel:
                    relicAction += () =>
                    {
                        RoulettePiece roulettePiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.triggerPos];
                        roulettePiece.Trigger(false);
                        RouletteManager.Inst.isTriggerActivated = false;
                        roulettePiece.Setup(RouletteManager.Inst.triggerPiece_None);
                    }; break;
                case ERelicActivateEffectType.Enemy_Action_Hide:
                    relicAction += () => { EnemyManager.Inst.HideAction(localEffect.value); }; break;
                case ERelicActivateEffectType.Enemy_Action_Delete:
                    relicAction += () => { EnemyManager.Inst.RemoveAction(localEffect.value); }; break;
                case ERelicActivateEffectType.Enemy_Spin_Reverse:
                    relicAction += () => { EnemyManager.Inst.ReverseSpin(); }; break;
                case ERelicActivateEffectType.Enemy_Spin_Ignore:
                    relicAction += () => { EnemyManager.Inst.RemoveAllSpin(); }; break;
                case ERelicActivateEffectType.Enemy_Damage:
                    relicAction += () => { TurnManager.Inst.EnemyTakeDmg(localEffect.value, EDamageSource.Relic); }; break;
                case ERelicActivateEffectType.Enemy_Shield:
                    relicAction += () => { TurnManager.Inst.GetShield(true, localEffect.value, EDamageSource.Relic); }; break;
                case ERelicActivateEffectType.Enemy_Heal:
                    relicAction += () => { TurnManager.Inst.EnemyTakeDmg(-localEffect.value, EDamageSource.Relic); }; break;
                case ERelicActivateEffectType.Enemy_Trigger_Increase:
                    relicAction += () => { TurnManager.Inst.TriggerEnemyPassive(localEffect.value); }; break;
                case ERelicActivateEffectType.Enemy_Trigger_Decrease:
                    relicAction += () => { TurnManager.Inst.TriggerEnemyPassive(-localEffect.value); }; break;
                case ERelicActivateEffectType.Develop_Test:
                    relicAction += () => { Debug.LogWarning("Develop relic effect activated"); }; break;
                default:
                    Debug.LogWarning("Error in relic effect"); break;
            }
        }
        Action relicActivation = null;
        if(relicItem.relicConditions.Length == 0)
        {
            relicActivation = relicAction;
        }
        foreach (var conditionAND in relicItem.relicConditions)
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
                    case ERelicActivateConditionType.Player_Card_Num_GE:
                        totalCondition = () =>
                        {
                            if (CardManager.Inst.myCards.Count >= localCondition.value)
                            {
                                temp?.Invoke();
                            }
                        }; break;
                    case ERelicActivateConditionType.Player_Card_Num_EQ:
                        totalCondition = () =>
                        {
                            if (CardManager.Inst.myCards.Count == localCondition.value)
                            {
                                temp?.Invoke();
                            }
                        }; break;
                    case ERelicActivateConditionType.Player_Card_Num_LE:
                        totalCondition = () =>
                        {
                            if (CardManager.Inst.myCards.Count <= localCondition.value)
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
        foreach (var timing in relicItem.relicTimings)
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
                    TurnManager.OnRouletteSpin += (x) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Roulette_Trigger:
                    TurnManager.OnRouletteTrigger += relicActivation; break;
                case ERelicActivateTimingType.Roulette_Enchant:
                    TurnManager.OnRouletteEnchant += (x) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Roulette_Activate:
                    TurnManager.OnRouletteActivate += relicActivation; break;
                case ERelicActivateTimingType.Card_Use:
                    TurnManager.OnUseCard += relicActivation; break;
                case ERelicActivateTimingType.Card_Draw:
                    TurnManager.OnAddCard += relicActivation; break;
                case ERelicActivateTimingType.Enemy_Damage:
                    TurnManager.OnEnemyDamaged += (x, s) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Enemy_Heal:
                    TurnManager.OnEnemyHealed += (x, s) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Enemy_Trigger:
                    TurnManager.OnEnemyTrigger += relicActivation; break;
                case ERelicActivateTimingType.Enemy_Trigger_Increase:
                    TurnManager.OnEnemyTriggerIncrease += (x) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Enemy_Trigger_Decrease:
                    TurnManager.OnEnemyTriggerDecrease += (x) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Enemy_Shield:
                    TurnManager.OnEnemyShielded += (x, s) => relicActivation(); break;
                case ERelicActivateTimingType.Enemy_Action:
                    TurnManager.OnEnemyAction += relicActivation; break;
                case ERelicActivateTimingType.Player_Damage:
                    TurnManager.OnPlayerDamaged += (x, s) => relicActivation(); break;
                case ERelicActivateTimingType.Player_Heal:
                    TurnManager.OnPlayerHealed += (x, s) => relicActivation(); break;
                case ERelicActivateTimingType.Player_Trigger:
                    TurnManager.OnPlayerTrigger += relicActivation; break;
                case ERelicActivateTimingType.Player_Trigger_Increase:
                    TurnManager.OnPlayerTriggerIncrease += (x) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Player_Trigger_Decrease:
                    TurnManager.OnPlayerTriggerDecrease += (x) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Player_Shield:
                    TurnManager.OnPlayerShielded += (x, s) => relicActivation(); break;
                case ERelicActivateTimingType.Cost_Change:
                    TurnManager.OnCostChange += (x) => relicActivation(); break;
                default:
                    Debug.LogWarning("Error in relic timing"); break;
            }
        }
    }

    public void ActivateRelics()
    {
        InitRelicList();
        for (int i = 0; i < relicList.Count; i++)
        {
            if (relicList[i].isEnhanced) ActivateRelic(relicList[i].enhancedRelicItem);
            else ActivateRelic(relicList[i]);
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
        TurnManager.OnPlayerTriggerIncrease = null;
        TurnManager.OnPlayerTriggerDecrease = null;
        TurnManager.OnEnemyDamaged = null;
        TurnManager.OnEnemyHealed = null;
        TurnManager.OnEnemyShielded = null;
        TurnManager.OnEnemyTrigger = null;
        TurnManager.OnEnemyTriggerIncrease = null;
        TurnManager.OnEnemyTriggerDecrease = null;
        TurnManager.OnEnemyAction = null;
        TurnManager.OnRouletteSpin = null;
        TurnManager.OnRouletteTrigger = null;
        TurnManager.OnRouletteEnchant = null;
        TurnManager.OnRouletteActivate = null;
    }
}

