using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DialogueUI : MonoBehaviour
{
    public DialogueManager dialogueManager;

    public TextMeshProUGUI MoooText; // Guest 발화
    public TextMeshProUGUI PlayerText; // Player 발화

    public GameObject branchPanel;
    public Button branchButtonPrefab;
    public GameObject moooPanel;
    public GameObject playerPanel;

    private int currentID = 1;
    private InputSystem_Actions input;
    private bool isBranchActive = false;

    private void Awake()
    {
        input = new InputSystem_Actions();
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

    private void Start()
    {
        ShowDialogue(currentID);
    }

    private void OnScreenClickPerformed(InputAction.CallbackContext context)
    {
        if (!isBranchActive)
        {
            ShowDialogue(currentID);
        }
    }

    void ClearBranchButtons()
    {
        foreach (Transform t in branchPanel.transform)
        {
            if (t.GetComponent<Button>() != null)
            {
                Destroy(t.gameObject);
            }
        }
    }


    public void ShowDialogue(int id)
    {
        ClearBranchButtons();
        //branchPanel.SetActive(false);
        isBranchActive = false;
        if (id == 0)
        {
            MoooText.text = "";
            PlayerText.text = "";
            branchPanel.SetActive(false);
            moooPanel.SetActive(false);
            playerPanel.SetActive(false);
            Debug.Log("End of dialogue");

            int newRelicIndex = DialogueRelicManager.Inst.GetMaxWeightIndex() - 1;
            DialogueRelicManager.Inst.AddPlayerRelic(newRelicIndex);

            SceneManager.LoadScene("BattleScene");
            return;
        }
        Debug.Log(id);
        List<DialogueEntry> entries = dialogueManager.GetDialogueOptionsByID(id);
        DialogueEntry firstEntry = entries[0];

        MoooText.text = "";
        PlayerText.text = "";

        if(firstEntry.IdToGet != 0)
        {
            DialogueRelicManager.Inst.relicWeights[firstEntry.IdToGet] += firstEntry.IdPoint;
        }

        if (firstEntry.Type == "Normal")
        {
            if (firstEntry.BoxLocation == "Guest")
                MoooText.text = firstEntry.Dialogue_KO;
            else if (firstEntry.BoxLocation == "Player")
                PlayerText.text = firstEntry.Dialogue_KO;

            currentID = firstEntry.NextID != 0 ? firstEntry.NextID : 0;
        }
        else if (firstEntry.Type == "Branch")
        {
            playerPanel.SetActive(false);
            isBranchActive = true;
            currentID = 0;

            foreach (var option in entries)
            {
                Button btn = Instantiate(branchButtonPrefab, branchPanel.transform);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = option.Dialogue_KO;

                int nextId = option.NextID;
                btn.onClick.AddListener(() =>
                {
                    isBranchActive = false;
                    currentID = nextId; 
                    ShowDialogue(nextId);
                    ClearBranchButtons();
                    playerPanel.SetActive(true);
                });
            }
        }
    }

}