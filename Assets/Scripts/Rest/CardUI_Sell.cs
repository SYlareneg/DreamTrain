using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardUI_Sell : CardUI, IPointerClickHandler
{
    [SerializeField] TMP_Text sellCostTMP;
    public SellCard sellCard;
    private void Awake()
    {
        sellCard.cost = 1;
    }

    public void Setup(Item item, int cost, bool isValid)
    {
        base.Setup(item);
        sellCard = new SellCard();
        sellCard.cardItem = item;
        sellCard.cost = cost;
        sellCard.isValid = isValid;
        sellCostTMP.text = "<sprite=0>" + sellCard.cost.ToString();
        gameObject.SetActive(isValid);
    }
    public void OnPointerClick(PointerEventData data)
    {
        if (PlayerManager.Inst.characterSO.dreamDust < sellCard.cost) return;
        PlayerManager.Inst.characterSO.dreamDust -= sellCard.cost;
        NPCMerchantManager.Inst.AddCard(this.item);
        gameObject.SetActive(false);
        sellCard.isValid = false;
    }

    private void Update()
    {
        if (PlayerManager.Inst.characterSO.dreamDust < sellCard.cost)
        {
            sellCostTMP.color = Color.red;
        }
        else
        {
            sellCostTMP.color = Color.blue;
        }
    }
}
