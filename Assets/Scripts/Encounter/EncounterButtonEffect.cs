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

    private Coroutine idleCoroutine;
    private bool isHovered = false;
    private bool isClicked = false;

    private const int frameDelay = 30;
    private readonly float[] alphaSteps = { 0.25f, 0.30f, 0.35f, 0.40f, 0.45f, 0.50f }; 
    private const float hoverAlpha = 0.55f;
    private const float hoverScale = 1.1f; 
    private const float clickedAlpha = 0.0f;

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
        
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        idleCoroutine = StartCoroutine(IdleAnimationRoutine());
    }

    void OnDisable()
    {
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
    }

    IEnumerator IdleAnimationRoutine()
    {
        int index = 0;
        bool goingUp = true;

        while (!isHovered && !isClicked)
        {
            canvasGroup.alpha = alphaSteps[index];

            for (int i = 0; i < frameDelay; i++) yield return null;

            if (goingUp)
            {
                index++;
                if (index >= alphaSteps.Length - 1) goingUp = false;
            }
            else
            {
                index--;
                if (index <= 0) goingUp = true;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isClicked || !button.interactable) return;

        isHovered = true;
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);

        canvasGroup.alpha = hoverAlpha;
        rectTransform.localScale = Vector3.one * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isClicked || !button.interactable) return;

        isHovered = false;
        rectTransform.localScale = Vector3.one;

        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        idleCoroutine = StartCoroutine(IdleAnimationRoutine());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!button.interactable) return;
        
        isClicked = true;
        if (idleCoroutine != null) StopCoroutine(idleCoroutine);
        
        canvasGroup.alpha = clickedAlpha;
        rectTransform.localScale = Vector3.one;
    }
}