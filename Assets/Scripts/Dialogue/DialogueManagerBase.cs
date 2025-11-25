using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using HallControll.SO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public abstract class DialogueManagerBase : MonoBehaviour
{
    public static DialogueManagerBase Instance { get; protected set; }

    [Header("Common References")]
    public GameObject dialoguePanel; 
    public TextAsset dialogueDataCSV; 

    [Header("Data")]
    public List<DialogueBundle> dialogueBundles = new List<DialogueBundle>();
    public List<DialogueEntry> dialogueList = new List<DialogueEntry>();
    public enum DialogueMode { Opening, Main }
    protected DialogueMode currentMode = DialogueMode.Main;

    protected InteractableObject currentInteractableObject;
    protected InteractableObjectData itemToCollectAfterDialogue;

    protected virtual void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    public abstract void ShowDialogueSelectionPanel();
    public virtual void ShowEmotionSelection(System.Action<FeelingType> callback) { }
    public void StartDialogue(DialogueMode mode, string fileName, string characterName)
    {
        currentMode = mode;
        LoadDialogueCSV(fileName, characterName);

        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        
        DialogueUI.Instance.ShowDialogue(1);
    }

    public void LoadDialogueCSV(string fileName, string characterName)
    {
        TextAsset csvFile = Resources.Load<TextAsset>($"Dialogues/{characterName}/{fileName}");
        if (csvFile == null)
        {
            Debug.LogError($"Dialogue CSV not found: Resources/Dialogues/{characterName}/{fileName}.csv");
            return;
        }
        ParseCSV(csvFile);
    }

    protected void ParseCSV(TextAsset csvAsset)
    {
        dialogueList.Clear();
        string[] lines = csvAsset.text.Split(new string[] { "\r\n", "\n" }, System.StringSplitOptions.None);
        
        int lastParsedId = -1;

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] cells = Regex.Split(line, ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
            if (cells.Length < 10) continue;

            try
            {
                DialogueEntry entry = new DialogueEntry();
                
                if (!string.IsNullOrEmpty(cells[0])) { entry.ID = int.Parse(cells[0]); lastParsedId = entry.ID; }
                else entry.ID = lastParsedId;

                entry.BoxLocation = cells[1].Trim();
                entry.Dialogue_KO = cells[2].Trim().Trim('"');
                entry.Dialogue_EN = cells[3].Trim().Trim('"');
                entry.Type = cells[4].Trim();
                entry.SFX = cells[5].Trim();
                entry.IdToGet = string.IsNullOrEmpty(cells[6]) ? 0 : int.Parse(cells[6]);
                entry.IdPoint = string.IsNullOrEmpty(cells[7]) ? 0 : int.Parse(cells[7]);
                entry.NextID = string.IsNullOrEmpty(cells[8]) ? 0 : int.Parse(cells[8]);
                entry.Function = cells[9].Trim();
                if (cells.Length > 10)
                {
                    string feelingStr = cells[10].Trim().Trim('"');
                    if (!string.IsNullOrEmpty(feelingStr))
                    {
                        if (System.Enum.TryParse(feelingStr, true, out FeelingType fType))
                        {
                            entry.feelingType = fType;
                            Debug.Log(entry.feelingType.ToString());
                        }
                        else
                        {
                            Debug.LogError($"[CSV Error] Invalid FeelingType '{feelingStr}' at Line {i + 1}. Check spelling (joy, sad, etc).");
                        }
                    }
                    else if (entry.Type.Equals("Feeling", System.StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogError($"[CSV Error] Line {i + 1} has Type 'Feeling' but no FeelingType specified!");
                    }
                }
                dialogueList.Add(entry);
            }
            catch (System.Exception e) { Debug.LogWarning($"CSV Parse Error: {e.Message}"); }
        }
    }

    public List<DialogueEntry> GetDialogueOptionsByID(int id)
    {
        return dialogueList.FindAll(d => d.ID == id);
    }

    public virtual void OnDialogueEnded()
    {
        if (itemToCollectAfterDialogue != null && currentInteractableObject != null)
        {
            if (itemToCollectAfterDialogue.itemIcon != null)
            {
                bool success = InventoryManager.Instance.CollectItem(itemToCollectAfterDialogue);
                if (success) currentInteractableObject.OnCollectionComplete();
            }
            itemToCollectAfterDialogue = null;
            currentInteractableObject = null;
        }

        if (dialoguePanel != null) dialoguePanel.SetActive(false);

        if (currentMode == DialogueMode.Opening)
        {
            ShowDialogueSelectionPanel(); 
        }
        else if (currentMode == DialogueMode.Main)
        {
            SceneManager.LoadScene("BattleScene");
        }
    }

    public virtual void OnRerollRequested()
    {
        ShowDialogueSelectionPanel();
    }

    protected (string character, string fileName) FindCharacterAndFileName(string csvText, string targetID)
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
                if (id == targetID)
                {
                    string character = parts[1].Trim().Trim('"');
                    string fileName = parts[2].Trim().Trim('"');
                    if (fileName.EndsWith(".csv")) fileName = fileName.Substring(0, fileName.Length - 4);
                    return (character, fileName);
                }
            }
        }
        catch { }
        return (null, null);
    }
    

    public void EvokeFeeling(FeelingType fType)
    {
        Debug.Log($"[DialogueManager] EvokeFeeling Called: {fType}");
    }
}