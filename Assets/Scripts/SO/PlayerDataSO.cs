using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerDataSO", menuName = "Scriptable Objects/PlayerDataSO")]
public class PlayerDataSO : ScriptableObject
{
    [Header("전투 외적 데이터")]
    public int maxHealth;
    public int curHealth;
    public int dreamDust;
    public int leftPassengers;
    public bool isTutorial;
    public string lastSceneName;
    [Header("카드 데이터")]
    public string personaPiece;
    public bool isPersonaEnhanced;
    public List<Item_Num> personaCards;
    public string shadowPiece;
    public bool isShadowEnhanced;
    public List<Item_Num> shadowCards;
    public List<Item_Num> normalCards;
    [Header("오브제 데이터")]
    public List<int> relics;
    public List<bool> relicEnhancements;
    [Header("아이템 데이터")]
    public List<string> useableItems;
    public List<string> earnedKeys = new List<string>();
    [Header("스탯 데이터")]
    public int courage;
    public int wisdom;
    public int luck;
    [Header("맵 데이터")]
    public int currentActNum;
    public List<MapNode_Data> mapNodes;
    public int totalLevel;
    public List<string> visitedNodeIDList;
    public int curNodeIndex;
    public string curNodeLocationID;
    public EncounterType lastEncounterType;
    public List<string> encounterQueue = new List<string>(); 
    public string currentStepID;
    public string currentEncounterID;
    [Header("전투 데이터")]
    public string enemyName;
}
