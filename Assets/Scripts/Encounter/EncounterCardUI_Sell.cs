using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EncounterCardUI_Sell : MonoBehaviour, IPointerClickHandler
{
    [Header("Core Reference")]
    public CardUI cardUI; // [핵심] 카드의 비주얼(그림, 텍스트)을 담당하는 컴포넌트

    [Header("Shop UI")]
    public TMP_Text sellCostTMP;    

    // 데이터
    public SellCard sellCard;       
    private CharacterSO _playerData; 
    private System.Action _onBuyRequest; 

    // [최적화] 매 프레임 컬러 변경 방지
    private bool _wasAffordable = true; 

    private void Awake()
    {
        if(sellCard == null) sellCard = new SellCard();
    }

    // 초기화 함수
    public void Setup(Item item, int cost, bool isValid, CharacterSO playerData, System.Action onBuyRequest)
    {
        // 1. 데이터 설정
        if(sellCard == null) sellCard = new SellCard();
        sellCard.cardItem = item;
        sellCard.cost = cost;
        sellCard.isValid = isValid;

        this._playerData = playerData; 
        this._onBuyRequest = onBuyRequest;

        // 2. [핵심] 카드 비주얼은 CardUI에게 전적으로 위임
        if (cardUI != null)
        {
            cardUI.Setup(item);
        }
        else
        {
            Debug.LogError($"[EncounterCardUI_Sell] CardUI가 연결되지 않았습니다! 프리팹을 확인하세요. ({gameObject.name})");
        }

        // 3. 상점 전용 UI(가격표) 설정
        if (sellCostTMP != null)
        {
            sellCostTMP.text = "<sprite=0>" + sellCard.cost.ToString();
            UpdateColor(true); 
        }

        // 4. 유효성(재고 있음/없음)에 따른 활성화
        gameObject.SetActive(isValid);
    }

    // 클릭 이벤트 (구매 시도)
    public void OnPointerClick(PointerEventData data)
    {
        if (_onBuyRequest != null)
        {
            _onBuyRequest.Invoke();
        }
    }

    // 가격 색상 갱신 (돈이 부족하면 빨간색)
    private void Update()
    {
        if (_playerData == null || sellCard == null || sellCostTMP == null) return;

        bool isAffordable = _playerData.dreamDust >= sellCard.cost;

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