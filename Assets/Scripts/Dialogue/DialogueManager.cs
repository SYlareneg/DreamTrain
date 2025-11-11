using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using HallControll.SO;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    public TextAsset dialogueCSV;
    public List<DialogueEntry> dialogueList = new List<DialogueEntry>();
    public DialogueUI dialogueUI;

    public List<DialogueBundle> dialogueBundles = new List<DialogueBundle>();

    public GameObject dialoguePanel;
    public Button[] dialogueButtons;
    public GameObject dialogueBundle;
    public GameObject rerollButton;
    public GameObject background_default; 
    
    private int rerollCost = 1;
    private bool isRerollActive = true;
    private DreamDustManager dreamDustManager;
    
    private HashSet<int> completedDialogueIDs = new HashSet<int>();
    private InteractableObject currentInteractableObject;
    private InteractableObjectData itemToCollectAfterDialogue;

    private int idNumber=0;
    
    public enum DialogueMode
    {
        Opening,
        Main
    }

    private DialogueMode currentMode = DialogueMode.Main;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (dialogueCSV != null)
        {
            ParseCSV(dialogueCSV);
        }
    }

    // CSV 로드 
    public void LoadDialogueCSV(string fileName, string characterName)
    {
        TextAsset csvFile = Resources.Load<TextAsset>($"Dialogues/{characterName}/{fileName}");
        if (csvFile == null)
        {
            Debug.LogError($"Dialogue CSV not found: Resources/Dialogues/{characterName}/{fileName}.csv");
            return;
        }

        dialogueCSV = csvFile;
        ParseCSV(dialogueCSV);
        Debug.Log($"[DialogueManager] Loaded dialogue file: {fileName}");
    }

    // CSV 파싱
    void ParseCSV(TextAsset csvAsset)
    {
        dialogueList.Clear();
        idNumber = 0;
        string[] lines = csvAsset.text.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        if (lines.Length <= 1)
        {
            Debug.LogError("CSV file is empty or has only a header row.");
            return;
        }

        int lastParsedId = -1;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line) || string.IsNullOrWhiteSpace(line.Replace(",", "")))
                continue;

            string[] cells = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

            if (cells.Length < 10)
            {
                Debug.LogWarning($"Skipping line {i + 1}: {line}");
                continue;
            }

            try
            {
                DialogueEntry entry = new DialogueEntry();

                if (!string.IsNullOrEmpty(cells[0]))
                {
                    entry.ID = int.Parse(cells[0]);
                    lastParsedId = entry.ID;
                }
                else
                {
                    entry.ID = lastParsedId;
                }

                entry.BoxLocation = cells[1].Trim();
                entry.Dialogue_KO = cells[2].Trim().Trim('"');
                entry.Dialogue_EN = cells[3].Trim().Trim('"');
                entry.Type = cells[4].Trim();
                entry.SFX = cells[5].Trim();
                entry.IdToGet = string.IsNullOrEmpty(cells[6]) ? 0 : int.Parse(cells[6]);
                entry.IdPoint = string.IsNullOrEmpty(cells[7]) ? 0 : int.Parse(cells[7]);
                entry.NextID = string.IsNullOrEmpty(cells[8]) ? 0 : int.Parse(cells[8]);
                entry.Function = cells[9].Trim();

                dialogueList.Add(entry);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error parsing line {i + 1}: {e.Message}");
            }
        }

    }
    public void OnDialogueEnded()
    {
        // 아이템 처리
        if (itemToCollectAfterDialogue != null && currentInteractableObject != null)
        {
            if (itemToCollectAfterDialogue.itemIcon != null)
            {
                bool success = InventoryManager.Instance.CollectItem(itemToCollectAfterDialogue);
                if (success)
                {
                    currentInteractableObject.OnCollectionComplete();
                }
            }
            itemToCollectAfterDialogue = null;
            currentInteractableObject = null;
        }
        dialoguePanel.SetActive(false);
        if (currentMode == DialogueMode.Opening)
        {
            ShowDialogueSelectionPanel();
        }
        else if (currentMode == DialogueMode.Main)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
        }
    }

    // 메인 대화 선택 패널 표시
    public void ShowDialogueSelectionPanel()
    {
        dialoguePanel.SetActive(false);
        dialogueBundle.SetActive(true);
        
        // 사용 가능한 번들 중 banned=false 인 것만 필터링
        List<DialogueBundle> available = dialogueBundles.FindAll(b => !b.isBanned);

        // 3개 랜덤 선택
        List<DialogueBundle> selected = new List<DialogueBundle>();
        while (selected.Count < 3 && available.Count > 0)
        {
            int idx = Random.Range(0, available.Count);
            selected.Add(available[idx]);
            available.RemoveAt(idx);
        }

        // 버튼 세팅
        for (int i = 0; i < dialogueButtons.Length; i++)
        {
            if (i < selected.Count)
            {
                DialogueBundle bundle = selected[i];
                dialogueButtons[i].gameObject.SetActive(true);
                rerollButton.gameObject.SetActive(true);
                dialogueButtons[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text = bundle.bundleName;

                // 클릭 시 해당 대화 시작
                dialogueButtons[i].onClick.RemoveAllListeners();
                dialogueButtons[i].onClick.AddListener(() =>
                {
                    dialoguePanel.SetActive(true);
                    StartDialogueByBundle(bundle);
                });
            }
            else
            {
                dialogueButtons[i].gameObject.SetActive(false);
                rerollButton.gameObject.SetActive(false);
            }
        }
        
    }
    private void StartDialogueByBundle(HallControll.SO.DialogueBundle selected)
    {
        
        Debug.Log($"[BundleManager] Selected dialogue: {selected.bundleName} (FileID: {selected.connectedFileID})");
        dialogueBundle.SetActive(false);
        rerollButton.SetActive(false);

        // DialogueData.csv 로드
        TextAsset csvData = Resources.Load<TextAsset>("Dialogues/DialogueData");
        if (csvData == null)
        {
            Debug.LogError("[BundleManager] Failed to load DialogueData.csv from Resources/Dialogues/");
            return;
        }

        // connectedFileID에 맞는 Character와 FileName 찾기
        (string character, string fileName) = FindCharacterAndFileName(csvData.text, selected.connectedFileID.ToString());

        if (!string.IsNullOrEmpty(fileName))
        {
            Debug.Log($"[BundleManager] Found -> Character: {character}, FileName: {fileName}");
            DialogueManager.Instance.StartDialogue(DialogueMode.Main, fileName, character);
        }
        else
        {
            Debug.LogError($"[BundleManager] FileID {selected.connectedFileID} not found in DialogueData.csv!");
        }
    }

    private (string character, string fileName) FindCharacterAndFileName(string csvText, string targetID)
    {
        try
        {
            string[] lines = csvText.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);

            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                string[] parts = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                if (parts.Length < 3) continue;

                string id = parts[0].Trim().Trim('"');
                string character = parts[1].Trim().Trim('"');
                string fileName = parts[2].Trim().Trim('"');

                if (id == targetID)
                {
                    if (fileName.EndsWith(".csv"))
                        fileName = fileName.Substring(0, fileName.Length - 4);

                    return (character, fileName);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[BundleManager] Error parsing DialogueData.csv: {ex.Message}");
        }

        return (null, null);
    }




    public void MarkDialogueAsCompleted(int dialogueId)
    {
        if (!completedDialogueIDs.Contains(dialogueId))
        {
            completedDialogueIDs.Add(dialogueId);
            Debug.Log($"Dialogue {dialogueId} is marked as completed.");
        }
    }

    public List<DialogueEntry> GetDialogueOptionsByID(int id)
    {
        return dialogueList.FindAll(d => d.ID == id);
    }
    
    public void StartDialogue(DialogueMode mode, string fileName, string character)
    {
        currentMode = mode;
        LoadDialogueCSV(fileName, character);
        DialogueUI.Instance.ShowDialogue(1);
    }
    
    public void OnRerollRequested()
    {
        ShowDialogueSelectionPanel();
    }
    
    
}
