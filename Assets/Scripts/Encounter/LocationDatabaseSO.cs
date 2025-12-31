using UnityEngine;
using System.Collections.Generic;
using System.Linq;
[System.Serializable]
public class LocationMetaInfo
{
    public string id;
    public string nameKO;
    public List<string> encounterPool;
    public int howManyEnc;
}
[CreateAssetMenu(fileName = "LocationDatabase", menuName = "Data/Location Database")]
public class LocationDatabaseSO : ScriptableObject
{
    public List<LocationMetaInfo> locationTable = new List<LocationMetaInfo>();

    // [중요] EncounterManager에서 사용하는 함수입니다. (누락되었던 부분)
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

    // [MapScene 등에서 사용하는 함수]
    public List<EncounterType> GetUniqueEncounterTypes(string locationID, EncounterDatabaseSO encounterDB)
    {
        List<EncounterType> typeList = new List<EncounterType>();

        // 1. Location 정보 찾기
        LocationMetaInfo locInfo = FindById(locationID);
        if (locInfo == null) return typeList;

        // 2. EncounterDB 유효성 체크
        if (encounterDB == null)
        {
            Debug.LogError("EncounterDatabaseSO가 null입니다.");
            return typeList;
        }

        // 3. Pool 순회
        foreach (string encID in locInfo.encounterPool)
        {
            EncounterMetaInfo encInfo = encounterDB.FindById(encID);
            if (encInfo != null)
            {
                typeList.Add(encInfo.type);
            }
        }

        // 4. 중복 제거 후 반환
        return typeList.Distinct().ToList();
    }
}