using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

public class DialogueUI : MonoBehaviour
{
    public DialogueManager dialogueManager;

    public TextMeshProUGUI MoooText; // Guest 발화
    public TextMeshProUGUI PlayerText; // Player 발화

    public Button branchButtonPrefab;
    public GameObject playerPanel;

    private int currentID = 1;
    private InputSystem_Actions input;
    private bool isBranchActive = false;
    private VerticalLayoutGroup playerLayoutGroup;

    private void Awake()
    {
        input = new InputSystem_Actions();
        playerLayoutGroup = playerPanel.GetComponent<VerticalLayoutGroup>();
        if (playerLayoutGroup != null)
            playerLayoutGroup.enabled = false; // 초기엔 LayoutGroup 끔
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
        foreach (Transform t in playerPanel.transform)
        {
            if (t.GetComponent<Button>() != null)
                Destroy(t.gameObject);
        }
    }
public void ShowDialogue(int id)
    {
        Debug.Log($"ShowDialogue({id}) 호출됨");

        isBranchActive = false;
        ClearBranchButtons();

        if (id == 0)
        {
            EndDialogue();
            return;
        }

        List<DialogueEntry> entries = dialogueManager.GetDialogueOptionsByID(id);
        if (entries.Count == 0)
        {
            Debug.LogWarning($"No dialogue entry found for ID {id}");
            return;
        }

        DialogueEntry firstEntry = entries[0];

        MoooText.text = "";
        PlayerText.text = "";

        if (firstEntry.IdToGet != 0)
        {
            DialogueRelicManager.Inst.relicWeights[firstEntry.IdToGet] += firstEntry.IdPoint;
        }

        if (firstEntry.Type == "Normal")
        {
            // Normal 대사 처리
            playerLayoutGroup.enabled = false;
            if (firstEntry.BoxLocation == "Guest")
                MoooText.text = firstEntry.Dialogue_KO;
            else
                PlayerText.text = firstEntry.Dialogue_KO;

            currentID = firstEntry.NextID;
        }
        else if (firstEntry.Type == "Branch")
        {
            // Branch 대사 처리
            playerLayoutGroup.enabled = true; // LayoutGroup 켬
            isBranchActive = true;

            foreach (var option in entries)
            {
                Button btn = Instantiate(branchButtonPrefab, playerPanel.transform);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = option.Dialogue_KO;

                int nextId = option.NextID;
                btn.onClick.AddListener(() =>
                {
                    // 버튼 클릭 시만 다음 ID로 진행
                    isBranchActive = false;
                    currentID = nextId;
                    ClearBranchButtons();
                    playerLayoutGroup.enabled = false;
                    ShowDialogue(nextId);
                });
            }
        }
    }

    private void EndDialogue()
    {
        MoooText.text = "";
        PlayerText.text = "";
        playerPanel.SetActive(false);

        int newRelicIndex = DialogueRelicManager.Inst.GetMaxWeightIndex() - 1;
        DialogueRelicManager.Inst.AddPlayerRelic(newRelicIndex);

        Debug.Log("End of dialogue");
        SceneManager.LoadScene("BattleScene");
    }

}