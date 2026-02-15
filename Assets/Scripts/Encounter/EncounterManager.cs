using System;
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
#if UNITY_EDITOR
using UnityEditor.U2D;
#endif
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
    public PlayerDataSO playerDataSO;
    public PlayerStatsSo playerStats;
    public ActSO actData;
    public RelicSO playerRelicSO;
    public RelicDataSO relicDatabase;
    public DreamPieceSO dreamPieceDatabase;
    public ItemDataSO cardDatabase;
    public StageSO stageSO;
    
    [Header("Typewriter Settings")]
    public float typingSpeed = 0.03f; 
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private System.Action onTypingComplete;

    public EncSofaManager sofaManager;
    public CardUI cardGetUI;

    private Dictionary<string, EncounterMetaInfo> masterDatabase;
    private Dictionary<string, LocationMetaInfo> locationDatabase;
    
    private Dictionary<string, EncounterStep> stepDictionary = new Dictionary<string, EncounterStep>();
    private EncounterStep currentStep;
    private bool isSceneLoading = false;
    private Queue<string> encounterSequenceQueue = new Queue<string>();
    
    public bool isDebuging = true;
    public string debuggerID = "ACT1_Souvenir";
    
    private string currentLocID = "";
    private bool changeScene = false; 
    
    [Header("Get Card UI References")] 
    public GameObject cardGetPanel;    
    public Button cardGetConfirmBtn;

    public GameObject EndPanel;
    public Button endButton;
    private string pendingSystemMessage = "";

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
        if (endButton != null)
        {
            endButton.onClick.RemoveAllListeners();
            endButton.onClick.AddListener(QuitGame);
        }
        if (actData != null && actData.curNodeLocationID == "END")
        {
            Debug.Log("[EncounterManager] 엔딩 지점 도착.");
            
            if (encounterPanel != null) encounterPanel.SetActive(false);
            if (EndPanel != null) EndPanel.SetActive(true);

            SceneChangeManager.Inst.SceneFadeIn(() => { });
            return; 
        }
        
        ResetChoiceContainers();
        RestoreOrStartEncounter();
        SceneChangeManager.Inst.SceneFadeIn(() => {  });
    }
    
    void Update()
    {
        if (isTyping)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                SkipTyping();
            }
        }
    }
    public void QuitGame()
    {
        Debug.Log("Game Quit");
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
    void RestoreOrStartEncounter()
    {
        if (actData == null)
        {
            Debug.LogError("[EncounterManager] ActSO가 없습니다!");
            return;
        }
        Debug.Log(isDebuging);
        Debug.Log(actData.currentStepID);
        // 해당 인카운터 파일 로드 및 파싱
        if (isDebuging )
        {
            if (actData.currentStepID == "")
            {
                LoadEncounterData(debuggerID);
                encounterPanel.SetActive(true);
                PlayStep("P1"); 
            }
            else
            {
                LoadEncounterData(debuggerID);
                // 저장된 스텝으로 이동 (없으면 P1)
                string savedStep = string.IsNullOrEmpty(actData.currentStepID) ? "P1" : actData.currentStepID;
                encounterPanel.SetActive(true);
                PlayStep(savedStep);
            }

            return;
        }
        
        
        // 1. 진행 중이던 인카운터가 있는지 확인 (BattleScene 등에서 돌아온 경우)
        if (!string.IsNullOrEmpty(actData.currentEncounterID))
        {
            Debug.Log($"[EncounterManager] 중단된 인카운터 복구: {actData.currentEncounterID}, Step: {actData.currentStepID}");
            
            LoadEncounterData(actData.currentEncounterID);
            // 저장된 스텝으로 이동 (없으면 P1)
            string savedStep = string.IsNullOrEmpty(actData.currentStepID) ? "P1" : actData.currentStepID;
            encounterPanel.SetActive(true);
            PlayStep(savedStep);
        }
        else
        {
            if (isDebuging)
            {
                LoadEncounterData(debuggerID);
                encounterPanel.SetActive(true);
                PlayStep("P1");
            }
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
            SceneChangeManager.Inst.SceneFadeOut("MapScene");
        }
    }
    
    void LoadEncounterData(string encounterID)
    {
        if (!masterDatabase.ContainsKey(encounterID)) return;

        EncounterMetaInfo meta = masterDatabase[encounterID];
        TextAsset csvAsset = null;
        string resourcePath = $"Encounters/{meta.filePath}"; 
        Debug.Log(resourcePath);
        // 1차 시도: ID 그대로 로드
        csvAsset = Resources.Load<TextAsset>(resourcePath);

        // 2차 시도: 실패했고 ID에 '_'가 있다면 접두사 제거 후 재시도 (예: ACT1_Rabbit -> Rabbit)
        if (csvAsset == null && encounterID.Contains("_"))
        {
            string fileNameOnly = encounterID.Substring(encounterID.IndexOf('_') + 1);
            string alternativePath = $"Encounters/{fileNameOnly}";
            csvAsset = Resources.Load<TextAsset>(alternativePath);
            
            if (csvAsset != null)
            {
                Debug.Log($"[EncounterManager] '{encounterID}' 대신 '{fileNameOnly}' 파일을 로드했습니다.");
            }
        }

        if (csvAsset == null)
        {
             Debug.LogError($"[EncounterManager] CSV 파일을 찾을 수 없습니다.\n" +
                           $"경로 1: Resources/Encounters/{encounterID}\n" +
                           $"경로 2: (접두사 제외 시도함)");
            return;
        }

        meta.csvRawData = csvAsset.text;
        var info = masterDB.masterTable.Find(x => x.id == encounterID);
        
        info.csvRawData = csvAsset.text;
        Debug.Log(info.csvRawData);

        if (!string.IsNullOrEmpty(meta.imagePath)) 
        {
            string imgName = Path.GetFileNameWithoutExtension(meta.imagePath);
            Sprite img = Resources.Load<Sprite>($"Encounters/Images/{imgName}"); // 경로가 Encounters/Images 라고 가정
            if (img == null) img = Resources.Load<Sprite>(imgName); 
            if (img != null) illustrationImage.sprite = img;
        }
        // 제목 설정
        if (titleText != null) titleText.text = meta.nameKO;

        ParseEncounterCSV(meta.csvRawData);
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
            //Debug.Log($"id: {id}");
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
    string FormatDialogueColor(string content)
    {
        string pattern = @"([""“])(.*?)([""”])";
        string replacement = "$1<color=#77B0FF>$2</color>$3";

        return Regex.Replace(content, pattern, replacement);
    }
    public void PlayStep(string id)
    {
        Debug.Log($"playstep: {isSceneLoading}");
        if (isSceneLoading) return;
        if (!stepDictionary.ContainsKey(id)) return;
    
        currentStep = stepDictionary[id];
        if (actData != null) actData.currentStepID = id;
    
        ResetChoiceContainers();


        if (IsValidFunction(currentStep.functionCall))
        {
            ParseAndExecuteFunctions(currentStep.functionCall);
        }

        if (actData.currentEncounterID == "" || SceneManager.GetActiveScene().name != "EncounterScene") return;

        string finalContent = currentStep.textContent + pendingSystemMessage;

        if (currentStep.type == EncounterStepType.DESC || !string.IsNullOrEmpty(finalContent))
        {
            // currentStep.textContent 대신 finalContent를 사용
            StartTyping(finalContent, false, () => 
            {
                UpdateOptionsUI(); 
            });
        }   
        else
        {
            UpdateOptionsUI();
        }
        pendingSystemMessage = "";
    }
    public void StartTyping(string content, bool isAppend, System.Action onComplete = null)
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        
        // 현재 완료 콜백 저장 (SkipTyping에서 쓰기 위해)
        this.onTypingComplete = onComplete; 
        
        typingCoroutine = StartCoroutine(TypeWriterRoutine(content, isAppend));
    }
    IEnumerator TypeWriterRoutine(string content, bool isAppend)
    {
        Debug.Log("typing");
        isTyping = true;
        string formattedContent = FormatDialogueColor(content);
        string parsedContent = formattedContent.Replace("\\n", "\n");

        if (isAppend)
        {
            descriptionText.text += parsedContent;
        }
        else
        {
            descriptionText.text = parsedContent;
            descriptionText.maxVisibleCharacters = 0;
            
            // 초기화: 새 페이지면 맨 위로
            descriptionText.ForceMeshUpdate(); 
            if (descriptionScrollRect != null) 
            {
                Canvas.ForceUpdateCanvases();
                descriptionScrollRect.verticalNormalizedPosition = 1f; 
            }
        }

        // 텍스트 정보 갱신 (글자 좌표 계산을 위해 필수)
        descriptionText.ForceMeshUpdate();

        int totalVisibleCharacters = descriptionText.textInfo.characterCount;
        int counter = descriptionText.maxVisibleCharacters; 

        while (counter < totalVisibleCharacters)
        {
            int visibleCount = counter % (totalVisibleCharacters + 1);
            descriptionText.maxVisibleCharacters = visibleCount;

            if (counter > 0)
            {
                ScrollToChar(counter - 1);
            }

            counter++;
            yield return new WaitForSeconds(typingSpeed);
        }

        descriptionText.maxVisibleCharacters = totalVisibleCharacters;
        
        // 타이핑 끝난 후, 마지막 글자에 맞춰 확실하게 스크롤 조정
        ScrollToChar(totalVisibleCharacters - 1);
        
        isTyping = false;
        
        onTypingComplete?.Invoke();
        onTypingComplete = null;
    }
    void ScrollToChar(int charIndex)
    {
        if (descriptionScrollRect == null || descriptionText == null) return;
        if (charIndex >= descriptionText.textInfo.characterCount) return;

        TMP_CharacterInfo cInfo = descriptionText.textInfo.characterInfo[charIndex];
        float charBottomY = cInfo.bottomRight.y;

        float contentHeight = descriptionText.rectTransform.rect.height;
        float viewportHeight = descriptionScrollRect.viewport.rect.height;
        
        if (contentHeight <= viewportHeight) 
        {
            descriptionScrollRect.verticalNormalizedPosition = 1f;
            return;
        }

        float targetContentY = Mathf.Abs(charBottomY) - viewportHeight + 10f; // 10f는 하단 여백

        float maxContentY = contentHeight - viewportHeight;
        targetContentY = Mathf.Clamp(targetContentY, 0, maxContentY);

        float newNormalizedPos = 1 - (targetContentY / maxContentY);

        if (newNormalizedPos < descriptionScrollRect.verticalNormalizedPosition)
        {
            descriptionScrollRect.verticalNormalizedPosition = newNormalizedPos;
        }
    }

    public void SkipTyping()
    {
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);

            descriptionText.maxVisibleCharacters = descriptionText.textInfo.characterCount;
            //ScrollToChar(descriptionText.textInfo.characterCount - 1);

            isTyping = false;

            onTypingComplete?.Invoke();
            onTypingComplete = null;
        }
    }

    void UpdateOptionsUI()
    {
        ResetChoiceContainers();
        List<TempOptionData> buttonsToCreate = new List<TempOptionData>();

        if (currentStep.options != null && currentStep.options.Count > 0)
        {
            foreach (var option in currentStep.options)
            {
                // 1. 조건 만족 여부 확인
                bool isConditionMet = CheckCondition(option.condition);
                string finalButtonText = option.text;

                if (!isConditionMet)
                {
                    string condName = option.condition.Split('(')[0].Trim();
                    string argsRaw = "";
                    Match match = Regex.Match(option.condition, @"\(([^)]*)\)");
                    if (match.Success) argsRaw = match.Groups[1].Value.Trim();

                    if (condName == "NeedKey")
                    {
                        continue; 
                    }

                    if (condName == "HasObjet")
                    {
                        string endsWith = $"({argsRaw} 필요)";
                        if (!finalButtonText.Contains(endsWith))
                        {
                            finalButtonText += $"  {endsWith}";
                        }
                    }
                    else if (condName == "HasDreamPiece")
                    {
                        string endsWith2 = $"({argsRaw} 조각 필요)";
                        if (!finalButtonText.Contains(endsWith2))
                        {
                            finalButtonText += $"  {endsWith2}";
                        }
                    }
                    
                }

                buttonsToCreate.Add(new TempOptionData(finalButtonText, option.nextStepId, option.functionCall, isConditionMet));
            }
        }
        else 
        {
            if (IsWaitState(currentStep.nextStepId)) { }
            else if (currentStep.nextStepId == "END") buttonsToCreate.Add(new TempOptionData("떠난다", "END", null, true));
            else buttonsToCreate.Add(new TempOptionData("다음", currentStep.nextStepId, null, true));
        }

        int count = buttonsToCreate.Count;

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
            int limit = Mathf.Min(count, 3);
            for(int i=0; i<limit; i++) CreateButtonAt(buttonsToCreate[i], threeChoicePos[i]);
        }
    }
    
    private class TempOptionData
    {
        public string text;
        public string nextId;
        public string func;
        public bool isValid;
        public TempOptionData(string t, string n, string f, bool v) 
        { 
            text = t; 
            nextId = n; 
            func = f; 
            isValid = v; 
        }    }
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
        if (!data.isValid)
        {
            CanvasGroup cg = btnObj.GetComponent<CanvasGroup>();
            if (cg == null) cg = btnObj.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false; 
            cg.interactable = false;

            cg.alpha = 0.4f; 
            btn.interactable = false;
            return; 
        }
        
        btn.onClick.AddListener(() => 
        {
            if (IsValidFunction(data.func))
            {
                ParseAndExecuteFunctions(data.func);
            }

            if (!isSceneLoading)
            {
                if (IsWaitState(data.nextId)) { } 
                else if (data.nextId == "END") EndEncounter();
                else PlayStep(data.nextId); 
            }
            else
            {
                if (actData != null && !string.IsNullOrEmpty(data.nextId) && data.nextId != "END")
                {
                    actData.currentStepID = data.nextId;
                    Debug.Log($"[EncounterManager] 씬 이동으로 인한 스텝 저장: {data.nextId}");
                }
            }
        });
    }

    bool IsWaitState(string id) => string.IsNullOrEmpty(id) || id == "-" || id == "R";
    bool IsValidFunction(string func) => !string.IsNullOrEmpty(func) && func != "-" && func != "DEFAULT";
    
    bool CheckCondition(string condition)
    {
        if (string.IsNullOrEmpty(condition) || condition == "DEFAULT" || condition == "-") return true;

        string condName = condition.Split('(')[0].Trim();
        string argsRaw = "";
        Match match = Regex.Match(condition, @"\(([^)]*)\)");
        if (match.Success) argsRaw = match.Groups[1].Value.Trim();
        Debug.Log(condName);
        switch (condName)
        {
            case "NeedKey":
                if (characterData != null && playerDataSO.earnedKeys != null)
                {
                    Debug.Log("contain: "+playerDataSO.earnedKeys.Contains(argsRaw));
                    return playerDataSO.earnedKeys.Contains(argsRaw);
                }
                return false;

            case "HasObjet":
                if (playerRelicSO != null && playerRelicSO.relicItems != null)
                {
                    bool hasRelic = playerRelicSO.relicItems.Exists(item => item.relicName == argsRaw);
                    
                    // Debug.Log($"[CheckCondition] HasObjet({argsRaw}) ? {hasRelic}");
                    
                    return hasRelic;
                }
                return false;
            case "HasDreamPiece":
                if (characterData != null && characterData.personaPiece != null)
                {
                    return characterData.personaPiece.name == argsRaw;
                }
                return false;
            case "HasDreamCoin":
                int neededCoins = int.Parse(argsRaw);
                if (characterData!= null && characterData.dreamDust >= neededCoins) return true;
                return false;
        }

        return true;
    }
   
    public void EndEncounter()
    {
        if (actData != null)
        {
            actData.currentEncounterID = "";
            actData.currentStepID = "";
        }
        Debug.Log(actData.encounterQueue.Count);
        if (actData.encounterQueue.Count > 0) PlayNextEncounterInQueue();
        else SceneChangeManager.Inst.SceneFadeOut("MapScene");
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
                if (args.Length >= 5 && rouletteUI != null)
                {
                    int difficulty = int.Parse(args[1]);
                    string statName = args[0];
                    string success = args[3];
                    string greatSuccess = args[4];
                    string fail = args[5];
                    roulettePanel.SetActive(true);
                    rouletteUI.Open(statName, difficulty, (result) =>
                    {
                        switch (result)
                        {
                            case RouletteResultType.Success:
                                PlayStep(success);
                                break;
                            case RouletteResultType.Fail:
                                PlayStep(fail);
                                break;
                            case RouletteResultType.GreatSuccess:
                                PlayStep(greatSuccess);
                                break;
                        }
                        roulettePanel.SetActive(false);
                        encounterPanel.SetActive(true);

                    });
                }

                break;
            
            
            case "GetObjet":
                if (args.Length >= 1)
                {
                    string objectName = args[0];
                    if (relicDatabase == null || playerRelicSO == null)
                    {
                        Debug.LogError("[EncounterManager] RelicDatabase 또는 PlayerRelicSO가 Inspector에 연결되지 않았습니다!");
                        return;
                    }
                    RelicItem_Data foundData = relicDatabase.relicItems.Find(x => x.relicName == objectName);

                    if (foundData != null)
                    {
                        bool hasRelic = playerRelicSO.relicItems.Exists(r => r.relicOwner == foundData.relicOwner);
                        if (!hasRelic)
                        {
                            RelicItem_Enhanceable newRelic = new RelicItem_Enhanceable(foundData);
                            playerRelicSO.relicItems.Add(newRelic);
                            string msg = $"\n<color=#5df86f>오브제 '{foundData.relicName}' 획득!</color>";
                            pendingSystemMessage += msg; 
                            Debug.Log($"[Encounter] 오브제 획득 성공: {foundData.relicName}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[Encounter] DB에서 오브제를 찾을 수 없습니다: {objectName}. 이름을 확인해주세요.");
                    }
                
                }
                break;

            case "GetDebris":
                if (args.Length >= 1)
                {
                    int amount = int.Parse(args[0]);
                    string msg = "";
                    if (amount > 0) msg = $"\n<color=#5df86f>드림 코인 {amount}개 획득!</color>";
                    else msg = $"\n<color=#FF0000>드림 코인 {-amount}개 지불!</color>";
                    pendingSystemMessage += msg;
                    characterData.dreamDust += int.Parse(args[0]);
                }
                break;

            case "UpStatus":
                if (args.Length >= 2 && playerStats != null && System.Enum.TryParse(args[0], true, out StatType sType))
                {
                    int amount = int.Parse(args[1]);
                    playerStats.ModifyStat(sType, amount);
                    string statName = "";
                    switch (sType)
                    {
                        case StatType.Courage:
                            statName = "용기";
                            break;
                        case StatType.Luck:
                            statName = "운";
                            break;
                        case StatType.Wisdom:
                            statName = "지혜";
                            break;
                    }
                    string msg = $"\n<color=#5df86f>'{statName}' {amount}증가!</color>";
                    pendingSystemMessage += msg; 
                }
                break;
            
            case "DownStatus":
                if (args.Length >= 2 && playerStats != null && System.Enum.TryParse(args[0], true, out StatType sType1))
                {
                    int amount = int.Parse(args[1]);
                    playerStats.ModifyStat(sType1, -amount);
                    string statName = "";
                    switch (sType1)
                    {
                        case StatType.Courage:
                            statName = "용기";
                            break;
                        case StatType.Luck:
                            statName = "운";
                            break;
                        case StatType.Wisdom:
                            statName = "지혜";
                            break;
                    }
                    string msg = $"\n<color=#FF0000>'{statName}' {amount} 감소!</color>";
                    pendingSystemMessage += msg;
                }
                break;
            case "StartBattle": 
                if (args.Length >= 1)
                {
                    //encounterPanel.SetActive(false);
                    isSceneLoading = true;
                    Debug.Log("Start Battle");
                    if (characterData != null) characterData.enemyName = args[0];
                    SceneChangeManager.Inst.SceneFadeOut("BattleScene");

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
            
                    string msg = $"\n<color=#5df86f>체력 {heal} 회복!</color>";
                    pendingSystemMessage += msg;
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

                    string msg = $"\n<color=#FF0000>체력 {dmg} 잃음!</color>";
                    pendingSystemMessage += msg;
                }
                break;
            
            case "DeleteCard":
                Debug.Log("deleteCard");
                sofaManager.SofaCardDelete();
                break;
            case "GetKey": 
                if (args.Length >= 1 && characterData != null)
                {
                    string keyToAdd = args[0];
                    if (playerDataSO.earnedKeys == null) playerDataSO.earnedKeys = new List<string>();
                    
                    if (!playerDataSO.earnedKeys.Contains(keyToAdd))
                    {
                        playerDataSO.earnedKeys.Add(keyToAdd);
                        Debug.Log($"[Encounter] Key 획득: {keyToAdd}");
                    }
                }
                break;
            case "IllustChange": 
                if (args.Length >= 1)
                {
                    string path = args[0];
                    Sprite newImg = Resources.Load<Sprite>($"Encounters/Images/{path}"); 
                    if (newImg == null) newImg = Resources.Load<Sprite>(path);

                    if (newImg != null) illustrationImage.sprite = newImg;
                    else Debug.LogError($"[IllustChange] 이미지를 찾을 수 없음: {path}");
                }
                break;
            case "GetCard": 
                if (args.Length >= 1)
                {
                    string cardName = args[0];
                    OpenGetCardUI(cardName);
                }
                break;
            case "SetDreamPiece": 
                if (args.Length >= 1)
                {
                    string targetName = args[0];
                    if (dreamPieceDatabase == null || characterData == null)
                    {
                        Debug.LogError("[Encounter] DreamPieceDatabase 또는 CharacterData가 연결되지 않았습니다.");
                        return;
                    }

                    DreamPiece_Reference foundRef = dreamPieceDatabase.dreamPieces.Find(x => x.name == targetName);

                    if (foundRef != null)
                    {
                        DreamPiece_Player newPiece = new DreamPiece_Player(foundRef);
                        characterData.personaPiece = newPiece;
                        playerDataSO.personaPiece = targetName;

                        // [수정됨] 직접 텍스트 수정 -> pendingSystemMessage 사용
                        string msg = $"\n<color=#D4AF37>꿈 조각 '{targetName}'(으)로 교체!</color>";
                        pendingSystemMessage += msg;
                    }
                    else
                    {
                        Debug.LogError($"꿈조각 찾기 실패: {targetName}");
                    }
                } 
                break;
            case "LoseObjet":
                if (args.Length >= 1)
                {
                    string objectName = args[0];
                    if (relicDatabase == null || playerRelicSO == null)
                    {
                        Debug.LogError("[EncounterManager] RelicDatabase 또는 PlayerRelicSO가 Inspector에 연결되지 않았습니다!");
                        return;
                    }
                    RelicItem_Data foundData = relicDatabase.relicItems.Find(x => x.relicName == objectName);

                    if (foundData != null)
                    {
                        bool hasRelic = playerRelicSO.relicItems.Exists(r => r.relicOwner == foundData.relicOwner);
                        if (hasRelic)
                        {
                            RelicItem_Enhanceable newRelic = new RelicItem_Enhanceable(foundData);
                            playerRelicSO.relicItems.Remove(newRelic);
                            string msg = $"\n<color=#5df86f>오브제 '{foundData.relicName}' 파괴!</color>";
                            pendingSystemMessage += msg; 
                            Debug.Log($"[Encounter] 오브제 획득 성공: {foundData.relicName}");
                        }
                    }
                    else
                    {
                        Debug.LogError($"[Encounter] DB에서 오브제를 찾을 수 없습니다: {objectName}. 이름을 확인해주세요.");
                    }
                
                }

                break;
            case "GetRandomObjet":
                if (args.Length >= 1)
                {
                    int amount = int.Parse(args[0]);
                    if (stageSO != null && relicDatabase != null && playerRelicSO != null)
                    {
                        int currentAct = playerDataSO.currentActNum; 

                        List<RelicItem_Data> candidates = relicDatabase.relicItems.Where(obj => 
                        {
                            bool hasRelic = playerRelicSO.relicItems.Exists(owned => owned.relicOwner == obj.relicOwner);
                            if (hasRelic) return false;

                            return obj.rarity != CardRarity.Normal && (obj.relicAct < currentAct);

                        }).ToList();
                        for (int i = 0; i < amount; i++)
                        {
                            if (candidates.Count == 0)
                            {
                                Debug.Log("GetRandomObjet: 더 이상 획득할 수 있는 오브제가 없습니다.");
                                break;
                            }
                            int randomIndex = Random.Range(0, candidates.Count);
                            RelicItem_Data itemToAdd = candidates[randomIndex];
                            RelicItem_Enhanceable newRelic = new RelicItem_Enhanceable(itemToAdd);
                            playerRelicSO.relicItems.Add(newRelic);
                            candidates.RemoveAt(randomIndex);
                            
                            string msg = $"\n<color=#5df86f>오브제 '{itemToAdd.relicName}' 획득!</color>";
                            pendingSystemMessage += msg; 
                            Debug.Log($"랜덤 오브제 획득: {itemToAdd.relicName}");
                        }
                    }
                }
                break;
        }
    }
    void OpenGetCardUI(string cardName)
    {
        if (cardGetPanel == null) return;

        if (cardDatabase == null || characterData == null)
        {
            Debug.LogError("[Encounter] CardDatabase(ItemDataSO) 또는 CharacterData가 연결되지 않았습니다.");
            return;
        }
        if (cardGetUI == null)
        {
            Debug.LogError("[Encounter] Inspector에서 'Card Get UI'에 CardUI 컴포넌트를 연결해주세요.");
            return;
        }

        Item_Data foundData = cardDatabase.items.Find(x => x.name == cardName);

        if (foundData != null)
        {
            // 3. UI 활성화
            encounterPanel.SetActive(false); 
            cardGetPanel.SetActive(true);
            Item newCard = new Item(foundData, false); 
            newCard.num = 1;

            cardGetUI.Setup(newCard);
            var existItem = characterData.personaPiece.cards.Find(x => x.name == newCard.name);
            if (existItem == null)
            {
                characterData.personaPiece.cards.Add(newCard);
                Debug.Log($"[GetCard] 신규 카드 획득: {cardName}");
            }
            else
            {
                existItem.num++;
                Debug.Log($"[GetCard] 카드 중복 획득 (개수 증가): {cardName}");
            }
        }
        else
        {
            Debug.LogError($"[Encounter] CardDB에서 카드를 찾을 수 없습니다: {cardName}. (이름 확인 필요)");
            return; 
        }
    }

    public void OnCardGetConfirmed()
    {
        if (cardGetPanel != null) cardGetPanel.SetActive(false);
        encounterPanel.SetActive(true); 

        if (currentStep != null)
        {
            string nextId = currentStep.nextStepId;
            if (nextId == "END") EndEncounter();
            else if (!string.IsNullOrEmpty(nextId) && nextId != "-" && nextId != "R") PlayStep(nextId);
            // else PlayStep(currentStep.id); 
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
                StartCoroutine(ScrollToBottomCoroutine()); 
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