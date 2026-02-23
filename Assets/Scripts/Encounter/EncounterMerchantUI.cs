using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System;
using System.Diagnostics.Contracts;
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
    public GameObject cardEnhanceButton;
    public GameObject cardEnhanceScreen;        
    public Transform cardEnhanceList;           
    public GameObject enhanceCardPrefab;        
    
    public GameObject cardEnhanceConfirmScreen; 
    public CardUI beforeEnhance_C;              
    public CardUI afterEnhance_C;                
    
    public int cardEnhanceCost = 100;           
    public CardUI_Enhance currentSelectedCard; 

    [Header("Relic Enhance UI")]
    public GameObject relicEnhanceButton;               
    public GameObject relicEnhanceScreen;       
    public Transform relicEnhanceList;          
    public GameObject enhanceRelicPrefab; 
    public GameObject selectedEnhance;
    public TMP_Text afterEnhance_R_Name;
    public Image afterEnhance_R_IMG;
    public TMP_Text tooltipName;
    public TMP_Text afterEnhance_R_Text;
    private EnhanceObjet currentSelectedRelicUI;

    public int relicEnhanceCost = 150;          
    private RelicItem_Enhanceable currentEnhanceRelicItem; 
    private RelicHalfUI_Enhance currentEnhanceRelicUI;

    [Header("Settings")] 
    public GameObject merchantBackground;
    public GameObject iceCreamBackground;
    public GameObject junkBackground;
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
        
        currentRelicToSell = null;
        GenerateShopInventory();
        
    }
    
    public void OpenEnhanceCardScreen()
    {
        if (characterSO.dreamDust < 2) return;
        
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
        
        List<Item> allCardItemList = characterSO.personaPiece.cards.Concat(characterSO.shadowPiece.cards).ToList();

        foreach (Item item in allCardItemList)
        {
            if (item.isEnhanced == false)
            {
                Item_Enhanceable eItem = null;
                Debug.Log($"dreamPieceNum: {item.dreamPieceNum}, dreampiece: {dreamPieceListSO.dreamPieces.Count}");
                var matchedPersona = dreamPieceListSO.dreamPieces.Find(p => p.name == characterSO.personaPiece.name);
                if (matchedPersona != null)
                {
                    eItem = matchedPersona.cards.Find(x => x.name == item.name);
                }

                if (eItem == null)
                {
                    var matchedShadow = dreamPieceListSO.dreamPieces.Find(p => p.name == characterSO.shadowPiece.name);
                    if (matchedShadow != null)
                    {
                        eItem = matchedShadow.cards.Find(x => x.name == item.name);
                    }
                }

                if (eItem != null && eItem.enhancedItem != null)
                {
                    for(int i = 0; i < item.num; i++)
                    {
                        var cardObj = Instantiate(enhanceCardPrefab, cardEnhanceList, false);
                        CardUI_Enhance cardUI = cardObj.GetComponent<CardUI_Enhance>();
                        if (cardUI != null) cardUI.Setup(item);
                    }
                }
            }
        }
    }

    public void EnhanceCardSelect(CardUI_Enhance cardUI)
    {
        Item_Enhanceable eItem = null;
        Debug.Log($"dreamPieceNum: {cardUI.item.dreamPieceNum}, dreampiece: {dreamPieceListSO.dreamPieces.Count}");
        var matchedPersona = dreamPieceListSO.dreamPieces.Find(p => p.name == characterSO.personaPiece.name);
        if (matchedPersona != null)
        {
            eItem = matchedPersona.cards.Find(x => x.name == cardUI.item.name);
        }
        if (eItem == null)
        {
            var matchedShadow = dreamPieceListSO.dreamPieces.Find(p => p.name == characterSO.shadowPiece.name);
            if (matchedShadow != null)
            {
                eItem = matchedShadow.cards.Find(x => x.name == cardUI.item.name);
            }
        }
        if (eItem == null) 
        {
            Debug.LogError($"[강화 에러] {cardUI.item.name}의 강화 데이터를 찾을 수 없어 UI를 띄우지 못했습니다!");
            return; 
        }

        currentSelectedCard = cardUI;
        beforeEnhance_C.Setup(cardUI.item);
        afterEnhance_C.Setup(eItem.enhancedItem);
        
        cardEnhanceConfirmScreen.SetActive(true);
        cardEnhanceScreen.transform.Find("ExitButton").GetComponent<Button>().interactable = false;
    }

    public void EnhanceCardConfirm()
    {
        //if (characterSO.dreamDust < cardEnhanceCost) return;
        //characterSO.dreamDust -= cardEnhanceCost;
        
        currentSelectedCard.item.num--;
        var matchedPersona = dreamPieceListSO.dreamPieces.Find(p => p.name == characterSO.personaPiece.name);
        if(currentSelectedCard.item.num == 0)
        {
            if(currentSelectedCard.item.dreamPieceNum < 0)
            {
                characterSO.normalCards.Remove(currentSelectedCard.item);
            }
            else if(matchedPersona.name == characterSO.personaPiece.name)
            {
                characterSO.personaPiece.cards.Remove(currentSelectedCard.item);
            }
            else if(matchedPersona.name == characterSO.shadowPiece.name)
            {
                characterSO.shadowPiece.cards.Remove(currentSelectedCard.item);
            }
        }

        AddCardToInventory(afterEnhance_C.item);

        characterSO.dreamDust -= 2;
        menuControll.RefreshUI();
        Destroy(currentSelectedCard.gameObject);
        cardEnhanceConfirmScreen.SetActive(false);
        cardEnhanceScreen.SetActive(false);
        menuControll.RefreshUI();
    }

    public void OpenEnhanceRelicScreen()
    {
        if (characterSO.dreamDust < 2) return;
        
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

        currentSelectedRelicUI = null;

        List<RelicItem_Enhanceable> enhanceableRelics = playerRelicSO.relicItems
            .Where(x => !x.isEnhanced)
            .ToList();
        
        int displayCount = Mathf.Min(enhanceableRelics.Count, 5);
        
        for (int i = 0; i < displayCount; i++)
        {
            var relicObj = Instantiate(enhanceRelicPrefab, relicEnhanceList, false);
            var enhanceObjetScript = relicObj.GetComponent<EnhanceObjet>();
            
            if (enhanceObjetScript != null)
            {
                enhanceObjetScript.Setup(enhanceableRelics[i]);
            }
        }
    }
public void EnhanceRelicSelect(EnhanceObjet clickedRelicUI)
    {
        if (currentSelectedRelicUI != null && currentSelectedRelicUI != clickedRelicUI)
        {
            currentSelectedRelicUI.SetSelected(false);
        }
        
        currentSelectedRelicUI = clickedRelicUI;
        currentSelectedRelicUI.SetSelected(true);

        RelicItem_Enhanceable rItem = clickedRelicUI.relicData;
        
        if (rItem == null || rItem.enhancedRelicItem == null)
        {
            Debug.LogError("강화된 유물 데이터를 찾을 수 없습니다.");
            return;
        }

        RelicItem enhancedData = rItem.enhancedRelicItem;

        selectedEnhance.SetActive(true);
        if (afterEnhance_R_Name)
        {
            afterEnhance_R_Name.text = enhancedData.relicName;
            tooltipName.text = enhancedData.relicName;
            afterEnhance_R_IMG.sprite = enhancedData.relicSprite;
        }
        if (afterEnhance_R_Text) afterEnhance_R_Text.text = enhancedData.relicTxt;
         
    }

    public void EnhanceRelicConfirm()
    {
        if (currentSelectedRelicUI == null) return;
        
        RelicItem_Enhanceable rItem = currentSelectedRelicUI.relicData;
        if (rItem == null || rItem.enhancedRelicItem == null) return;
        
        //if (characterSO.dreamDust < relicEnhanceCost) return;
        //characterSO.dreamDust -= relicEnhanceCost;
        rItem.relicName = rItem.enhancedRelicItem.relicName;
        rItem.relicTxt = rItem.enhancedRelicItem.relicTxt;
        if (rItem.enhancedRelicItem.relicVal != null)
        {
            rItem.relicVal = new List<int>(rItem.enhancedRelicItem.relicVal);
        }
        rItem.isEnhanced = true; 
        
        characterSO.dreamDust -= 2;
        menuControll.RefreshUI();
        selectedEnhance.SetActive(false);
        relicEnhanceScreen.SetActive(false);
        menuControll.RefreshUI();
        //SetEnhanceRelicList(); 
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
                if (cardEnhanceButton) cardEnhanceButton.SetActive(false);
                if (relicEnhanceButton) relicEnhanceButton.SetActive(false);
                if(merchantBackground) merchantBackground.SetActive(false);
                if(junkBackground)  junkBackground.SetActive(false);
                
                if(iceCreamBackground) iceCreamBackground.SetActive(true);
                if (objetRareSectionObj) objetRareSectionObj.SetActive(true);
                GenerateSpecialShopInventory();
                DrawRareObjets();
                break;
            case "IceCreamShop":
                if (cardSectionObj) cardSectionObj.SetActive(false);
                if (consumableSectionObj) consumableSectionObj.SetActive(false);
                if (objetSectionObj) objetSectionObj.SetActive(false);
                if(junkShopSectionObj) junkShopSectionObj.SetActive(false);
                if (cardEnhanceButton) cardEnhanceButton.SetActive(false);
                if (relicEnhanceButton) relicEnhanceButton.SetActive(false);
                if(merchantBackground) merchantBackground.SetActive(false);
                if(junkBackground)  junkBackground.SetActive(false);
                
                if(iceCreamBackground) iceCreamBackground.SetActive(true);
                if (objetRareSectionObj) objetRareSectionObj.SetActive(true);
                GenerateIcecreamShopInventory();
                DrawRareObjets();
                break;
            case "junkShop":
                if (cardSectionObj) cardSectionObj.SetActive(false);
                if (consumableSectionObj) consumableSectionObj.SetActive(false);
                if (objetSectionObj) objetSectionObj.SetActive(false);
                if (objetRareSectionObj) objetRareSectionObj.SetActive(false);
                if (cardEnhanceButton) cardEnhanceButton.SetActive(false);
                if (relicEnhanceButton) relicEnhanceButton.SetActive(false);
                if(merchantBackground) merchantBackground.SetActive(false);
                if(junkBackground)  iceCreamBackground.SetActive(false);
                
                if(iceCreamBackground) junkBackground.SetActive(true);
                if(junkShopSectionObj) junkShopSectionObj.SetActive(true);
                DrawJunkObjets();
                break;
            default:
                if (cardSectionObj) cardSectionObj.SetActive(true);
                if (objetSectionObj) objetSectionObj.SetActive(true);
                if (cardEnhanceButton) cardEnhanceButton.SetActive(true);
                if (relicEnhanceButton) relicEnhanceButton.SetActive(true);
                if(iceCreamBackground) merchantBackground.SetActive(true);
                
                if(merchantBackground) junkBackground.SetActive(false);
                if(junkBackground)  iceCreamBackground.SetActive(false);
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

        List<Item> normalCards = new List<Item>();
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
                int cardPrice = (pickedCard.rarity == CardRarity.Rare) ? 2 : 1;
                stageSO.merchantSellCards.Add(new SellCard
                {
                    cardItem = pickedCard,
                    cost = cardPrice,
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
            //stageSO.merchantSellCards.RemoveAt(index);
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
                //stageSO.merchantSellObjets.RemoveAt(index);
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
        else
        {
            bool isPersonaCard = false;
        
            var matchedPersona = dreamPieceListSO.dreamPieces.Find(p => p.name == characterSO.personaPiece.name);
        
            if (matchedPersona != null)
            {
                isPersonaCard = matchedPersona.cards.Exists(c => 
                    c.name == itemToAdd.name || 
                    (c.enhancedItem != null && c.enhancedItem.name == itemToAdd.name)
                );
            }

            if (isPersonaCard)
            {
                var existItem = characterSO.personaPiece.cards.Find(x => x.name == itemToAdd.name);
                if (existItem == null) characterSO.personaPiece.cards.Add(newItem);
                else existItem.num++;
            }
            else
            {
                var existItem = characterSO.shadowPiece.cards.Find(x => x.name == itemToAdd.name);
                if (existItem == null) characterSO.shadowPiece.cards.Add(newItem);
                else existItem.num++;
            }
        }
    }
    public void OnClickExitButton()
    {
        panelRoot.SetActive(false);
        if (EncounterManager.Instance != null) EncounterManager.Instance.OnMerchantClosed();
        currentShopId = ""; 
    }
}