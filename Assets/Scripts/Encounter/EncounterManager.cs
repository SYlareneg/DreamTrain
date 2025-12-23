using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions; // 정규식 사용을 위해 추가

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

    [Header("Data Source")]
    // 엑셀에서 뽑은 CSV 파일을 여기에 넣으세요
    public TextAsset encounterCsvFile; 
    
    // 파싱된 데이터를 담을 딕셔너리 (ID -> Step)
    private Dictionary<string, EncounterStep> stepDictionary = new Dictionary<string, EncounterStep>();
    private EncounterStep currentStep;

    void Awake()
    {
        Instance = this;
        encounterPanel.SetActive(true);
        StartEncounterFromCSV();
    }

    // CSV 파일을 읽어서 인카운터 시작
    public void StartEncounterFromCSV()
    {
        if (encounterCsvFile == null) 
        {
            Debug.LogError("CSV 파일이 연결되지 않았습니다!");
            return;
        }

        ParseCSV(encounterCsvFile.text);
        encounterPanel.SetActive(true);
        
        // CSV의 첫 번째 ID인 'P1'부터 시작 (규칙에 따라 변경 가능)
        if (stepDictionary.ContainsKey("P1"))
            PlayStep("P1");
        else
            Debug.LogError("CSV에 'P1' ID가 없습니다.");
    }

    void PlayStep(string id)
    {
        if (!stepDictionary.ContainsKey(id)) return;

        currentStep = stepDictionary[id];

        // 1. 텍스트 출력
        // BRANCH 타입은 텍스트가 선택지 내용이므로, DESC 타입일 때만 본문 갱신
        if (currentStep.type == EncounterStepType.DESC)
        {
            descriptionText.text = currentStep.textContent;
        }

        // 2. 함수 실행 (예: StartBattle(horse), Courage.add(1))
        if (!string.IsNullOrEmpty(currentStep.functionCall) && currentStep.functionCall != "-")
        {
            ParseAndExecuteFunction(currentStep.functionCall);
        }

        // 3. UI 업데이트 (선택지 표시 등)
        UpdateOptionsUI();
    }

    void UpdateOptionsUI()
    {
        // 기존 버튼 삭제
        foreach (Transform child in choiceContainer) Destroy(child.gameObject);

        // ★ 핵심 수정: 타입(Type)보다 '선택지 리스트(options)'가 있는지 먼저 검사합니다.
        // P1처럼 DESC로 시작했지만 뒤에 BRANCH가 붙어서 선택지가 생긴 경우를 처리하기 위함입니다.
        if (currentStep.options != null && currentStep.options.Count > 0)
        {
            // 선택지가 존재하면 무조건 선택지 버튼들을 생성
            foreach (var option in currentStep.options)
            {
                CreateButton(option.text, option.nextStepId, option.functionCall);
            }
        }
        // 선택지가 없는 순수 DESC 타입인 경우
        else 
        {
            // 다음 페이지가 있으면 '다음', 없으면 '떠난다'
            if (!string.IsNullOrEmpty(currentStep.nextStepId) && currentStep.nextStepId != "-" && currentStep.nextStepId != "")
            {
                CreateButton("다음", currentStep.nextStepId);
            }
            else
            {
                CreateButton("떠난다", "END");
            }
        }
    }

    void CreateButton(string text, string nextId, string functionCall = null)
    {
        GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);
        btnObj.GetComponentInChildren<TextMeshProUGUI>().text = text;
        btnObj.GetComponent<Button>().onClick.AddListener(() => 
        {
            // 선택지 클릭 시 함수가 있다면 실행
            if (!string.IsNullOrEmpty(functionCall) && functionCall != "-")
                ParseAndExecuteFunction(functionCall);

            if (nextId == "END") EndEncounter();
            else PlayStep(nextId);
        });
    }

    public void EndEncounter()
    {
        encounterPanel.SetActive(false);
        MapManager.Inst.player_moveable = true;
    }

    // ★ CSV 파서 (핵심)
    // ★ 안전한 CSV 파서 (에러 방지 버전)
    void ParseCSV(string csvText)
    {
        stepDictionary.Clear();
        string[] lines = Regex.Split(csvText, @"\r\n|\n(?=(?:[^""]*""[^""]*"")*[^""]*$)");

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            // 쉼표 분리 (따옴표 안의 쉼표 무시)
            string[] row = Regex.Split(lines[i], ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            
            // 데이터가 충분하지 않거나, 내용이 빈 줄(,,,,)인 경우 건너뛰기
            if (row.Length < 5 || string.IsNullOrWhiteSpace(row[0]) || string.IsNullOrWhiteSpace(row[1])) 
                continue;

            // 따옴표 제거
            for (int k = 0; k < row.Length; k++) row[k] = row[k].Trim().Replace("\"", "");

            string id = row[0];
            string typeStr = row[1];
            string content = row[2];
            string nextId = row[3];
            string func = row[4];

            // ★ 핵심 수정: Enum 변환을 안전하게 시도 (실패 시 에러 대신 로그 띄우고 넘어가기)
            if (!System.Enum.TryParse(typeStr, true, out EncounterStepType type))
            {
                Debug.LogWarning($"[CSV 파싱 경고] {i+1}번째 줄의 타입 '{typeStr}'이(가) 잘못되었습니다. (DESC, BRANCH 등이어야 함) - 행 무시됨");
                continue;
            }

            // --- 기존 로직 ---
            if (stepDictionary.ContainsKey(id))
            {
                if (type == EncounterStepType.BRANCH)
                {
                    stepDictionary[id].options.Add(new EncounterOption 
                    { 
                        text = content, 
                        nextStepId = nextId, 
                        functionCall = func 
                    });
                }
            }
            else
            {
                EncounterStep newStep = new EncounterStep
                {
                    id = id,
                    type = type,
                    textContent = content,
                    nextStepId = nextId,
                    functionCall = func,
                    options = new List<EncounterOption>()
                };

                if (type == EncounterStepType.BRANCH)
                {
                    newStep.options.Add(new EncounterOption 
                    { 
                        text = content, 
                        nextStepId = nextId, 
                        functionCall = func 
                    });
                }

                stepDictionary.Add(id, newStep);
            }
            Debug.Log(id + ", " + typeStr + ", " + content +  ", " + nextId + ", " + func);
        }
    }

    // ★ 함수 파서 (CSV 문법 대응: Courage.add(1), StartBattle(horse))
    void ParseAndExecuteFunction(string command)
    {
        command = command.Trim();
        
        // 1. "대상.명령(값)" 형태 (예: Courage.add(1))
        if (command.Contains("."))
        {
            string[] parts = command.Split('.');
            string target = parts[0]; // Courage
            string actionPart = parts[1]; // add(1)

            if (actionPart.Contains("add"))
            {
                string valueStr = Regex.Match(actionPart, @"\(([^)]*)\)").Groups[1].Value;
                int value = int.Parse(valueStr);

                // 스탯 증가 처리
                // StatType type = (StatType)System.Enum.Parse(typeof(StatType), target);
                // PlayerStatsSO.Instance.ModifyStat(type, value);
                
                Debug.Log($"[함수실행] {target} 스탯 {value} 증가");
                descriptionText.text += $"\n<color=blue>{target}가 {value} 증가했다!</color>";
            }
        }
        // 2. "명령(값)" 형태 (예: StartBattle(horse))
        else if (command.Contains("("))
        {
            string funcName = command.Split('(')[0];
            string arg = Regex.Match(command, @"\(([^)]*)\)").Groups[1].Value;

            if (funcName == "StartBattle")
            {
                Debug.Log($"[함수실행] 전투 시작! 적: {arg}");
                // BattleManager.Instance.StartBattle(arg);
            }
        }
    }
}