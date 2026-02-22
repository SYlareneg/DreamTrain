using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class EncounterCardUI_Sell : MonoBehaviour, IPointerClickHandler
{
    [Header("Core Reference")]
    public CardUI cardUI; 

    [Header("Shop UI")]
    public TMP_Text sellCostTMP;    

    public SellCard sellCard;       
    private CharacterSO _playerData; 
    private System.Action _onBuyRequest; 

    private bool _wasAffordable = true; 

    private void Awake()
    {
        if(sellCard == null) sellCard = new SellCard();
    }

    public void Setup(Item item, int cost, bool isValid, CharacterSO playerData, System.Action onBuyRequest)
    {
        if(sellCard == null) sellCard = new SellCard();
        sellCard.cardItem = item;
        sellCard.cost = cost;
        sellCard.isValid = isValid;

        this._playerData = playerData; 
        this._onBuyRequest = onBuyRequest;

        if (cardUI != null)
        {
            cardUI.Setup(item);
        }
        else
        {
            Debug.LogError($"[EncounterCardUI_Sell] CardUI가 연결되지 않았습니다! 프리팹을 확인하세요. ({gameObject.name})");
        }   

        if (sellCostTMP != null)
        {
            sellCostTMP.text = sellCard.cost.ToString();
            
            UpdateColor(true);
        }

        gameObject.SetActive(isValid);
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (_onBuyRequest != null)
        {
            _onBuyRequest.Invoke();
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
        sellCostTMP.color = isAffordable ? Color.white : Color.red;
    }
}