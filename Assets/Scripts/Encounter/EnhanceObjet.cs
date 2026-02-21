using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EnhanceObjet : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI References")]
    [SerializeField] Image objetIcon;         
    [SerializeField] TMP_Text nameTMP;        
    
    [Header("Tooltip Settings")]
    public GameObject cardUITooltipPrefab;
    public Transform tooltipPos;
    public Vector3 tooltipOffset = new Vector3(120f, 0, 0); // 툴팁을 오브제 우측에 띄우기 위한 오프셋 조절값
    
    [HideInInspector]
    public RelicItem_Enhanceable relicData;   
    
    private GameObject activeTooltip;
    
    // 툴팁 동시 표시 및 유지를 위한 상태 변수
    private bool isHovering = false;
    private bool isSelected = false;

    private void Awake()
    {
        if (tooltipPos == null) tooltipPos = this.transform;
    }

    public void Setup(RelicItem_Enhanceable data)
    {
        this.relicData = data;
        isSelected = false;
        isHovering = false;

        if (relicData != null) 
        {
            UpdateObjetVisual(relicData);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (relicData == null) return;
        
        if (EncounterMerchantUI.Inst != null)
        {
            EncounterMerchantUI.Inst.EnhanceRelicSelect(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        UpdateTooltipState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        UpdateTooltipState();
    }

    public void SetSelected(bool state)
    {
        isSelected = state;
        UpdateTooltipState();
    }

    private void UpdateTooltipState()
    {
        bool shouldShow = isHovering || isSelected;

        if (shouldShow && activeTooltip == null)
        {
            CreateTooltip();
        }
        else if (!shouldShow && activeTooltip != null)
        {
            Destroy(activeTooltip);
            activeTooltip = null;
        }
    }

    private void CreateTooltip()
    {
        if (relicData == null) return;

        if (cardUITooltipPrefab == null) 
        {
            Debug.LogError($"[EnhanceObjet] 툴팁 프리팹이 인스펙터에 연결되지 않았습니다! {gameObject.name} 확인 요망.");
            return;
        }

        Vector3 spawnPos = tooltipPos.position + tooltipOffset;
        activeTooltip = Instantiate(cardUITooltipPrefab, spawnPos, Quaternion.identity);
        
        GameObject mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        if (mainCanvas != null)
        {
            activeTooltip.transform.SetParent(mainCanvas.transform, true);
        }
        else
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null) activeTooltip.transform.SetParent(parentCanvas.transform, true);
        }

        activeTooltip.transform.SetAsLastSibling(); 

        CardTooltip cardTooltip = activeTooltip.GetComponent<CardTooltip>();
        if (cardTooltip != null)
        {
            cardTooltip.SetTooltip(relicData.relicName, relicData.relicTxt);
        }
    }

    void UpdateObjetVisual(RelicItem_Enhanceable data)
    {
        if (objetIcon != null)
        {
            Sprite sprite = data.relicSprite;
            
            if (sprite != null)
            {
                objetIcon.sprite = sprite;
                objetIcon.gameObject.SetActive(true);
            }
        }

        if (nameTMP != null)
        {
            nameTMP.text = data.relicName;
            nameTMP.gameObject.SetActive(true);
        }
    }
    private void OnDisable()
    {
        if (activeTooltip != null)
        {
            Destroy(activeTooltip);
        }
    }
}