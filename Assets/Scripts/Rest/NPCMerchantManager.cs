using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Linq;

public class NPCMerchantManager : MonoBehaviour
{
    public static NPCMerchantManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] GameObject merchantUI;
    public Merchant merchant;
    [SerializeField] CharacterSO characterSO;
    [SerializeField] ItemSO playerDeckSO;
    [SerializeField] RelicSO playerRelicSO;
    [Header("카드 구매")]
    [SerializeField] CardUI_Sell[] sellCards;
    [SerializeField] DreamPieceSO dreamPieceListSO;
    [SerializeField] ItemSO normalItemListSO;
    [SerializeField] int[] sellCardCosts = new int[7];
    [SerializeField] float[] sellCardWeights = new float[4];
    [SerializeField] float enhanceProbability;
    [SerializeField] Button rerollButton;
    [SerializeField] TMP_Text rerollTMP;
    [SerializeField] int rerollCost;
    [Header("카드 강화")]
    [SerializeField] GameObject cardEnhanceScreen;
    [SerializeField] GameObject cardEnhanceList;
    [SerializeField] Button cardEnhanceButton;
    [SerializeField] TMP_Text cardEnhanceButtonTMP;
    [SerializeField] GameObject enhanceCardPrefab;
    CardUI_Enhance enhanceCard;
    [SerializeField] GameObject cardEnhanceConfirmScreen;
    [SerializeField] CardUI beforeEnhance_C;
    [SerializeField] CardUI afterEnhance_C;
    [SerializeField] int cardEnhanceCost;
    [SerializeField] TMP_Text cardEnhanceConfirmButtonTMP;
    [Header("이드 강화")]
    [SerializeField] RelicSO relicListSO;
    [SerializeField] GameObject relicEnhanceScreen;
    [SerializeField] GameObject relicEnhanceList;
    [SerializeField] Button relicEnhanceButton;
    [SerializeField] TMP_Text relicEnhanceButtonTMP;
    [SerializeField] GameObject enhanceRelicPrefab;
    RelicItem_Enhanceable enhanceRelicItem;
    RelicHalfUI_Enhance enhanceRelicUI;
    [SerializeField] GameObject relicEnhanceConfirmScreen;
    [SerializeField] RelicUI beforeEnhance_R;
    [SerializeField] TMP_Text beforeEnhance_R_Name;
    [SerializeField] TMP_Text beforeEnhance_R_Text;
    [SerializeField] RelicUI afterEnhance_R;
    [SerializeField] TMP_Text afterEnhance_R_Name;
    [SerializeField] TMP_Text afterEnhance_R_Text;
    [SerializeField] int relicEnhanceCost;
    [SerializeField] TMP_Text relicEnhanceConfirmButtonTMP;
    [Header("소모품 구매")]
    [SerializeField] UseableItemUI_Sell[] useableItems;
    [SerializeField] UseableItemSO useableItemListSO;
    [SerializeField] UseableItemSO playerUseableItemSO;
    [SerializeField] int[] useableItemCosts = new int[2];

    public void ShowMerchantUI()
    {
        PlayerManager.Inst.isLoading = true;
        SetSellCards();
        SetSellUItems();
        merchantUI.SetActive(true);
    }
    public void HideMerchantUI()
    {
        PlayerManager.Inst.isLoading = false;
        merchantUI.SetActive(false);
    }

    // 카드 구매
    public void SetSellCards()
    {
        List<Item> shareCards = normalItemListSO.items;
        List<Item> normalCards = new List<Item>();
        List<Item> personaCards = new List<Item>();
        List<Item> shadowCards = new List<Item>();
        List<Item> normalCards_enhanced = new List<Item>();
        List<Item> personaCards_enhanced = new List<Item>();
        List<Item> shadowCards_enhanced = new List<Item>();
        foreach (Item_Enhanceable item in characterSO.personaPiece.cards)
        {
            if (item.element == EPassiveType.Normal)
            {
                normalCards.Add((Item)item);
                normalCards_enhanced.Add(item.enhancedItem);
            }
            else if (item.element == EPassiveType.Persona)
            {
                personaCards.Add((Item)item);
                personaCards_enhanced.Add(item.enhancedItem);
            }
        }
        foreach (Item_Enhanceable item in characterSO.shadowPiece.cards)
        {
            if (item.element == EPassiveType.Normal)
            {
                normalCards.Add((Item)item);
                normalCards_enhanced.Add(item.enhancedItem);
            }
            else if (item.element == EPassiveType.Shadow)
            {
                shadowCards.Add((Item)item);
                shadowCards_enhanced.Add(item.enhancedItem);
            }
        }
        foreach (CardUI_Sell sc in sellCards)
        {
            bool isE = Random.value < enhanceProbability;
            float totalW = 0f;
            for (int i = isE ? 1 : 0; i < sellCardWeights.Length; i++)
            {
                totalW += sellCardWeights[i];
            }
            float rPoint = Random.value * totalW;
            int chooseCardPool = isE ? 1 : 0;
            for (int i = isE ? 1 : 0; i < sellCardWeights.Length; i++)
            {
                if (rPoint < sellCardWeights[i])
                {
                    chooseCardPool = i;
                    break;
                }
                rPoint -= sellCardWeights[i];
            }
            List<Item> lookat;
            int sellCost = sellCardCosts[0];
            if (chooseCardPool > 0) sellCost = sellCardCosts[chooseCardPool * 2 + (isE ? 0 : -1)];
            switch (chooseCardPool)
            {
                case 0:
                    lookat = shareCards;
                    break;
                case 1:
                    if (isE) lookat = normalCards_enhanced;
                    else lookat = normalCards;
                    break;
                case 2:
                    if (isE) lookat = personaCards_enhanced;
                    else lookat = personaCards;
                    break;
                case 3:
                    if (isE) lookat = shadowCards_enhanced;
                    else lookat = shadowCards;
                    break;
                default:
                    lookat = new List<Item>();
                    break;
            }
            int cardIdx = Random.Range(0, lookat.Count);
            sc.Setup(lookat[cardIdx]);
            sc.SetSellCost(sellCost);
            sc.gameObject.SetActive(true);
        }
    }

    public void RerollSellCards()
    {
        characterSO.dreamDust -= rerollCost;
        SetSellCards();
    }

    public void AddCard(Item item)
    {
        Item newItem = new Item();
        newItem.SetItem(item);
        newItem.num = 1;
        playerDeckSO.items.Add(newItem);
    }

    // 카드 강화

    public void SetEnhanceCardList()
    {
        foreach (Transform child in cardEnhanceList.transform)
        {
            Destroy(child.gameObject);
        }
        foreach (Item item in playerDeckSO.items)
        {
            if (item.isEnhanced == false && item.dreamPieceNum >= 0)
            {
                var cardObj = Instantiate(enhanceCardPrefab, cardEnhanceList.transform, false);
                cardObj.transform.SetParent(cardEnhanceList.transform);
                CardUI_Enhance cardUI_Enhance = cardObj.GetComponent<CardUI_Enhance>();
                cardUI_Enhance.Setup(item);
            }
        }
        cardEnhanceScreen.SetActive(true);
        relicEnhanceScreen.SetActive(false);
    }

    public void EnhanceCardSelect(CardUI_Enhance card)
    {
        Item_Enhanceable eItem = dreamPieceListSO.dreamPieces[card.item.dreamPieceNum].cards.Find(x => x.name == card.item.name);
        if (eItem == null) return;
        enhanceCard = card;
        beforeEnhance_C.Setup(card.item);
        afterEnhance_C.Setup(eItem.enhancedItem);
        cardEnhanceConfirmButtonTMP.text = "강화(<sprite=0>" + cardEnhanceCost.ToString() + ")";
        cardEnhanceConfirmScreen.SetActive(true);
    }

    public void EnhanceCardConfirm()
    {
        characterSO.dreamDust -= cardEnhanceCost;
        playerDeckSO.items.Remove(beforeEnhance_C.item);
        playerDeckSO.items.Add(afterEnhance_C.item);
        Destroy(enhanceCard.gameObject);
        cardEnhanceConfirmScreen.SetActive(false);
    }

    // 이드 강화

    public void SetEnhanceRelicList()
    {
        foreach (Transform child in relicEnhanceList.transform)
        {
            Destroy(child.gameObject);
        }
        List<RelicItem_Enhanceable> sortedRelicList = playerRelicSO.relicItems.OrderBy(x => x.relicOwner).ToList();
        for (int i = 0; i < sortedRelicList.Count; i++)
        {
            var relicObject = Instantiate(enhanceRelicPrefab, relicEnhanceList.transform, false);
            relicObject.transform.SetParent(relicEnhanceList.transform);
            var relic = relicObject.GetComponent<RelicUI>();
            if (i < sortedRelicList.Count - 1 && sortedRelicList[i + 1].relicOwner == sortedRelicList[i].relicOwner)
            {
                var relic1 = sortedRelicList[i].isEnhanced ? sortedRelicList[i].enhancedRelicItem : sortedRelicList[i];
                var relic2 = sortedRelicList[i + 1].isEnhanced ? sortedRelicList[i + 1].enhancedRelicItem : sortedRelicList[i + 1];
                relic.Setup(relic1, relic2);
                i++;
            }
            else
            {
                var relic1 = sortedRelicList[i].isEnhanced ? sortedRelicList[i].enhancedRelicItem : sortedRelicList[i];
                relic.Setup(relic1, null);
            }
        }
        cardEnhanceScreen.SetActive(false);
        relicEnhanceScreen.SetActive(true);
    }

    public void EnhanceRelicSelect(RelicHalfUI_Enhance relicHalf)
    {
        RelicItem_Enhanceable rItem = relicListSO.relicItems.Find(x => x.relicName == relicHalf.relicItem.relicName);
        if (rItem == null) return;
        Debug.Log(rItem.relicName);
        enhanceRelicItem = rItem;
        enhanceRelicUI = relicHalf;
        beforeEnhance_R.Setup(rItem, null);
        beforeEnhance_R_Name.text = rItem.relicName;
        beforeEnhance_R_Text.text = rItem.relicTxt;
        afterEnhance_R.Setup(rItem.enhancedRelicItem, null);
        afterEnhance_R_Name.text = rItem.enhancedRelicItem.relicName;
        afterEnhance_R_Text.text = rItem.enhancedRelicItem.relicTxt;
        relicEnhanceConfirmButtonTMP.text = "강화(<sprite=0>" + relicEnhanceCost.ToString() + ")";
        relicEnhanceConfirmScreen.SetActive(true);
    }

    public void EnhanceRelicConfirm()
    {
        RelicItem_Enhanceable rItem = playerRelicSO.relicItems.Find(x => x.relicName == enhanceRelicItem.relicName);
        if (rItem == null) return;
        characterSO.dreamDust -= relicEnhanceCost;
        rItem.isEnhanced = true;
        enhanceRelicUI.SetRelicHalf(rItem.enhancedRelicItem);
        relicEnhanceConfirmScreen.SetActive(false);
    }

    // 소모품 구매

    public void SetSellUItems()
    {
        foreach(UseableItemUI_Sell uI_Sell in useableItems)
        {
            int uItemIdx = Random.Range(0, playerUseableItemSO.useableItems.Count);
            var setUItem = useableItemListSO.useableItems[uItemIdx];
            uI_Sell.Setup(setUItem);
            uI_Sell.SetCost(sellCardCosts[setUItem.rarity]);
            uI_Sell.gameObject.SetActive(true);
        }
    }
    
    public void AddUseableItem(UseItem uItem)
    {
        UseItem useItem = new UseItem();
        useItem.Setup(uItem);
        playerUseableItemSO.useableItems.Add(useItem);
    }
    
    void Update()
    {
        if (characterSO.dreamDust < rerollCost)
        {
            rerollButton.interactable = false;
            rerollTMP.color = Color.red;
        }
        else
        {
            rerollButton.interactable = true;
            rerollTMP.color = Color.blue;
        }
        rerollTMP.text = "새로고침:<sprite=0>" + rerollCost.ToString();

        if (characterSO.dreamDust < cardEnhanceCost)
        {
            cardEnhanceButton.interactable = false;
            cardEnhanceButtonTMP.color = Color.red;
        }
        else
        {
            cardEnhanceButton.interactable = true;
            cardEnhanceButtonTMP.color = Color.blue;
        }
        cardEnhanceButtonTMP.text = "<sprite=0>" + cardEnhanceCost.ToString();
        
        if (characterSO.dreamDust < relicEnhanceCost)
        {
            relicEnhanceButton.interactable = false;
            relicEnhanceButtonTMP.color = Color.red;
        }
        else
        {
            relicEnhanceButton.interactable = true;
            relicEnhanceButtonTMP.color = Color.blue;
        }
        relicEnhanceButtonTMP.text = "<sprite=0>" + relicEnhanceCost.ToString();
    }
}
