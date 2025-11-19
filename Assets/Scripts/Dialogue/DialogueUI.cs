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

    public RelicSO relicListSO;
    public RelicSO playerRelicSO;

    public RerollManager rerollManager;
    
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

        playerPanelRT = playerPanel.GetComponent<RectTransform>();
        playerTextRT = PlayerText.GetComponent<RectTransform>();
        originalPlayerPanelSize = playerPanelRT.rect.size;
        originalPlayerTextSize = playerTextRT.rect.size;
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
        if (rerollManager != null && rerollManager.IsRerollUIActive())
            return;
        
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
            AddPlayerRelic(firstEntry.IdToGet);
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
    
    
    public bool IsDialogueActive()
    {
        return dialogueActive;
    }
    private void AddPlayerRelic(int relicId)
    {
        // 1-based 인덱스만 유효
        if (relicId <= 0)
        {
            Debug.LogWarning($"[DialogueUI] relicId {relicId} is invalid (must be >= 1)");
            return;
        }

        if (relicListSO == null || relicListSO.relicItems == null || relicListSO.relicItems.Count == 0)
        {
            Debug.LogWarning("[DialogueUI] relicListSO or relicListSO.relicItems is null/empty");
            return;
        }

        int index = relicId - 1; // 1-based → 0-based 변환

        if (index < 0 || index >= relicListSO.relicItems.Count)
        {
            Debug.LogWarning($"[DialogueUI] relicId {relicId} is out of range. relicItems count = {relicListSO.relicItems.Count}");
            return;
        }

        // 바로 인덱스로 찾기
        RelicItem_Enhanceable found = relicListSO.relicItems[index];
        Debug.Log($"[DialogueUI] Found relic '{found.relicName}' at index {index}");

        if (found == null)
        {
            Debug.LogWarning($"[DialogueUI] relicItems[{index}] is null!");
            return;
        }

        if (playerRelicSO == null)
        {
            Debug.LogWarning("[DialogueUI] playerRelicSO is null!");
            return;
        }

        // 리스트 초기화 방어
        if (playerRelicSO.relicItems == null)
            playerRelicSO.relicItems = new List<RelicItem_Enhanceable>();

        // 중복 방지
        bool alreadyHas = playerRelicSO.relicItems.Exists(r => r == found);
        if (alreadyHas)
        {
            Debug.Log($"[DialogueUI] Player already has relic '{found.relicName}' → 획득 무시");
            return;
        }

        // 추가
        playerRelicSO.relicItems.Add(found);
        Debug.Log($"[DialogueUI] Player withdrew relic '{found.relicName}' (1-based Id = {relicId})");
    }

}
