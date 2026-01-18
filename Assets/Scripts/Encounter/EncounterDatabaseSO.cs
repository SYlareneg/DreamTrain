using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class EncounterMetaInfo
{
    public string id;
    public string nameKO;
    public EncounterType type;
    public string imageName;
    public string filePath;
    public int order;
    public bool isEssential;
    public TextAsset sourceCsvFile;
    public EncounterContext encounterContext;
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
    // OnValidate는 ScriptableObject인 이곳에 있어야 합니다.
    private void OnValidate()
    {
        // 리스트 전체를 돌면서 CSV 파일이 있으면 텍스트를 갱신합니다.
        if (masterTable != null)
        {
            bool isChanged = false;
            foreach (var info in masterTable)
            {
                if (info.sourceCsvFile != null)
                {
                    // 현재 저장된 텍스트와 파일의 텍스트가 다를 때만 갱신 (성능 최적화)
                    if (info.encounterContext.csvRawData != info.sourceCsvFile.text)
                    {
                        info.encounterContext.csvRawData = info.sourceCsvFile.text;
                        isChanged = true;
                    }
                }
            }

#if UNITY_EDITOR
            // 변경 사항이 있다면 Unity에게 "이 파일은 수정되었으니 저장해야 해"라고 알립니다.
            if (isChanged)
            {
                EditorUtility.SetDirty(this);
            }
#endif
        }
    }
}
[System.Serializable]
public struct EncounterContext
{
    [TextArea(3, 10)] 
    public string csvRawData;
    
    public string PathID; 
    //public List<ParsedStep> steps; 
}