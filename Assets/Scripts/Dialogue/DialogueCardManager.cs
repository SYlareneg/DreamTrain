using System.Collections.Generic;
using UnityEngine;
using System;
using HallControll.SO;
using UnityEngine.EventSystems;

public class DialogueCardManager : DialogueManagerBase
{
    
    [Header("Card Mode Settings")]
    public GameObject cardPrefab;
    public Transform cardSpawnParent;
    public GameObject rerollButton; 

    [Header("Positioning Anchors")]
    public Transform bundleLeft;  
    public Transform bundleRight;
    public Transform emotionLeft;
    public Transform emotionRight;
    public float arcHeight = 0.5f;
    public float cardScale = 6.0f; 

    [Header("Emotion Data")]
    public EmotionCardSO emotionData; 
    private List<FeelingType> usedFeelings = new List<FeelingType>();

    private List<GameObject> spawnedCards = new List<GameObject>();
    private bool isBundle = false;

    protected override void Awake()
    {
        base.Awake(); 
        DontDestroyOnLoad(this.gameObject);
        if (usedFeelings == null) usedFeelings = new List<FeelingType>();
        EnsureCameraRaycaster();
    }
    private void EnsureCameraRaycaster()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // Physics 2D Raycaster가 없으면 추가 (이게 있어야 콜라이더 클릭 감지됨)
            if (mainCam.GetComponent<Physics2DRaycaster>() == null)
            {
                mainCam.gameObject.AddComponent<Physics2DRaycaster>();
                Debug.Log("[DialogueCardManager] Physics 2D Raycaster added to Main Camera.");
            }
        }
    }

    public void ResetRun()
    {
        usedFeelings.Clear();
        Debug.Log("[DialogueCardManager] 감정 카드 덱이 초기화되었습니다.");
    }
    
    public override void ShowDialogueSelectionPanel()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        ClearCards();
        if (rerollButton != null) rerollButton.SetActive(true);

        List<DialogueBundle> available = dialogueBundles.FindAll(b => !b.isBanned);
        List<DialogueBundle> selected = new List<DialogueBundle>();

        int count = Mathf.Min(3, available.Count);
        while (selected.Count < count)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            selected.Add(available[idx]);
            available.RemoveAt(idx);
        }

        for (int i = 0; i < selected.Count; i++)
        {
            CreateBundleCard(selected[i]);
        }

        isBundle = true;
        AlignCards();
    }

    private void CreateBundleCard(DialogueBundle bundle)
    {
        if (cardPrefab == null) return;
        GameObject go = Instantiate(cardPrefab, cardSpawnParent);
        
        Order order = go.GetComponent<Order>();
        if (order != null) order.SetOriginOrder(10);

        Card cardComp = go.GetComponent<Card>();
        if (cardComp != null)
        {
            cardComp.SetupDialogue(
                bundle.bundleName,
                "",
                bundle.cardSprite,
                () => OnBundleClicked(bundle)
            );
        }
        spawnedCards.Add(go);
    }

    private void OnBundleClicked(DialogueBundle selectedBundle)
    {
        Debug.Log($"[CardManager] Bundle Selected: {selectedBundle.bundleName}");
        ClearCards();
        if (rerollButton != null) rerollButton.SetActive(false);
        StartDialogueByBundle(selectedBundle);
    }

    private void StartDialogueByBundle(DialogueBundle bundle)
    {
        if (dialogueDataCSV == null) dialogueDataCSV = Resources.Load<TextAsset>("Dialogues/DialogueData");

        (string character, string fileName) = FindCharacterAndFileName(dialogueDataCSV.text, bundle.connectedFileID.ToString());

        if (!string.IsNullOrEmpty(fileName))
        {
            StartDialogue(DialogueMode.Main, fileName, character);
        }
        else
        {
            Debug.LogError($"[CardManager] Info not found for ID: {bundle.connectedFileID}");
        }
    }
    
    public override void ShowEmotionSelection(Action<FeelingType> callback)
    {
        if (cardPrefab == null) return;
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        ClearCards();
        if (rerollButton != null) rerollButton.SetActive(false);
        foreach (var data in emotionData.emotionCards)
        {
            if (!usedFeelings.Contains(data.type))
            {
                CreateEmotionCard(data, callback);
            }
        }

        if (spawnedCards.Count == 0)
        {
            ResetRun();
            ShowEmotionSelection(callback);
            return;
        }

        isBundle = false;
        AlignCards();
    }

    private void CreateEmotionCard(EmotionCardSO.EmotionCardData data, Action<FeelingType> callback)
    {
        if (cardPrefab == null) return;
        GameObject go = Instantiate(cardPrefab, cardSpawnParent);
        
        Order order = go.GetComponent<Order>();
        if (order != null) order.SetOriginOrder(20);

        Card cardComp = go.GetComponent<Card>();
        if (cardComp != null)
        {
            cardComp.SetupEmotion(
                data.cardName, 
                "",
                data.cardSprite, 
                () => OnEmotionClicked(data.type, callback)
            );
        }
        spawnedCards.Add(go);
    }

    private void OnEmotionClicked(FeelingType type, Action<FeelingType> callback)
    {
        Debug.Log($"[CardManager] Emotion Selected: {type}");
        usedFeelings.Add(type);
        callback?.Invoke(type);
        
        ClearCards();
    }
    
    private void ClearCards()
    {
        foreach (GameObject go in spawnedCards)
        {
            if (go != null) Destroy(go);
        }
        spawnedCards.Clear();
    }

    private void AlignCards()
    {
        int count = spawnedCards.Count;
        if (count == 0) return;

        float[] objLerps = new float[count];
        
        if (count == 1)
        {
            objLerps = new float[] { 0.5f };
        }
        else
        {
            float interval = 1f / (count - 1);
            for (int i = 0; i < count; i++) objLerps[i] = interval * i;
        }

        for (int i = 0; i < count; i++)
        {
            GameObject cardObj = spawnedCards[i];
            Vector3 targetPos = Vector3.zero;
            if (isBundle) targetPos = Vector3.Lerp(bundleLeft.position, bundleRight.position, objLerps[i]);
            else targetPos = Vector3.Lerp(emotionLeft.position, emotionRight.position, objLerps[i]);
            
            // 곡률
            float curve = Mathf.Sqrt(Mathf.Pow(arcHeight, 2) - Mathf.Pow(objLerps[i] - 0.5f, 2));
            if (float.IsNaN(curve)) curve = 0;
            float arcOffset = (1f - 4f * Mathf.Pow(objLerps[i] - 0.5f, 2)) * arcHeight;
            targetPos.y += arcOffset;
            Quaternion targetRot = Quaternion.identity;
            
            if (isBundle) targetRot = Quaternion.Slerp(bundleLeft.rotation, bundleRight.rotation, objLerps[i]);
            else targetRot = Quaternion.Slerp(emotionLeft.rotation, emotionRight.rotation, objLerps[i]);


            // Transform 적용
            cardObj.transform.position = targetPos;
            cardObj.transform.rotation = targetRot;
            cardObj.transform.localScale = Vector3.one * cardScale;

            // Card.cs의 마우스 오버 확대를 위한 원본 정보 저장
            Card cardComp = cardObj.GetComponent<Card>();
            if (cardComp != null)
            {
                cardComp.originPRS = new PRS(targetPos, targetRot, Vector3.one * cardScale);

            }
        }
    }
}