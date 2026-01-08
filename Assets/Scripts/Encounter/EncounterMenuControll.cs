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
    public TextMeshProUGUI courageText;
    public TextMeshProUGUI wisdomText;
    public TextMeshProUGUI luckText;

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
        hpText.text = $"{characterData.curHealth}/{characterData.maxHealth}";
    }

    void UpdateDreamFragment()
    {
        if (dreamFragmentText == null ||  characterData == null) return;

        dreamFragmentText.text = $"{characterData.dreamDust}";
    }

    void UpdateStats()
    {
        if (courageText == null || wisdomText == null || luckText == null || playerStats == null) return;
        courageText.text = $"{playerStats.courage}";
        wisdomText.text = $"{playerStats.wisdom}";
        luckText.text = $"{playerStats.luck}";
    }
}
