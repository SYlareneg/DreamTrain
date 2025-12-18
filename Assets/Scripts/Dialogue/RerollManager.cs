using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RerollManager : MonoBehaviour
{
    [SerializeField] private Button rerollButton;
    [SerializeField] private TMP_Text dustNeededText;
    [SerializeField] private TMP_Text currentDustText;
    public DialogueManagerBase dialogueManager;
    [SerializeField] private GameObject backgroundDefault;

    [Header("플레이어 상태 데이터")]
    [SerializeField] private CharacterSO characterSO;
    [SerializeField] private TMP_Text playerHealthText;
    [SerializeField] private Image playerHealthBar;
    [SerializeField] private TMP_Text passengerNumText;

    [Header("리롤 설정")]
    [SerializeField] private int rerollCost = 1;
    private bool isRerollActive = true;
    private int lastDreamDust = -1;

    void Start()
    {
        UpdateStaticPlayerUI();
        UpdateDustNeededUI();
    }
    private void Awake()
    {
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManagerBase>();
        }
    }
    void Update()
    {
        if (characterSO != null && characterSO.dreamDust != lastDreamDust)
        {
            UpdateCurrentDustUI();
        }
    }
    private void UpdateStaticPlayerUI()
    {
        if (characterSO == null) return;

        // Health 표시
        if (playerHealthText != null)
            playerHealthText.text = $"{characterSO.curHealth}/{characterSO.maxHealth}";
        
        if (playerHealthBar != null)
            playerHealthBar.fillAmount = (float)characterSO.curHealth / characterSO.maxHealth;

        // Passenger 표시
        if (passengerNumText != null)
            passengerNumText.text = $"남은 승객: {characterSO.leftPassengers}명";
        
        
    }

    public void OnRerollClicked()
    {
        if (characterSO == null || dialogueManager == null)
        {
            Debug.LogError("[RerollManager] CharacterSO 또는 DialogueManager 참조가 없습니다.");
            return;
        }

        int currentDust = characterSO.dreamDust;

        if (currentDust >= rerollCost)
        {
            // 꿈 가루 차감
            characterSO.dreamDust -= rerollCost;
            Debug.Log($"[Reroll] DreamDust {rerollCost}개 사용. 남은 수: {characterSO.dreamDust}");

            // 다음 리롤 비용 증가
            rerollCost++;
            UpdateDustNeededUI();

            isRerollActive = true;

            dialogueManager.OnRerollRequested();
        }

        currentDust = characterSO.dreamDust;
        if (currentDust < rerollCost)
        {
            if (backgroundDefault != null)
                backgroundDefault.SetActive(false);

            isRerollActive = false;
            Debug.Log($"[Reroll] DreamDust 부족: {currentDust}/{rerollCost}");
        }
    }

    private void UpdateDustNeededUI()
    {
        if (dustNeededText != null)
        {
            dustNeededText.text = $"{rerollCost}";
        }
    }

    private void UpdateCurrentDustUI()
    {
        if (characterSO == null || currentDustText == null)
            return;

        currentDustText.text = $"꿈 가루: {characterSO.dreamDust}개";
        lastDreamDust = characterSO.dreamDust;
    }
    
    public bool IsRerollUIActive()
    {
        return rerollButton != null && rerollButton.gameObject.activeInHierarchy;
    }
}
