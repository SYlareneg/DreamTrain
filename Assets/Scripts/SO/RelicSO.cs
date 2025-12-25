using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ERelicActivateTimingType
{
    Player_Turn_Start,
    Player_Turn_End,
    Enemy_Turn_Start,
    Enemy_Turn_End,
    Game_Start,
    Game_End,
    Roulette_Spin,
    Roulette_Trigger,
    Roulette_Enchant,
    Roulette_Activate,
    Card_Use,
    Card_Draw,
    Enemy_Damage,
    Enemy_Heal,
    Enemy_Trigger,
    Enemy_Trigger_Increase,
    Enemy_Trigger_Decrease,
    Enemy_Shield,
    Enemy_Action,
    Player_Damage,
    Player_Heal,
    Player_Trigger,
    Player_Trigger_Increase,
    Player_Trigger_Decrease,
    Player_Shield,
    Cost_Change
}
public enum ERelicActivateConditionType
{
    None,
    Turn_Count,
    Turn_GE,
    Turn_EQ,
    Roulette_Count,
    Roulette_Count_Turn,
    Roulette_Count_GE,
    Roulette_Count_GE_Turn,
    Roulette_Count_EQ,
    Roulette_Count_EQ_Turn,
    Roulette_Direction,
    Roulette_Distance,
    Roulette_Distance_Turn,
    Roulette_Distance_GE,
    Roulette_Distance_GE_Turn,
    Roulette_Distance_EQ,
    Roulette_Distance_EQ_Turn,
    Roulette_IsSpinned,
    Roulette_IsSpinned_Turn,
    Card_Count,
    Card_Count_Turn,
    Card_Count_GE,
    Card_Count_GE_Turn,
    Card_Count_EQ,
    Card_Count_EQ_Turn,
    Card_IsUsed,
    Card_IsUsed_Turn,
    Enemy_Health_GE,
    Enemy_Health_LE,
    Enemy_Shield_GE,
    Enemy_Action_Type,
    Player_Health_GE,
    Player_Health_LE,
    Player_Shield_GE,
    Player_Card_Num_GE,
    Player_Card_Num_EQ,
    Player_Card_Num_LE,
    Activate_Trigger
}
[System.Serializable]
public struct RelicActivateCondition
{
    public ERelicActivateConditionType type;
    public int value;
    public float fvalue;
    public Item ivalue;
    public EEnemyActionType actionType;
}
[System.Serializable]
public struct RelicActivateConditionArray
{
    public RelicActivateCondition[] conditions;
}
public enum ERelicActivateEffectType
{
    Player_Shield,
    Player_Heal,
    Player_Damage,
    Player_Cost_Increase,
    Player_Cost_Decrease,
    Player_Max_Cost_Increase,
    Player_Max_Hand_Change,
    Player_Trigger_Increase,
    Player_Trigger_Decrease,
    Card_Draw,
    Card_Cost_Change,
    Card_Value_Change,
    Card_Duplicate_Hand,
    Card_Duplicate_Deck,
    Card_Add_Hand,
    Card_Add_Draw,
    Card_Add_Discard,
    Card_Block,
    Roulette_Value_Change_ADD,
    Roulette_Value_Change_MUL,
    Roulette_Spin_CW,
    Roulette_Spin_CCW,
    Roulette_Enchant_Type,
    Roulette_Enchant_Val,
    Roulette_Trigger,
    Roulette_Trigger_Cancel,
    Enemy_Action_Hide,
    Enemy_Action_Delete,
    Enemy_Spin_Reverse,
    Enemy_Spin_Ignore,
    Enemy_Damage,
    Enemy_Shield,
    Enemy_Heal,
    Enemy_Trigger_Increase,
    Enemy_Trigger_Decrease,
    Develop_Test
}
[System.Serializable]
public class RelicActivateEffect
{
    public ERelicActivateEffectType type;
    public int value;
    public int value2;
    public Item ivalue;
    public RouletteItem rlvalue;
}

[System.Serializable]
public class RelicItem
{
    public int relicOwner;
    public Sprite relicSprite;
    public string relicName;
    public string relicTxt;
    public ERelicActivateTimingType[] relicTimings;
    public RelicActivateConditionArray[] relicConditions;
    public RelicActivateEffect[] relicEffects;
    public bool isEnhanced;

    public RelicItem(RelicItem relicItem)
    {
        relicOwner = relicItem.relicOwner;
        relicSprite = relicItem.relicSprite;
        relicName = relicItem.relicName;
        relicTxt = relicItem.relicTxt;
        relicTimings = relicItem.relicTimings;
        relicConditions = relicItem.relicConditions;
        relicEffects = relicItem.relicEffects;
        isEnhanced = relicItem.isEnhanced;
    }
}

[System.Serializable]
public class RelicItem_Enhanceable : RelicItem
{
    public RelicItem enhancedRelicItem;

    public RelicItem_Enhanceable(RelicItem_Enhanceable relicItem_Enhanceable) : base((RelicItem)relicItem_Enhanceable)
    {
        enhancedRelicItem = new RelicItem(relicItem_Enhanceable.enhancedRelicItem);
    }
}

[CreateAssetMenu(fileName = "RelicSO", menuName = "Scriptable Objects/RelicSO")]
public class RelicSO : ScriptableObject
{
    public List<RelicItem_Enhanceable> relicItems;
}
