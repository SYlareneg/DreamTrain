using System.Collections.Generic;
using UnityEngine;
using System;

public class EmotionCardManager : MonoBehaviour
{
    public static EmotionCardManager Instance { get; private set; }

    [Header("Data")]
    public EmotionCardSO emotionData; 
    private List<FeelingType> usedFeelings = new List<FeelingType>(); 

    [Header("World Object Settings")]
    public GameObject cardPrefab;       
    public Transform cardSpawnParent;   
    
    [Header("Alignment Settings")]
    public Transform leftAnchor;        
    public Transform rightAnchor;       
    public float arcHeight = 0.5f;      
    public float cardScale = 1.5f;      

    private List<GameObject> spawnedCards = new List<GameObject>();
    private Action<FeelingType> onCardSelectedCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetRun();
    }

    public void ResetRun()
    {
        usedFeelings.Clear();
    }

    public void ShowEmotionSelection(Action<FeelingType> callback)
    {
        onCardSelectedCallback = callback;
        ClearCards();

        foreach (var data in emotionData.emotionCards)
        {
            if (!usedFeelings.Contains(data.type))
            {
                CreateEmotionCard(data);
            }
        }
        
        if (spawnedCards.Count == 0)
        {
             ResetRun();
             ShowEmotionSelection(callback);
             return;
        }

        AlignCards();
    }

    private void CreateEmotionCard(EmotionCardSO.EmotionCardData data)
    {
        if (cardPrefab == null) return;

        GameObject go = Instantiate(cardPrefab, cardSpawnParent);
        
        Order order = go.GetComponent<Order>();
        if (order != null) order.SetOriginOrder(20); 

        Card cardComp = go.GetComponent<Card>();
        if (cardComp != null)
        {
            cardComp.SetupDialogue(
                data.cardName, 
                "", 
                data.cardSprite, 
                () => OnCardClick(data.type) 
            );
            
            cardComp.isDialogueCard = true; 
        }

        spawnedCards.Add(go);
    }

    private void AlignCards()
    {
        int count = spawnedCards.Count;
        if (count == 0) return;

        float[] objLerps = new float[count];

        if (count == 1) objLerps = new float[] { 0.5f };
        else
        {
            float step = 1f / (count + 1);
            for (int i = 0; i < count; i++) objLerps[i] = step * (i + 1);
        }

        for (int i = 0; i < count; i++)
        {
            GameObject cardObj = spawnedCards[i];

            Vector3 targetPos = Vector3.Lerp(leftAnchor.position, rightAnchor.position, objLerps[i]);
            
            float curve = Mathf.Sqrt(Mathf.Pow(arcHeight, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
            if(float.IsNaN(curve)) curve = 0;
            float arcOffset = (1f - 4f * Mathf.Pow(objLerps[i] - 0.5f, 2)) * arcHeight; 
            targetPos.y += arcOffset;

            Quaternion targetRot = Quaternion.Slerp(leftAnchor.rotation, rightAnchor.rotation, objLerps[i]);

            cardObj.transform.position = targetPos;
            cardObj.transform.rotation = targetRot;
            cardObj.transform.localScale = Vector3.one * cardScale;

            Card cardComp = cardObj.GetComponent<Card>();
            if (cardComp != null)
            {
                cardComp.originPRS = new PRS(targetPos, targetRot, Vector3.one * cardScale);
            }
        }
    }

    private void OnCardClick(FeelingType selectedType)
    {
        Debug.Log($"[EmotionCard] Selected: {selectedType}");
        usedFeelings.Add(selectedType);
        ClearCards();
        onCardSelectedCallback?.Invoke(selectedType);
    }

    private void ClearCards()
    {
        foreach (GameObject go in spawnedCards)
        {
            if (go != null) Destroy(go);
        }
        spawnedCards.Clear();
    }
}