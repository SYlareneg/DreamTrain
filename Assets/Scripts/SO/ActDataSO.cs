using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Location_Data
{
    public string id;
    public string nameKO;
    public string descriptionKO;
    public string sprite;
    public string hideSprite;
    public List<string> encounterPool;
    public int encounterNum;
    public int difficulty;
    public bool isNormalLocation;
}

[System.Serializable]
public class Act_Data
{
    public int actNum;
    public List<string> essentialLocations;
    public List<int> essentialIntervalLayerCount;
    public List<string> specialLocations;
}

[CreateAssetMenu(fileName = "ActDataSO", menuName = "Scriptable Objects/ActDataSO")]
public class ActDataSO : ScriptableObject
{
    public List<Act_Data> actDataList;
    public List<Location_Data> locationDataList;
    public List<EncounterMetaInfo> encounterDataList;
}
