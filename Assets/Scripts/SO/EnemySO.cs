using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EEnemyActionType { Turn, Attack, Heal, Shield, Enchant_Random_1, Enchant_Random_2, Special_Activate };

[System.Serializable]
public struct EnemyPattern
{
    public EEnemyActionType type;
    public int typeNum;
    public int val;
    public bool isTrigger;

    public EnemyPattern(EEnemyActionType t, int v)
    {
        type = t;
        typeNum = 0;
        val = v;
        isTrigger = false;
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
public class EnemySpecialAction
{
    public Sprite sprite;
    public string title;
    public string text;

    public EnemySpecialAction(Sprite sprite, string title, string text)
    {
        this.sprite = sprite;
        this.title = title;
        this.text = text;
    }

    public EnemySpecialAction(EnemySpecialAction esa)
    {
        this.sprite = esa.sprite;
        this.title = esa.title;
        this.text = esa.text;
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
    public List<EnemyPhase> triggerPhase;
    public RouletteItem[] roulettePattern;
    
    public static int enemySpecialRouletteNum = 2;
    public SpecialRoulette[] enemySpecialRoulettes = new SpecialRoulette[enemySpecialRouletteNum];
    public static int enemySpecialActionNum = 4;
    public EnemySpecialAction[] enemySpecialActions = new EnemySpecialAction[enemySpecialActionNum];
}

[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject
{
    public List<Enemy> enemies;
}
