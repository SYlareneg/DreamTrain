using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EncounterObjetUI_Sell : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] Image objetIcon;         
    [SerializeField] TMP_Text nameTMP;        
    [SerializeField] public TMP_Text sellCostTMP; 

    private RelicItem_Data relicData;   
    
    public int cost;                
    public bool isValid;            
    
    public CharacterSO _playerData; 
    private System.Action _onBuyRequest;
    private bool _wasAffordable = true;

    public void Setup(RelicItem_Data data, int cost, bool isValid, CharacterSO playerData, System.Action onBuyRequest)
    {
        this.relicData = data;
        this.cost = cost;
        this.isValid = isValid;
        this._playerData = playerData;
        this._onBuyRequest = onBuyRequest;

        // UI 시각화 업데이트
        if (relicData != null) 
        {
            UpdateObjetVisual(relicData);
        }

        if (sellCostTMP != null)
        {
            TMP_SpriteAsset newSpriteAsset = Resources.Load<TMP_SpriteAsset>("Cards/coin");

            if (newSpriteAsset != null)
            {
                // TMP 컴포넌트의 spriteAsset 속성을 교체합니다.
                sellCostTMP.spriteAsset = newSpriteAsset;
                // 변경 사항을 즉시 반영하기 위해 업데이트를 호출합니다.
                sellCostTMP.SetVerticesDirty();
                sellCostTMP.SetMaterialDirty();
            }
            else
            {
                Debug.LogWarning("새로운 Sprite Asset을 찾을 수 없습니다. 경로를 확인하세요.");
            }

            sellCostTMP.text = "<sprite=0>" + this.cost;
            
            UpdateColor(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isValid || _onBuyRequest == null) return;
        _onBuyRequest.Invoke();
    }

    // [핵심 수정 부분]
    void UpdateObjetVisual(RelicItem_Data data)
    {
        // 1. 아이콘 설정
        if (objetIcon != null)
        {
            string spriteName = data.relicSprite; 

            Sprite sprite = Resources.Load<Sprite>($"Relics/{spriteName}");
            
            if (sprite != null)
            {
                objetIcon.sprite = sprite;
                objetIcon.gameObject.SetActive(true);
            }
            else
            {
                // 이미지를 못 찾았을 때 (디버깅용)
                Debug.LogWarning($"이미지 로드 실패! 이름: {spriteName}, 경로: Resources/Relics/{spriteName}");
                // 임시로 투명하게 처리하거나 기본 이미지 유지
                 objetIcon.color = Color.clear; 
            }
        }

        // 2. 이름 설정
        if (nameTMP != null)
        {
            nameTMP.text = data.relicName;
            nameTMP.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (_playerData == null || !isValid || sellCostTMP == null) return;

        bool isAffordable = _playerData.dreamDust >= cost;
        if (isAffordable != _wasAffordable)
        {
            UpdateColor(isAffordable);
            _wasAffordable = isAffordable;
        }
    }

    void UpdateColor(bool isAffordable)
    {
        if (sellCostTMP == null) return;
        sellCostTMP.color = isAffordable ? Color.blue : Color.red;
    }
}