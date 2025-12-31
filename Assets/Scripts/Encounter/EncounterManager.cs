using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System.IO; 
using System.Text;
using System.Linq;
using Random = UnityEngine.Random;

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
    
    [Header("Sub-Systems")]
    public EncounterRouletteUI rouletteUI;
    public EncounterMenuControll headerUI;
    public GameObject merchantPanel;
    public GameObject cardRemovalPanel;
    public EncounterMerchantUI merchantUI;

    [Header("Data Databases (SO)")]
    public EncounterDatabaseSO masterDB;   
    public LocationDatabaseSO locationDB;  

    [Header("Game Data")]
    public CharacterSO characterData;
    public PlayerStatsSo playerStats;
    public ActSO actData; 

    private Dictionary<string, EncounterMetaInfo> masterDatabase;
    private Dictionary<string, LocationMetaInfo> locationDatabase;
    
    private Dictionary<string, EncounterStep> stepDictionary = new Dictionary<string, EncounterStep>();
    private EncounterStep currentStep;
    private bool isSceneLoading = false;
    private Queue<string> encounterSequenceQueue = new Queue<string>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else { Destroy(gameObject); }
    }

    IEnumerator Start()
    {
        yield return null;
        isSceneLoading = false;
        
        if (masterDB != null) 
            masterDatabase = masterDB.GetDictionary();
        else 
            Debug.LogError("[EncounterManager] Master DB SO가 연결되지 않았습니다!");

        if (locationDB != null) 
            locationDatabase = locationDB.GetDictionary();
        else 
            Debug.LogError("[EncounterManager] Location DB SO가 연결되지 않았습니다!");

        InitializeEncounterSequence();
    }

    void InitializeEncounterSequence()
    {
        string currentLocID = "";

        if (actData != null)
        {
            currentLocID = actData.curNodeLocationID;
            
            Debug.Log(currentLocID);
        }
        else
        {
            Debug.LogError("ActSO가 연결되지 않았습니다. 인스펙터를 확인해주세요.");
            return;
        }

        if (locationDatabase == null || !locationDatabase.ContainsKey(currentLocID))
        {
            
            Debug.LogError($"Location Database에서 ID '{currentLocID}'를 찾을 수 없습니다.");
            return;
        }

        LocationMetaInfo locInfo = locationDatabase[currentLocID];
        List<string> selectedIDs = SelectEncounters(locInfo);

        encounterSequenceQueue.Clear();
        foreach (var id in selectedIDs)
        {
            encounterSequenceQueue.Enqueue(id);
        }

        PlayNextEncounterInQueue();
    }

    List<string> SelectEncounters(LocationMetaInfo locInfo)
    {
        List<string> result = new List<string>();
        var candidates = new List<EncounterCandidate>();

        EncounterType prevType = (actData != null) ? actData.lastEncounterType : EncounterType.Battle;

        foreach (string id in locInfo.encounterPool)
        {
            if (!masterDatabase.ContainsKey(id)) continue;

            EncounterMetaInfo info = masterDatabase[id];
            candidates.Add(new EncounterCandidate { id = id, info = info, score = 0 });
        }

        foreach (var cand in candidates)
        {
            cand.score = Random.Range(1, 100);

            if (cand.info.isEssential)
            {
                cand.score = 100;
            }
            else
            {
                if (prevType == EncounterType.Rest && cand.info.type == EncounterType.Rest) cand.score = 0;
                if (prevType == EncounterType.Merchant && cand.info.type == EncounterType.Merchant) cand.score = 0;
            }
        }

        int pickCount = 0;
        int targetCount = locInfo.howManyEnc;

        while (pickCount < targetCount)
        {
            var best = candidates
                .Where(c => c.score > 0 && !result.Contains(c.id))
                .OrderByDescending(c => c.score)
                .FirstOrDefault();

            if (best == null) break; 

            result.Add(best.id);
            pickCount++;

            // 같은 Order 제거
            foreach (var other in candidates)
            {
                if (other.id == best.id) continue; 
                if (other.info.order == best.info.order) other.score = 0; 
            }
        }

        return result.OrderBy(id => masterDatabase[id].order).ToList();
    }

    private class EncounterCandidate
    {
        public string id;
        public EncounterMetaInfo info;
        public int score;
    }

    void PlayNextEncounterInQueue()
    {
        if (encounterSequenceQueue.Count > 0)
        {
            string nextID = encounterSequenceQueue.Dequeue();
            
            if (masterDatabase.ContainsKey(nextID) && actData != null)
                actData.lastEncounterType = masterDatabase[nextID].type;

            StartEncounterByID(nextID);
        }
        else
        {
            Debug.Log("[System] 지역 인카운터 완료. MapScene으로 이동.");
            SceneManager.LoadScene("MapScene");
        }
    }

    public void StartEncounterByID(string encounterID)
    {
        if (masterDatabase == null || !masterDatabase.ContainsKey(encounterID))
        {
            Debug.LogError($"ID '{encounterID}'가 Master DB에 없습니다.");
            return;
        }

        EncounterMetaInfo meta = masterDatabase[encounterID];
        
        string targetPath = meta.filePath;
        if (!targetPath.EndsWith(".csv")) targetPath += ".csv";
        string projectRoot = Directory.GetParent(Application.dataPath).ToString();
        string fullPath = Path.Combine(projectRoot, targetPath);

        if (!File.Exists(fullPath))
        {
            Debug.LogError($"시나리오 파일 없음: {fullPath}");
            return;
        }

        Debug.Log($"[Encounter Start] {meta.id}");
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
        if (stepDictionary.ContainsKey("P1")) PlayStep("P1");
        else Debug.LogError("인카운터 시작 실패: 'P1' ID가 없습니다.");
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
    bool CheckCondition(string condition) => true; 

    public void EndEncounter()
    {
        if (encounterPanel != null) encounterPanel.SetActive(false);
        PlayNextEncounterInQueue();
    }
    
    // --- Static 메서드: MapScene 등 외부에서 타입 확인용 ---
    public static List<EncounterType> GetEncounterType(string locationID)
    {
        if (Instance == null || Instance.locationDatabase == null)
        {
            // EncounterScene이 아니라면 Instance가 없을 수 있음.
            // 이 경우 MapScene에서는 별도의 MapManager가 SO를 직접 참조해서 처리하는 것이 안전함.
            Debug.LogWarning("EncounterManager 인스턴스가 없어 타입을 확인할 수 없습니다.");
            return new List<EncounterType>();
        }

        if (!Instance.locationDatabase.ContainsKey(locationID))
            return new List<EncounterType>();

        List<EncounterType> typeList = new List<EncounterType>();
        LocationMetaInfo locInfo = Instance.locationDatabase[locationID];

        foreach (string encID in locInfo.encounterPool)
        {
            if (Instance.masterDatabase.ContainsKey(encID))
            {
                typeList.Add(Instance.masterDatabase[encID].type);
            }
        }
        return typeList.Distinct().ToList();
    }
    
    // --- 함수 파싱 및 실행 (기존 유지) ---
    void ParseAndExecuteFunctions(string commandLine)
    {
        string[] commands = Regex.Split(commandLine, @",\s*(?![^()]*\))");
        foreach (string cmd in commands)
        {
            ExecuteSingleFunction(cmd.Trim());
            if (isSceneLoading) break; 
        }
        if (headerUI != null) headerUI.RefreshUI();
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
            case "StartRoulette": 
                // 룰렛 로직...
                 if (args.Length >= 2 && rouletteUI != null)
                {
                    string statName = args[0];
                    int difficulty = int.Parse(args[1]);
                    string winPage = (args.Length > 3) ? args[3] : "P_WIN"; 
                    string losePage = (args.Length > 4) ? args[4] : "P_LOSE";

                    encounterPanel.SetActive(false);
                    rouletteUI.Open(statName, difficulty, (result) => 
                    {
                        encounterPanel.SetActive(true);
                        if (result == RouletteResultType.Success || result == RouletteResultType.GreatSuccess)
                            PlayStep(winPage);
                        else
                            PlayStep(losePage);
                    });
                }
                break;
            
            case "GetObjet":
                if (args.Length >= 1) descriptionText.text += $"\n<color=#0000FF>오브제 {args[0]} 획득!</color>";
                break;

            case "GetDebris":
                if (args.Length >= 1) descriptionText.text += $"\n<color=#FFFF00>꿈의 파편 {args[0]}개 획득!</color>";
                break;

            case "UpStatus":
                if (args.Length >= 2 && playerStats != null && System.Enum.TryParse(args[0], true, out StatType sType))
                {
                    playerStats.ModifyStat(sType, int.Parse(args[1]));
                    descriptionText.text += $"\n<color=#0000FF>{sType} 증가!</color>";
                }
                break;

            case "StartBattle": 
                if (args.Length >= 1)
                {
                    if (characterData != null) characterData.enemyName = args[0]; 
                    isSceneLoading = true;
                    SceneManager.LoadScene("BattleScene"); 
                }
                break;

            case "meetMerchant":
                if (merchantPanel != null)
                {
                    encounterPanel.SetActive(false);
                    merchantPanel.SetActive(true);
                    merchantUI.Open();
                }
                break;

            case "Heal":
                if (args.Length >= 1 && characterData != null)
                {
                    int heal = Mathf.RoundToInt(characterData.maxHealth * (int.Parse(args[0]) / 100f));
                    characterData.curHealth = Mathf.Min(characterData.curHealth + heal, characterData.maxHealth);
                    descriptionText.text += $"\n<color=#00FF00>체력 {heal} 회복.</color>";
                }
                break;

            case "LoseHP":
                if (args.Length >= 1 && characterData != null)
                {
                    int amount = int.Parse(args[0]);
                    int dmg = amount; 
                    if (args.Length >= 2 && args[1].Contains("ratio")) 
                        dmg = Mathf.RoundToInt(characterData.maxHealth * (amount / 100f));
                    
                    characterData.curHealth = Mathf.Max(1, characterData.curHealth - dmg);
                    descriptionText.text += $"\n<color=#FF0000>체력 {dmg} 감소...</color>";
                }
                break;
            
            case "DeleteCard":
                if (cardRemovalPanel != null) cardRemovalPanel.SetActive(true);
                break;
        }
    }

    public void OnMerchantClosed()
    {
        if (merchantPanel != null) merchantPanel.SetActive(false);
        if (cardRemovalPanel != null) cardRemovalPanel.SetActive(false);
        if (encounterPanel != null) encounterPanel.SetActive(true);

        if (currentStep != null)
        {
            string nextId = currentStep.nextStepId;
            if (nextId == "END") EndEncounter();
            else if (!string.IsNullOrEmpty(nextId) && nextId != "-" && nextId != "R") PlayStep(nextId);
            else PlayStep(currentStep.id);
        }
    }
}