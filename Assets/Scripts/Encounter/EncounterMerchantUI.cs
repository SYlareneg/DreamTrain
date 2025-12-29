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
    public GameObject panelRoot;            // 상점 패널 전체
    public CharacterSO characterSO;         // 플레이어 데이터 (재화, 카드)
    public StageSO stageSO;                 // 상점 판매 목록 저장용 (재방문 시 유지)
    public DreamPieceSO dreamPieceListSO;   // 카드 데이터베이스
    
    public ItemSO normalItemListSO;         // 일반 카드 데이터베이스
    public UseableItemSO useableItemListSO; // 소비 아이템 데이터베이스

    [Header("UI Containers")]
    public Transform cardContainer;         // 카드들이 생성될 부모 Transform

    [Header("Prefabs")]
    public GameObject sellCardPrefab;       // CardUI_Sell 프리팹

    [Header("Settings")]
    public int[] sellCardCosts = new int[Enum.GetNames(typeof(CardRarity)).Length * 2 + 1];
    public float[] sellCardWeights = new float[Enum.GetNames(typeof(CardRarity)).Length + 1];
    [Range(0, 1)] public float enhanceProbability = 0.3f;


    // 초기화 및 열기
    public void Open()
    {
        panelRoot.SetActive(true);

        // 저장된 상점 데이터가 없으면 새로 생성, 있으면 불러오기
        if (stageSO.merchantSellCards.Count == 0 && stageSO.merchantSellUItems.Count == 0)
        {
            GenerateShopInventory();
        }
        
        DrawShopUI();
    }

    // 상점 닫기 (인카운터로 복귀)
    public void Close()
    {
        panelRoot.SetActive(false);
        // EncounterManager에게 복귀 신호 보냄
        // (EncounterManager.cs의 EndMerchant 로직이 호출되어야 함)
        if (EncounterManager.Instance != null)
        {
            EncounterManager.Instance.OnMerchantClosed();
        }
    }


    // --- [로직 1] 상점 물품 데이터 생성 (NPCMerchantManager 로직 이식) ---
    void GenerateShopInventory()
    {
        stageSO.merchantSellCards.Clear();
        stageSO.merchantSellUItems.Clear();

        // 1. 카드 생성 로직
        List<Item> normalCards = new List<Item>(normalItemListSO.items);
        List<Item>[] dreamCards = new List<Item>[Enum.GetNames(typeof(CardRarity)).Length];
        List<Item>[] dreamCards_enhanced = new List<Item>[Enum.GetNames(typeof(CardRarity)).Length];

        // 배열 초기화
        for (int i = 0; i < dreamCards.Length; i++)
        {
            dreamCards[i] = new List<Item>();
            dreamCards_enhanced[i] = new List<Item>();
        }

        DreamPiece_Reference persona_ref = dreamPieceListSO.dreamPieces.Find(x => x.name == characterSO.personaPiece.name);
        DreamPiece_Reference shadow_ref = dreamPieceListSO.dreamPieces.Find(x => x.name == characterSO.shadowPiece.name);

        // 카드 풀 분류
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

        // 판매할 카드 슬롯 수만큼 반복 (예: 4개 고정이라고 가정, 혹은 기존 sellCards 배열 길이만큼)
        int cardSlotCount = 4; 
        for (int i = 0; i < cardSlotCount; i++)
        {
            // 가중치 랜덤 뽑기
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

            if (chooseCardPool == 0) // 일반 카드
            {
                lookat = normalCards;
                sellCost = sellCardCosts[0];
            }
            else // 희귀도별 카드
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
                // StageSO에 저장 (UI 표시용 데이터)
                stageSO.merchantSellCards.Add(new SellCard { cardItem = lookat[cardIdx], cost = sellCost, isValid = true });
            }
        }

        // 2. 소비 아이템 생성 로직
        int itemSlotCount = 2; // 예: 2개
        for (int i = 0; i < itemSlotCount; i++)
        {
            if (useableItemListSO.useableItems.Count > 0)
            {
                int uItemIdx = Random.Range(0, useableItemListSO.useableItems.Count);
                var setUItem = useableItemListSO.useableItems[uItemIdx];
                // 가격은 레어도 등에 따라 다를 수 있으나, 일단 임시 로직 혹은 고정값 사용
                int cost = (sellCardCosts.Length > setUItem.rarity) ? sellCardCosts[setUItem.rarity] : 50; 
                
                stageSO.merchantSellUItems.Add(new SellUItem { useItem = setUItem, cost = cost, isValid = true });
            }
        }
    }

    void DrawShopUI()
    {
        // 기존 UI 삭제
        foreach (Transform child in cardContainer) Destroy(child.gameObject);

        // 카드 목록 생성
        for (int i = 0; i < stageSO.merchantSellCards.Count; i++)
        {
            var data = stageSO.merchantSellCards[i];
            
            // 프리팹 생성
            GameObject obj = Instantiate(sellCardPrefab, cardContainer);
            
            // [수정] 새로 만든 스크립트 가져오기
            EncounterCardUI_Sell script = obj.GetComponent<EncounterCardUI_Sell>();

            if (script != null)
            {
                int index = i; // 클로저 캡처 방지

                // Setup 호출 시, 구매 로직을 람다식으로 전달
                script.Setup(data.cardItem, data.cost, data.isValid, () => 
                {
                    TryBuyCard(index);
                });
            }
        }
    }

   public void TryBuyCard(int index)
    {
        var data = stageSO.merchantSellCards[index];

        if (!data.isValid) return; // 이미 팔림

        // 돈 확인
        if (characterSO.dreamDust >= data.cost)
        {
            // 결제
            characterSO.dreamDust -= data.cost;

            // 인벤토리에 추가 (아래 함수 참조)
            AddCardToInventory(data.cardItem);

            // 품절 처리
            data.isValid = false;
            stageSO.merchantSellCards[index] = data; // 데이터 갱신
            
            // UI 갱신 (구매된 상태 반영)
            DrawShopUI(); 
            Debug.Log("구매 성공!");
        }
    }

    // --- [로직 3] 카드 추가 로직 (삭제 코드의 역순 로직 적용) ---
    void AddCardToInventory(Item itemToAdd)
    {
        // 1. 참조 문제가 없도록 새 아이템 객체 생성
        Item newItem = new Item();
        newItem.SetItem(itemToAdd);
        newItem.num = 1;

        // 2. 카드의 종류에 따라 적절한 리스트에 추가
        // (NPCMerchantManager 및 NPCSofaManager의 리스트 분류 방식 따름)

        // A. 일반 카드 (DreamPieceNum < 0)
        if (itemToAdd.dreamPieceNum < 0)
        {
            // 이미 있는 카드인지 확인
            var existItem = characterSO.normalCards.Find(x => x.name == itemToAdd.name);
            if (existItem == null) 
                characterSO.normalCards.Add(newItem); // 없으면 리스트에 추가
            else 
                existItem.num++; // 있으면 개수 증가
        }
        // B. 페르소나 카드
        else if (dreamPieceListSO.dreamPieces[itemToAdd.dreamPieceNum].name == characterSO.personaPiece.name)
        {
            var existItem = characterSO.personaPiece.cards.Find(x => x.name == itemToAdd.name);
            if (existItem == null) 
                characterSO.personaPiece.cards.Add(newItem);
            else 
                existItem.num++;
        }
        // C. 쉐도우 카드
        else if (dreamPieceListSO.dreamPieces[itemToAdd.dreamPieceNum].name == characterSO.shadowPiece.name)
        {
            var existItem = characterSO.shadowPiece.cards.Find(x => x.name == itemToAdd.name);
            if (existItem == null) 
                characterSO.shadowPiece.cards.Add(newItem);
            else 
                existItem.num++;
        }
        else
        {
            Debug.LogError("알 수 없는 카드 타입입니다. 추가 실패.");
        }
    }
}
