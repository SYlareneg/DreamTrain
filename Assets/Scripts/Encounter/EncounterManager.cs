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
    public ScrollRect descriptionScrollRect;
    
    [Header("Choice Container Layouts")]
    public GameObject oneChoiceContainer;
    public Transform oneChoicePos; 

    public GameObject twoChoiceContainer;
    public Transform[] twoChoicePos;

    public GameObject threeChoiceContainer;
    public Transform[] threeChoicePos;
    public GameObject choiceButtonPrefab; 
    
    
    [Header("Sub-Systems")]
    public EncounterRouletteUI rouletteUI;
    public GameObject roulettePanel;
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

    public EncSofaManager sofaManager;

    private Dictionary<string, EncounterMetaInfo> masterDatabase;
    private Dictionary<string, LocationMetaInfo> locationDatabase;
    
    private Dictionary<string, EncounterStep> stepDictionary = new Dictionary<string, EncounterStep>();
    private EncounterStep currentStep;
    private bool isSceneLoading = false;
    private Queue<string> encounterSequenceQueue = new Queue<string>();
    
    public bool isDebuging = true;
    public string debuggerID = "ACT1_Souvenir";
    
    private string currentLocID = "";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else { Destroy(gameObject); }
    }

    void Start()
    {
        if (masterDB != null) masterDatabase = masterDB.GetDictionary();
        if (locationDB != null) locationDatabase = locationDB.GetDictionary();
        ResetChoiceContainers();
        RestoreOrStartEncounter();
        SceneChangeManager.Inst.SceneFadeIn(() => {  });
    }
    void RestoreOrStartEncounter()
    {
        if (actData == null)
        {
            Debug.LogError("[EncounterManager] ActSO가 없습니다!");
            return;
        }

        // 1. 진행 중이던 인카운터가 있는지 확인 (BattleScene 등에서 돌아온 경우)
        if (!string.IsNullOrEmpty(actData.currentEncounterID))
        {
            Debug.Log($"[EncounterManager] 중단된 인카운터 복구: {actData.currentEncounterID}, Step: {actData.currentStepID}");
            
            // 해당 인카운터 파일 로드 및 파싱
            if (isDebuging)
            {
                LoadEncounterData(debuggerID);
                encounterPanel.SetActive(true);
                PlayStep("P1");
            }
            else LoadEncounterData(actData.currentEncounterID);
            // 저장된 스텝으로 이동 (없으면 P1)
            string savedStep = string.IsNullOrEmpty(actData.currentStepID) ? "P1" : actData.currentStepID;
            encounterPanel.SetActive(true);
            PlayStep(savedStep);
        }
        else
        {
            // 2. 진행 중인 게 없다면 대기열(Queue) 확인 및 지역 초기화
            if (actData.encounterQueue == null || actData.encounterQueue.Count == 0)
            {
                // 대기열도 비어있다면 새로 지역 진입한 것으로 간주하고 초기화
                InitializeLocationEncounters();
            }
            
            // 대기열에서 하나 뽑아서 시작
            PlayNextEncounterInQueue();
        }
    }
    
    void InitializeLocationEncounters()
    {
        string locID = actData.curNodeLocationID;
        Debug.Log($"[EncounterManager] 지역 인카운터 초기화: {locID}");

        if (!locationDatabase.ContainsKey(locID))
        {
            Debug.LogError($"Location DB에 {locID} 없음");
            return;
        }

        LocationMetaInfo locInfo = locationDatabase[locID];
        
        // ActSO에 큐 초기화 (ActSO에 List<string> encounterQueue가 있어야 함)
        actData.encounterQueue = new List<string>();

        // 고정 인카운터 추가
        foreach (var id in locInfo.selectedEncounterPool)
        {
            actData.encounterQueue.Add(id);
        }

        // 랜덤 인카운터 로직 (기존 로직 활용)
        // 만약 랜덤 뽑기가 필요하다면 SelectEncounters 함수 사용해서 추가
        // List<string> randomEncounters = SelectEncounters(locInfo);
        // actData.encounterQueue.AddRange(randomEncounters);
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
        
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "EncounterScene")
        {
            isSceneLoading = false;
            
            if (actData != null && masterDatabase != null && locationDatabase != null)
            {
                string newLocationID = actData.curNodeLocationID;
                if (currentLocID != newLocationID || currentLocID == "")
                {
                    Debug.Log($"[EncounterManager] 시퀀스 초기화: {currentLocID} -> {newLocationID}");
                    currentLocID = newLocationID;
                    InitializeEncounterSequence();
                }
            }
        }
    }
    void InitializeEncounterSequence()
    {
        Debug.Log($"[EncounterManager] 초기화 시퀀스 시작 (Target: {actData?.curNodeLocationID})");
        currentStep = null; 
        stepDictionary.Clear();
        
        if (actData == null) {Debug.LogError("ActSO가 연결되지 않았습니다."); return;}
        currentLocID = actData.curNodeLocationID;

        if (locationDatabase == null)
        {
            if (locationDB != null) locationDatabase = locationDB.GetDictionary();
            else {Debug.LogError("Location DB SO가 연결되지 않아 초기화할 수 없습니다!"); return;}
        }
        
        if (masterDatabase == null && masterDB != null) masterDatabase = masterDB.GetDictionary();

        if (!locationDatabase.ContainsKey(currentLocID)) {Debug.LogError($"[EncounterManager] LocationDB에 ID '{currentLocID}'가 존재하지 않습니다."); return;}

        LocationMetaInfo locInfo = locationDatabase[currentLocID];
        List<string> selectedIDs = locInfo.selectedEncounterPool;

        encounterSequenceQueue.Clear();
        foreach (var id in selectedIDs)
        {
            encounterSequenceQueue.Enqueue(id);
        }
        Debug.Log(currentLocID);
        Debug.Log($"[EncounterManager] 시퀀스 큐 초기화 완료 (개수: {encounterSequenceQueue.Count})");
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
        if (isDebuging)
        {
            actData.currentEncounterID = debuggerID;
            actData.currentStepID = "P1";
            LoadEncounterData(debuggerID);
            PlayStep("P1");
            return;
        }

        if (actData.encounterQueue != null && actData.encounterQueue.Count > 0)
        {
            string nextID = actData.encounterQueue[0];
            actData.encounterQueue.RemoveAt(0); // 큐에서 제거

            // ActSO에 현재 상태 기록 시작
            actData.currentEncounterID = nextID;
            actData.currentStepID = "P1"; 
            
            if (masterDatabase.ContainsKey(nextID))
                actData.lastEncounterType = masterDatabase[nextID].type;

            LoadEncounterData(nextID);
            encounterPanel.SetActive(true);
            PlayStep("P1");
        }
        else
        {
            Debug.Log("[System] 모든 인카운터 완료. MapScene 이동.");
            actData.currentEncounterID = "";
            actData.currentStepID = "";
            SceneManager.LoadScene("MapScene");
        }
    }
    
    void LoadEncounterData(string encounterID)
    {
        if (!masterDatabase.ContainsKey(encounterID)) return;

        EncounterMetaInfo meta = masterDatabase[encounterID];
        TextAsset csvAsset = new TextAsset();
        Debug.Log(encounterID);
        
        if (encounterID.Contains("ACT1_"))
        {
            string fileNameOnly = encounterID.Substring(encounterID.IndexOf('_') + 1);
            string resourcePath = $"Encounters/{fileNameOnly}";
            
            csvAsset = Resources.Load<TextAsset>(resourcePath);
            
            if (csvAsset != null)
            {
                Debug.Log($"[EncounterManager] '{encounterID}' 대신 '{fileNameOnly}' 파일을 로드했습니다.");
            }
        }
        else
        {
            string resourcePath = $"Encounters/{encounterID}"; 
            csvAsset = Resources.Load<TextAsset>(resourcePath);
            Debug.Log("success");
        }

        // 3. 여전히 파일이 없다면 에러 출력
        if (csvAsset == null)
        {
            Debug.LogError($"[EncounterManager] 파일을 찾을 수 없습니다. 경로를 확인하세요.\n" +
                           $"1차 시도: Resources/Encounters/{encounterID}\n" +
                           $"2차 시도(접두사 제거): Resources/Encounters/{(encounterID.Contains("_") ? encounterID.Substring(encounterID.IndexOf('_') + 1) : "해당 없음")}");
            return;
        }

        // 텍스트 내용 가져오기
        string csvText = csvAsset.text;
            
        if (!string.IsNullOrEmpty(meta.imagePath))
        {
            string imgName = Path.GetFileNameWithoutExtension(meta.imagePath);
            Sprite img = Resources.Load<Sprite>($"Encounters/Images/{imgName}");
            if (img == null) img = Resources.Load<Sprite>(imgName);
            if (img != null) illustrationImage.sprite = img;
        }

        if (titleText != null) titleText.text = meta.nameKO;
        
        // CSV 파싱 실행
        ParseEncounterCSV(csvText);
    }

    void ParseEncounterCSV(string csvText)
    {
        csvText = csvText.TrimStart('\uFEFF');
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
            Debug.Log($"id: {id}, content: {content}, nextID: {nextId}, functionCall: {functionCall},  condition: {condition}");
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
        Debug.Log($"[PlayStep] '{id}' 재생 시도..."); // 로그 추가
        if (!stepDictionary.ContainsKey(id)) return;
        
        currentStep = stepDictionary[id];
        if (actData != null) actData.currentStepID = id;
        if (currentStep.type == EncounterStepType.DESC)
        {

            descriptionText.text = currentStep.textContent.Replace("\\n", "\n");
            ResetScrollPosition(true);
        }

        if (IsValidFunction(currentStep.functionCall))
        {
            ParseAndExecuteFunctions(currentStep.functionCall);
        }
        
        if (actData.currentEncounterID == "" || SceneManager.GetActiveScene().name != "EncounterScene") return;
        UpdateOptionsUI();
    }

    void UpdateOptionsUI()
    {
        // 1. 기존 버튼들 및 컨테이너 초기화
        ResetChoiceContainers();

        // 2. 생성할 버튼 정보 수집
        List<TempOptionData> buttonsToCreate = new List<TempOptionData>();

        if (currentStep.options != null && currentStep.options.Count > 0)
        {
            foreach (var option in currentStep.options)
            {
                if (!CheckCondition(option.condition)) continue; 
                buttonsToCreate.Add(new TempOptionData(option.text, option.nextStepId, option.functionCall));
            }
        }
        else 
        {
            // 옵션이 없는 경우 (기본 버튼)
            if (IsWaitState(currentStep.nextStepId)) { /* 대기 상태면 버튼 없음 */ }
            else if (currentStep.nextStepId == "END") buttonsToCreate.Add(new TempOptionData("떠난다", "END", null));
            else buttonsToCreate.Add(new TempOptionData("다음", currentStep.nextStepId, null));
        }

        int count = buttonsToCreate.Count;

        // 3. 개수에 따른 컨테이너 활성화 및 버튼 생성
        if (count == 1)
        {
            oneChoiceContainer.SetActive(true);
            CreateButtonAt(buttonsToCreate[0], oneChoicePos);
        }
        else if (count == 2)
        {
            twoChoiceContainer.SetActive(true);
            for(int i=0; i<2; i++) CreateButtonAt(buttonsToCreate[i], twoChoicePos[i]);
        }
        else if (count >= 3)
        {
            threeChoiceContainer.SetActive(true);
            // 3개 이상일 경우 3개까지만 표시하거나, 3번 자리에 마지막꺼 배치 등 기획 필요. 여기선 앞에서부터 3개.
            int limit = Mathf.Min(count, 3);
            for(int i=0; i<limit; i++) CreateButtonAt(buttonsToCreate[i], threeChoicePos[i]);
        }
    }
    
    private class TempOptionData
    {
        public string text;
        public string nextId;
        public string func;
        public TempOptionData(string t, string n, string f) { text = t; nextId = n; func = f; }
    }
    void ResetChoiceContainers()
    {
        // 모든 컨테이너 비활성화 및 기존 생성된 버튼 삭제
        if (oneChoiceContainer != null)
        {
            ClearContainer(oneChoicePos);
            oneChoiceContainer.SetActive(false);
        }
        if (twoChoiceContainer != null)
        {
            foreach(Transform t in twoChoicePos) ClearContainer(t);
            twoChoiceContainer.SetActive(false);
        }
        if (threeChoiceContainer != null)
        {
            foreach(Transform t in threeChoicePos) ClearContainer(t);
            threeChoiceContainer.SetActive(false);
        }
    }

    void ClearContainer(Transform parent)
    {
        if (parent == null) return;
        foreach (Transform child in parent) Destroy(child.gameObject);
    }
    
    void CreateButtonAt(TempOptionData data, Transform targetParent)
    {
        if (targetParent == null) return;
        GameObject btnObj = Instantiate(choiceButtonPrefab, targetParent);
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();

        TextMeshProUGUI tmp = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null)
        {
            tmp.text = data.text;
            Vector2 textSize = tmp.GetPreferredValues(data.text);
            btnRect.sizeDelta = new Vector2(textSize.x, textSize.y);
        }
        
        Button btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(() => 
        {
            if (!isSceneLoading)
            {
                if (IsWaitState(data.nextId)) { } 
                else if (data.nextId == "END") EndEncounter();
                else PlayStep(data.nextId); 
            }
            if (IsValidFunction(data.func))
                ParseAndExecuteFunctions(data.func);
        });
    }

    bool IsWaitState(string id) => string.IsNullOrEmpty(id) || id == "-" || id == "R";
    bool IsValidFunction(string func) => !string.IsNullOrEmpty(func) && func != "-" && func != "DEFAULT";
    bool CheckCondition(string condition) => true; 
   
    public void EndEncounter()
    {
        if (actData != null)
        {
            actData.currentEncounterID = "";
            actData.currentStepID = "";
        }
        
        if (encounterPanel != null) encounterPanel.SetActive(false);
        
        PlayNextEncounterInQueue();
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
            case "StartRoullete":
                if (args.Length >= 2 && rouletteUI != null)
                {
                    int difficulty = int.Parse(args[1]);
                    string statName = args[0];
                    string winPage = (args.Length > 3) ? args[3] : "P_WIN";
                    string losePage = (args.Length > 4) ? args[4] : "P_LOSE";
                    roulettePanel.SetActive(true);
                    rouletteUI.Open(statName, difficulty, (result) =>
                    {
                        if (result == RouletteResultType.Success || result == RouletteResultType.GreatSuccess)
                        {
                            PlayStep(winPage);
                            Debug.Log("성공");
                        }
                        else
                        {
                            PlayStep(losePage);
                            Debug.Log("실패");
                        }

                        roulettePanel.SetActive(false);
                        encounterPanel.SetActive(true);

                    });
                }

                break;
            
            case "GetObjet":
                if (args.Length >= 1) descriptionText.text += $"\n<color=#77B0FF>오브제 {args[0]} 획득!</color>";
                break;

            case "GetDebris":
                if (args.Length >= 1) descriptionText.text += $"\n<color=#FFFF00>꿈의 파편 {args[0]}개 획득!</color>";
                break;

            case "UpStatus":
                Debug.Log("up "+ args.Length);
                if (args.Length >= 2 && playerStats != null && System.Enum.TryParse(args[0], true, out StatType sType))
                {
                    playerStats.ModifyStat(sType, int.Parse(args[1]));

                    if(descriptionText != null)
                    {
                        descriptionText.text += $"\n<color=#77B0FF>{sType} 증가!</color>";
                        ResetScrollPosition(false);
                        Canvas.ForceUpdateCanvases(); 
                    }
                    else
                    {
                        Debug.LogError("[UpStatus] descriptionText가 비어있습니다!");
                    }
                }
                break;
            
            case "DownStatus":
                if (args.Length >= 2 && playerStats != null && System.Enum.TryParse(args[0], true, out StatType sType1))
                {
                    playerStats.ModifyStat(sType1, -int.Parse(args[1]));

                    if(descriptionText != null)
                    {
                        descriptionText.text += $"\n<color=#FF0000>{sType1} 감소!</color>";
                        ResetScrollPosition(false);
                        Canvas.ForceUpdateCanvases(); 
                    }
                    else
                    {
                        Debug.LogError("[DownStatus] descriptionText가 비어있습니다!");
                    }
                }
                break;
            case "StartBattle": 
                if (args.Length >= 1)
                {
                    encounterPanel.SetActive(false);
                    if (characterData != null) characterData.enemyName = args[0];
                    SceneManager.LoadScene("BattleScene"); 
                }
                break;
            case "MeetMerchant":
            case "meetMerchant":
                if (merchantPanel != null)
                {
                    encounterPanel.SetActive(false);
                    merchantPanel.SetActive(true);
                    
                    string shopId = "";
                    if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
                    {
                        merchantUI.currentShopId = args[0]; 
                    }
                    
                    merchantUI.Open(merchantUI.currentShopId);
                }
                else Debug.Log("nulll");
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
                Debug.Log("deleteCard");
                sofaManager.SofaCardDelete();
                break;
        }
    }
    void ResetScrollPosition(bool toTop)
    {
        if (descriptionScrollRect != null)
        {
            Canvas.ForceUpdateCanvases(); 
            if (toTop)
                descriptionScrollRect.verticalNormalizedPosition = 1f; 
            else
                StartCoroutine(ScrollToBottomCoroutine()); // 맨 아래는 프레임 딜레이가 필요할 때가 많음
        }
    }

    IEnumerator ScrollToBottomCoroutine()
    {
        yield return new WaitForEndOfFrame(); // UI 렌더링 끝난 후
        if(descriptionScrollRect != null) 
            descriptionScrollRect.verticalNormalizedPosition = 0f;
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