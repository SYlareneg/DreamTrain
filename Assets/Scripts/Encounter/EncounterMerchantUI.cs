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
    [Header("Core References")]
    public GameObject panelRoot;            
    public CharacterSO characterSO;         
    public StageSO stageSO;                 
    public DreamPieceSO dreamPieceListSO;  
    public PlayerDataSO playerDataSO;   
    public ObjetData objetDataList;
    public RelicSO playerRelicSO;
    
    [Header("Databases")]
    public ItemSO normalItemListSO;         // [기존] 카드 DB
    public UseableItemSO useableItemListSO; // [기존] 소모품 DB
    
    [Header("UI Containers")]
    public Transform cardContainer;   
    public GameObject cardSectionObj;  
    public Transform objetContainer;  
    public GameObject objetSectionObj;
    public GameObject objetRareSectionObj;
    public Transform objetRareContainer;
    public GameObject consumableSectionObj; // 소모품 UI 부모 오브젝트
    
    [Header("Prefabs")]
    public GameObject sellCardPrefab;       
    public GameObject objetPrefab;        
    public GameObject objetRarePrefab;      

    [Header("Settings")]
    public int[] sellCardCosts = new int[Enum.GetNames(typeof(CardRarity)).Length * 2 + 1];
    public float[] sellCardWeights = new float[Enum.GetNames(typeof(CardRarity)).Length + 1];
    [Range(0, 1)] public float enhanceProbability = 0.3f;

    public int objetBaseCost = 100;
    public string currentShopId = "";

    void Awake()
    {
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
        InitializeObjetDatabase();
    }
    void InitializeObjetDatabase()
    {
        if (objetDataList == null || playerRelicSO == null)
        {
            Debug.LogError("ObjetDataList 혹은 PlayerRelicSO가 연결되지 않았습니다!");
            return;
        }

        foreach (var obj in objetDataList.ObjetItems)
        {
            if (obj.relicData != null && !string.IsNullOrEmpty(obj.relicData.relicName)) continue;

            var match = playerRelicSO.relicItems.Find(r => r.relicName == obj.name_ko);

            if (match != null)
            {
                obj.relicData = match; 
                obj.id = match.relicOwner; 
            }
            else
            {
                Debug.LogWarning($"[상점 초기화] '{obj.name_ko}'와 일치하는 유물을 찾을 수 없습니다.");
            }
        }
    }

    public void Open(string shopId = "")
    {
        currentShopId = shopId;
        panelRoot.SetActive(true);

        GenerateShopInventory();
        DrawShopUI();
    }
    void GenerateShopInventory()
    {
        stageSO.merchantSellCards.Clear();
        stageSO.merchantSellUItems.Clear();
        stageSO.merchantSellObjets.Clear();
    
        if (currentShopId == "souvenir")
        {
            cardSectionObj.SetActive(false);
            consumableSectionObj.SetActive(false);
            objetSectionObj.SetActive(false);
            objetRareSectionObj.SetActive(true);
            GenerateSpecialShopInventory();
            DrawRareObjets();
        }
        else
        {
            cardSectionObj.SetActive(true);
            //consumableSectionObj.SetActive(true);
            objetSectionObj.SetActive(true);
            objetRareSectionObj.SetActive(false);
            
            GenerateCardInventory();
            GenerateConsumableInventory();
            GenerateObjetInventory();      
        }
    }
    void GenerateCardInventory()
    {
        Debug.Log("called card inventory");
        List<Item> normalCards = new List<Item>(normalItemListSO.items);
        List<Item>[] dreamCards = new List<Item>[Enum.GetNames(typeof(CardRarity)).Length];
        List<Item>[] dreamCards_enhanced = new List<Item>[Enum.GetNames(typeof(CardRarity)).Length];

        for (int i = 0; i < dreamCards.Length; i++)
        {
            dreamCards[i] = new List<Item>();
            dreamCards_enhanced[i] = new List<Item>();
        }

        DreamPiece_Reference persona_ref = dreamPieceListSO.dreamPieces.Find(x => x.name == characterSO.personaPiece.name);
        DreamPiece_Reference shadow_ref = dreamPieceListSO.dreamPieces.Find(x => x.name == characterSO.shadowPiece.name);

        if (persona_ref != null)
        {
            foreach (Item_Enhanceable item in persona_ref.cards)
            {
                dreamCards[(int)item.rarity].Add(item);
                dreamCards_enhanced[(int)item.rarity].Add(item.enhancedItem);
            }
        }
        if (shadow_ref != null)
        {
            foreach (Item_Enhanceable item in shadow_ref.cards)
            {
                dreamCards[(int)item.rarity].Add(item);
                dreamCards_enhanced[(int)item.rarity].Add(item.enhancedItem);
            }
        }

        int cardSlotCount = 4; 
        for (int i = 0; i < cardSlotCount; i++)
        {
            float totalW = sellCardWeights.Sum();
            float rPoint = Random.value * totalW;
            int chooseCardPool = 0;
            
            for (int j = 0; j < sellCardWeights.Length; j++)
            {
                if (rPoint < sellCardWeights[j])
                {
                    chooseCardPool = j;
                    break;
                }
                rPoint -= sellCardWeights[j];
            }

            List<Item> lookat = new List<Item>();
            int sellCost = sellCardCosts[0];

            if (chooseCardPool == 0) 
            {
                lookat = normalCards;
                sellCost = sellCardCosts[0];
            }
            else 
            {
                bool isE = Random.value < enhanceProbability;
                int rarityIndex = chooseCardPool - 1;

                if (rarityIndex < dreamCards.Length)
                {
                    if (isE)
                    {
                        lookat = dreamCards_enhanced[rarityIndex];
                        sellCost = sellCardCosts[chooseCardPool * 2];
                    }
                    else
                    {
                        lookat = dreamCards[rarityIndex];
                        sellCost = sellCardCosts[chooseCardPool * 2 - 1];
                    }
                }
            }

            if (lookat.Count > 0)
            {
                int cardIdx = Random.Range(0, lookat.Count);
                stageSO.merchantSellCards.Add(new SellCard { cardItem = lookat[cardIdx], cost = sellCost, isValid = true });
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
                int cost = (sellCardCosts.Length > (int)setUItem.rarity) ? sellCardCosts[(int)setUItem.rarity] : 50; 
                
                stageSO.merchantSellUItems.Add(new SellUItem { useItem = setUItem, cost = cost, isValid = true });
            }
        }
    }
    
    void GenerateObjetInventory()
    {
        Debug.Log("called objet inventory");
        int currentAct = playerDataSO.currentActNum;
        List<Item_Objets> candidates = objetDataList.ObjetItems.Where(obj => 
        {
            if (obj.relicData == null)
            {
                Debug.Log("연결에러");
                return false;
            }

            bool isConditionMet = obj.rarity == CardRarity.Normal &&
                                  obj.act < currentAct &&
                                  obj.act > 0 &&
                                  !obj.isBought;
            Debug.Log($"rarity: {obj.rarity},  act: {obj.act}, isConditionMet: {isConditionMet}");
            Debug.Log($"currentAct: {currentAct},  isBought: {obj.isBought}");
            if (!isConditionMet) return false;

            bool hasRelic = playerDataSO.relics.Contains(obj.relicData.relicOwner);
            Debug.Log($"hasRelic : {hasRelic}");
            return !hasRelic;

        }).ToList();
        Debug.Log($"candidates: {candidates}");
        int slotCount = 3; 
        List<Item_Objets> selectedObjets = new List<Item_Objets>();

        for (int i = 0; i < slotCount; i++)
        {
            if (candidates.Count <= 0) break;

            int rndIdx = Random.Range(0, candidates.Count);
            selectedObjets.Add(candidates[rndIdx]);
            candidates.RemoveAt(rndIdx);
        }

        foreach (var obj in selectedObjets)
        {
            stageSO.merchantSellObjets.Add(new SellObjet 
            { 
                objetItem = obj, 
                cost = obj.price,
                isValid = true 
            });
        }
    }
    public void GenerateSpecialShopInventory()
    {
        List<Item_Objets> selectedObjets = objetDataList.ObjetItems.Where(obj => 
        {
            if (obj.relicData == null) return false;

            bool isConditionMet = obj.rarity == CardRarity.Rare && !obj.isBought;
            if (!isConditionMet) return false;

            bool hasRelic = playerDataSO.relics.Contains(obj.relicData.relicOwner);
            return !hasRelic;

        }).ToList();
        foreach (var obj in selectedObjets)
        {
            stageSO.merchantSellObjets.Add(new SellObjet 
            { 
                objetItem = obj, 
                cost = obj.price,
                isValid = true 
            });
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
            else
            {
                child.gameObject.SetActive(false);
            }
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
                    script.Setup(data.objetItem, data.cost, data.isValid, characterSO, () => TryBuyObjet(index));
                }
                child.gameObject.SetActive(data.isValid); 
                Debug.Log(data.isValid);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }
    void DrawObjets()
    {
        int dataCount = stageSO.merchantSellObjets.Count;
        int currentChildCount = objetContainer.childCount;

        // 프리팹 부족하면 생성
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
                    script.Setup(data.objetItem, data.cost, data.isValid, characterSO, () => TryBuyObjet(index));
                }
                child.gameObject.SetActive(data.isValid); 
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
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
            
            DrawShopUI(); // UI 갱신
            Debug.Log("카드 구매 성공!");
        }
    }
    
    public bool TryBuyObjet(int index)
    {
        var data = stageSO.merchantSellObjets[index];
        if (!data.isValid) return false;

        // 재화 체크
        if (characterSO.dreamDust >= data.cost)
        {
            characterSO.dreamDust -= data.cost;
            AddObjectToInventory(data.objetItem);
            Debug.Log($"구매 시도 중... 현재 ShopID: '{currentShopId}'");

            data.isValid = false;
            stageSO.merchantSellObjets[index] = data; 
            
            data.objetItem.isBought = true; 
            if (currentShopId == "souvenir") DrawRareObjets();
            else DrawShopUI();
            Debug.Log($"오브제 구매 성공: {data.objetItem.name_ko}");
            return true;
        }
        else
        {
            Debug.Log("돈이 부족합니다.");
            return false;
        }
    }

    void AddObjectToInventory(Item_Objets itemToAdd)
    {
        if (itemToAdd.relicData == null)
        {
            Debug.LogError($"구매 오류: {itemToAdd.name_ko}의 relicData가 비어있습니다.");
            return;
        }

        int relicID = itemToAdd.relicData.relicOwner;

        if (!playerDataSO.relics.Contains(relicID))
        {
            playerDataSO.relics.Add(relicID);
            playerDataSO.relicEnhancements.Add(false);
        
            itemToAdd.isBought = true;

            Debug.Log($"오브제 획득: {itemToAdd.name_ko} (ID: {relicID})");
        }
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
    
        if (EncounterManager.Instance != null)
        {
            EncounterManager.Instance.OnMerchantClosed();
        }
    
        currentShopId = ""; 
    }
}