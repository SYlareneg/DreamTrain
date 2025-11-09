using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance { get; private set; } 
    public DialogueManager dialogueManager;

    public TextMeshProUGUI MoooText;
    public TextMeshProUGUI PlayerText;
    public TextMeshProUGUI objectNameText;

    public Button branchButtonPrefab;
    public GameObject playerPanel;
    public GameObject moooPanel;
    
    private int nextIdForNormalDialogue; 
    private InputSystem_Actions input;
    private bool isBranchActive = false;
    private VerticalLayoutGroup playerLayoutGroup;
    
    private DialogueEntry lastShownEntry = null;
    
    private bool dialogueActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("중복된 DialogueUI 인스턴스가 발견되어 하나를 파괴합니다.");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Instance = this;
        input = new InputSystem_Actions();
        playerLayoutGroup = playerPanel.GetComponent<VerticalLayoutGroup>();
        if (playerLayoutGroup != null) playerLayoutGroup.enabled = false;
        if (objectNameText != null) objectNameText.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.Click.performed += OnScreenClickPerformed; 
    }

    private void OnDisable()
    {
        input.Player.Click.performed -= OnScreenClickPerformed;
        input.Player.Disable();
    }

    private void OnScreenClickPerformed(InputAction.CallbackContext context)
    {
        if (isBranchActive) return;
        Debug.Log($"[OnScreenClickPerformed] nextIdForNormalDialogue: {nextIdForNormalDialogue}");
        
        
        if (nextIdForNormalDialogue != 0)
            ShowDialogue(nextIdForNormalDialogue);
        else EndDialogue(); 
    }

    public void ShowDialogue(int id)
    {

        isBranchActive = false;
        ClearBranchButtons();

        if (id == 0)
        {
            EndDialogue();
            return;
        }
        if (dialogueManager == null)
        {
            return;
        }

        List<DialogueEntry> entries = dialogueManager.GetDialogueOptionsByID(id);
        if (entries.Count == 0)
        {
            Debug.LogWarning($"No dialogue entry found for ID {id}");
            EndDialogue(); 
            return;
        }
        
        dialogueActive = true;
        DialogueEntry firstEntry = entries[0];
        lastShownEntry = firstEntry;
        Debug.Log(firstEntry.NextID);
    
        MoooText.text = "";
        PlayerText.text = "";

        if (firstEntry.IdToGet != 0)
        {
            DialogueRelicManager.Inst.relicWeights[firstEntry.IdToGet] += firstEntry.IdPoint;
        }
        if (firstEntry.Type == "Normal")
        {
            playerLayoutGroup.enabled = false;
            if (firstEntry.BoxLocation == "Guest")
            {

                moooPanel.SetActive(true);
                MoooText.text = firstEntry.Dialogue_KO;
                Debug.Log(firstEntry.Dialogue_KO);
            }
            else
            {

                PlayerText.text = firstEntry.Dialogue_KO;
                playerPanel.SetActive(true);
            }
        
            nextIdForNormalDialogue = firstEntry.NextID;
            Debug.Log($"[ShowDialogue] nextIdForNormalDialogue set to: {nextIdForNormalDialogue}");

        }
        else if (firstEntry.Type == "Branch")
        { 

            playerLayoutGroup.enabled = true;
            isBranchActive = true;

            foreach (var option in entries)
            {
                Button btn = Instantiate(branchButtonPrefab, playerPanel.transform);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = option.Dialogue_KO;

                int nextId = option.NextID;
                btn.onClick.AddListener(() => {
                    isBranchActive = false;
                    playerLayoutGroup.enabled = false;
                    ClearBranchButtons();
                    ShowDialogue(nextId);
                });
            }
        }

    }
    
    private void EndDialogue()
    {
        dialogueActive = false;
        MoooText.text = "";
        PlayerText.text = "";
        playerPanel.SetActive(false);
        moooPanel.SetActive(false);
        DialogueManager.Instance.OnDialogueEnded();

        Debug.Log("Dialogue Ended");
        if (lastShownEntry != null && lastShownEntry.Function == "EndScene")
        {
            lastShownEntry = null; 
            SceneManager.LoadScene("BattleScene");
        }
    }

    void ClearBranchButtons()
    {
        foreach (Transform t in playerPanel.transform)
        {
            if (t.GetComponent<Button>() != null)
                Destroy(t.gameObject);
        }
    }
    
    public void ShowObjectName(string name)
    {
        objectNameText.text = name;
        objectNameText.gameObject.SetActive(true);
    }
    
    public void HideObjectName()
    {
        objectNameText.gameObject.SetActive(false);
    }
    
    public bool IsDialogueActive()
    {
        return dialogueActive;
    }
}