using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RerollManager : MonoBehaviour
{
    [SerializeField] private Button rerollButton;
    [SerializeField] private TMP_Text dustNeededText; 
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DreamDustManager dreamDustManager;
    [SerializeField] private GameObject backgroundDefault;
    [SerializeField] private int rerollCost = 1;

    private bool isRerollActive = true;

    void Start()
    {
        if (rerollButton != null)
            rerollButton.onClick.AddListener(OnRerollClicked);
        UpdateDustNeededUI();
    }

    private void OnRerollClicked()
    {
        if (dreamDustManager == null || dialogueManager == null)
        {
            Debug.LogError("[RerollManager] 필수 참조가 없습니다.");
            return;
        }

        int currentDust = dreamDustManager.GetDreamDust();

        if (currentDust >= rerollCost)
        {
            dreamDustManager.UseDust(rerollCost);
            Debug.Log($"[Reroll] DreamDust {rerollCost}개 사용. 남은 수: {dreamDustManager.GetDreamDust()}");
            
            rerollCost++;
            UpdateDustNeededUI();
            isRerollActive = true;
            dialogueManager.OnRerollRequested();
        }
        currentDust = dreamDustManager.GetDreamDust();
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
    public void ResetRerollCost(int newCost = 1)
    {
        rerollCost = newCost;
        UpdateDustNeededUI();
    }
}