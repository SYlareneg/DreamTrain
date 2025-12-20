using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EEnemyActionType { Turn, Attack, Heal, Shield, Enchant_Random_1, Enchant_Random_2, Special_Activate_1, Special_Activate_2 };

[System.Serializable]
public struct EnemyPattern
{
    public EEnemyActionType type;
    public int val;

    public EnemyPattern(EEnemyActionType t, int v)
    {
        type = t;
        val = v;
    }
}

[System.Serializable]
public struct EnemyPatterns
{
    public List<EnemyPattern> pattern;
}

[System.Serializable]
public class EnemyPhase
{
    public string name;
    public string text;
    public List<EnemyPatterns> patterns;
    public bool phaseClear;
    public bool phaseRepeat;

    public EnemyPhase(string s, string t)
    {
        name = s;
        text = t;
        patterns = new List<EnemyPatterns>();
        phaseClear = false;
    }
}

[System.Serializable]
public class Enemy
{
    public string name;
    public int health;
    public int actionNum;
    public Passive passive;
    public List<RelicItem> relics;
    public int triggerNum;
    public List<EnemyPhase> phase;
    public RouletteItem[] roulettePattern;
    public Sprite triggerSprite;
    public Sprite specialRoulette1Sprite;
    public string specialRoulette1Title;
    public string specialRoulette1Text;
    public Sprite specialRoulette2Sprite;
    public string specialRoulette2Title;
    public string specialRoulette2Text;
    public Sprite specialAction1Sprite;
    public string specialAction1Title;
    public string specialAction1Text;
    public Sprite specialAction2Sprite;
    public string specialAction2Title;
    public string specialAction2Text;
}

[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject
{
    public List<Enemy> enemies;
}
