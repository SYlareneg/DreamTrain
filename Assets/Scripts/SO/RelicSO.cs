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
    Roulette_Spin_Count,
    Roulette_Spin_Direction,
    Roulette_Spin_Distance,
    Roulette_Trigger,
    Roulette_Enchant,
    Roulette_Activate,
    Card_Use_Cost,
    Card_Use_Count,
    Card_Use_Type,
    Card_Draw,
    Enemy_Damage,
    Enemy_Heal,
    Enemy_Trigger,
    Enemy_Shield,
    Enemy_Action,
    Player_Damage,
    Player_Heal,
    Player_Trigger,
    Player_Shield
}
public enum ERelicActivateConditionType
{
    None,
    Turn_Count,
    Turn_Begin,
    Roulette_Count,
    Roulette_Direction,
    Roulette_Distance,
    Roulette_IsSpinned,
    Card_Cost,
    Card_Count,
    Card_IsUsed,
    Card_Type,
    Enemy_Health_GE,
    Enemy_Health_LE,
    Enemy_Action_Type,
    Enemy_Shield_GE,
    Player_Health_GE,
    Player_Health_LE,
    Player_Shield_GE,
    Activate_Trigger
}
[System.Serializable]
public struct RelicActivateCondition
{
    public ERelicActivateConditionType type;
    public int value;
    public float fvalue;
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
    Player_Cost_Increase,
    Player_Max_Cost_Increase,
    Card_Draw,
    Card_Cost_Change,
    Card_Value_Change,
    Card_Duplicate_Hand,
    Card_Duplicate_Deck,
    Card_Add_Hand,
    Card_Add_Deck,
    Roulette_Value_Change,
    Roulette_Spin_CW,
    Roulette_Spin_CCW,
    Roulette_Enchant,
    Roulette_Trigger,
    Enemy_Action_Hide,
    Enemy_Action_Delete,
    Enemy_Spin_Reverse,
    Enemy_Spin_Ignore,
    Enemy_Damage,
    Develop_Test
}
[System.Serializable]
public struct RelicActivateEffect
{
    public ERelicActivateEffectType type;
    public int value;
}

[System.Serializable]
public class RelicItem
{
    public Sprite relicSprite;
    public string relicName;
    public string relicTxt;
    public ERelicActivateTimingType[] relicTimings;
    public RelicActivateConditionArray[] relicConditions;
    public RelicActivateEffect[] relicEffects;
}

[CreateAssetMenu(fileName = "RelicSO", menuName = "Scriptable Objects/RelicSO")]
public class RelicSO : ScriptableObject
{
    public RelicItem[] relicItems;
}
