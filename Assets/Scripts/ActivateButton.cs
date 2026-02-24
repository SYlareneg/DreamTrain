using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections;

public class ActivateButton : MonoBehaviour
{
    [Header("Settings")]
    public bool useClickEffect = true; // 클릭 시 눌리는 효과 사용 여부
    [SerializeField] float clickScale = 0.9f; // 클릭 시 크기 비율
    [SerializeField] Sprite originalSprite;
    [SerializeField] Sprite pressedSprite;
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
        pressedScale = originalScale * clickScale;

        GetComponent<SpriteRenderer>().sprite = originalSprite;

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
        if(TurnManager.Inst.isLoading && (TurnManager.Inst.characterSO.isTutorial == false || TutorialManager.Inst.rouletteActivate == false)) return;
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
        if(TurnManager.Inst.isLoading && (TurnManager.Inst.characterSO.isTutorial == false || TutorialManager.Inst.rouletteActivate == false)) return;
        if (useClickEffect)
        {
            transform.localScale = pressedScale;
            GetComponent<SpriteRenderer>().sprite = pressedSprite;
        }
    }

    // 마우스 버튼을 뗐을 때
    private void OnMouseUp()
    {
        if (useClickEffect)
        {
            transform.localScale = originalScale;
            GetComponent<SpriteRenderer>().sprite = originalSprite;
        }
    }

    // "버튼 클릭"으로 인정될 때 (눌렀다가 같은 오브젝트 위에서 뗐을 때만 실행)
    private void OnMouseUpAsButton()
    {
        if(TurnManager.Inst.isLoading && (TurnManager.Inst.characterSO.isTutorial == false || TutorialManager.Inst.rouletteActivate == false)) return;
        if (RouletteManager.Inst.isTriggerActivated == true || TurnManager.Inst.nowCost >= useCost)
        {
            StartCoroutine(ButtonActivate());
            // if(useCost < maxUseCost) useCost += 1;
        }
        else
        {
            GameManager.Inst.ShowCostWarning(false);
        }
    }

    IEnumerator ButtonActivate()
    {
        bool checkTrigger = RouletteManager.Inst.isTriggerActivated;
        RouletteManager.Inst.ActivateRoulette();
        if(checkTrigger == false) TurnManager.Inst.IncreaseCost(-useCost);
        TurnManager.Inst.isLoading = true;
        GetComponent<AudioSource>().PlayOneShot(SoundManager.Inst.rouletteButtonSFX);
        yield return new WaitForSeconds(0.5f);
        if(checkTrigger == false) RouletteManager.Inst.Spin(true, 1);
        if(TurnManager.Inst.characterSO.isTutorial == false || TutorialManager.Inst.tutorialStage == 0) TurnManager.Inst.isLoading = false;
    }

    private void Update()
    {
        costSR.sprite = costSprites[useCost - 1];
    }
}
