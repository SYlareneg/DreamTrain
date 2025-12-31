using UnityEngine;
using System.Collections.Generic;
[System.Serializable]
public class EncounterMetaInfo
{
    public string id;
    public string nameKO;
    public EncounterType type;
    public string imagePath;
    public string filePath;
    public int order;
    public bool isEssential;
}
[CreateAssetMenu(fileName = "EncounterDatabase", menuName = "Data/Encounter Database")]
public class EncounterDatabaseSO : ScriptableObject
{
    public List<EncounterMetaInfo> masterTable = new List<EncounterMetaInfo>();

    public EncounterMetaInfo FindById(string id) => masterTable.Find(x => x.id == id);

    public Dictionary<string, EncounterMetaInfo> GetDictionary()
    {
        Dictionary<string, EncounterMetaInfo> dict = new Dictionary<string, EncounterMetaInfo>();
        foreach (var info in masterTable)
        {
            if (!dict.ContainsKey(info.id)) dict.Add(info.id, info);
        }
        return dict;
    }
}