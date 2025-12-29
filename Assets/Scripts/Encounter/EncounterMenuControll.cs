using UnityEngine;
using TMPro;

public class EncounterMenuControll : MonoBehaviour
{
    [Header("Data Sources")]
    public PlayerStatsSo playerStats;   
    public CharacterSO characterData;

    [Header("UI Text References")]
    public TextMeshProUGUI hpText;       
    public TextMeshProUGUI dreamFragmentText;  
    public TextMeshProUGUI statText;         

    private void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnStatChanged += RefreshUI;
        }
        RefreshUI();
    }

    private void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnStatChanged -= RefreshUI;
        }
    }

    private void Start()
    {
        RefreshUI();
    }
    
    public void RefreshUI()
    {
        UpdateHP();
        UpdateDreamFragment();
        UpdateStats();
    }

    void UpdateHP()
    {
        if (hpText == null || characterData == null) return;
        hpText.text = $"HP: {characterData.curHealth}/{characterData.maxHealth}";
    }

    void UpdateDreamFragment()
    {
        if (dreamFragmentText == null ||  characterData == null) return;

        dreamFragmentText.text = $"DreamFragment: {characterData.dreamDust}";
    }

    void UpdateStats()
    {
        if (statText == null || playerStats == null) return;

        // 가독성을 위해 간격을 둠
        statText.text = $"용기: {playerStats.courage}     지혜: {playerStats.wisdom}     행운: {playerStats.luck}";
    }
}
