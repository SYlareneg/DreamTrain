using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

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
    public GameObject relicPanel;
    public GameObject relicUIPrefab;

    [Header("효과 지속 시간")] 
    public float continuousTime = 5f;
    [Header("숫자 롤링 간격")] 
    public float countInterval = 0.02f; 

    private Color goodChangeColor; 
    private Color badChangeColor; 
    private Color originalColor = Color.white; 
    
    private Vector3 originalScale = Vector3.one;
    [SerializeField] private float highlightScale = 1.5f; 

    private int prevHP;
    private int prevDust;
    private int prevCourage;
    private int prevWisdom;
    private int prevLuck;

    private Dictionary<TextMeshProUGUI, Coroutine> activeCoroutines = new Dictionary<TextMeshProUGUI, Coroutine>();

    private void Awake()
    {
        ColorUtility.TryParseHtmlString("#3CE74A", out goodChangeColor);
        ColorUtility.TryParseHtmlString("#FD1E12", out badChangeColor);
    }

    private void OnEnable()
    {
        if (playerStats != null) playerStats.OnStatChanged += RefreshUI;
    }

    private void OnDisable()
    {
        if (playerStats != null) playerStats.OnStatChanged -= RefreshUI;
    }

    private void Start()
    {
        InitializeValues();
        
        if (hpText != null) originalColor = hpText.color; 
        
        UpdateUI_NoEffect();
        ShowRelics();
    }

    void InitializeValues()
    {
        if (characterData != null)
        {
            prevHP = characterData.curHealth;
            prevDust = characterData.dreamDust;
        }
        if (playerStats != null)
        {
            prevCourage = playerStats.courage;
            prevWisdom = playerStats.wisdom;
            prevLuck = playerStats.luck;
        }
    }

    public void RefreshUI()
    {
        if (characterData == null || playerStats == null) return;
        if (characterData.dreamDust < 0) characterData.dreamDust = 0;
        CheckAndAnimate(hpText, characterData.curHealth, ref prevHP, true, characterData.maxHealth);
        CheckAndAnimate(dreamFragmentText, characterData.dreamDust, ref prevDust);
        CheckAndAnimate(courageText, playerStats.courage, ref prevCourage);
        CheckAndAnimate(wisdomText, playerStats.wisdom, ref prevWisdom);
        CheckAndAnimate(luckText, playerStats.luck, ref prevLuck);
    }

    void UpdateUI_NoEffect()
    {
        if (characterData != null)
        {
            hpText.text = $"{characterData.curHealth}/{characterData.maxHealth}";
            dreamFragmentText.text = $"{characterData.dreamDust}";
        }
        if (playerStats != null)
        {
            courageText.text = $"{playerStats.courage}";
            wisdomText.text = $"{playerStats.wisdom}";
            luckText.text = $"{playerStats.luck}";
        }
    }

    void CheckAndAnimate(TextMeshProUGUI uiText, int targetVal, ref int prevVal, bool isHP = false, int maxHP = 0)
    {
        if (uiText == null) return;
        if (targetVal == prevVal) return;
        int startVal = GetDisplayedValue(uiText, isHP);
        Color targetColor = (targetVal > prevVal) ? goodChangeColor : badChangeColor;
        TriggerEffect(uiText, startVal, targetVal, targetColor, isHP, maxHP);
        prevVal = targetVal;
    }

    int GetDisplayedValue(TextMeshProUGUI uiText, bool isHP)
    {
        if (string.IsNullOrEmpty(uiText.text)) return 0;

        try
        {
            if (isHP)
            {
                string[] parts = uiText.text.Split('/');
                return int.Parse(parts[0]);
            }
            else
            {
                return int.Parse(uiText.text);
            }
        }
        catch
        {
            return 0; 
        }
    }

    void TriggerEffect(TextMeshProUGUI textComp, int startVal, int endVal, Color targetColor, bool isHP, int maxHP)
    {
        if (activeCoroutines.ContainsKey(textComp) && activeCoroutines[textComp] != null)
        {
            StopCoroutine(activeCoroutines[textComp]);
        }

        Coroutine co = StartCoroutine(EffectRoutine(textComp, startVal, endVal, targetColor, isHP, maxHP));
        activeCoroutines[textComp] = co; 
    }

    IEnumerator EffectRoutine(TextMeshProUGUI textComp, int startVal, int endVal, Color targetColor, bool isHP, int maxHP)
    {
        textComp.color = targetColor;
        textComp.transform.localScale = Vector3.one * highlightScale;

        int current = startVal;
        
        int step = (endVal > startVal) ? 1 : -1; 
        
        while (current != endVal)
        {
            current += step;
            
            // 텍스트 갱신
            if (isHP) textComp.text = $"{current}/{maxHP}";
            else textComp.text = current.ToString();

            yield return new WaitForSeconds(countInterval);
        }

        if (isHP) textComp.text = $"{endVal}/{maxHP}";
        else textComp.text = endVal.ToString();
        yield return new WaitForSeconds(continuousTime);
        ResetSingleText(textComp);
    }

    void ResetSingleText(TextMeshProUGUI textComp)
    {
        if (textComp == null) return;

        textComp.color = originalColor;
        textComp.transform.localScale = originalScale;
        
        if (activeCoroutines.ContainsKey(textComp))
        {
            activeCoroutines[textComp] = null;
        }
    }


    public void ShowRelics()
    {
        foreach (Transform child in relicPanel.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (RelicItem rItem in characterRelicData.relicItems)
        {
            GameObject relicUIObj = Instantiate(relicUIPrefab, relicPanel.transform);
            RelicUI relicUI = relicUIObj.GetComponent<RelicUI>();
            relicUI.Setup(rItem, null);
        }
        if (characterRelicData.relicItems.Count > 0)
        {
            relicPanel.SetActive(true);
        }
    }
}