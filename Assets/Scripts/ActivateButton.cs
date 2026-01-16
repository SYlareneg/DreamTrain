using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class ActivateButton : MonoBehaviour
{
    [Header("Settings")]
    public bool useClickEffect = true; // 클릭 시 눌리는 효과 사용 여부
    [Header("Cost")]
    public int useCost;
    [SerializeField] int maxUseCost;
    [SerializeField] SpriteRenderer costSR;
    [SerializeField] Sprite[] costSprites;

    // 내부 변수
    private Vector3 originalScale;
    private Vector3 pressedScale;

    void Start()
    {
        // 원래 크기 저장
        originalScale = transform.localScale;
        // 눌렸을 때 크기 (원래 크기의 90%)
        pressedScale = originalScale * 0.9f;

        useCost = 1;
        TurnManager.OnRouletteSpin += (x, y) =>
        {
            useCost = 1;
        };
        TurnManager.OnPlayerTurnStart += () =>
        {
            useCost = 1;
        };
    }

    private void OnMouseEnter()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color color = Color.white;
        color.r *= 0.9f;
        sr.color = color;
    }

    private void OnMouseExit()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.white;
    }

    // 마우스 버튼을 눌렀을 때
    private void OnMouseDown()
    {
        if (useClickEffect)
        {
            transform.localScale = pressedScale;
        }
    }

    // 마우스 버튼을 뗐을 때
    private void OnMouseUp()
    {
        if (useClickEffect)
        {
            transform.localScale = originalScale;
        }
    }

    // "버튼 클릭"으로 인정될 때 (눌렀다가 같은 오브젝트 위에서 뗐을 때만 실행)
    private void OnMouseUpAsButton()
    {
        if (TurnManager.Inst.nowCost >= useCost)
        {
            RouletteManager.Inst.ActivateRoulette();
            TurnManager.Inst.IncreaseCost(-useCost);
            if(useCost < maxUseCost) useCost += 1;
        }
    }

    private void Update()
    {
        costSR.sprite = costSprites[useCost - 1];
    }
}
