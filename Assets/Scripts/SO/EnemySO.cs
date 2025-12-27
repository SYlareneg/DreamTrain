using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EEnemyActionType { Turn, Attack, Heal, Shield, Enchant_Random, Special_Activate, Spawn_SubEnemy };

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

    public EnemyPattern(EnemyPattern enemyPattern)
    {
        type = enemyPattern.type;
        typeNum = enemyPattern.typeNum;
        val = enemyPattern.val;
        isTrigger = enemyPattern.isTrigger;
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
        phaseRepeat = false;
    }

    public EnemyPhase(EnemyPhase ep)
    {
        name = ep.name;
        text = ep.text;
        patterns = new List<EnemyPatterns>();
        for(int i = 0; i < ep.patterns.Count; i++)
        {
            patterns.Add(ep.patterns[i]);
        }
        phaseClear = ep.phaseClear;
        phaseRepeat = ep.phaseRepeat;
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
        if(esa == null) return;
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

    public static int maxSubEnemyNum = 4;
    public List<string> subEnemies;
    public List<string> subEnemies_Spawn;

    public Enemy(Enemy enemy)
    {
        name = enemy.name;
        health = enemy.health;
        actionNum = enemy.actionNum;
        passive = enemy.passive;
        relics = new List<RelicItem>();
        for(int i = 0; i < enemy.relics.Count; i++)
        {
            relics.Add(new RelicItem(enemy.relics[i]));
        }
        triggerNum = enemy.triggerNum;
        phase = new List<EnemyPhase>();
        for(int i = 0; i < enemy.phase.Count; i++)
        {
            phase.Add(new EnemyPhase(enemy.phase[i]));
        }
        triggerPhase = new List<EnemyPhase>();
        for(int i = 0; i < enemy.triggerPhase.Count; i++)
        {
            triggerPhase.Add(new EnemyPhase(enemy.triggerPhase[i]));
        }
        roulettePattern = new RouletteItem[RouletteManager.rouletteNum];
        for(int i = 0; i < enemy.roulettePattern.Length; i++)
        {
            roulettePattern[i] = new RouletteItem(enemy.roulettePattern[i]);
        }
        for(int i = 0; i < enemy.enemySpecialRoulettes.Length; i++)
        {
            enemySpecialRoulettes[i] = new SpecialRoulette(enemy.enemySpecialRoulettes[i]);
        }
        for(int i = 0; i < enemy.enemySpecialActions.Length; i++)
        {
            enemySpecialActions[i] = new EnemySpecialAction(enemy.enemySpecialActions[i]);
        }
        subEnemies = new List<string>();
        for(int i = 0; i < enemy.subEnemies.Count; i++)
        {
            subEnemies.Add(enemy.subEnemies[i]);
        }
        subEnemies_Spawn = new List<string>();
        for(int i = 0; i < enemy.subEnemies_Spawn.Count; i++)
        {
            subEnemies_Spawn.Add(enemy.subEnemies_Spawn[i]);
        }
    }
}

[System.Serializable]
public class SubEnemy
{
    public string name;
    public int health;
    public int actionNum;
    public int roulettePos;
    public List<EnemyPhase> phase;
    
    public static int enemySpecialRouletteNum = 2;
    public SpecialRoulette[] enemySpecialRoulettes = new SpecialRoulette[enemySpecialRouletteNum];
    public static int enemySpecialActionNum = 4;
    public EnemySpecialAction[] enemySpecialActions = new EnemySpecialAction[enemySpecialActionNum];
    

    public SubEnemy(SubEnemy subEnemy)
    {
        name = subEnemy.name;
        health = subEnemy.health;
        actionNum = subEnemy.actionNum;
        roulettePos = subEnemy.roulettePos;
        phase = new List<EnemyPhase>();
        for(int i = 0; i < subEnemy.phase.Count; i++)
        {
            phase.Add(new EnemyPhase(subEnemy.phase[i]));
        }
        enemySpecialRoulettes = new SpecialRoulette[enemySpecialRouletteNum];
        for(int i = 0; i < subEnemy.enemySpecialRoulettes.Length; i++)
        {
            enemySpecialRoulettes[i] = new SpecialRoulette(subEnemy.enemySpecialRoulettes[i]);
        }
        for(int i = 0; i < subEnemy.enemySpecialActions.Length; i++)
        {
            enemySpecialActions[i] = new EnemySpecialAction(subEnemy.enemySpecialActions[i]);
        }
    }
}

[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject
{
    public List<Enemy> enemies;
    public List<SubEnemy> subEnemies;
}
