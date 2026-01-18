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
    public GameObject panelRoot;        // 룰렛 전체 패널
    public Transform wheelContainer;    // 회전할 룰렛 원판 (Z축 회전)
    public Button spinButton;           // 회전 버튼
    public TextMeshProUGUI probabilityText; // 성공 확률 텍스트

    [Header("Resources")]
    public PlayerStatsSo playerStats;
    public GameObject segmentPrefab;    // 룰렛 조각 프리팹 (Pie Piece)
    // 조각 색상 (기획서 기반)
    public Color colorFail = new Color(0.5f, 0.5f, 0.5f);     // 회색
    public Color colorSuccess = new Color(0.3f, 0.3f, 0.3f);  // 진한 회색 (예시)
    public Color colorGreat = Color.white;                    // 흰색

    private List<RouletteResultType> currentSegments = new List<RouletteResultType>();
    private bool isSpinning = false;
    public TextMeshProUGUI resultText;
    
    // 콜백: 결과가 나왔을 때 실행할 함수 (성공여부 반환)
    private System.Action<RouletteResultType> onCompleteCallback;

    private const int TOTAL_SLOTS = 12;

    private void Start()
    {
        //panelRoot.SetActive(false);
    }

    public void Open(string statName, int difficulty, System.Action<RouletteResultType> onComplete)
    {
        resultText.text = "";
        panelRoot.SetActive(true);
        Debug.Log(statName);
        this.onCompleteCallback = onComplete;
        isSpinning = false;
        
        Debug.Log(spinButton);
        if (spinButton == null)
        {
            Debug.Log("finding button");
            Button[] allButtons = panelRoot.GetComponentsInChildren<Button>(true);
            foreach (var btn in allButtons)
            {
                Debug.Log($"button name : {btn.name}");
                if (btn.name == "RollButton" )
                {
                    spinButton = btn;
                    break;
                }
            }
            Debug.Log(spinButton);
            if (spinButton != null)
            {
                // 혹시 모를 중복 방지를 위해 기존 리스너 제거 후 추가
                spinButton.onClick.RemoveAllListeners(); 
                spinButton.onClick.AddListener(OnClickSpin);
            }
            else
            {
                Debug.LogWarning("[EncounterRouletteUI] 'RollButton'이라는 이름의 버튼을 찾을 수 없습니다. 이름을 확인해주세요.");
            }
        }
        
        spinButton.interactable = true;

        // 2. 슬롯 계산 (기획서 공식 적용)
        CalculateSegments(statName, difficulty);

        // 3. UI 그리기
        DrawWheel();
        
        float successChance = (float)currentSegments.FindAll(x => x != RouletteResultType.Fail).Count / TOTAL_SLOTS * 100f;
        probabilityText.text = $"성공 확률 {successChance:F0}%";
    }

    void CalculateSegments(string statName, int requiredStat)
    {
        StatType type = (StatType)System.Enum.Parse(typeof(StatType), statName);
        int currentStatVal = playerStats.GetStat(type);
        
        // 스탯이 0이면 무조건 실패 
        if (playerStats.IsAutoFail(type))
        {
            currentSegments.Clear();
            for(int i=0; i<TOTAL_SLOTS; i++) currentSegments.Add(RouletteResultType.Fail);
            return;
        }

        int failCount = 4;
        int successCount = 7;
        int greatCount = 1;

        int diff = currentStatVal - requiredStat;

        if (diff > 0) 
        {
            // 차이만큼 실패 -> 성공
            int convertToSuccess = Mathf.Min(failCount - 1, diff); // 실패는 최소 1개 남아야 함 [cite: 157]
            failCount -= convertToSuccess;
            successCount += convertToSuccess;

            int remainingDiff = diff - convertToSuccess;
            if (remainingDiff > 0)
            {
                successCount -= remainingDiff; // 성공 칸을 대성공으로
                greatCount += remainingDiff;
                // 성공 칸도 최소 1개는 남아야 한다면 체크 필요, 기획서엔 실패 칸 제한만 명시됨
            }
        }
        else if (diff < 0) // 플레이어 스탯이 더 낮음
        {
            int absDiff = Mathf.Abs(diff);
            // 차이만큼 성공 -> 실패
            int convertToFail = Mathf.Min(successCount - 1, absDiff); // 성공은 최소 1개 남아야 함 
            successCount -= convertToFail;
            failCount += convertToFail;
        }
        
        if (playerStats.GetStat(StatType.Luck) >= 9)
        {
            int convertToGreat = Mathf.FloorToInt(successCount / 2f); // [cite: 371]
            successCount -= convertToGreat;
            greatCount += convertToGreat;
            Debug.Log($"행운 9 효과 발동: 성공 {convertToGreat}칸이 대성공으로 변경됨");
        }

        // 리스트 구성 (순서대로 배치)
        currentSegments.Clear();
        
        // 대성공 -> 성공 -> 실패 순으로 배치
        for (int i = 0; i < greatCount; i++) currentSegments.Add(RouletteResultType.GreatSuccess);
        for (int i = 0; i < successCount; i++) currentSegments.Add(RouletteResultType.Success);
        for (int i = 0; i < failCount; i++) currentSegments.Add(RouletteResultType.Fail);
    }

    void DrawWheel()
    {
        foreach (Transform child in wheelContainer) Destroy(child.gameObject);

        // 룰렛은 총 12조각
        float angleStep = 360f / TOTAL_SLOTS; // 30도

        for (int i = 0; i < TOTAL_SLOTS; i++)
        {
            // 2. 프리팹 생성 (부모는 WheelContainer)
            GameObject segObj = Instantiate(segmentPrefab, wheelContainer);
            
            // 3. 위치와 크기 맞추기 (RectTransform)
            RectTransform rect = segObj.GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero; // 중앙 정렬
            rect.sizeDelta = Vector2.zero; // 부모 크기에 꽉 채우기 (Stretch-Stretch 일 경우)
            // 만약 부모가 고정 크기라면 아래처럼 부모 크기에 맞춤
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image segImg = segObj.GetComponent<Image>();
            
            // 4. 색상 지정
            switch (currentSegments[i])
            {
                case RouletteResultType.Fail: segImg.color = colorFail; break;
                case RouletteResultType.Success: segImg.color = colorSuccess; break;
                case RouletteResultType.GreatSuccess: segImg.color = colorGreat; break;
            }

            // 5. Fill Amount 설정 (12조각 중 1조각 크기)
            segImg.fillAmount = 1f / TOTAL_SLOTS; 

            // 6. 회전시켜서 자리 잡기
            // i번째 조각은 i * 30도 만큼 회전해야 자기 자리에 감.
            // 시계방향(-)으로 돌려야 함.
            // Fill Origin이 Top이므로 z축 회전만 하면 됨.
            rect.localRotation = Quaternion.Euler(0, 0, -i * angleStep);
        }
        
        // 룰렛 컨테이너 초기화
        wheelContainer.rotation = Quaternion.identity;
    }

    public void OnClickSpin()
    {
        if (isSpinning) return;
        StartCoroutine(SpinRoutine());
    }

    System.Collections.IEnumerator SpinRoutine()
    {
        isSpinning = true;
        spinButton.interactable = false;

        // 랜덤 결과 각도 계산 (360 * 바퀴 수 + 랜덤 각도)
        int laps = 5; 
        // 12개 슬롯 중 하나에 멈춤. 
        // 바늘이 12시 고정(상단)이라고 가정하면, 컨테이너가 회전해서 멈춘 각도에 따라 결과 결정.
        // 슬롯 i의 각도 범위: -(i * 30) 도.
        
        int targetIndex = Random.Range(0, TOTAL_SLOTS); // 결정된 결과 인덱스
        float segmentAngle = 360f / TOTAL_SLOTS;
        
        // 목표 각도: (TargetIndex * 30) 도 만큼 '반대'로 돌려야 12시에 옴.
        // 하지만 시계방향 회전이므로, 360 - (Index * 30) + 오차범위
        float finalAngle = (targetIndex * segmentAngle); 
        
        float duration = 2.5f;
        Vector3 targetRot = new Vector3(0, 0, 360 * laps + finalAngle);

        wheelContainer.DORotate(targetRot, duration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutCubic); // 급격한 감속 [cite: 177]

        yield return new WaitForSeconds(duration);

        // 결과 판정
        // 실제 회전한 각도에서 360으로 나눈 나머지로 인덱스 역산 (정확성을 위해)
        float normalizedAngle = wheelContainer.eulerAngles.z % 360;
        // 12시(0도)에 걸린 조각 찾기. 
        // 컨테이너가 시계방향으로 돌았으므로, 0도에 있는 건 
        // (360 - angle) / 30 근사값 인덱스
        
        int landedIndex = Mathf.RoundToInt(normalizedAngle / segmentAngle);
        if (landedIndex >= TOTAL_SLOTS) landedIndex = 0;
        
        // 인덱스 방향 보정 (회전 방향에 따라 인덱스 순서가 다를 수 있음, 여기서는 0번이 12시 시작 기준)
        // 컨테이너가 30도(시계) 돌면 12시엔 11번 인덱스(반시계 방향 인덱스)가 옴.
        // 단순하게 Random.Range로 정한 targetIndex를 그대로 결과로 써도 무방함 (시각적 싱크만 맞다면)
        RouletteResultType result = currentSegments[targetIndex]; // 미리 정한 결과 사용
        yield return new WaitForSeconds(0.5f); // 결과 확인 대기
        switch (result){
            case (RouletteResultType.Success): resultText.text = "성 공 !"; break;
            case (RouletteResultType.GreatSuccess): resultText.text = "대 성 공 !"; break;
            case (RouletteResultType.Fail): resultText.text = "실 패 !"; break;
        }
        yield return new WaitForSeconds(1f); // 결과 확인 대기
        resultText.text = "";
        if (playerStats != null)
        {
            playerStats.EvaluateRouletteResult(result);
        }
        
        panelRoot.SetActive(false);
        

        onCompleteCallback?.Invoke(result);
    }

    // TODO: 실제 캐릭터 스탯 연결 필요
    int GetPlayerStat(string statName)
    {
        // EncounterManager.Instance.characterData 등을 참조
        // 여기서는 테스트 값 반환
        return 5; 
    }
}