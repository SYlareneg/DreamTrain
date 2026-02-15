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
    [SerializeField] private float minAlpha = 0.40f;
    [SerializeField] private float maxAlpha = 0.70f;
    [SerializeField] private float animationSpeed = 2.0f;
    
    [Header("Hover Settings")]
    [SerializeField] private float hoverAlpha = 0.9f; 
    [SerializeField] private float hoverScale = 1.1f;
    
    [Header("Click Settings")]
    [SerializeField] private float clickedAlpha = 0.7f; 
    [SerializeField] private float transitionDuration = 0.1f; 

    [Header("Disabled Settings")]
    [SerializeField] private float disabledAlpha = 0.15f; 
    [SerializeField] private float disabledScale = 1.0f;

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

        if (button != null && !button.interactable)
        {
            SetDisabledState();
        }
        else
        {
            StartAnimation(IdleAnimationRoutine());
        }
    }

    void OnDisable()
    {
        StopAnimation();
    }
    
    void SetDisabledState()
    {
        StopAnimation(); 
        canvasGroup.alpha = disabledAlpha; 
        rectTransform.localScale = Vector3.one * disabledScale;
    }

    void StartAnimation(IEnumerator routine)
    {
        StopAnimation();
        currentCoroutine = StartCoroutine(routine);
    }

    void StopAnimation()
    {
        if (currentCoroutine != null) StopCoroutine(currentCoroutine);
        currentCoroutine = null;
    }

    IEnumerator IdleAnimationRoutine()
    {
        float time = Random.Range(0f, 10f); 

        while (!isHovered && !isClicked)
        {
            if (!button.interactable)
            {
                SetDisabledState();
                yield break;
            }

            time += Time.deltaTime * animationSpeed;
            float t = (Mathf.Sin(time) + 1f) * 0.5f; 
            canvasGroup.alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

            yield return null;
        }
    }

    IEnumerator TransitionRoutine(float targetScale, float targetAlpha)
    {
        float startScale = rectTransform.localScale.x;
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / transitionDuration;
            
            if (!button.interactable)
            {
                SetDisabledState();
                yield break;
            }

            t = Mathf.SmoothStep(0f, 1f, t);

            float currentScale = Mathf.Lerp(startScale, targetScale, t);
            rectTransform.localScale = Vector3.one * currentScale;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        rectTransform.localScale = Vector3.one * targetScale;
        canvasGroup.alpha = targetAlpha;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isClicked || !button.interactable) return;

        isHovered = true;
        StartAnimation(TransitionRoutine(hoverScale, hoverAlpha));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isClicked || !button.interactable) return;

        isHovered = false;
        StartCoroutine(ExitRoutine());
    }

    IEnumerator ExitRoutine()
    {
        yield return StartCoroutine(TransitionRoutine(1.0f, (minAlpha + maxAlpha) / 2));
        if (button.interactable)
        {
            StartAnimation(IdleAnimationRoutine());
        }
        else
        {
            SetDisabledState();
        }
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