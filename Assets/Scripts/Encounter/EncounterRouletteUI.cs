using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;

public enum RouletteResultType
{
    Fail,
    Success,
    GreatSuccess
}

public class EncounterRouletteUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject panelRoot;        
    public Transform wheelContainer;    
    public Button spinButton;           
    public TextMeshProUGUI probabilityText; 
    public TextMeshProUGUI resultText;

    [Header("Roulette Images")]
    public Image imgFail;     
    public Image imgSuccess;  
    public Image imgGreat;    
    
    [Header("Data")]
    public PlayerStatsSo playerStats;
    private Sprite originalSpinSprite;
    
    [Header("Audio")]
    public AudioClip rouletteSfx;
    private AudioSource audioSource;

    private int totalSlots = 12; 
    private List<RouletteResultType> currentSegments = new List<RouletteResultType>();
    private bool isSpinning = false;
    private System.Action<RouletteResultType> onCompleteCallback;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void Start()
    {
        originalSpinSprite = Resources.Load<Sprite>("Encounters/Images/버튼_누르기전_01");
    }

    public void Open(string statName, int difficulty, System.Action<RouletteResultType> onComplete)
    {
        resultText.text = "";
        panelRoot.SetActive(true);
        this.onCompleteCallback = onComplete;
        isSpinning = false;
        
        // 버튼 연결
        if (spinButton != null)
        {
            if (originalSpinSprite == null) originalSpinSprite = spinButton.image.sprite;
            // 이미지 초기화 (재사용 시 버튼 이미지가 눌린 상태로 남아있는 것 방지)
            if (originalSpinSprite != null) spinButton.image.sprite = originalSpinSprite;
            
            spinButton.interactable = true;
            spinButton.onClick.RemoveAllListeners(); 
            spinButton.onClick.AddListener(OnClickSpin);
        }

        // 1. 슬롯 데이터 계산
        CalculateSegments(statName, difficulty);

        // 2. 이미지 FillAmount 조절 (Visual 업데이트)
        DrawWheelImages();
        
        // 3. 확률 텍스트 갱신
        int successCount = currentSegments.FindAll(x => x != RouletteResultType.Fail).Count;
        float successChance = (float)successCount / totalSlots * 100f;
        probabilityText.text = $"성공 확률 {successChance:F0}%";
    }

    void CalculateSegments(string statName, int requiredStat)
    {
        StatType type = (StatType)System.Enum.Parse(typeof(StatType), statName);
        int currentStatVal = playerStats.GetStat(type);
        
        currentSegments.Clear();

        if (playerStats.IsAutoFail(type))
        {
            for(int i=0; i<totalSlots; i++) currentSegments.Add(RouletteResultType.Fail);
            return;
        }

        int failCount = 4;
        int successCount = 7;
        int greatCount = 1;

        int diff = currentStatVal - requiredStat;

        if (diff > 0) 
        {
            int convertToSuccess = Mathf.Min(failCount - 1, diff);
            failCount -= convertToSuccess;
            successCount += convertToSuccess;
            int remainingDiff = diff - convertToSuccess;
            if (remainingDiff > 0)
            {
                successCount -= remainingDiff;
                greatCount += remainingDiff;
            }
        }
        else if (diff < 0)
        {
            int absDiff = Mathf.Abs(diff);
            int convertToFail = Mathf.Min(successCount - 1, absDiff);
            successCount -= convertToFail;
            failCount += convertToFail;
        }
        
        if (playerStats.GetStat(StatType.Luck) >= 9)
        {
            int convertToGreat = Mathf.FloorToInt(successCount / 2f);
            successCount -= convertToGreat;
            greatCount += convertToGreat;
        }

        for (int i = 0; i < greatCount; i++) currentSegments.Add(RouletteResultType.GreatSuccess);
        for (int i = 0; i < successCount; i++) currentSegments.Add(RouletteResultType.Success);
        for (int i = 0; i < failCount; i++) currentSegments.Add(RouletteResultType.Fail);
    }

    void DrawWheelImages()
    {
        wheelContainer.rotation = Quaternion.identity;

        int greatCount = currentSegments.FindAll(x => x == RouletteResultType.GreatSuccess).Count;
        int successCount = currentSegments.FindAll(x => x == RouletteResultType.Success).Count;

        if (imgFail != null) imgFail.gameObject.SetActive(true);

        if (imgSuccess != null)
        {
            float totalSuccessAmount = (float)(greatCount + successCount) / totalSlots;
            imgSuccess.fillAmount = totalSuccessAmount;
            imgSuccess.gameObject.SetActive(totalSuccessAmount > 0);
        }

        if (imgGreat != null)
        {
            float greatAmount = (float)greatCount / totalSlots;
            imgGreat.fillAmount = greatAmount;
            imgGreat.gameObject.SetActive(greatAmount > 0);
        }
    }

    public void OnClickSpin()
    {
        if (isSpinning) return;

        Sprite pressedSprite = Resources.Load<Sprite>("Encounters/Images/버튼_누름_01");
        if (pressedSprite != null && spinButton != null)
        {
            spinButton.image.sprite = pressedSprite;
        }

        StartCoroutine(SpinRoutine());
    }

    System.Collections.IEnumerator SpinRoutine()
    {
        isSpinning = true;
        spinButton.interactable = false;
        
        // [수정됨] 사운드 재생: 피치 조절 없이 단순 1회 재생
        if (audioSource != null && rouletteSfx != null)
        {
            audioSource.PlayOneShot(rouletteSfx);
        }
        
        int targetIndex = Random.Range(0, totalSlots);
        float segmentAngle = 360f / totalSlots; 
        float targetAngleZ = (targetIndex * segmentAngle) + (segmentAngle / 2f); 

        int laps = 5; 
        float randomOffset = Random.Range(-10f, 10f);
        
        float finalRotationZ = targetAngleZ - (360 * laps) + randomOffset; 

        Vector3 targetRot = new Vector3(0, 0, finalRotationZ);
        wheelContainer.DORotate(targetRot, 3.0f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic); 

        yield return new WaitForSeconds(3.0f);
        
        // 회전 종료 후 로직
        RouletteResultType result = currentSegments[targetIndex];
        
        yield return new WaitForSeconds(0.5f);
        probabilityText.text = "";
        switch (result){
            case (RouletteResultType.Success): 
                resultText.text = "<color=#FFFFFF>성 공 !</color>";
                break;
            case (RouletteResultType.GreatSuccess): 
                resultText.text = "<color=#77B0FF>대 성 공 !</color>"; 
                break;
            case (RouletteResultType.Fail): 
                resultText.text = "<color=#FF0000>실 패 !</color>";
                break;
        }

        if (playerStats != null) playerStats.EvaluateRouletteResult(result);
        
        yield return new WaitForSeconds(2f);
        
        panelRoot.SetActive(false);
        onCompleteCallback?.Invoke(result);
    }

    public float GetSuccessProbability(string statName, int requiredStat)
    {
        StatType type = (StatType)System.Enum.Parse(typeof(StatType), statName);
    
        if (playerStats.IsAutoFail(type)) return 0f;

        int currentStatVal = playerStats.GetStat(type);
        int failCount = 4;
        int successCount = 7;
        int greatCount = 1;

        int diff = currentStatVal - requiredStat;

        if (diff > 0) 
        {
            int convertToSuccess = Mathf.Min(failCount - 1, diff);
            failCount -= convertToSuccess;
            successCount += convertToSuccess;
            int remainingDiff = diff - convertToSuccess;
            if (remainingDiff > 0)
            {
                successCount -= remainingDiff;
                greatCount += remainingDiff;
            }
        }
        else if (diff < 0)
        {
            int absDiff = Mathf.Abs(diff);
            int convertToFail = Mathf.Min(successCount - 1, absDiff);
            successCount -= convertToFail;
            failCount += convertToFail;
        }
    
        if (playerStats.GetStat(StatType.Luck) >= 9)
        {
            int convertToGreat = Mathf.FloorToInt(successCount / 2f);
            successCount -= convertToGreat;
            greatCount += convertToGreat;
        }

        // (성공 + 대성공) / 전체 슬롯 비율 반환
        return (float)(successCount + greatCount) / totalSlots * 100f;
    }
}
