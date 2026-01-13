using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EncounterObjetUI_Sell : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] Image objetIcon;
    [SerializeField] TMP_Text nameTMP;
    //[SerializeField] TMP_Text descTMP;
    [SerializeField] public TMP_Text sellCostTMP; // EncounterCardUI_Sell과 동일하게 public 유지
    //[SerializeField] GameObject soldOutPanel;     // 판매 완료 시 표시할 패널

    // 데이터
    public Item_Objets objetItem;   // 오브제 데이터
    public int cost;                // 판매 가격
    public bool isValid;            // 구매 가능 여부
    
    public CharacterSO _playerData; // 재화 확인용 (EncounterCardUI_Sell과 통일)
    private System.Action _onBuyRequest;

    // [최적화용 변수] 매 프레임 컬러 변경 방지
    private bool _wasAffordable = true;

    /// <summary>
    /// 상점 UI에서 이 함수를 호출하여 데이터를 세팅합니다.
    /// </summary>
    public void Setup(Item_Objets item, int cost, bool isValid, CharacterSO playerData, System.Action onBuyRequest)
    {
        // 디버깅용 로그 (EncounterCardUI_Sell과 통일)
        Debug.Log($"[Objet SETUP 실행됨] ID: {this.GetInstanceID()} / 오브젝트: {gameObject.name} / 부모: {transform.parent?.name}");
        Debug.Log($" -> 받은 데이터 Cost: {cost}, Callback: {(onBuyRequest != null)}");

        this.objetItem = item;
        this.cost = cost;
        this.isValid = isValid;
        this._playerData = playerData;
        this._onBuyRequest = onBuyRequest;

        // UI 시각화 업데이트
        if (item != null) UpdateObjetVisual(item);

        // 가격 텍스트 설정
        if (sellCostTMP != null)
        {
            sellCostTMP.text = "<sprite=0>" + this.cost.ToString();
            UpdateColor(true); // 초기화 시 색상 강제 업데이트
        }

        // 판매 완료 상태 처리
        /*if (soldOutPanel != null)
        {
            soldOutPanel.SetActive(!isValid);
        }*/
        
        // 유효하지 않아도 오브젝트는 켜두되, SoldOut 패널로 덮음 (기획에 따라 SetActive(isValid)로 변경 가능)
        //gameObject.SetActive(true); 
    }

    public void OnPointerClick(PointerEventData data)
    {
        // 클릭 로그
        Debug.Log($"[Objet CLICK 실행됨] ID: {this.GetInstanceID()} / 오브젝트: {gameObject.name} / 부모: {transform.parent?.name}");
        Debug.Log($" -> 가지고 있는 Cost: {cost}, Callback: {(_onBuyRequest != null)}");

        // 이미 팔렸다면 클릭 무시
        if (!isValid) 
        {
            Debug.Log("이미 판매된 상품입니다.");
            return;
        }

        if (_onBuyRequest == null)
        {
            Debug.LogError("범인 검거: 이 오브제는 Setup되지 않았거나 콜백이 없습니다!");
        }
        else
        {
            _onBuyRequest.Invoke();
        }
    }

    void UpdateObjetVisual(Item_Objets item)
    {

        // 2. 텍스트 설정
        if (nameTMP != null)
        {
            nameTMP.text = item.name_ko;
            nameTMP.gameObject.SetActive(true);
        }

       /* if (descTMP != null)
        {
            // 설명이 너무 길면 잘리거나 크기가 줄어들도록 TMP 설정 확인 필요
            descTMP.text = item.desc_ko;
            descTMP.gameObject.SetActive(true);
        }*/
    }

    private void Update()
    {
        // 데이터가 없거나 이미 팔렸으면 업데이트 중단
        if (_playerData == null || objetItem == null || sellCostTMP == null || !isValid) return;

        // 현재 재화로 구매 가능한지 체크
        bool isAffordable = _playerData.dreamDust >= cost;

        // 상태가 변했을 때만 텍스트 색상 변경 (최적화)
        if (isAffordable != _wasAffordable)
        {
            UpdateColor(isAffordable);
            _wasAffordable = isAffordable;
        }
    }

    void UpdateColor(bool isAffordable)
    {
        if (sellCostTMP == null) return;
        // 구매 가능하면 파란색, 불가능하면 빨간색 (EncounterCardUI_Sell과 동일)
        sellCostTMP.color = isAffordable ? Color.blue : Color.red;
    }
}