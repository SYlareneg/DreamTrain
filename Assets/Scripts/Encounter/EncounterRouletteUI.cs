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

    private int totalSlots = 12; // 12조각 고정
    private List<RouletteResultType> currentSegments = new List<RouletteResultType>();
    private bool isSpinning = false;
    private System.Action<RouletteResultType> onCompleteCallback;

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
            spinButton.image.sprite = originalSpinSprite;
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
        // ... (기존 계산 로직 그대로 유지) ...
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

        // 리스트 구성: 대성공 -> 성공 -> 실패 순서 (중요)
        for (int i = 0; i < greatCount; i++) currentSegments.Add(RouletteResultType.GreatSuccess);
        for (int i = 0; i < successCount; i++) currentSegments.Add(RouletteResultType.Success);
        for (int i = 0; i < failCount; i++) currentSegments.Add(RouletteResultType.Fail);
    }

    // [핵심 로직] 이미지를 겹쳐서 표현
    void DrawWheelImages()
    {
        // 1. 회전값 초기화
        wheelContainer.rotation = Quaternion.identity;

        // 2. 개수 카운트
        int greatCount = currentSegments.FindAll(x => x == RouletteResultType.GreatSuccess).Count;
        int successCount = currentSegments.FindAll(x => x == RouletteResultType.Success).Count;
        // failCount는 나머지 영역이므로 계산 불필요

        // 3. 이미지 세팅
        
        // 배경(실패): 항상 100% 보이게 둠
        if (imgFail != null) 
        {
            imgFail.gameObject.SetActive(true);
        }

        // 성공 레이어 (노란색)
        // [중요] 성공 이미지는 (대성공 + 성공) 영역만큼 채웁니다.
        // 왜냐하면 대성공 이미지가 그 위를 덮을 것이기 때문입니다.
        if (imgSuccess != null)
        {
            float totalSuccessAmount = (float)(greatCount + successCount) / totalSlots;
            imgSuccess.fillAmount = totalSuccessAmount;
            imgSuccess.gameObject.SetActive(totalSuccessAmount > 0);
        }

        // 대성공 레이어 (민트색)
        // 맨 위에 그려지며, 대성공 개수만큼만 채웁니다.
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
        else
        {
            Debug.LogWarning("눌린 버튼 이미지를 찾을 수 없습니다. 경로를 확인하세요: Resources/Encounters/Images/버튼_누름");
        }

        StartCoroutine(SpinRoutine());
    }

    System.Collections.IEnumerator SpinRoutine()
    {
        isSpinning = true;
        spinButton.interactable = false;

        // 결과 인덱스 랜덤 결정 (0 ~ 11)
        int targetIndex = Random.Range(0, totalSlots);
        

        float segmentAngle = 360f / totalSlots; // 30도
        float targetAngleZ = (targetIndex * segmentAngle); 
        
        // 5바퀴 + 목표 각도 + 랜덤 오차(칸 내부)
        int laps = 5; 
        float randomOffset = Random.Range(-12f, 12f); // 경계선 피하기 위한 오차
        float finalRotationZ = (360 * laps) + targetAngleZ + randomOffset; 

        Vector3 targetRot = new Vector3(0, 0, finalRotationZ);

        // DOTween 회전
        wheelContainer.DORotate(targetRot, 3.0f, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic); 

        yield return new WaitForSeconds(3.0f);

        // 결과 판정
        RouletteResultType result = currentSegments[targetIndex];
        
        yield return new WaitForSeconds(0.5f);
        
        switch (result){
            case (RouletteResultType.Success): 
                resultText.text = "<color=#FFFFFF>성 공 !</color>";
                break;
            case (RouletteResultType.GreatSuccess): 
                resultText.text = "<color=#FFFFFF>대 성 공 !</color>"; 
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
}