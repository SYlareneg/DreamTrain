using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EEnemyActionType { Turn, Attack, Heal, Shield, Enchant_Random, Drain };

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
    public Sprite specialRoulette1Sprite;
    public string specialRoulette1Title;
    public string specialRoulette1Text;
    public Sprite specialRoulette2Sprite;
    public string specialRoulette2Title;
    public string specialRoulette2Text;
}

[CreateAssetMenu(fileName = "CharacterSO", menuName = "Scriptable Objects/CharacterSO")]
public class CharacterSO : ScriptableObject
{
    [Header("Develop")]
    [Tooltip("플레이어 최대 체력")] public int maxHealth;
    [Tooltip("플레이어 남은 체력")] public int curHealth;

    public DreamPiece personaPiece;
    public DreamPiece shadowPiece;

    public Enemy enemy;
}
