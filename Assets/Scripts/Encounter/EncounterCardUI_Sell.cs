using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 클릭 이벤트 처리를 위해 필요
using TMPro;

// CardUI를 상속받고, IPointerClickHandler로 클릭 입력을 받음 (기존 구조 유지)
public class EncounterCardUI_Sell : CardUI, IPointerClickHandler
{
    [SerializeField] TMP_Text sellCostTMP;
    
    // 기존 데이터 구조 사용 (SellCard가 프로젝트에 정의되어 있다고 가정)
    public SellCard sellCard; 

    // 클릭 시 실행할 함수 (Manager에게 알림용)
    private System.Action _onBuyRequest;

    private void Awake()
    {
        if(sellCard == null) sellCard = new SellCard();
    }

    // Setup 함수 수정: 클릭 시 실행할 콜백(onBuyRequest)을 받음
    public void Setup(Item item, int cost, bool isValid, System.Action onBuyRequest)
    {
        base.Setup(item); // 부모 클래스(CardUI)의 Setup 실행
        
        if(sellCard == null) sellCard = new SellCard();
        sellCard.cardItem = item;
        sellCard.cost = cost;
        sellCard.isValid = isValid;

        this._onBuyRequest = onBuyRequest;

        if (sellCostTMP != null)
        {
            sellCostTMP.text = "<sprite=0>" + sellCard.cost.ToString();
        }
            
        // 팔린 카드면 비활성화
        gameObject.SetActive(isValid);
    }

    // 인터페이스 구현: 클릭 시 호출됨
    public void OnPointerClick(PointerEventData data)
    {
        // 1. 돈이 부족하면 클릭 무시 (PlayerManager 사용)
        if (PlayerManager.Inst.characterSO.dreamDust < sellCard.cost) return;

        // 2. 직접 구매 처리하지 않고, Manager에게 요청 보냄
        _onBuyRequest?.Invoke();

        // 참고: 구매 성공 후 처리는 Manager가 UI를 다시 그리기 때문에 
        // 여기서 gameObject.SetActive(false)를 굳이 안 해도 되지만,
        // 즉각적인 반응을 위해 남겨두어도 됩니다.
    }

    private void Update()
    {
        // 돈 부족 시 텍스트 색상 변경 (기존 로직 유지)
        if (PlayerManager.Inst.characterSO.dreamDust < sellCard.cost)
        {
            if(sellCostTMP != null) sellCostTMP.color = Color.red;
        }
        else
        {
            if(sellCostTMP != null) sellCostTMP.color = Color.blue;
        }
    }
}