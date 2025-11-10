using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UseableItemUI_Sell : MonoBehaviour, IPointerClickHandler
{
    UseItem uItem;
    [SerializeField] Image image;
    [SerializeField] TMP_Text useableItemCostTMP;
    [SerializeField] int sellCost;

    public void Setup(UseItem item)
    {
        uItem = item;
        image.sprite = item.sprite;
        useableItemCostTMP.text = "<sprite=0>" + sellCost.ToString();
    }

    public void SetCost(int c)
    {
        sellCost = c;
        useableItemCostTMP.text = "<sprite=0>" + sellCost.ToString();
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (PlayerManager.Inst.characterSO.dreamDust < sellCost) return;
        PlayerManager.Inst.characterSO.dreamDust -= sellCost;
        NPCMerchantManager.Inst.AddUseableItem(uItem);
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (PlayerManager.Inst.characterSO.dreamDust < sellCost)
        {
            useableItemCostTMP.color = Color.red;
        }
        else
        {
            useableItemCostTMP.color = Color.blue;
        }
    }
}
