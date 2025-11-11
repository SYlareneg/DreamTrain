using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CardUI_Sell : CardUI, IPointerClickHandler
{
    [SerializeField] TMP_Text sellCostTMP;
    int sellCost;
    private void Awake()
    {
        sellCost = 1;
    }
    public void SetSellCost(int c)
    {
        sellCost = c;
        sellCostTMP.text = "<sprite=0>" + sellCost.ToString();
    }
    public void OnPointerClick(PointerEventData data)
    {
        if (PlayerManager.Inst.characterSO.dreamDust < sellCost) return;
        PlayerManager.Inst.characterSO.dreamDust -= sellCost;
        NPCMerchantManager.Inst.AddCard(this.item);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (PlayerManager.Inst.characterSO.dreamDust < sellCost)
        {
            sellCostTMP.color = Color.red;
        }
        else
        {
            sellCostTMP.color = Color.blue;
        }
    }
}
