using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EncounterObjet : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public Image objetImage;      
    public TMP_Text costText;     

    private Item _itemData;      
    private int _cost;              
    private bool _isValid;          

    private CharacterSO _playerData;     
    private System.Action _onBuyRequest;

    // 초기화 함수 (EncounterMerchantUI에서 호출)
    public void Setup(Item item, int cost, bool isValid, CharacterSO playerData, System.Action onBuyRequest)
    {
        // 1. 데이터 저장
        _itemData = item;
        _cost = cost;
        _isValid = isValid;
        _playerData = playerData;
        _onBuyRequest = onBuyRequest;

        if (objetImage != null)
        {
            objetImage.sprite = item.sprite; 
        }

        if (costText != null)
        {
            costText.text = "<sprite=0>" + cost.ToString();
        }

        gameObject.SetActive(isValid);
    }

    public void OnPointerClick(PointerEventData data)
    {
        // 데이터가 없거나 이미 팔린 경우 무시
        if (_playerData == null || !_isValid) return;

        // 돈 부족 확인
        if (_playerData.dreamDust < _cost) return;

        // 구매 요청 (EncounterMerchantUI로 로직 위임)
        _onBuyRequest?.Invoke();
    }

    private void Update()
    {
        if (_playerData == null) return;

        // 돈이 부족하면 가격 텍스트를 빨간색으로, 충분하면 파란색으로 표시
        if (_playerData.dreamDust < _cost)
        {
            if (costText != null) costText.color = Color.red;
        }
        else
        {
            if (costText != null) costText.color = Color.blue;
        }
    }
}