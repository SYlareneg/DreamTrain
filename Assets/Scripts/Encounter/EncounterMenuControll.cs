using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EncounterMenuControll : MonoBehaviour
{
    [Header("Data Sources")]
    public PlayerStatsSo playerStats;   
    public CharacterSO characterData;
    public RelicSO characterRelicData;

    [Header("UI Text References")]
    public TextMeshProUGUI hpText;       
    public TextMeshProUGUI dreamFragmentText;  
    public TextMeshProUGUI courageText;
    public TextMeshProUGUI wisdomText;
    public TextMeshProUGUI luckText;

    [Header("UI Button References")]
    public Button relicButton;
    public GameObject relicPanel;
    public GameObject relicUIPrefab;

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
        relicPanel.SetActive(false);
        if(characterRelicData.relicItems == null || characterRelicData.relicItems.Count == 0)
        {
            relicButton.enabled = false;
            relicButton.GetComponent<Image>().color = Color.gray;
        }
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

    public void ShowRelics()
    {
        if (relicPanel.activeSelf)
        {
            relicPanel.SetActive(false);
        }
        else
        {
            foreach(Transform child in relicPanel.transform)
            {
                Destroy(child.gameObject);
            }
            foreach(RelicItem rItem in characterRelicData.relicItems)
            {
                GameObject relicUIObj = Instantiate(relicUIPrefab, relicPanel.transform);
                RelicUI relicUI = relicUIObj.GetComponent<RelicUI>();
                relicUI.Setup(rItem, null);
            }
            if(characterRelicData.relicItems.Count > 0)
            {
                relicPanel.SetActive(true);
            }
        }
    }
}
