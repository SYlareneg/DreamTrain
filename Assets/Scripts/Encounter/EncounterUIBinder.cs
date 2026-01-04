using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EncounterUIBinder : MonoBehaviour
{
    [Header("UI")]
    public GameObject encounterPanel;       
    public Image illustrationImage;         
    public TextMeshProUGUI titleText;       
    public TextMeshProUGUI descriptionText; 
    public Transform choiceContainer;       
    public GameObject choiceButtonPrefab;
    public GameObject merchantPanel;
    public GameObject cardRemovalPanel;
    public EncounterRouletteUI rouletteUI;
    public GameObject roulettePanel;

    void Awake()
    {
        if (EncounterManager.Instance != null)
        {
            EncounterManager.Instance.RegisterSceneUI(
                encounterPanel, 
                illustrationImage, 
                titleText, 
                descriptionText, 
                choiceContainer, 
                choiceButtonPrefab,
                merchantPanel,
                cardRemovalPanel,
                rouletteUI,
                roulettePanel
            );
            
        }
        else
        {
            Debug.LogError("[Binder] 치명적 오류: EncounterManager 인스턴스를 찾을 수 없습니다!");
        }
    }
    void Start()
    {
        if (EncounterManager.Instance != null && encounterPanel != null)
        {
            Debug.Log("Err");
        }
    }
}