using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EncounterCardUI_Sell : MonoBehaviour, IPointerClickHandler
{
    public CardUI cardUI;
    [Header("UI References")]      
    [SerializeField] Image cardImg;
    [SerializeField] Image character;
    [SerializeField] Image type;
    [SerializeField] Image rarity;
    [SerializeField] Image cost;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text costTMP;
    [SerializeField] TMP_Text textTMP;
    [SerializeField] Sprite[] cardTypes;
    [SerializeField] Sprite[] rarityTypes;
    [SerializeField] Sprite[] costTypes;
    public TMP_Text sellCostTMP;    

    // 데이터
    public SellCard sellCard;       
    public CharacterSO _playerData; 
    private System.Action _onBuyRequest; 

    // [최적화용 변수] 매 프레임 컬러 변경 방지
    private bool _wasAffordable = true; 

    private void Awake()
    {
        if(sellCard == null) sellCard = new SellCard();
    }

    public void Setup(Item item, int cost, bool isValid, CharacterSO playerData, System.Action onBuyRequest)
    {
        // 주민등록번호(Instance ID)와 게임오브젝트 이름 출력
        Debug.Log($"[SETUP 실행됨] ID: {this.GetInstanceID()} / 오브젝트: {gameObject.name} / 부모: {transform.parent?.name}");
        Debug.Log($" -> 받은 데이터 Cost: {cost}, Callback: {(onBuyRequest != null)}");

        if (cardUI != null) cardUI.Setup(item);
        
        if(sellCard == null) sellCard = new SellCard();
        sellCard.cardItem = item;
        sellCard.cost = cost;
        sellCard.isValid = isValid;

        this._playerData = playerData; 
        this._onBuyRequest = onBuyRequest;

        if (item != null) UpdateCardVisual(item);

        if (sellCostTMP != null)
        {
            sellCostTMP.text = "<sprite=0>" + sellCard.cost.ToString();
            UpdateColor(true); 
        }
        gameObject.SetActive(isValid);
    }

    public void OnPointerClick(PointerEventData data)
    {
        // 클릭된 녀석의 주민등록번호 출력
        Debug.Log($"[CLICK 실행됨] ID: {this.GetInstanceID()} / 오브젝트: {gameObject.name} / 부모: {transform.parent?.name}");
        Debug.Log($" -> 가지고 있는 Cost: {sellCard.cost}, Callback: {(_onBuyRequest != null)}");

        if (_onBuyRequest == null)
        {
            Debug.LogError("범인 검거: 이 녀석은 Setup되지 않은 녀석입니다!");
            // return; // 일단 주석처리해서 에러가 나더라도 로그를 다 보게 함
        }
        else
        {
            _onBuyRequest.Invoke();
        }
    }
    void UpdateCardVisual(Item item)
    {
        // 이미지 및 텍스트 활성화
        if(character != null) character.gameObject.SetActive(true);
        if(nameTMP != null) nameTMP.gameObject.SetActive(true);
        if(textTMP != null) textTMP.gameObject.SetActive(true);
        if(cardImg != null) cardImg.color = Color.white;

        // 1. 기본 정보 표시
        if(nameTMP != null) nameTMP.text = item.name;
        if(character != null) character.sprite = item.sprite;
        

        // 3. 코스트 설정 (배열 범위 체크 추가)
        // item.cost가 배열 길이보다 크면 마지막거 사용하거나 예외처리
        if(cost != null && costTypes != null)
        {
            int costIdx = Mathf.Clamp(item.cost, 0, costTypes.Length - 1);
            if (costTypes.Length > 0) cost.sprite = costTypes[costIdx];
        }
        if(costTMP != null) costTMP.text = item.cost.ToString();

        // 4. 희귀도 설정
        if(rarity != null && rarityTypes != null && (int)item.rarity < rarityTypes.Length)
        {
            rarity.sprite = rarityTypes[(int)item.rarity];
        }

        // 5. 텍스트 파싱 (CardUI의 Regex 로직)
        if(textTMP != null)
        {
            string showText = item.text;
            // 설명 텍스트 내의 수치 변환 로직 (CardUI 그대로)
            // item.cardValues 리스트가 비어있지 않다고 가정
            if (item.cardValues != null && item.cardValues.Count > 0)
            {
                int index = 0;
                showText = Regex.Replace(showText, @"(\d+)(<(피해|수비|회복|특수)>)?", match => 
                {
                    if (index < item.cardValues.Count)
                        return item.cardValues[index++].ToString();
                    return match.Value;
                });
            }
            textTMP.text = showText;
        }
    }


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