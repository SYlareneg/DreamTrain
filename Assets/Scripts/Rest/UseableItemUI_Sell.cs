using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UseableItemUI_Sell : MonoBehaviour, IPointerClickHandler
{
    public SellUItem uItem;
    [SerializeField] Image image;
    [SerializeField] TMP_Text useableItemCostTMP;

    public void Setup(UseItem item, int cost, bool isValid)
    {
        uItem.useItem = item;
        image.sprite = item.sprite;
        uItem.cost = cost;
        useableItemCostTMP.text = "<sprite=0>" + cost.ToString();
        uItem.isValid = isValid;
        gameObject.SetActive(isValid);
    }

    public void OnPointerClick(PointerEventData data)
    {
        if (PlayerManager.Inst.characterSO.dreamDust < uItem.cost) return;
        PlayerManager.Inst.characterSO.dreamDust -= uItem.cost;
        NPCMerchantManager.Inst.AddUseableItem(uItem.useItem);
        gameObject.SetActive(false);
        uItem.isValid = false;
    }

    private void Update()
    {
        if (PlayerManager.Inst.characterSO.dreamDust < uItem.cost)
        {
            useableItemCostTMP.color = Color.red;
        }
        else
        {
            useableItemCostTMP.color = Color.blue;
        }
    }
}
