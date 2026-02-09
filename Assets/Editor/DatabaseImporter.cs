#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class DatabaseImporter : EditorWindow
{public TextAsset masterCsv;
    public TextAsset locationCsv;
    
    public EncounterDatabaseSO masterSO;
    public LocationDatabaseSO locationSO;

    [MenuItem("Tools/Update Databases")]
    public static void ShowWindow()
    {
        GetWindow<DatabaseImporter>("CSV to SO Importer");
    }

    void OnGUI()
    {
        GUILayout.Label("CSV Files", EditorStyles.boldLabel);
        masterCsv = (TextAsset)EditorGUILayout.ObjectField("Master Table CSV", masterCsv, typeof(TextAsset), false);
        locationCsv = (TextAsset)EditorGUILayout.ObjectField("Location Table CSV", locationCsv, typeof(TextAsset), false);

        GUILayout.Space(10);
        GUILayout.Label("Target ScriptableObjects", EditorStyles.boldLabel);
        masterSO = (EncounterDatabaseSO)EditorGUILayout.ObjectField("Master DB SO", masterSO, typeof(EncounterDatabaseSO), false);
        locationSO = (LocationDatabaseSO)EditorGUILayout.ObjectField("Location DB SO", locationSO, typeof(LocationDatabaseSO), false);

        GUILayout.Space(20);

        if (GUILayout.Button("Update Master Database"))
        {
            if (CheckReferences(masterCsv, masterSO)) UpdateMasterDB();
        }

        if (GUILayout.Button("Update Location Database"))
        {
            if (CheckReferences(locationCsv, locationSO)) UpdateLocationDB();
        }
        
        if (GUILayout.Button("Update ALL"))
        {
            if (CheckReferences(masterCsv, masterSO)) UpdateMasterDB();
            if (CheckReferences(locationCsv, locationSO)) UpdateLocationDB();
        }
    }

    bool CheckReferences(TextAsset csv, ScriptableObject so)
    {
        if (csv == null || so == null)
        {
            Debug.LogError("[Importer] CSV 파일이나 Target SO가 연결되지 않았습니다.");
            return false;
        }
        return true;
    }

    void UpdateMasterDB()
    {
        masterSO.masterTable.Clear();
        
        var rows = ParseCSVRaw(masterCsv.text);

        for (int i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Count < 5 || string.IsNullOrWhiteSpace(row[0])) continue;

            EncounterMetaInfo info = new EncounterMetaInfo();
            
            info.id = row[0].Trim();
            
            info.nameKO = row[1].Trim();
            
            string typeStr = row[2].Trim();
            if (System.Enum.TryParse(typeStr, true, out EncounterType type)) 
                info.type = type;
            else 
            {
                Debug.LogWarning($"[Importer] '{info.id}'의 타입({typeStr})을 알 수 없어 Battle로 설정합니다.");
                info.type = EncounterType.Battle;
            }

            info.imagePath = row[3].Trim();
            
            info.imagePath = row[4].Trim();

            if (row.Count > 5 && int.TryParse(row[5], out int order)) 
                info.order = order;
            
            if (row.Count > 6) {
                string ess = row[6].Trim().ToUpper();
                info.isEssential = (ess == "TRUE" || ess == "T" || ess == "1");
            }

            masterSO.masterTable.Add(info);
        }
        
        EditorUtility.SetDirty(masterSO);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Importer] Master DB Updated. Total Encounters: {masterSO.masterTable.Count}");
    }

    void UpdateLocationDB()
    {
        locationSO.locationTable.Clear();
        var rows = ParseCSVRaw(locationCsv.text);

        for (int i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Count < 6 || string.IsNullOrWhiteSpace(row[0])) continue;

            LocationMetaInfo info = new LocationMetaInfo();
            
            info.id = row[0].Trim();
            
            info.nameKO = row[1].Trim();
            string poolRaw = row[4];
            info.encounterPool = new List<string>();
            string[] poolSplit = poolRaw.Split(new char[] { ',', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in poolSplit) 
            {
                info.encounterPool.Add(p.Trim());
            }

            if (int.TryParse(row[5], out int count)) 
                info.howManyEnc = count;
            else 
                info.howManyEnc = 1;

            locationSO.locationTable.Add(info);
        }

        EditorUtility.SetDirty(locationSO);
        AssetDatabase.SaveAssets();
        Debug.Log($"[Importer] Location DB Updated. Total Locations: {locationSO.locationTable.Count}");
    }

    List<List<string>> ParseCSVRaw(string text)
    {
        var result = new List<List<string>>();
        var currentRow = new List<string>();
        var currentCell = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (inQuotes)
            {
                if (c == '"') {
                    if (i + 1 < text.Length && text[i + 1] == '"') { currentCell.Append('"'); i++; }
                    else inQuotes = false;
                }
                else currentCell.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { currentRow.Add(currentCell.ToString()); currentCell.Clear(); }
                else if (c == '\n') { currentRow.Add(currentCell.ToString()); result.Add(currentRow); currentRow = new List<string>(); currentCell.Clear(); }
                else if (c != '\r') currentCell.Append(c);
            }
        }
        if (currentCell.Length > 0 || currentRow.Count > 0) { currentRow.Add(currentCell.ToString()); result.Add(currentRow); }
        return result;
    }
}
#endif