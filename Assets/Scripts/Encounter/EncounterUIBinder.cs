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

    void Start()
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
                cardRemovalPanel
            );
        }
    }
}