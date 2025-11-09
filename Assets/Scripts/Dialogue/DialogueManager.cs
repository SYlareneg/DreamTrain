using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using HallControll.SO;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Dialogue Settings")]
    public TextAsset dialogueCSV;
    public List<DialogueEntry> dialogueList = new List<DialogueEntry>();
    public DialogueUI dialogueUI;

    public List<DialogueBundle> dialogueBundles = new List<DialogueBundle>();

    [Header("Dialogue Panel 관련")]
    public GameObject dialoguePanel;
    public Button[] dialogueButtons;
    public GameObject dialogueBundle;
    

    private HashSet<int> completedDialogueIDs = new HashSet<int>();
    private InteractableObject currentInteractableObject;
    private InteractableObjectData itemToCollectAfterDialogue;

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

        Debug.Log($"[DialogueManager] Parsed {dialogueList.Count} entries from CSV.");
    }

    // 대화 종료 시 처리
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
        // 메인 대화 선택 패널 띄우기
        ShowDialogueSelectionPanel();
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
            }
        }
    }

    // 선택된 번들의 connectedFileID로 대화 시작
    void StartDialogueByBundle(DialogueBundle bundle)
    {
        string characterName = "Vampire";
        string fileName = $"CupOP_{bundle.connectedFileID}"; 

        LoadDialogueCSV(fileName, characterName);
        dialogueUI.ShowDialogue(1);
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
}
