using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System.IO; 
using System.Text;

public class EncounterMetaInfo
{
    public string id;           
    public string nameKO;       
    public EncounterType type;  
    public string imagePath;    
    public string filePath;     
}

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager Instance;

    [Header("UI References")]
    public GameObject encounterPanel;       
    public Image illustrationImage;         
    public TextMeshProUGUI titleText;       
    public TextMeshProUGUI descriptionText; 
    public Transform choiceContainer;       
    public GameObject choiceButtonPrefab;   

    [Header("Master Data")]
    public TextAsset masterTableCsv; 
    
    [Header("System")]
    public CharacterSO characterData; 
    
    private Dictionary<string, EncounterMetaInfo> masterDatabase = new Dictionary<string, EncounterMetaInfo>();
    private Dictionary<string, EncounterStep> stepDictionary = new Dictionary<string, EncounterStep>();
    private EncounterStep currentStep;
    
    private bool isSceneLoading = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        isSceneLoading = false;
        
        if (masterTableCsv != null)
        {
            ParseMasterTable(masterTableCsv.text);
        }
        else
        {
            Debug.LogError("[EncounterManager] Master Table CSV가 연결되지 않았습니다.");
            return;
        }

        if (masterDatabase.Count > 0)
        {
            var enumerator = masterDatabase.Keys.GetEnumerator();
            enumerator.MoveNext();
            string autoID = enumerator.Current;
            StartEncounterByID(autoID);
        }
        else
        {
            Debug.LogWarning("[EncounterManager] 마스터 테이블이 비어있습니다.");
        }
    }

    public void StartEncounterByID(string encounterID)
    {
        if (!masterDatabase.ContainsKey(encounterID))
        {
            Debug.LogError($"ID '{encounterID}'가 마스터 테이블에 없습니다.");
            return;
        }

        EncounterMetaInfo meta = masterDatabase[encounterID];
        
        string targetPath = meta.filePath;
        if (!targetPath.EndsWith(".csv")) targetPath += ".csv";

        string projectRoot = Directory.GetParent(Application.dataPath).ToString();
        string fullPath = Path.Combine(projectRoot, targetPath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"파일을 찾을 수 없습니다: {fullPath}");
            return;
        }

        Debug.Log($"[Encounter Load] {meta.id}");
        string csvContent = File.ReadAllText(fullPath);

        if (!string.IsNullOrEmpty(meta.imagePath))
        {
            string imgName = Path.GetFileNameWithoutExtension(meta.imagePath);
            Sprite img = Resources.Load<Sprite>($"Images/{imgName}");
            if (img == null) img = Resources.Load<Sprite>(imgName); 

            if (img != null && illustrationImage != null) 
                illustrationImage.sprite = img;
        }

        if (titleText != null) titleText.text = meta.nameKO;

        StartEncounterFromText(csvContent);
    }

    public void StartEncounterFromText(string csvText)
    {
        ParseEncounterCSV(csvText);
        
        if (encounterPanel != null) encounterPanel.SetActive(true);

        if (stepDictionary.ContainsKey("P1"))
            PlayStep("P1");
        else
            Debug.LogError("인카운터 시작 실패: 'P1' ID가 없습니다.");
    }

    public void PlayStep(string id)
    {
        if (isSceneLoading) return;
        if (!stepDictionary.ContainsKey(id)) return;

        currentStep = stepDictionary[id];

        if (currentStep.type == EncounterStepType.DESC)
        {
            descriptionText.text = currentStep.textContent.Replace("\\n", "\n");
        }

        if (IsValidFunction(currentStep.functionCall))
        {
            ParseAndExecuteFunctions(currentStep.functionCall);
        }
        
        if (isSceneLoading) return;

        UpdateOptionsUI();
    }

    void UpdateOptionsUI()
    {
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);

        if (currentStep.options != null && currentStep.options.Count > 0)
        {
            foreach (var option in currentStep.options)
            {
                if (!CheckCondition(option.condition)) continue; 
                CreateButton(option.text, option.nextStepId, option.functionCall);
            }
        }
        else 
        {
            if (IsWaitState(currentStep.nextStepId)) { }
            else if (currentStep.nextStepId == "END") CreateButton("떠난다", "END");
            else CreateButton("다음", currentStep.nextStepId);
        }
    }

    void CreateButton(string text, string nextId, string functionCall = null)
    {
        GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
        btnObj.GetComponentInChildren<TextMeshProUGUI>().text = text;
        
        btnObj.GetComponent<Button>().onClick.AddListener(() => 
        {
            if (IsValidFunction(functionCall))
                ParseAndExecuteFunctions(functionCall);

            if (isSceneLoading) return;

            if (IsWaitState(nextId)) { }
            else if (nextId == "END") EndEncounter();
            else PlayStep(nextId);
        });
    }

    bool IsWaitState(string id) => string.IsNullOrEmpty(id) || id == "-" || id == "R";
    bool IsValidFunction(string func) => !string.IsNullOrEmpty(func) && func != "-" && func != "DEFAULT";

    bool CheckCondition(string condition)
    {
        if (string.IsNullOrEmpty(condition) || condition == "-" || condition == "DEFAULT") return true;
        if (condition.StartsWith("HasDream") || condition.StartsWith("HasObjet"))
        {
            // TODO: 인벤토리 연동
            return true; 
        }
        return true; 
    }

    public void EndEncounter()
    {
        if (encounterPanel != null) encounterPanel.SetActive(false);
    }

    void ParseAndExecuteFunctions(string commandLine)
    {
        string[] commands = Regex.Split(commandLine, @",\s*(?![^()]*\))");
        foreach (string cmd in commands)
        {
            ExecuteSingleFunction(cmd.Trim());
            if (isSceneLoading) break; 
        }
    }

    void ExecuteSingleFunction(string command)
    {
        if (string.IsNullOrEmpty(command) || command == "-") return;

        string funcName = command.Split('(')[0].Trim();
        string argsRaw = "";
        
        Match match = Regex.Match(command, @"\(([^)]*)\)");
        if (match.Success) argsRaw = match.Groups[1].Value;

        string[] args = argsRaw.Split(',');
        for(int i=0; i<args.Length; i++) args[i] = args[i].Trim();

        switch (funcName)
        {
            case "LoseHP": 
                if (args.Length >= 2)
                {
                    int amount = int.Parse(args[0]);
                    string type = args[1].ToLower();

                    // int currentHP = 100;
                    // int maxHP = 100; 
                    // int damage = (type == "per") ? (int)(maxHP * (amount / 100f)) : amount;
                    // int finalHP = Mathf.Max(1, currentHP - damage);
                    // int actualLoss = currentHP - finalHP;

                    descriptionText.text += $"\n<color=red>체력을 잃었습니다.</color>";
                }
                break;

            case "GetDebris": 
                if (args.Length >= 1)
                {
                    int amount = int.Parse(args[0]);
                    descriptionText.text += $"\n<color=yellow>꿈의 잔해 {amount}개를 얻었습니다.</color>";
                }
                break;

            case "StartBattle": 
                if (args.Length >= 1)
                {
                    string enemyID = args[0];
                    if (characterData != null)
                    {
                        characterData.enemyName = enemyID; 
                        Debug.Log($"[StartBattle] 적 '{enemyID}' 전투 시작");
                        
                        isSceneLoading = true;
                        if (encounterPanel != null) encounterPanel.SetActive(false);
                        SceneManager.LoadScene("BattleScene"); 
                    }
                }
                break;

            case "StartRoullete": 
                if (args.Length >= 3)
                {
                    // string stat = args[0];
                    // int val = int.Parse(args[1]);
                    // string cond = args[2];
                    string winPage = (args.Length > 3) ? args[3] : "P_WIN"; 
                    string losePage = (args.Length > 4) ? args[4] : "P_LOSE";
                    // 룰렛 UI 오픈
                }
                break;
            
            case "GetObjet": 
                 Debug.Log($"[GetObjet] 오브제 '{args[0]}' 획득");
                 break;

            default:
                if (command.Contains(".add"))
                {
                    string[] parts = command.Split('.');
                    string targetStat = parts[0];
                    int val = int.Parse(Regex.Match(parts[1], @"\(([^)]*)\)").Groups[1].Value);
                    descriptionText.text += $"\n<color=blue>{targetStat}가 {val} 증가했다!</color>";
                }
                break;
        }
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
                if (c == '"')
                {
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

    void ParseMasterTable(string csvText)
    {
        masterDatabase.Clear();
        var rows = ParseCSVRaw(csvText);

        for (int i = 1; i < rows.Count; i++) 
        {
            var row = rows[i];
            if (row.Count < 5 || string.IsNullOrWhiteSpace(row[0])) continue;

            for (int k = 0; k < row.Count; k++) row[k] = row[k].Trim();

            EncounterMetaInfo info = new EncounterMetaInfo();
            info.id = row[0];
            info.nameKO = row[1];
            if (System.Enum.TryParse(row[2], true, out EncounterType type)) info.type = type;
            else info.type = EncounterType.Battle;
            info.imagePath = row[3];
            info.filePath = row[4]; 

            if (!masterDatabase.ContainsKey(info.id))
                masterDatabase.Add(info.id, info);
        }
    }

    void ParseEncounterCSV(string csvText)
    {
        stepDictionary.Clear();
        var rows = ParseCSVRaw(csvText);

        for (int i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Count < 5 || string.IsNullOrWhiteSpace(row[0])) continue;
            
            for (int k = 0; k < row.Count; k++) row[k] = row[k].Trim();

            string id = row[0];
            if (!System.Enum.TryParse(row[1], true, out EncounterStepType type)) continue;
            string content = row[2];
            string nextId = row[3];
            string functionCall = row[4];
            string condition = (row.Count > 5) ? row[5] : "DEFAULT";

            if (!stepDictionary.ContainsKey(id))
            {
                stepDictionary.Add(id, new EncounterStep
                {
                    id = id, type = type, textContent = content, nextStepId = nextId, functionCall = functionCall,
                    options = new List<EncounterOption>()
                });
            }

            if (type == EncounterStepType.BRANCH)
            {
                stepDictionary[id].options.Add(new EncounterOption 
                { 
                    text = content, nextStepId = nextId, functionCall = functionCall, condition = condition 
                });
            }
        }
    }
}