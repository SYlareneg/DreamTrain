using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System.IO; 
using System.Text;
using System;
using System.Linq;
using Random = UnityEngine.Random;

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

public class LocationMetaInfo
{
    public string id;
    public string nameKO;
    public List<string> encounterPool; 
    public int howManyEnc;            
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
    
    [Header("Sub-Systems")]
    public EncounterRouletteUI rouletteUI;
    public EncounterMenuControll headerUI;
    public GameObject merchantPanel;
    public GameObject cardRemovalPanel;
    public EncounterMerchantUI merchantUI;

    [Header("Data Tables")]
    public TextAsset masterTableCsv;   
    public TextAsset locationTableCsv;  

    [Header("Game Data")]
    public CharacterSO characterData;
    public PlayerStatsSo playerStats;
    public ActSO actData; 

    
    public static EncounterType LastEncounterType = EncounterType.Battle; 

    private Dictionary<string, EncounterMetaInfo> masterDatabase = new Dictionary<string, EncounterMetaInfo>();
    private Dictionary<string, LocationMetaInfo> locationDatabase = new Dictionary<string, LocationMetaInfo>();
    private Dictionary<string, EncounterStep> stepDictionary = new Dictionary<string, EncounterStep>();
    
    private EncounterStep currentStep;
    private bool isSceneLoading = false;
    private const string DEFAULT_DEBUG_ID = "ACT1_BEST_HORSE";

    
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

    void Start()
    {
        isSceneLoading = false;
        
        if (masterTableCsv != null) ParseMasterTable(masterTableCsv.text);
        else Debug.LogError("[EncounterManager] Master Table CSV Missing!");

        if (locationTableCsv != null) ParseLocationTable(locationTableCsv.text);
        else Debug.LogError("[EncounterManager] Location Table CSV Missing!");

        InitializeEncounterSequence();
    }

    void InitializeEncounterSequence()
    {
        string currentLocID = "";

        if (actData != null) currentLocID = actData.curNodeLocationID;
        else
        {
            Debug.LogWarning("ActSO가 연결되지 않았습니다. 디버그용 위치를 사용합니다.");
            currentLocID = "MERRY_GO_ROUND"; 
        }

        if (!locationDatabase.ContainsKey(currentLocID))
        {
            Debug.LogError($"Location ID '{currentLocID}' 정보를 찾을 수 없습니다.");
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

        foreach (string id in locInfo.encounterPool)
        {
            if (!masterDatabase.ContainsKey(id))
            {
                Debug.LogWarning($"Pool에 있는 ID '{id}'가 Master Table에 없습니다.");
                continue;
            }

            EncounterMetaInfo info = masterDatabase[id];
            candidates.Add(new EncounterCandidate { id = id, info = info, score = 0 });
        }

        foreach (var cand in candidates)
        {
            cand.score = Random.Range(1, 100);

            if (!cand.info.isEssential)
            {
                if (LastEncounterType == EncounterType.Rest && cand.info.type == EncounterType.Rest)
                {
                    cand.score = 0;
                }
                if (LastEncounterType == EncounterType.Merchant && cand.info.type == EncounterType.Merchant)
                {
                    cand.score = 0;
                }
            }

            if (cand.info.isEssential)
            {
                cand.score = 100;
            }
        }

        int pickCount = 0;
        int targetCount = locInfo.howManyEnc;

        while (pickCount < targetCount)
        {
            var bestCandidate = candidates
                .Where(c => c.score > 0 && !result.Contains(c.id))
                .OrderByDescending(c => c.score)
                .FirstOrDefault();

            if (bestCandidate == null) break; 

            result.Add(bestCandidate.id);
            pickCount++;

            foreach (var other in candidates)
            {
                if (other.id == bestCandidate.id) continue; 

                if (other.info.order == bestCandidate.info.order)
                {
                    other.score = 0; 
                }
            }
        }

        result = result.OrderBy(id => masterDatabase[id].order).ToList();

        Debug.Log($"[Encounter Selection] 장소: {locInfo.id}, 선택됨: {string.Join(", ", result)}");

        return result;
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
            
            if (masterDatabase.ContainsKey(nextID))
                LastEncounterType = masterDatabase[nextID].type;

            StartEncounterByID(nextID);
        }
        else
        {
            Debug.Log("[System] 해당 지역의 모든 인카운터 종료. MapScene으로 이동합니다.");
            SceneManager.LoadScene("MapScene");
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

        Debug.Log($"[Encounter Start] {meta.id} (Order: {meta.order})");
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

        // BRANCH
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
            else CreateButton("다음", currentStep.nextStepId); // [cite: 94]
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
        
        // TODO: 실제 인벤토리 매니저와 연동 필요
        if (condition.StartsWith("HasDream")) return true; 
        if (condition.StartsWith("HasObjet")) return true;
        
        return true; 
    }

    public void EndEncounter()
    {
        if (encounterPanel != null) encounterPanel.SetActive(false);
        PlayNextEncounterInQueue();
    }
    
    public static List<EncounterType> GetEncounterType(string locationID)
    {
        if (Instance == null)
        {
            Debug.LogError("[EncounterManager] 인스턴스가 생성되지 않았습니다.");
            return new List<EncounterType>();
        }

        if (!Instance.locationDatabase.ContainsKey(locationID))
        {
            Debug.LogError($"[EncounterManager] Location ID '{locationID}'를 찾을 수 없습니다.");
            return new List<EncounterType>();
        }

        List<EncounterType> typeList = new List<EncounterType>();
        LocationMetaInfo locInfo = Instance.locationDatabase[locationID];

        if (locInfo.encounterPool != null)
        {
            foreach (string encID in locInfo.encounterPool)
            {
                if (Instance.masterDatabase.ContainsKey(encID))
                {
                    EncounterType type = Instance.masterDatabase[encID].type;
                    typeList.Add(type);
                }
                else
                {
                    Debug.LogWarning($"[GetEncounterType] Pool에 있는 ID '{encID}'가 Master Table에 없습니다.");
                }
            }
        }

        return typeList.Distinct().ToList(); // 중복된 타입은 제거하고 종류만
    }
    
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
                if (args.Length >= 2)
                {
                    string statName = args[0];
                    int difficulty = int.Parse(args[1]);
                    // TODO: args[2] 조건(upper 등) 반영 필요

                    string winPage = (args.Length > 3) ? args[3] : "P_WIN"; 
                    string losePage = (args.Length > 4) ? args[4] : "P_LOSE";

                    if (rouletteUI != null)
                    {
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
                }
                break;

            case "GetObjet":
                if (args.Length >= 1)
                {
                    string objetID = args[0];
                    // TODO: InventoryManager.Add(objetID);
                    Debug.Log($"[Item] 오브제 획득: {objetID}");
                    descriptionText.text += $"\n<color=#0000FF>오브제 {objetID}을(를) 얻었다!</color>";
                }
                break;

            case "GetDebris":
                if (args.Length >= 1 && playerStats != null)
                {
                    int amount = int.Parse(args[0]);
                    // TODO: playerSO.ModifyDreamFragment(amount); 이거 연결하기
                    descriptionText.text += $"\n<color=#FFFF00>꿈의 파편을 {amount}개 얻었다!</color>";
                }
                break;

            case "UpStatus":
                if (args.Length >= 2 && playerStats != null)
                {
                    if (Enum.TryParse(args[0], true, out StatType statType))
                    {
                        int amount = int.Parse(args[1]);
                        playerStats.ModifyStat(statType, amount);
                        descriptionText.text += $"\n<color=#0000FF>{statType}가(이) {amount} 증가했다!</color>";
                    }
                }
                break;
            case "DownStatus":
                Debug.Log("downStat");
                if (args.Length >= 2 && playerStats != null)
                {
                    if (Enum.TryParse(args[0], true, out StatType statType))
                    {
                        int amount = int.Parse(args[1]);
                        playerStats.ModifyStat(statType, -amount);
                        descriptionText.text += $"\n<color=#FF0000>{statType}가(이) {amount} 감소했다...</color>";
                    }
                }
                break;

            case "StartBattle": 
                if (args.Length >= 1)
                {
                    string enemyID = args[0];
                    if (characterData != null) characterData.enemyName = enemyID; 
                    
                    Debug.Log($"[Battle] 적 '{enemyID}' 조우");
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
                    Debug.Log("상점 UI 오픈");
                }
                break;  

            case "Heal":
                if (args.Length >= 1 && characterData != null)
                {
                    int percent = int.Parse(args[0]);
                    int healAmount = Mathf.RoundToInt(characterData.maxHealth * (percent / 100f));
                    if (healAmount < 1) healAmount = 1;

                    characterData.curHealth = Mathf.Min(characterData.curHealth + healAmount, characterData.maxHealth);
                    descriptionText.text += $"\n<color=#00FF00>체력이 {healAmount} 회복되었다.</color>";
                }
                break;

            case "LoseHP": 
                if (args.Length >= 1 && characterData != null)
                {
                    int amount = int.Parse(args[0]);
                    bool isRatio = false;
                    if (args.Length >= 2)
                    {
                        string option = args[1].ToLower();
                        if (option.Contains("ratio") || option.Contains("비율") || option.Contains("퍼센트"))
                            isRatio = true;
                    }

                    int damage = amount;
                    if (isRatio)
                    {
                        damage = Mathf.RoundToInt(characterData.maxHealth * (amount / 100f));
                    }

                    int currentHP = characterData.curHealth;
                    int finalHP = Mathf.Max(1, currentHP - damage);
                    int actualDamage = currentHP - finalHP;

                    characterData.curHealth = finalHP;
                    
                    descriptionText.text += $"\n<color=#FF0000>체력을 {actualDamage} 잃었다...</color>";
                }
                break;
            
            case "Delete Card":
            case "DeleteCard":
                if (cardRemovalPanel != null)
                {
                    cardRemovalPanel.SetActive(true);
                    Debug.Log("카드 삭제 UI 오픈");
                }
                else
                {
                    Debug.Log("카드 삭제 UI 에러");
                    //TODO: 카드 삭제 로직 버그 수정
                }
                break;

            default:
                if (command.Contains(".add"))
                {
                    Debug.LogWarning($"Deprecated command format: {command}");
                }
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
            Debug.Log($"[Merchant Closed] 다음 페이지로 이동: {nextId}");
            if (nextId == "END") EndEncounter();
            else if (!string.IsNullOrEmpty(nextId) && nextId != "-" && nextId != "R") PlayStep(nextId);
            else PlayStep(currentStep.id);
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

    void ParseLocationTable(string csvText)
    {
        locationDatabase.Clear();
        var rows = ParseCSVRaw(csvText);

        for (int i = 1; i < rows.Count; i++)
        {
            var row = rows[i];
            if (row.Count < 6 || string.IsNullOrWhiteSpace(row[0])) continue;

            LocationMetaInfo info = new LocationMetaInfo();
            info.id = row[0].Trim();
            info.nameKO = row[1].Trim();
            string poolRaw = row[4]; 
            info.encounterPool = new List<string>();
            string[] poolSplit = poolRaw.Split(new char[] { ',', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach(var p in poolSplit) info.encounterPool.Add(p.Trim());

            if (int.TryParse(row[5], out int count)) info.howManyEnc = count;
            else info.howManyEnc = 1;

            if (!locationDatabase.ContainsKey(info.id))
                locationDatabase.Add(info.id, info);
        }
    }

    void ParseMasterTable(string csvText)
    {
        masterDatabase.Clear();
        var rows = ParseCSVRaw(csvText);

        for (int i = 1; i < rows.Count; i++) 
        {
            var row = rows[i];
            if (row.Count < 5 || string.IsNullOrWhiteSpace(row[0])) continue;

            EncounterMetaInfo info = new EncounterMetaInfo();
            info.id = row[0].Trim();
            info.nameKO = row[1].Trim();
            
            if (System.Enum.TryParse(row[2].Trim(), true, out EncounterType type)) info.type = type;
            else info.type = EncounterType.Battle;
            
            info.imagePath = row[3].Trim();
            info.filePath = row[4].Trim();
            
            if (row.Count > 5)
            {
                string essRaw = row[5].Trim().ToUpper();
                info.isEssential = (essRaw == "TRUE" || essRaw == "T" || essRaw == "1");
            }
            else info.isEssential = false;
            
            if (row.Count > 6 && int.TryParse(row[6], out int order)) 
                info.order = order;
            else 
                info.order = 0;

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