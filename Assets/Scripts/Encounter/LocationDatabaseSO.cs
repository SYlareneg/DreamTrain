using UnityEngine;
using System.Collections.Generic;
using System.Linq;
[System.Serializable]
public class LocationMetaInfo
{
    public string id;
    public string nameKO;
    public List<string> encounterPool;
    public List<string> selectedEncounterPool;
    public int howManyEnc;
}
[CreateAssetMenu(fileName = "LocationDatabase", menuName = "Data/Location Database")]
public class LocationDatabaseSO : ScriptableObject
{
    public List<LocationMetaInfo> locationTable = new List<LocationMetaInfo>();
    public Dictionary<string, LocationMetaInfo> GetDictionary()
    {
        Dictionary<string, LocationMetaInfo> dict = new Dictionary<string, LocationMetaInfo>();
        foreach (var info in locationTable)
        {
            if (!dict.ContainsKey(info.id)) dict.Add(info.id, info);
        }
        return dict;
    }

    public LocationMetaInfo FindById(string id) => locationTable.Find(x => x.id == id);

    public List<EncounterType> GetUniqueEncounterTypes(string locationID, EncounterDatabaseSO encounterDB)
    {
        List<EncounterType> typeList = new List<EncounterType>();

        LocationMetaInfo locInfo = FindById(locationID);
        if (locInfo == null) return typeList;

        if (encounterDB == null)
        {
            Debug.LogError("EncounterDatabaseSO가 null입니다.");
            return typeList;
        }

        foreach (string encID in locInfo.encounterPool)
        {
            EncounterMetaInfo encInfo = encounterDB.FindById(encID);
            if (encInfo != null)
            {
                typeList.Add(encInfo.type);
            }
        }
        return typeList.Distinct().ToList();
    }
}