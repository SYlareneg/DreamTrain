using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StageEnemy
{
    public string enemyName;
    public Sprite enemySprite;
    public Vector3 enemyPos;
    public bool isClear;
}

[System.Serializable]
public class Stage
{
    public List<StageEnemy> stageEnemies = new List<StageEnemy>();
    public string bossName;
    public bool stageClear;
}

public enum EPlayerSpawn
{
    Station, Rest, Room
};

[CreateAssetMenu(fileName = "StageSO", menuName = "Scriptable Objects/StageSO")]
public class StageSO : ScriptableObject
{
    public int currentStage;
    public Stage[] stageList;
    public EPlayerSpawn playerSpawn;
    public bool restUsed;
}
