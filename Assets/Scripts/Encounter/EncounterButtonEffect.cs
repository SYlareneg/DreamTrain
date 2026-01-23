using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class EncounterButtonEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Button button;

    private Coroutine currentCoroutine;
    private bool isHovered = false;
    private bool isClicked = false;

    [Header("Animation Settings")]
    [SerializeField] private float minAlpha = 0.25f;
    [SerializeField] private float maxAlpha = 0.60f;
    [SerializeField] private float animationSpeed = 2.0f;
    [SerializeField] private float hoverAlpha = 070f;
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float clickedAlpha = 0.0f;
    [SerializeField] private float transitionDuration = 0.1f; 

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
    }

    void OnEnable()
    {
        isHovered = false;
        isClicked = false;
        rectTransform.localScale = Vector3.one;
        
        StartAnimation(IdleAnimationRoutine());
    }

    void OnDisable()
    {
        StopAnimation();
    }

    void StartAnimation(IEnumerator routine)
    {
        StopAnimation();
        currentCoroutine = StartCoroutine(routine);
    }

    void StopAnimation()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
    }

    // [핵심 변경] 부드러운 Idle 애니메이션
    IEnumerator IdleAnimationRoutine()
    {
        float time = 0f;
        // 시작 시 랜덤한 시간으로 설정하여 여러 버튼이 있을 때 동시에 깜빡이는 현상 방지 (선택 사항)
        // time = Random.Range(0f, 10f); 

        while (!isHovered && !isClicked)
        {
            time += Time.deltaTime * animationSpeed;
            
            // 0.0 ~ 1.0 사이를 오가는 값 생성 (PingPong과 유사하지만 Sin이 더 부드러움)
            float t = (Mathf.Sin(time) + 1f) * 0.5f; 
            
            // Min과 Max 사이를 t만큼 보간
            canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

            yield return null;
        }
    }

    // [추가] 부드러운 스케일/알파 전환용 코루틴
    IEnumerator TransitionRoutine(float targetScale, float targetAlpha)
    {
        float startScale = rectTransform.localScale.x;
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            
            // 부드러운 움직임을 위해 SmoothStep 사용
            t = Mathf.SmoothStep(0f, 1f, t);

            float currentScale = Mathf.Lerp(startScale, targetScale, t);
            rectTransform.localScale = Vector3.one * currentScale;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        // 최종값 보정
        rectTransform.localScale = Vector3.one * targetScale;
        canvasGroup.alpha = targetAlpha;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isClicked || !button.interactable) return;

        isHovered = true;
        // 즉시 전환 대신 부드러운 전환 실행
        StartAnimation(TransitionRoutine(hoverScale, hoverAlpha));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isClicked || !button.interactable) return;

        isHovered = false;
        
        // 크기는 원래대로 줄이고, 알파값은 Idle로 돌아가기 위해 코루틴 재시작
        // *여기서는 Transition 없이 바로 Idle로 가거나, 줄어드는 연출 후 Idle로 가게 할 수 있습니다.
        // 아래 코드는 '줄어드는 연출' 후 'Idle'로 넘어가는 방식입니다.
        StartCoroutine(ExitRoutine());
    }

    IEnumerator ExitRoutine()
    {
        // 1. 먼저 원래 크기로 부드럽게 복귀
        yield return StartCoroutine(TransitionRoutine(1.0f, (minAlpha + maxAlpha) / 2));
        
        // 2. 다시 깜빡임 시작
        StartAnimation(IdleAnimationRoutine());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isClicked = true;
        StopAnimation();
        
        canvasGroup.alpha = clickedAlpha;
        rectTransform.localScale = Vector3.one;
    }
}