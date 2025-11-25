using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageEnemy
{
    public string enemyName;
    public Sprite enemySprite;
    public Vector3 enemyPos;
    public bool isClear;
    public int dreamDustReward;
}

[System.Serializable]
public class Stage
{
    public List<StageEnemy> stageEnemies = new List<StageEnemy>();
    public StageEnemy bossEnemy;
    public bool stageClear;
}

public enum EPlayerSpawn
{
    Station, Rest, Room
};

[System.Serializable]
public class SellCard
{
    public Item cardItem;
    public int cost;
    public bool isValid;
}

[System.Serializable]
public class SellUItem
{
    public UseItem useItem;
    public int cost;
    public bool isValid;
}

[CreateAssetMenu(fileName = "StageSO", menuName = "Scriptable Objects/StageSO")]
public class StageSO : ScriptableObject
{
    public int currentStage;
    public Stage[] stageList;
    public bool sofaUsed;
    public List<SellCard> merchantSellCards = new List<SellCard>();
    public List<SellUItem> merchantSellUItems = new List<SellUItem>();
}
