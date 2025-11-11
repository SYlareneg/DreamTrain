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
    
    // --- 새로 추가된 필드
    private RectTransform playerPanelRT;
    private RectTransform playerTextRT;
    private Vector2 originalPlayerPanelSize;
    private Vector2 originalPlayerTextSize;
    private ContentSizeFitter playerTextFitter;

    private DialogueEntry lastShownEntry = null;
    
    private bool dialogueActive = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        input = new InputSystem_Actions();

        playerLayoutGroup = playerPanel.GetComponent<VerticalLayoutGroup>();
        if (playerLayoutGroup != null) playerLayoutGroup.enabled = false;
        if (objectNameText != null) objectNameText.gameObject.SetActive(false);

        // RectTransform / original size 저장
        playerPanelRT = playerPanel.GetComponent<RectTransform>();
        playerTextRT = PlayerText.GetComponent<RectTransform>();
        // rect.size 는 런타임에서 레이아웃 적용 상태에 따라 값이 달라질 수 있지만
        // Awake 시점의 "현재" 값을 원래값으로 저장해 둡니다.
        originalPlayerPanelSize = playerPanelRT.rect.size;
        originalPlayerTextSize = playerTextRT.rect.size;

        // ContentSizeFitter가 붙어있다면 참조
        playerTextFitter = PlayerText.GetComponent<ContentSizeFitter>();
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
            EndDialogue(); 
            return;
        }
        
        dialogueActive = true;
        DialogueEntry firstEntry = entries[0];
        lastShownEntry = firstEntry;
    
        MoooText.text = "";
        PlayerText.text = "";

        if (firstEntry.IdToGet != 0)
        {
            DialogueRelicManager.Inst.AddPlayerRelic(firstEntry.IdToGet);
            Debug.Log($"[DialogueUI] Relic ID {firstEntry.IdToGet} 획득 (from normal line)");
        }
        if (firstEntry.Type == "Normal")
        {
            playerLayoutGroup.enabled = false;
            if (playerTextFitter != null) playerTextFitter.enabled = false;
            RestorePlayerSizes();
            LayoutRebuilder.ForceRebuildLayoutImmediate(playerPanelRT);
            Canvas.ForceUpdateCanvases();

            if (firstEntry.BoxLocation == "Guest")
            {
                moooPanel.SetActive(true);
                MoooText.text = firstEntry.Dialogue_KO;
            }
            else
            {
                PlayerText.text = firstEntry.Dialogue_KO;
                playerPanel.SetActive(true);
            }
        
            nextIdForNormalDialogue = firstEntry.NextID;

        }
        else if (firstEntry.Type == "Branch")
        { 
            playerLayoutGroup.enabled = true;
            if (playerTextFitter != null) playerTextFitter.enabled = false;

            isBranchActive = true;
            PlayerText.gameObject.SetActive(false);

            foreach (var option in entries)
            {
                Button btn = Instantiate(branchButtonPrefab, playerPanel.transform);
                btn.GetComponentInChildren<TextMeshProUGUI>().text = option.Dialogue_KO;
                int relicIdToGet = option.IdToGet;
                int nextId = option.NextID;
                btn.onClick.AddListener(() => {
                    isBranchActive = false;
                    playerLayoutGroup.enabled = false;
                    
                    if (playerTextFitter != null) playerTextFitter.enabled = false;
                    PlayerText.gameObject.SetActive(true);
                    RestorePlayerSizes();
                    LayoutRebuilder.ForceRebuildLayoutImmediate(playerPanelRT);
                    Canvas.ForceUpdateCanvases();
                    if (relicIdToGet != 0)
                    {
                        DialogueRelicManager.Inst.AddPlayerRelic(relicIdToGet);
                        Debug.Log($"[DialogueUI] Relic ID {relicIdToGet} 획득 (from branch)");
                    }
                    ClearBranchButtons();
                    ShowDialogue(nextId);
                });
            }

            // 새로 생성된 버튼들에 대해 즉시 레이아웃 계산하여 크기랑 위치 확정
            LayoutRebuilder.ForceRebuildLayoutImmediate(playerPanelRT);
            Canvas.ForceUpdateCanvases();
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
    }

    void ClearBranchButtons()
    {
        // 기존 버튼들 삭제
        for (int i = playerPanel.transform.childCount - 1; i >= 0; i--)
        {
            Transform t = playerPanel.transform.GetChild(i);
            if (t.GetComponent<Button>() != null)
                Destroy(t.gameObject);
        }
    }

    private void RestorePlayerSizes()
    {
        if (playerPanelRT != null)
        {
            playerPanelRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalPlayerPanelSize.x);
            playerPanelRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalPlayerPanelSize.y);
        }

        if (playerTextRT != null)
        {
            playerTextRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, originalPlayerTextSize.x);
            playerTextRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, originalPlayerTextSize.y);
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
