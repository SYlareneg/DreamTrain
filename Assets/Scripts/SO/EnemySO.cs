using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EEnemyActionType { Turn, Attack, Heal, Shield, Enchant_Random, Spawn_SubEnemy, Special_Activate };

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

    public EnemyPatterns(EnemyPatterns ep)
    {
        pattern = new List<EnemyPattern>();
        for(int i = 0; i < ep.pattern.Count; i++)
        {
            pattern.Add(new EnemyPattern(ep.pattern[i]));
        }
    }
}

[System.Serializable]
public class EnemyPhase_Base
{
    public string name;
    public string text;
    public List<EnemyPatterns> patterns;
    public bool phaseClear;
    public bool phaseRepeat;

    public EnemyPhase_Base(string s, string t)
    {
        name = s;
        text = t;
        patterns = new List<EnemyPatterns>();
        phaseClear = false;
        phaseRepeat = false;
    }

    public EnemyPhase_Base(EnemyPhase_Base ep)
    {
        name = ep.name;
        text = ep.text;
        patterns = new List<EnemyPatterns>();
        for(int i = 0; i < ep.patterns.Count; i++)
        {
            patterns.Add(new EnemyPatterns(ep.patterns[i]));
        }
        phaseClear = ep.phaseClear;
        phaseRepeat = ep.phaseRepeat;
    }
}

[System.Serializable]
public class EnemyPhase : EnemyPhase_Base
{
    public Sprite sprite;
    public EnemyPhase(EnemyPhase ep) : base(ep)
    {
        sprite = ep.sprite;
    }

    public EnemyPhase(EnemyPhase_Data epData) : base(epData)
    {
        sprite = Utils.LoadSpriteByName("Enemy", epData.sprite);
    }
}

[System.Serializable]
public class EnemyPhase_Data : EnemyPhase_Base
{
    public string sprite;
    public EnemyPhase_Data(EnemyPhase_Data ep) : base(ep)
    {
        sprite = ep.sprite;
    }

    public EnemyPhase_Data(EnemyPhase ep) : base(ep)
    {
        sprite = ep.sprite != null ? ep.sprite.name : "";
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

    public EnemySpecialAction(EnemySpecialAction_Data esaData)
    {
        if(esaData == null) return;
        this.sprite = Utils.LoadSpriteByName("SpecialAction", esaData.sprite);
        this.title = esaData.title;
        this.text = esaData.text;
    }
}

[System.Serializable]
public class EnemySpecialAction_Data
{
    public string sprite;
    public string title;
    public string text;

    public EnemySpecialAction_Data(EnemySpecialAction_Data esa)
    {
        if(esa == null) return;
        sprite = esa.sprite;
        title = esa.title;
        text = esa.text;
    }

    public EnemySpecialAction_Data(EnemySpecialAction esa)
    {
        if(esa == null) return;
        sprite = esa.sprite != null ? esa.sprite.name : "";
        title = esa.title;
        text = esa.text;
    }
}

[System.Serializable]
public class Enemy
{
    public string name;
    public string id;
    public int health;
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
        id = enemy.id;
        health = enemy.health;
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
        subEnemies = new List<string>(enemy.subEnemies);
        subEnemies_Spawn = new List<string>(enemy.subEnemies_Spawn);
    }

    public Enemy(Enemy_Data enemyData)
    {
        name = enemyData.name;
        id = enemyData.id;
        health = enemyData.health;
        triggerNum = enemyData.triggerNum;
        phase = new List<EnemyPhase>();
        for(int i = 0; i < enemyData.phase.Count; i++)
        {
            phase.Add(new EnemyPhase(enemyData.phase[i]));
        }
        triggerPhase = new List<EnemyPhase>();
        for(int i = 0; i < enemyData.triggerPhase.Count; i++)
        {
            triggerPhase.Add(new EnemyPhase(enemyData.triggerPhase[i]));
        }
        roulettePattern = new RouletteItem[RouletteManager.rouletteNum];
        for(int i = 0; i < enemyData.roulettePattern.Length; i++)
        {
            roulettePattern[i] = new RouletteItem(enemyData.roulettePattern[i]);
        }
        for(int i = 0; i < enemyData.enemySpecialRoulettes.Length; i++)
        {
            enemySpecialRoulettes[i] = new SpecialRoulette(enemyData.enemySpecialRoulettes[i]);
        }
        for(int i = 0; i < enemyData.enemySpecialActions.Length; i++)
        {
            enemySpecialActions[i] = new EnemySpecialAction(enemyData.enemySpecialActions[i]);
        }
        subEnemies = new List<string>(enemyData.subEnemies);
        subEnemies_Spawn = new List<string>(enemyData.subEnemies_Spawn);
    }
}

[System.Serializable]
public class Enemy_Data
{
    public string name;
    public string id;
    public int health;
    public int triggerNum;
    public List<EnemyPhase_Data> phase;
    public List<EnemyPhase_Data> triggerPhase;
    public RouletteItem[] roulettePattern;
    
    public static int enemySpecialRouletteNum = 2;
    public SpecialRoulette_Data[] enemySpecialRoulettes = new SpecialRoulette_Data[enemySpecialRouletteNum];
    public static int enemySpecialActionNum = 4;
    public EnemySpecialAction_Data[] enemySpecialActions = new EnemySpecialAction_Data[enemySpecialActionNum];

    public static int maxSubEnemyNum = 4;
    public List<string> subEnemies;
    public List<string> subEnemies_Spawn;

    public Enemy_Data(Enemy enemy)
    {
        name = enemy.name;
        id = enemy.id;
        health = enemy.health;
        triggerNum = enemy.triggerNum;
        phase = new List<EnemyPhase_Data>();
        for(int i = 0; i < enemy.phase.Count; i++)
        {
            phase.Add(new EnemyPhase_Data(enemy.phase[i]));
        }
        triggerPhase = new List<EnemyPhase_Data>();
        for(int i = 0; i < enemy.triggerPhase.Count; i++)
        {
            triggerPhase.Add(new EnemyPhase_Data(enemy.triggerPhase[i]));
        }
        roulettePattern = new RouletteItem[RouletteManager.rouletteNum];
        for(int i = 0; i < enemy.roulettePattern.Length; i++)
        {
            roulettePattern[i] = new RouletteItem(enemy.roulettePattern[i]);
        }
        for(int i = 0; i < enemy.enemySpecialRoulettes.Length; i++)
        {
            enemySpecialRoulettes[i] = new SpecialRoulette_Data(enemy.enemySpecialRoulettes[i]);
        }
        for(int i = 0; i < enemy.enemySpecialActions.Length; i++)
        {
            enemySpecialActions[i] = new EnemySpecialAction_Data(enemy.enemySpecialActions[i]);
        }
        subEnemies = new List<string>(enemy.subEnemies);
        subEnemies_Spawn = new List<string>(enemy.subEnemies_Spawn);
    }
}

[System.Serializable]
public class SubEnemy
{
    public string name;
    public int health;
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

    public SubEnemy(SubEnemy_Data subEnemyData)
    {
        name = subEnemyData.name;
        health = subEnemyData.health;
        roulettePos = subEnemyData.roulettePos;
        phase = new List<EnemyPhase>();
        for(int i = 0; i < subEnemyData.phase.Count; i++)
        {
            phase.Add(new EnemyPhase(subEnemyData.phase[i]));
        }
        enemySpecialRoulettes = new SpecialRoulette[enemySpecialRouletteNum];
        for(int i = 0; i < subEnemyData.enemySpecialRoulettes.Length; i++)
        {
            enemySpecialRoulettes[i] = new SpecialRoulette(subEnemyData.enemySpecialRoulettes[i]);
        }
        enemySpecialActions = new EnemySpecialAction[enemySpecialActionNum];
        for(int i = 0; i < subEnemyData.enemySpecialActions.Length; i++)
        {
            enemySpecialActions[i] = new EnemySpecialAction(subEnemyData.enemySpecialActions[i]);
        }
    }
}

[System.Serializable]
public class SubEnemy_Data
{
    public string name;
    public int health;
    public int roulettePos;
    public List<EnemyPhase_Data> phase;
    
    public static int enemySpecialRouletteNum = 2;
    public SpecialRoulette_Data[] enemySpecialRoulettes = new SpecialRoulette_Data[enemySpecialRouletteNum];
    public static int enemySpecialActionNum = 4;
    public EnemySpecialAction_Data[] enemySpecialActions = new EnemySpecialAction_Data[enemySpecialActionNum];
    

    public SubEnemy_Data(SubEnemy subEnemy)
    {
        name = subEnemy.name;
        health = subEnemy.health;
        roulettePos = subEnemy.roulettePos;
        phase = new List<EnemyPhase_Data>();
        for(int i = 0; i < subEnemy.phase.Count; i++)
        {
            phase.Add(new EnemyPhase_Data(subEnemy.phase[i]));
        }
        enemySpecialRoulettes = new SpecialRoulette_Data[enemySpecialRouletteNum];
        for(int i = 0; i < subEnemy.enemySpecialRoulettes.Length; i++)
        {
            enemySpecialRoulettes[i] = new SpecialRoulette_Data(subEnemy.enemySpecialRoulettes[i]);
        }
        enemySpecialActions = new EnemySpecialAction_Data[enemySpecialActionNum];
        for(int i = 0; i < subEnemy.enemySpecialActions.Length; i++)
        {
            enemySpecialActions[i] = new EnemySpecialAction_Data(subEnemy.enemySpecialActions[i]);
        }
    }
}

[CreateAssetMenu(fileName = "EnemySO", menuName = "Scriptable Objects/EnemySO")]
public class EnemySO : ScriptableObject
{
    public List<Enemy> enemies;
    public List<SubEnemy> subEnemies;
}
