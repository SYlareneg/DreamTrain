using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;
using DG.Tweening;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Inst { get; private set; }
    private void Awake() => Inst = this;

    public RelicSO relicSO;
    public GameObject relicUIPrefab;
    public List<RelicItem_Enhanceable> relicList;
    public List<RelicItem> relicActivationList;
    [SerializeField][Tooltip("이드 발동 효과 표시 위치")] RectTransform relicActivateEffectPos;
    [SerializeField][Tooltip("이드 발동 효과 표시 시간")] float relicActivateEffectTime = 1.5f;

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

    public void RelicActivateEffect()
    {
        if(relicActivateEffectPos.gameObject.activeSelf == false && relicActivationList.Count > 0)
        {
            relicActivateEffectPos.gameObject.SetActive(true);
            for(int i = relicActivationList.Count - 1; i >= 0; i--)
            {
                var relicUIObj = Instantiate(relicUIPrefab, relicActivateEffectPos.transform);
                var relicUI = relicUIObj.GetComponent<RelicUI>();
                relicUI.Setup(relicActivationList[i], null);
                relicActivationList.RemoveAt(i);
            }
            Sequence seq = DOTween.Sequence();
            seq.Append(DOTween.To(() => relicActivateEffectPos.pivot, x => relicActivateEffectPos.pivot = x, new Vector2(1f, relicActivateEffectPos.pivot.y), relicActivateEffectTime * 0.33f).SetEase(Ease.InOutQuad));
            seq.AppendInterval(relicActivateEffectTime * 0.33f);
            seq.Append(DOTween.To(() => relicActivateEffectPos.pivot, x => relicActivateEffectPos.pivot = x, new Vector2(0f, relicActivateEffectPos.pivot.y), relicActivateEffectTime * 0.33f).SetEase(Ease.InOutQuad));
            seq.OnComplete(() =>
            {
                foreach (Transform child in relicActivateEffectPos)
                {
                    Destroy(child.gameObject);
                }
                relicActivateEffectPos.gameObject.SetActive(false);
            });
            seq.Play();
        }
    }
    
    public void ActivateRelic(RelicItem relicItem)
    {
        // 특수 이드
        switch(relicItem.relicName)
        {
            case "순진무구":
            case "순진무구+":
                float damageMul = 1.5f;
                if(relicItem.relicName == "순진무구+") damageMul = 2f;
                bool cardUsed = false;
                TurnManager.OnPlayerTurnStart += () =>
                {
                    cardUsed = false;
                    BuffManager.AddBuffToTarget(BuffManager.Inst.playerBuff_Damage_Type[(int)EDamageSource.Roulette], 0, damageMul, -1);
                    BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Damage_Type[0, (int)EDamageSource.Roulette], 0, damageMul, -1);
                };
                TurnManager.OnUseCard += (x) =>
                {
                    if(cardUsed == false)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.playerBuff_Damage_Type[(int)EDamageSource.Roulette], 0, 1f / damageMul, -1);
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Damage_Type[0, (int)EDamageSource.Roulette], 0, 1f / damageMul, -1);
                        cardUsed = true;
                    }
                };
                TurnManager.OnPlayerTurnEnd += () =>
                {
                    if(cardUsed == false)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.playerBuff_Damage_Type[(int)EDamageSource.Roulette], 0, 1f / damageMul, -1);
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Damage_Type[0, (int)EDamageSource.Roulette], 0, 1f / damageMul, -1);
                        cardUsed = true;
                    }
                };
                TurnManager.OnPlayerDamaged += (x, s) =>
                {
                    if (cardUsed == false && s == EDamageSource.Roulette)
                    {
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                TurnManager.OnEnemyDamaged += (x, s, i) =>
                {
                    if (cardUsed == false && s == EDamageSource.Roulette)
                    {
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                break;
            case "박쥐 날개":
            case "박쥐 날개+":
                int shieldMul = 3;
                if (relicItem.relicName == "박쥐 날개+") shieldMul = 4;
                TurnManager.OnPlayerTurnEnd += () =>
                {
                    if (TurnManager.Inst.nowCost != 0)
                    {
                        TurnManager.Inst.GetShield(false, TurnManager.Inst.nowCost * shieldMul, EDamageSource.Relic);
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                return;
            case "저주 인형":
            case "저주 인형+":
                Action<int, EDamageSource> curseDollActivate = null;
                curseDollActivate = (x, s) =>
                {
                    TurnManager.Inst.shieldHealth += x;
                    relicItem.relicVal -= x;
                    if(relicItem.relicVal <= 0)
                    {
                        relicItem.relicVal = 0;
                        RelicItem_Enhanceable curseDollRelic = relicSO.relicItems.Find(r => r.relicName == relicItem.relicName || r.enhancedRelicItem.relicName == relicItem.relicName);
                        relicSO.relicItems.Remove(curseDollRelic);
                    }
                    TurnManager.OnPlayerDamaged -= curseDollActivate;
                    Debug.Log("Relic Activate: " + relicItem.relicName);
                    if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                };
                TurnManager.OnPlayerDamaged += curseDollActivate;
                return;
            case "꿀":
            case "꿀+":
                float healMul = 0.25f;
                if (relicItem.relicName == "꿀+") healMul = 0.4f;
                TurnManager.BeforePlayerTurnStart += () =>
                {
                    int healVal = (int)(TurnManager.Inst.shieldHealth * healMul);
                    Debug.Log("Healing for " + healVal + " from Relic: " + relicItem.relicName);
                    TurnManager.Inst.TakeDmg(-healVal, EDamageSource.Relic);
                    Debug.Log("Relic Activate: " + relicItem.relicName);
                    if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                };
                return;
            case "영원한 웃음":
            case "영원한 웃음+":
                TurnManager.OnGameStart += () =>
                {
                    BuffManager.Inst.AddShowBuff("보호", EBuffAffectType.Player, 2, false);
                    Debug.Log("Relic Activate: " + relicItem.relicName);
                    if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                };
                return;
            case "붉은 꽃잎":
            case "붉은 꽃잎+":
                int threshold = (int)(TurnManager.Inst.maxHealth * 0.3f);
                if (relicItem.relicName == "붉은 꽃잎+") threshold = (int)(TurnManager.Inst.maxHealth * 0.6f);
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if (TurnManager.Inst.curHealth <= threshold)
                    {
                        TurnManager.Inst.IncreaseCost(1);
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                return;
            case "와인의 눈물":
            case "와인의 눈물+":
                threshold = (int)(TurnManager.Inst.maxHealth * 0.5f);
                if (relicItem.relicName == "와인의 눈물+") threshold = (int)(TurnManager.Inst.maxHealth * 0.7f);
                TurnManager.OnGameStart += () =>
                {
                    if (TurnManager.Inst.curHealth <= threshold)
                    {
                        BuffManager.Inst.AddShowBuff("활력", EBuffAffectType.Player, 1, false);
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                TurnManager.OnPlayerHealthChange += (x) =>
                {
                    if (TurnManager.Inst.curHealth <= threshold && TurnManager.Inst.curHealth + x > threshold)
                    {
                        BuffManager.Inst.AddShowBuff("활력", EBuffAffectType.Player, 1, false);
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                    else if (TurnManager.Inst.curHealth + x <= threshold && TurnManager.Inst.curHealth > threshold)
                    {
                        BuffManager.Inst.AddShowBuff("활력", EBuffAffectType.Player, -1, false);
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
                        switch (localEffect.rlvalue.rtype.type)
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
                        switch (localEffect.rlvalue.rtype.type)
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
                    relicAction += () => { RouletteManager.Inst.EnchantRoulettePiece(localEffect.value, localEffect.rlvalue.rtype, RouletteManager.Inst.roulettePieces[localEffect.value].roulette.value); }; break;
                case ERelicActivateEffectType.Roulette_Enchant_Val:
                    relicAction += () => { RouletteManager.Inst.EnchantRoulettePiece(localEffect.value, RouletteManager.Inst.roulettePieces[localEffect.value].roulette.rtype, localEffect.rlvalue.value); }; break;
                case ERelicActivateEffectType.Roulette_Trigger:
                    relicAction += () => { RouletteManager.Inst.TriggerRoulette(); }; break;
                case ERelicActivateEffectType.Roulette_Trigger_Cancel:
                    relicAction += () => { RouletteManager.Inst.DeTriggerRoulette(); }; break;
                case ERelicActivateEffectType.Enemy_Action_Hide:
                    relicAction += () => { EnemyManager.Inst.HideAction(localEffect.value); }; break;
                case ERelicActivateEffectType.Enemy_Action_Delete:
                    relicAction += () => { EnemyManager.Inst.RemoveAction(localEffect.value); }; break;
                case ERelicActivateEffectType.Enemy_Spin_Reverse:
                    relicAction += () => { EnemyManager.Inst.ReverseSpin(); }; break;
                case ERelicActivateEffectType.Enemy_Spin_Ignore:
                    relicAction += () => { EnemyManager.Inst.RemoveActionType(EEnemyActionType.Turn); }; break;
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
        relicAction += () =>
        {
            Debug.Log("Relic Activate: " + relicItem.relicName);
            if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
        };
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
                    case ERelicActivateConditionType.Enemy_Health_GE:
                        totalCondition = () =>
                        {
                            if (((float)TurnManager.Inst.enemyCurHealth[0] / TurnManager.Inst.enemyMaxHealth[0]) >= localCondition.fvalue)
                            {
                                temp?.Invoke();
                            }
                        }; break;
                    case ERelicActivateConditionType.Enemy_Health_LE:
                        totalCondition = () =>
                        {
                            if (((float)TurnManager.Inst.enemyCurHealth[0] / TurnManager.Inst.enemyMaxHealth[0]) <= localCondition.fvalue)
                            {
                                temp?.Invoke();
                            }
                        }; break;
                    case ERelicActivateConditionType.Enemy_Shield_GE:
                        totalCondition = () =>
                        {
                            if (TurnManager.Inst.enemyShieldHealth[0] >= localCondition.value)
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
                            if (CardManager.Inst.myCardNum() >= localCondition.value)
                            {
                                temp?.Invoke();
                            }
                        }; break;
                    case ERelicActivateConditionType.Player_Card_Num_EQ:
                        totalCondition = () =>
                        {
                            if (CardManager.Inst.myCardNum() == localCondition.value)
                            {
                                temp?.Invoke();
                            }
                        }; break;
                    case ERelicActivateConditionType.Player_Card_Num_LE:
                        totalCondition = () =>
                        {
                            if (CardManager.Inst.myCardNum() <= localCondition.value)
                            {
                                temp?.Invoke();
                            }
                        }; break;
                    case ERelicActivateConditionType.Activate_Trigger:
                        totalCondition = () =>
                        {
                            if (RouletteManager.Inst.isTriggerActivated)
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
                    TurnManager.OnRouletteSpin += (x, y) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Roulette_Trigger:
                    TurnManager.OnRouletteTrigger += relicActivation; break;
                case ERelicActivateTimingType.Roulette_Enchant:
                    TurnManager.OnRouletteEnchant += (x) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Roulette_Activate:
                    TurnManager.OnRouletteActivate += relicActivation; break;
                case ERelicActivateTimingType.Card_Use:
                    TurnManager.OnUseCard += (x) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Card_Draw:
                    TurnManager.OnAddCard += relicActivation; break;
                case ERelicActivateTimingType.Enemy_Damage:
                    TurnManager.OnEnemyDamaged += (x, s, i) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Enemy_Heal:
                    TurnManager.OnEnemyHealed += (x, s, i) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Enemy_Trigger:
                    TurnManager.OnEnemyTrigger += relicActivation; break;
                case ERelicActivateTimingType.Enemy_Trigger_Increase:
                    TurnManager.OnEnemyTriggerIncrease += (x) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Enemy_Trigger_Decrease:
                    TurnManager.OnEnemyTriggerDecrease += (x) => relicActivation?.Invoke(); break;
                case ERelicActivateTimingType.Enemy_Shield:
                    TurnManager.OnEnemyShielded += (x, s, i) => relicActivation(); break;
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

    private void LateUpdate()
    {
        RelicActivateEffect();
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

