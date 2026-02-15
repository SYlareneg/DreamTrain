using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using Random = UnityEngine.Random;

public class EncounterMerchantUI : MonoBehaviour
{
    public static EncounterMerchantUI Inst;

    [Header("Core References")]
    public GameObject panelRoot;            
    public CharacterSO characterSO;         
    public StageSO stageSO;                 
    public DreamPieceSO dreamPieceListSO;  
    public PlayerDataSO playerDataSO;  
    public RelicDataSO relicDataList;
    public RelicSO playerRelicSO;
    public PlayerStatsSo playerStatsSO;
    public EncounterMenuControll menuControll;
    public ItemSO normalItemListSO;
    
    [Header("Databases")]
    public ItemDataSO normalItemDataListSO; 
    public UseableItemSO useableItemListSO; 
    
    [Header("UI Containers")]
    public Transform cardContainer;   
    public GameObject cardSectionObj;  
    public Transform objetContainer;  
    public GameObject objetSectionObj;
    public GameObject objetRareSectionObj;
    public Transform objetRareContainer;
    public GameObject consumableSectionObj;
    public GameObject junkShopSectionObj;
    public Transform objetJunkContainer;
    public Transform junkChosenContainer;
    public Button junkSellButton;
    public Image junkSellButtonImage;
    public Sprite buttonBasicSprite;           
    public Sprite buttonPressedSprite;
    
    [Header("Prefabs")]
    public GameObject sellCardPrefab;       
    public GameObject objetPrefab;        
    public GameObject objetRarePrefab;      

    [Header("Card Enhance UI")]
    public GameObject cardEnhanceScreen;        
    public Transform cardEnhanceList;           
    public GameObject enhanceCardPrefab;        
    
    public GameObject cardEnhanceConfirmScreen; 
    public CardUI beforeEnhance_C;              
    public CardUI afterEnhance_C;               
    public TMP_Text cardEnhanceConfirmButtonTMP; 
    
    public int cardEnhanceCost = 100;           
    public CardUI_Enhance currentSelectedCard; 

    [Header("Relic Enhance UI")]
    public RelicSO relicListSO;                 
    public GameObject relicEnhanceScreen;       
    public Transform relicEnhanceList;          
    public GameObject enhanceRelicPrefab;       
    
    public GameObject relicEnhanceConfirmScreen;
    public RelicUI beforeEnhance_R;             
    public TMP_Text beforeEnhance_R_Name;       
    public TMP_Text beforeEnhance_R_Text;
    public RelicUI afterEnhance_R;              
    public TMP_Text afterEnhance_R_Name;
    public TMP_Text afterEnhance_R_Text;
    public TMP_Text relicEnhanceConfirmButtonTMP;

    public int relicEnhanceCost = 150;          
    private RelicItem_Enhanceable currentEnhanceRelicItem; 
    private RelicHalfUI_Enhance currentEnhanceRelicUI;     
    
    [Header("Settings")]
    public string currentShopId = "";
    private RelicItem_Enhanceable currentRelicToSell;
    //public float[] rewardCardWeights = new float[Enum.GetNames(typeof(CardRarity)).Length + 1];
    public float[] rewardCardWeights = { 20f, 60f, 20f };
    float enhanceProbability = 0.2f;
    
    public void Awake()
    {
        Inst = this; // 싱글톤 할당

        if (panelRoot == null) panelRoot = this.gameObject;

        if (cardContainer == null)
        {
            Transform findT = transform.Find("CardContainer");
            if (findT != null) cardContainer = findT;
        }

        if (objetContainer == null)
        {
            Transform findObjT = transform.Find("ObjetContainer");
            if (findObjT != null) objetContainer = findObjT;
        }
        
        Button[] allButtons = panelRoot.GetComponentsInChildren<Button>(true);
        foreach (var btn in allButtons)
        {
            if (btn.name == "ExitButton" || btn.name == "CloseButton" || btn.name.Contains("Exit"))
            {
                btn.onClick.RemoveAllListeners(); 
                btn.onClick.AddListener(OnClickExitButton);
                break;
            }
        }
        if(junkSellButton != null)
        {
            junkSellButton.onClick.RemoveAllListeners();
            junkSellButton.onClick.AddListener(OnClickSellRelic);
        }
    }

    public void Open(string shopId = "")
    {
        currentShopId = shopId;
        panelRoot.SetActive(true);
        if(cardEnhanceScreen != null) cardEnhanceScreen.SetActive(false);
        if(relicEnhanceScreen != null) relicEnhanceScreen.SetActive(false);
        if(cardEnhanceConfirmScreen != null) cardEnhanceConfirmScreen.SetActive(false);
        if(relicEnhanceConfirmScreen != null) relicEnhanceConfirmScreen.SetActive(false);
        
        currentRelicToSell = null;
        GenerateShopInventory();
        
    }
    
    public void OpenEnhanceCardScreen()
    {
        SetEnhanceCardList();
        cardEnhanceScreen.SetActive(true);
        if(relicEnhanceScreen) relicEnhanceScreen.SetActive(false);
    }

    public void SetEnhanceCardList()
    {
        foreach (Transform child in cardEnhanceList)
        {
            Destroy(child.gameObject);
        }

        List<Item> allCardItemList = characterSO.normalCards
            .Concat(characterSO.personaPiece.cards)
            .Concat(characterSO.shadowPiece.cards)
            .ToList();

        foreach (Item item in allCardItemList)
        {
            if (item.isEnhanced == false && item.dreamPieceNum >= 0)
            {
                for(int i = 0; i < item.num; i++)
                {
                    var cardObj = Instantiate(enhanceCardPrefab, cardEnhanceList, false);
                    // Instantiate 할 때 부모를 지정하지 않고, 여기서 transform.SetParent 사용 (원본 스타일)
                    // (단, Instantiate(prefab, parent)가 더 깔끔하긴 합니다)
                    cardObj.transform.SetParent(cardEnhanceList); 
                    
                    CardUI_Enhance cardUI = cardObj.GetComponent<CardUI_Enhance>();
                    if (cardUI != null) cardUI.Setup(item);
                }
            }
        }
    }

    public void EnhanceCardSelect(CardUI_Enhance cardUI)
    {
        Item_Enhanceable eItem = null;
        if (cardUI.item.dreamPieceNum >= 0 && cardUI.item.dreamPieceNum < dreamPieceListSO.dreamPieces.Count)
        {
             eItem = dreamPieceListSO.dreamPieces[cardUI.item.dreamPieceNum].cards.Find(x => x.name == cardUI.item.name);
        }

        if (eItem == null) return;

        currentSelectedCard = cardUI;

        beforeEnhance_C.Setup(cardUI.item);
        afterEnhance_C.Setup(eItem.enhancedItem);
        
        if(cardEnhanceConfirmButtonTMP)
            cardEnhanceConfirmButtonTMP.text = "강화(<sprite=0>" + cardEnhanceCost.ToString() + ")";
        
        cardEnhanceConfirmScreen.SetActive(true);
    }

    public void EnhanceCardConfirm()
    {
        if (characterSO.dreamDust < cardEnhanceCost) return;

        characterSO.dreamDust -= cardEnhanceCost;
        
        currentSelectedCard.item.num--;
        if(currentSelectedCard.item.num == 0)
        {
            if(currentSelectedCard.item.dreamPieceNum < 0)
            {
                characterSO.normalCards.Remove(currentSelectedCard.item);
            }
            else if(dreamPieceListSO.dreamPieces[currentSelectedCard.item.dreamPieceNum].name == characterSO.personaPiece.name)
            {
                characterSO.personaPiece.cards.Remove(currentSelectedCard.item);
            }
            else if(dreamPieceListSO.dreamPieces[currentSelectedCard.item.dreamPieceNum].name == characterSO.shadowPiece.name)
            {
                characterSO.shadowPiece.cards.Remove(currentSelectedCard.item);
            }
        }

        AddCardToInventory(afterEnhance_C.item);

        Destroy(currentSelectedCard.gameObject);
        cardEnhanceConfirmScreen.SetActive(false);
    }

    public void OpenEnhanceRelicScreen()
    {
        SetEnhanceRelicList();
        relicEnhanceScreen.SetActive(true);
        if(cardEnhanceScreen) cardEnhanceScreen.SetActive(false);
    }

    public void SetEnhanceRelicList()
    {
        foreach (Transform child in relicEnhanceList)
        {
            Destroy(child.gameObject);
        }

        List<RelicItem_Enhanceable> sortedRelicList = playerRelicSO.relicItems.OrderBy(x => x.relicOwner).ToList();
        
        for (int i = 0; i < sortedRelicList.Count; i++)
        {
            var relicObject = Instantiate(enhanceRelicPrefab, relicEnhanceList, false);
            var relicUI = relicObject.GetComponent<RelicUI>();
            
            if (relicUI != null)
            {
                if (i < sortedRelicList.Count - 1 && sortedRelicList[i + 1].relicOwner == sortedRelicList[i].relicOwner)
                {
                    var relic1 = sortedRelicList[i].isEnhanced ? sortedRelicList[i].enhancedRelicItem : sortedRelicList[i];
                    var relic2 = sortedRelicList[i + 1].isEnhanced ? sortedRelicList[i + 1].enhancedRelicItem : sortedRelicList[i + 1];
                    relicUI.Setup(relic1, relic2);
                    i++;
                }
                else
                {
                    var relic1 = sortedRelicList[i].isEnhanced ? sortedRelicList[i].enhancedRelicItem : sortedRelicList[i];
                    relicUI.Setup(relic1, null);
                }
            }
        }
    }

    public void EnhanceRelicSelect(RelicHalfUI_Enhance relicHalf)
    {
        RelicItem_Enhanceable rItem = relicListSO.relicItems.Find(x => x.relicName == relicHalf.relicItem.relicName);
        if (rItem == null) return;

        currentEnhanceRelicItem = rItem;
        currentEnhanceRelicUI = relicHalf;

        beforeEnhance_R.Setup(rItem, null);
        beforeEnhance_R_Name.text = rItem.relicName;
        beforeEnhance_R_Text.text = rItem.relicTxt;

        afterEnhance_R.Setup(rItem.enhancedRelicItem, null);
        afterEnhance_R_Name.text = rItem.enhancedRelicItem.relicName;
        afterEnhance_R_Text.text = rItem.enhancedRelicItem.relicTxt;

        if(relicEnhanceConfirmButtonTMP)
            relicEnhanceConfirmButtonTMP.text = "강화(<sprite=0>" + relicEnhanceCost.ToString() + ")";
        
        relicEnhanceConfirmScreen.SetActive(true);
    }

    public void EnhanceRelicConfirm()
    {
        RelicItem_Enhanceable rItem = playerRelicSO.relicItems.Find(x => x.relicName == currentEnhanceRelicItem.relicName);
        if (rItem == null) return;
        
        if (characterSO.dreamDust < relicEnhanceCost) return;

        characterSO.dreamDust -= relicEnhanceCost;
        rItem.isEnhanced = true;
        
        currentEnhanceRelicUI.SetRelicHalf(rItem.enhancedRelicItem);
        
        relicEnhanceConfirmScreen.SetActive(false);
    }
    
    void GenerateShopInventory()
    {
        stageSO.merchantSellCards.Clear();
        stageSO.merchantSellUItems.Clear();
        stageSO.merchantSellObjets.Clear();

        switch (currentShopId)
        {
            case "souvenir":
                if (cardSectionObj) cardSectionObj.SetActive(false);
                if (consumableSectionObj) consumableSectionObj.SetActive(false);
                if (objetSectionObj) objetSectionObj.SetActive(false);
                if(junkShopSectionObj) junkShopSectionObj.SetActive(false);
                if (objetRareSectionObj) objetRareSectionObj.SetActive(true);
                GenerateSpecialShopInventory();
                DrawRareObjets();
                break;
            case "IceCreamShop":
                if (cardSectionObj) cardSectionObj.SetActive(false);
                if (consumableSectionObj) consumableSectionObj.SetActive(false);
                if (objetSectionObj) objetSectionObj.SetActive(false);
                if(junkShopSectionObj) junkShopSectionObj.SetActive(false);
                if (objetRareSectionObj) objetRareSectionObj.SetActive(true);
                GenerateIcecreamShopInventory();
                DrawRareObjets();
                break;
            case "junkShop":
                if (cardSectionObj) cardSectionObj.SetActive(false);
                if (consumableSectionObj) consumableSectionObj.SetActive(false);
                if (objetSectionObj) objetSectionObj.SetActive(false);
                if (objetRareSectionObj) objetRareSectionObj.SetActive(false);
                if(junkShopSectionObj) junkShopSectionObj.SetActive(true);
                DrawJunkObjets();
                break;
            default:
                if (cardSectionObj) cardSectionObj.SetActive(true);
                if (objetSectionObj) objetSectionObj.SetActive(true);
                if (objetRareSectionObj) objetRareSectionObj.SetActive(false);
                if(junkShopSectionObj) junkShopSectionObj.SetActive(false);

                GenerateCardInventory();
                GenerateConsumableInventory();
                GenerateObjetInventory();
                DrawShopUI();
                break;
        }
    }


    void GenerateCardInventory()
    {
        Debug.Log("called card inventory (Weighted Pool Logic)");

        // 1. 카드 데이터 분류 (SetCardReward 로직 그대로 차용)
        List<Item> normalCards = new List<Item>();
        
        // 희귀도별 리스트 배열 초기화
        int rarityCount = Enum.GetNames(typeof(CardRarity)).Length;
        List<Item>[] dreamCards = new List<Item>[rarityCount];
        List<Item>[] dreamCards_enhanced = new List<Item>[rarityCount];

        for (int i = 0; i < rarityCount; i++)
        {
            dreamCards[i] = new List<Item>();
            dreamCards_enhanced[i] = new List<Item>();
        }

        DreamPiece_Reference persona_ref = dreamPieceListSO.dreamPieces.Find(x => x.name == characterSO.personaPiece.name);
        DreamPiece_Reference shadow_ref = dreamPieceListSO.dreamPieces.Find(x => x.name == characterSO.shadowPiece.name);

        if (normalItemListSO != null)
        {
            foreach (Item item in normalItemListSO.items)
            {
                normalCards.Add(item);
            }
        }

        if (persona_ref != null)
        {
            Debug.Log(persona_ref.name);
            Debug.Log(persona_ref.cards.Count);
            foreach (Item_Enhanceable item in persona_ref.cards)
            {
                dreamCards[(int)item.rarity].Add((Item)item);
                if (item.enhancedItem != null) 
                    dreamCards_enhanced[(int)item.rarity].Add(item.enhancedItem);
            }
        }

        if (shadow_ref != null)
        {
            foreach (Item_Enhanceable item in shadow_ref.cards)
            {
                dreamCards[(int)item.rarity].Add((Item)item);
                if (item.enhancedItem != null)
                    dreamCards_enhanced[(int)item.rarity].Add(item.enhancedItem);
            }
        }

        int targetCount = 4; 

        int currentCount = stageSO.merchantSellCards.Count;
        int safetyLoop = 0;

        while (currentCount < targetCount && safetyLoop < 100)
        {
            safetyLoop++;

            float totalW = 0f;
            if (rewardCardWeights == null || rewardCardWeights.Length == 0)
            {
                Debug.LogError("Reward Card Weights are not set in the Inspector!");
                break;
            }

            for (int i = 0; i < rewardCardWeights.Length; i++)
            {
                totalW += rewardCardWeights[i];
            }

            float rPoint = Random.value * totalW;
            int chooseCardPool = 0;

            for (int i = 0; i < rewardCardWeights.Length; i++)
            {
                if (rPoint < rewardCardWeights[i])
                {
                    chooseCardPool = i;
                    break;
                }
                rPoint -= rewardCardWeights[i];
            }

            List<Item> lookat = new List<Item>();
            bool isEnhanced = false;

            if (chooseCardPool == 0) 
            {
                lookat = normalCards;
            }
            else if (chooseCardPool > 0 && chooseCardPool <= rarityCount)
            {
                
                isEnhanced = Random.value < enhanceProbability;
                int rarityIndex = chooseCardPool - 1;

                if (isEnhanced) 
                    lookat = dreamCards_enhanced[rarityIndex];
                else 
                    lookat = dreamCards[rarityIndex];
            }

            if (lookat == null || lookat.Count == 0) continue;

            int cardIdx = Random.Range(0, lookat.Count);
            Item pickedCard = lookat[cardIdx];

            if (pickedCard != null)
            {
                stageSO.merchantSellCards.Add(new SellCard
                {
                    cardItem = pickedCard,
                    cost = pickedCard.cost,
                    isValid = true
                });
                
                currentCount++;
            }
        }
    }

    void GenerateConsumableInventory()
    {
        int itemSlotCount = 2; 
        for (int i = 0; i < itemSlotCount; i++)
        {
            if (useableItemListSO.useableItems.Count > 0)
            {
                int uItemIdx = Random.Range(0, useableItemListSO.useableItems.Count);
                var setUItem = useableItemListSO.useableItems[uItemIdx];
                int cost = 50; 
                stageSO.merchantSellUItems.Add(new SellUItem { useItem = setUItem, cost = cost, isValid = true });
            }
        }
    }
    
    public void GenerateObjetInventory()
    {
        int currentAct = playerDataSO.currentActNum;
        List<RelicItem_Data> candidates = relicDataList.relicItems.Where(obj => 
        {
            bool hasRelic = playerRelicSO.relicItems.Exists(owned => owned.relicOwner == obj.relicOwner);
            if (hasRelic) return false;
            return (obj.rarity == CardRarity.Normal && obj.relicAct < currentAct);
        }).ToList();
        int slotCount = 3; 
        for (int i = 0; i < slotCount; i++)
        {
            if (candidates.Count <= 0) break;
            int rndIdx = Random.Range(0, candidates.Count);
            stageSO.merchantSellObjets.Add(new SellObjet { objetItem = candidates[rndIdx], cost = candidates[rndIdx].cost, isValid = true });
            candidates.RemoveAt(rndIdx);
        }
    }
    
    public void GenerateSpecialShopInventory()
    {
        List<RelicItem_Data> selectedObjets = relicDataList.relicItems.Where(obj => 
        {
            bool hasRelic = playerRelicSO.relicItems.Exists(owned => owned.relicOwner == obj.relicOwner);
            if (hasRelic) return false;
            return (obj.rarity == CardRarity.Rare);
        }).ToList();
        foreach (var obj in selectedObjets)
        {
            stageSO.merchantSellObjets.Add(new SellObjet { objetItem = obj, cost = obj.cost, isValid = true });
        }
    }
    
    public void GenerateIcecreamShopInventory()
    {
        List<int> iceCreamIds = new List<int> { 24, 22, 23 };

        foreach (int id in iceCreamIds)
        {
            RelicItem_Data itemData = relicDataList.relicItems.Find(x => x.relicOwner == id);
            if (itemData != null)
            {
                if (!playerRelicSO.relicItems.Exists(owned => owned.relicOwner == itemData.relicOwner))
                {
                    stageSO.merchantSellObjets.Add(new SellObjet 
                    { 
                        objetItem = itemData, 
                        cost = itemData.cost, 
                        isValid = true 
                    });
                }
            }
        }
    }
    
    
    void DrawShopUI()
    {
        DrawCards();
        DrawObjets(); 
    }
    void DrawCards()
    {
        int dataCount = stageSO.merchantSellCards.Count;
        int currentChildCount = cardContainer.childCount;
        if (currentChildCount < dataCount)
        {
            int diff = dataCount - currentChildCount;
            for (int i = 0; i < diff; i++) Instantiate(sellCardPrefab, cardContainer);
        }
        for (int i = 0; i < cardContainer.childCount; i++)
        {
            Transform child = cardContainer.GetChild(i);
            if (i < dataCount)
            {
                var data = stageSO.merchantSellCards[i];
                EncounterCardUI_Sell script = child.GetComponent<EncounterCardUI_Sell>();
                if (script != null)
                {
                    int index = i; 
                    script.Setup(data.cardItem, data.cost, data.isValid, characterSO, () => TryBuyCard(index));
                }
                child.gameObject.SetActive(data.isValid); 
            }
            else child.gameObject.SetActive(false);
        }
    }
    void DrawRareObjets()
    {
        int dataCount = stageSO.merchantSellObjets.Count;
        int currentChildCount = objetRareContainer.childCount;
        if (currentChildCount < dataCount)
        {
            int diff = dataCount - currentChildCount;
            for (int i = 0; i < diff; i++) Instantiate(objetRarePrefab, objetRareContainer);
        }
        for (int i = 0; i < objetRareContainer.childCount; i++)
        {
            Transform child = objetRareContainer.GetChild(i);
            if (i < dataCount)
            {
                var data = stageSO.merchantSellObjets[i];
                EncounterObjetUI_Sell script = child.GetComponent<EncounterObjetUI_Sell>();
                if (script != null)
                {
                    int index = i;
                    script.Setup(data.objetItem, data.cost, data.isValid, characterSO, false, () => TryBuyObjet(index));
                }
                child.gameObject.SetActive(data.isValid); 
            }
            else child.gameObject.SetActive(false);
        }
    }
    void DrawObjets()
    {
        int dataCount = stageSO.merchantSellObjets.Count;
        int currentChildCount = objetContainer.childCount;
        if (currentChildCount < dataCount)
        {
            int diff = dataCount - currentChildCount;
            for (int i = 0; i < diff; i++) Instantiate(objetPrefab, objetContainer);
        }
        
        for (int i = 0; i < objetContainer.childCount; i++)
        {
            Transform child = objetContainer.GetChild(i);
            if (i < dataCount)
            {
                var data = stageSO.merchantSellObjets[i];
                EncounterObjetUI_Sell script = child.GetComponent<EncounterObjetUI_Sell>();
                if (script != null)
                {
                    int index = i;
                    script.Setup(data.objetItem, data.cost, data.isValid, characterSO, false, () => TryBuyObjet(index));
                }
                child.gameObject.SetActive(data.isValid); 
            }
            else child.gameObject.SetActive(false);
        }
    }

    void DrawJunkObjets()
    {
        List<RelicItem_Enhanceable> playerObjets = playerRelicSO.relicItems;
        
        List<RelicItem_Enhanceable> displayList = new List<RelicItem_Enhanceable>();
        foreach(var relic in playerObjets)
        {
            if (relic != currentRelicToSell) 
            {
                displayList.Add(relic);
            }
        }
        Debug.Log(displayList.Count);

        int dataCount = displayList.Count;
        int currentChildCount = objetJunkContainer.childCount;
        if (currentChildCount < dataCount)
        {
            int diff = dataCount - currentChildCount;
            for (int i = 0; i < diff; i++) Instantiate(objetRarePrefab, objetJunkContainer);
        }

        for (int i = 0; i < objetJunkContainer.childCount; i++)
        {
            Transform child = objetJunkContainer.GetChild(i);
            if (i < dataCount)
            {
                RelicItem_Enhanceable currentRelic = displayList[i];
                RelicItem_Data displayData = new RelicItem_Data(currentRelic);
                int sellPrice = currentRelic.sellCost; 

                EncounterObjetUI_Sell script = child.GetComponent<EncounterObjetUI_Sell>();
            
                if (script != null)
                { 
                    script.Setup(displayData, sellPrice, true, characterSO, true, () => SelectRelicToSell(currentRelic));
                }
                child.gameObject.SetActive(true);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void SelectRelicToSell(RelicItem_Enhanceable relic)
    {
        currentRelicToSell = relic;
        UpdateJunkShopState();
        DrawJunkObjets();     
    }

    public void DeselectRelic()
    {
        currentRelicToSell = null;
        UpdateJunkShopState();
        DrawJunkObjets();
    }

    public void UpdateJunkShopState()
    {
        foreach (Transform child in junkChosenContainer)
        {
            Destroy(child.gameObject);
        }
 
        if (currentRelicToSell != null)
        {
            GameObject chosenObj = Instantiate(objetRarePrefab, junkChosenContainer);
            
            RelicItem_Data displayData = new RelicItem_Data(currentRelicToSell);
            int sellPrice = currentRelicToSell.sellCost;
            
            EncounterObjetUI_Sell script = chosenObj.GetComponent<EncounterObjetUI_Sell>();
            if (script != null)
            {
                script.Setup(displayData, sellPrice, true, characterSO, false, () => DeselectRelic());
            }

            if (junkSellButton != null)
            {
                junkSellButton.interactable = true;
                if (junkSellButtonImage) junkSellButtonImage.sprite = buttonBasicSprite;
            }
        }
        else
        {
            if (junkSellButton != null)
            {
                junkSellButton.interactable = false;

                if (junkSellButtonImage) junkSellButtonImage.sprite = buttonBasicSprite;
                
            }
        }
    }

    public void OnClickSellRelic()
    {
        
        if (currentRelicToSell == null) return;
        StartCoroutine(SellProcess());
    }

    IEnumerator SellProcess()
    {
        if (junkSellButtonImage != null && buttonPressedSprite != null)
            junkSellButtonImage.sprite = buttonPressedSprite;

        yield return new WaitForSeconds(0.1f);

        int sellPrice = currentRelicToSell.sellCost;
        characterSO.dreamDust += sellPrice;
        menuControll.RefreshUI();
        
        playerRelicSO.relicItems.Remove(currentRelicToSell);
        Debug.Log($"오브제 판매 완료: {currentRelicToSell.relicName}, +{sellPrice} Dust");

        if (junkSellButtonImage != null && buttonBasicSprite != null)
            junkSellButtonImage.sprite = buttonBasicSprite;

        currentRelicToSell = null;
        UpdateJunkShopState();
        DrawJunkObjets();
    }
    
    public void TryBuyCard(int index)
    {
        var data = stageSO.merchantSellCards[index];
        if (!data.isValid) return; 
        if (characterSO.dreamDust >= data.cost)
        {
            characterSO.dreamDust -= data.cost;
            AddCardToInventory(data.cardItem);
            data.isValid = false;
            stageSO.merchantSellCards[index] = data; 
            menuControll.RefreshUI();
            DrawShopUI(); 
            Debug.Log("카드 구매 성공!");
        }
    }
    public void TryBuyObjet(int index)
    {
        if (stageSO != null)
        {
            var data = stageSO.merchantSellObjets[index];
            if (!data.isValid) return;
            if (characterSO.dreamDust >= data.cost)
            {
                characterSO.dreamDust -= data.cost;
                AddObjectToInventory(data.objetItem); 
                data.isValid = false;
                stageSO.merchantSellObjets[index] = data; 
                menuControll.RefreshUI();
                if (currentShopId == "souvenir" || currentShopId == "IceCreamShop") DrawRareObjets();
                else DrawShopUI();
            }
        }
    }
    void AddObjectToInventory(RelicItem_Data itemToAdd)
    {
        if (itemToAdd == null) return;
        if (playerRelicSO.relicItems.Exists(r => r.relicOwner == itemToAdd.relicOwner)) return;
        RelicItem_Enhanceable newRelic = new RelicItem_Enhanceable(itemToAdd);
        playerRelicSO.relicItems.Add(newRelic);
        if (newRelic.relicOwner == 22) playerStatsSO.ModifyStat(StatType.Wisdom, 1);
        else if  (newRelic.relicOwner == 23) playerStatsSO.ModifyStat(StatType.Luck, 1);
        else if (newRelic.relicOwner == 24) playerStatsSO.ModifyStat(StatType.Courage, 1);
    }
    
    void AddCardToInventory(Item itemToAdd)
    {
        Item newItem = new Item();
        newItem.SetItem(itemToAdd);
        newItem.num = 1;
        if (itemToAdd.dreamPieceNum < 0)
        {
            var existItem = characterSO.normalCards.Find(x => x.name == itemToAdd.name);
            if (existItem == null) characterSO.normalCards.Add(newItem);
            else existItem.num++;
        }
        else if (dreamPieceListSO.dreamPieces[itemToAdd.dreamPieceNum].name == characterSO.personaPiece.name)
        {
            var existItem = characterSO.personaPiece.cards.Find(x => x.name == itemToAdd.name);
            if (existItem == null) characterSO.personaPiece.cards.Add(newItem);
            else existItem.num++;
        }
        else if (dreamPieceListSO.dreamPieces[itemToAdd.dreamPieceNum].name == characterSO.shadowPiece.name)
        {
            var existItem = characterSO.shadowPiece.cards.Find(x => x.name == itemToAdd.name);
            if (existItem == null) characterSO.shadowPiece.cards.Add(newItem);
            else existItem.num++;
        }
    }
    public void OnClickExitButton()
    {
        panelRoot.SetActive(false);
        if (EncounterManager.Instance != null) EncounterManager.Instance.OnMerchantClosed();
        currentShopId = ""; 
    }
}