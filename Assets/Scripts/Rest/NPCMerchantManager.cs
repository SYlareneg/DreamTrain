using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using System.Linq;
using System;

public class NPCMerchantManager : MonoBehaviour
{
    public static NPCMerchantManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] GameObject merchantUI;
    public Merchant merchant;
    [SerializeField] CharacterSO characterSO;
    [SerializeField] RelicSO playerRelicSO;
    [SerializeField] StageSO stageSO;
    [Header("카드 구매")]
    [SerializeField] CardUI_Sell[] sellCards;
    [SerializeField] DreamPieceSO dreamPieceListSO;
    [SerializeField] ItemSO normalItemListSO;
    [SerializeField] int[] sellCardCosts = new int[Enum.GetNames(typeof(CardRarity)).Length * 2 + 1];
    [SerializeField] float[] sellCardWeights = new float[Enum.GetNames(typeof(CardRarity)).Length + 1];
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
        if(stageSO.merchantSellCards.Count == 0)
        {
            SetSellCards();
        }
        else
        {
            for(int i = 0; i < sellCards.Length; i++)
            {
                sellCards[i].Setup(stageSO.merchantSellCards[i].cardItem, stageSO.merchantSellCards[i].cost, stageSO.merchantSellCards[i].isValid);
            }
        }
        if(stageSO.merchantSellUItems.Count == 0)
        {
            SetSellUItems();
        }
        else
        {
            for(int i = 0; i < useableItems.Length; i++)
            {
                useableItems[i].Setup(stageSO.merchantSellUItems[i].useItem, stageSO.merchantSellUItems[i].cost, stageSO.merchantSellUItems[i].isValid);
            }
        }
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
        stageSO.merchantSellCards.Clear();

        List<Item> shareCards = normalItemListSO.items;
        List<Item> normalCards = new List<Item>();
        List<Item>[] dreamCards = new List<Item>[Enum.GetNames(typeof(CardRarity)).Length];
        List<Item>[] dreamCards_enhanced = new List<Item>[Enum.GetNames(typeof(CardRarity)).Length];
        DreamPiece_Reference persona_ref = dreamPieceListSO.dreamPieces.Find(x => x.name == characterSO.personaPiece.name);
        DreamPiece_Reference shadow_ref = dreamPieceListSO.dreamPieces.Find(x => x.name == characterSO.shadowPiece.name);
        foreach(Item item in normalItemListSO.items)
        {
            normalCards.Add(item);
        }
        foreach (Item_Enhanceable item in persona_ref.cards)
        {
            dreamCards[(int)item.rarity].Add((Item)item);
            dreamCards_enhanced[(int)item.rarity].Add(item.enhancedItem);
        }
        foreach (Item_Enhanceable item in shadow_ref.cards)
        {
            dreamCards[(int)item.rarity].Add((Item)item);
            dreamCards_enhanced[(int)item.rarity].Add(item.enhancedItem);
        }
        foreach (CardUI_Sell sc in sellCards)
        {
            float totalW = 0f;
            for (int i = 0; i < sellCardWeights.Length; i++)
            {
                totalW += sellCardWeights[i];
            }
            float rPoint = Random.value * totalW;
            int chooseCardPool = 0;
            for (int i = 0; i < sellCardWeights.Length; i++)
            {
                if (rPoint < sellCardWeights[i])
                {
                    chooseCardPool = i;
                    break;
                }
                rPoint -= sellCardWeights[i];
            }
            List<Item> lookat = new List<Item>();
            int sellCost = sellCardCosts[0];
            if(chooseCardPool == 0)
            {
                lookat = normalCards;
                sellCost = sellCardCosts[0];
            }
            else if(chooseCardPool > 0 && chooseCardPool <= Enum.GetNames(typeof(CardRarity)).Length)
            {
                bool isE = Random.value < enhanceProbability;
                if(isE)
                {
                    lookat = dreamCards_enhanced[chooseCardPool - 1];
                    sellCost = sellCardCosts[chooseCardPool * 2];
                }
                else
                {
                    lookat = dreamCards[chooseCardPool - 1];
                    sellCost = sellCardCosts[chooseCardPool * 2 - 1];
                }
            }
            int cardIdx = Random.Range(0, lookat.Count);

            sc.Setup(lookat[cardIdx], sellCost, true);
            stageSO.merchantSellCards.Add(sc.sellCard);
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
        if(item.dreamPieceNum < 0)
        {
            var existItem = characterSO.normalCards.Find(x => x.name == item.name);
            if(existItem == null) characterSO.normalCards.Add(newItem);
            else existItem.num++;
        }
        else if(dreamPieceListSO.dreamPieces[item.dreamPieceNum].name == characterSO.personaPiece.name)
        {
            var existItem = characterSO.personaPiece.cards.Find(x => x.name == item.name);
            if(existItem == null) characterSO.personaPiece.cards.Add(newItem);
            else existItem.num++;
        }
        else if(dreamPieceListSO.dreamPieces[item.dreamPieceNum].name == characterSO.shadowPiece.name)
        {
            var existItem = characterSO.shadowPiece.cards.Find(x => x.name == item.name);
            if(existItem == null) characterSO.shadowPiece.cards.Add(newItem);
            else existItem.num++;
        }
        else
        {
            Debug.LogError("undefined card added!");
        }
    }

    // 카드 강화

    public void SetEnhanceCardList()
    {
        foreach (Transform child in cardEnhanceList.transform)
        {
            Destroy(child.gameObject);
        }
        List<Item> allCardItemList = characterSO.normalCards.Concat(characterSO.personaPiece.cards).Concat(characterSO.shadowPiece.cards).ToList();
        foreach (Item item in allCardItemList)
        {
            if (item.isEnhanced == false && item.dreamPieceNum >= 0)
            {
                for(int i = 0; i < item.num; i++)
                {
                    var cardObj = Instantiate(enhanceCardPrefab, cardEnhanceList.transform, false);
                    cardObj.transform.SetParent(cardEnhanceList.transform);
                    CardUI_Enhance cardUI_Enhance = cardObj.GetComponent<CardUI_Enhance>();
                    cardUI_Enhance.Setup(item);
                }
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
        beforeEnhance_C.item.num--;
        if(beforeEnhance_C.item.num == 0)
        {
            if(beforeEnhance_C.item.dreamPieceNum < 0)
            {
                characterSO.normalCards.Remove(beforeEnhance_C.item);
            }
            else if(dreamPieceListSO.dreamPieces[beforeEnhance_C.item.dreamPieceNum].name == characterSO.personaPiece.name)
            {
                characterSO.personaPiece.cards.Remove(beforeEnhance_C.item);
            }
            else if(dreamPieceListSO.dreamPieces[beforeEnhance_C.item.dreamPieceNum].name == characterSO.shadowPiece.name)
            {
                characterSO.shadowPiece.cards.Remove(beforeEnhance_C.item);
            }
            else
            {
                Debug.LogError("unknown card enhanced!");
            }
        }
        AddCard(afterEnhance_C.item);
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
        stageSO.merchantSellUItems.Clear();

        foreach(UseableItemUI_Sell uI_Sell in useableItems)
        {
            int uItemIdx = Random.Range(0, useableItemListSO.useableItems.Count);
            var setUItem = useableItemListSO.useableItems[uItemIdx];
            uI_Sell.Setup(setUItem, sellCardCosts[setUItem.rarity], true);
            stageSO.merchantSellUItems.Add(uI_Sell.uItem);
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
