using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
public struct EnemyPhase
{
    public string name;
    public List<EnemyPatterns> patterns;
    public bool phaseClear;

    public EnemyPhase(string s)
    {
        name = s;
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
    public string passive;
    public int triggerNum;
    public List<EnemyPhase> phase;
    public RouletteItem[] roulettePattern;
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
