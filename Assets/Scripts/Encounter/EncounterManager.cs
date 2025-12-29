using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using System.IO; 
using System.Text;
using System;
using Random = UnityEngine.Random;

// 데이터 파싱용 클래스
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
    
    [Header("Sub-Systems")]
    public EncounterRouletteUI rouletteUI;      // 룰렛 시스템
    public EncounterMenuControll headerUI;          // 상단 정보바 (HP, 스탯 갱신용)
    public GameObject merchantPanel;            // 상점 UI 패널 [cite: 543]
    public GameObject cardRemovalPanel;         // 카드 삭제 UI 패널 [cite: 543]

    public EncounterMerchantUI merchantUI;
    public TextAsset masterTableCsv; 
    
    [Header("Game Data")]
    public CharacterSO characterData;           // HP 관리용
    public PlayerStatsSo playerStats;           // 스탯/재화 관리용 [cite: 541]
    
    private Dictionary<string, EncounterMetaInfo> masterDatabase = new Dictionary<string, EncounterMetaInfo>();
    private Dictionary<string, EncounterStep> stepDictionary = new Dictionary<string, EncounterStep>();
    private EncounterStep currentStep;
    
    private bool isSceneLoading = false;
    private const string DEFAULT_DEBUG_ID = "ACT1_WHITE_RABBIT";

    private Dictionary<string, int> locationProgress = new Dictionary<string, int>(); // 장소별 진행 단계 저장
    private string currentLocationID = "";
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);  
        }
    }
    void Start()
    {
        isSceneLoading = false;
        
        // 마스터 테이블 로드
        if (masterTableCsv != null)
        {
            ParseMasterTable(masterTableCsv.text);
        }
        else
        {
            Debug.LogError("[EncounterManager] Master Table CSV가 연결되지 않았습니다.");
            return;
        }

        List<string> ePool = new List<string>();
        ePool.Add(DEFAULT_DEBUG_ID);
        SetEncounterPool(ePool);
    }

    // --- 인카운터 시작 및 진행 로직 ---
    public void SetEncounterPool(List<string> poolIds)
    {
        string targetID = DEFAULT_DEBUG_ID;

        // 1. 풀에서 인카운터 선택 (기본적으로 랜덤, 추후 규칙 적용 가능)
        if (poolIds != null && poolIds.Count > 0)
        {
            // 예: 랜덤 선택
            int randomIndex = Random.Range(0, poolIds.Count);
            targetID = poolIds[randomIndex];
        }
        else
        {
            Debug.LogWarning("인카운터 풀이 비어있습니다. 디버그용 기본 인카운터를 실행합니다.");
        }

        // 2. 인카운터 유효성 검사 및 실행
        StartEncounter(targetID);
    }
    
    public void StartEncounter(string encounterID)
    {
        // 마스터 테이블에 ID가 존재하는지 확인
        if (!masterDatabase.ContainsKey(encounterID))
        {
            Debug.LogError($"ID '{encounterID}'가 마스터 테이블에 없습니다. 디버깅용 '{DEFAULT_DEBUG_ID}'를 실행합니다.");
            encounterID = DEFAULT_DEBUG_ID;

            // 만약 디버깅용 ID조차 없다면 리턴
            if (!masterDatabase.ContainsKey(encounterID))
            {
                Debug.LogError($"치명적 오류: 디버깅용 ID '{DEFAULT_DEBUG_ID}'도 마스터 테이블에 없습니다.");
                return;
            }
        }

        // 3. 인카운터 로드 및 UI 활성화
        StartEncounterByID(encounterID);
    }
    
    public void StartEncounterByID(string encounterID)
    {
        if (!masterDatabase.ContainsKey(encounterID))
        {
            Debug.LogError($"ID '{encounterID}'가 마스터 테이블에 없습니다.");
            return;
        }

        EncounterMetaInfo meta = masterDatabase[encounterID];
        
        // CSV 파일 경로 처리
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

        // 이미지 로드
        if (!string.IsNullOrEmpty(meta.imagePath))
        {
            string imgName = Path.GetFileNameWithoutExtension(meta.imagePath);
            Sprite img = Resources.Load<Sprite>($"Images/{imgName}"); // 경로에 맞춰 수정 필요
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

        // 서술 텍스트 출력 (줄바꿈 처리)
        if (currentStep.type == EncounterStepType.DESC)
        {
            descriptionText.text = currentStep.textContent.Replace("\\n", "\n");
        }

        // 함수 실행 (텍스트 출력 후에 실행하여 결과 텍스트가 덧붙여지도록 함)
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

        // 선택지가 있는 경우 (BRANCH)
        if (currentStep.options != null && currentStep.options.Count > 0)
        {
            foreach (var option in currentStep.options)
            {
                if (!CheckCondition(option.condition)) continue; 
                CreateButton(option.text, option.nextStepId, option.functionCall);
            }
        }
        // 선택지가 없는 경우 (DESC -> NextPage 자동 연결 버튼)
        else 
        {
            // 다음 페이지가 없거나 대기 상태면 버튼 생성 안 함
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
            // 버튼 클릭 시 함수 실행
            if (IsValidFunction(functionCall))
                ParseAndExecuteFunctions(functionCall);

            if (isSceneLoading) return;

            // 페이지 이동
            if (IsWaitState(nextId)) { }
            else if (nextId == "END") EndEncounter();
            else PlayStep(nextId);
        });
    }

    // --- 유틸리티 함수 ---
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
        // 1. 현재 장소의 진행도(인카운터 순서) 저장
        // (이게 있어야 다음에 같은 장소 왔을 때 다음 인카운터가 나옴)
        if (!string.IsNullOrEmpty(currentLocationID))
        {
            if (!locationProgress.ContainsKey(currentLocationID))
                locationProgress[currentLocationID] = 0;

            locationProgress[currentLocationID]++;
            Debug.Log($"[Location: {currentLocationID}] 인카운터 완료. 다음 진행도: {locationProgress[currentLocationID]}");
        }

        // 2. 인카운터 패널 끄기
        if (encounterPanel != null) encounterPanel.SetActive(false);

        // 3. MapScene으로 전환
        Debug.Log("[System] MapScene으로 이동합니다.");
        SceneManager.LoadScene("MapScene"); 
    }

    // --- [핵심] 함수 파싱 및 실행 ---
    // 복수의 함수가 콤마로 연결되어 있을 수 있음
    void ParseAndExecuteFunctions(string commandLine)
    {
        // 괄호 안의 콤마는 무시하고, 함수 간의 콤마만 분리하기 위한 정규식
        string[] commands = Regex.Split(commandLine, @",\s*(?![^()]*\))");
        foreach (string cmd in commands)
        {
            ExecuteSingleFunction(cmd.Trim());
            if (isSceneLoading) break; 
        }
        
        // 함수 실행 후 UI(HP, 스탯) 갱신
        if (headerUI != null) headerUI.RefreshUI();
    }

    void ExecuteSingleFunction(string command)
    {
        if (string.IsNullOrEmpty(command) || command == "-") return;

        // 함수명과 인자 분리 (예: "LoseHP(10, value)" -> "LoseHP", ["10", "value"])
        string funcName = command.Split('(')[0].Trim();
        string argsRaw = "";
        
        Match match = Regex.Match(command, @"\(([^)]*)\)");
        if (match.Success) argsRaw = match.Groups[1].Value;

        string[] args = argsRaw.Split(',');
        for(int i=0; i<args.Length; i++) args[i] = args[i].Trim();

        switch (funcName)
        {
            case "StartRoullete": 
            case "StartRoulette": // 오타 방지
                if (args.Length >= 2)
                {
                    string statName = args[0];
                    int difficulty = int.Parse(args[1]);
                    // args[2]는 조건(upper 등)이지만 현재 로직엔 미반영

                    // 성공/실패 시 이동할 페이지 ID (CSV에 추가 인자로 있다고 가정하거나 기본값 사용)
                    string winPage = (args.Length > 3) ? args[3] : "P_WIN"; 
                    string losePage = (args.Length > 4) ? args[4] : "P_LOSE";

                    if (rouletteUI != null)
                    {
                        encounterPanel.SetActive(false); // UI 잠시 숨김
                        rouletteUI.Open(statName, difficulty, (result) => 
                        {
                            encounterPanel.SetActive(true); // 복귀
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

            case "GetDreamDebris":
            case "GetDebris": // 기획서 혼용 대응
                if (args.Length >= 1 && playerStats != null)
                {
                    int amount = int.Parse(args[0]);
                    //playerSO.ModifyDreamFragment(amount);
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
                        // 감소니까 음수로 전달
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
            
            case "MeetMerchant":
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
                    
                    // 두 번째 인자가 "ratio" 또는 "비율"이면 % 데미지
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
                break;

            default:
                if (command.Contains(".add"))
                {
                    // 예: Courage.add(1) 처리
                    // 위 upStatus 로직으로 대체하는 것을 권장
                    Debug.LogWarning($"Deprecated command format: {command}");
                }
                break;
        }
    }
    public void OnMerchantClosed()
    {
        if (encounterPanel != null)
        {
            encounterPanel.SetActive(true);
            PlayStep(currentStep.id); 
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