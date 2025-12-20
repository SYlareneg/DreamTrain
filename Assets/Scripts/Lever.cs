using DG.Tweening;
using UnityEngine;
using TMPro;

public class Lever : MonoBehaviour
{
    public static Lever Inst { get; private set; }
    void Awake() => Inst = this;

    public float leverSensitivity = 1.0f;
    public float leverRecoveryTime = 0.5f;
    public int useCost;
    [SerializeField] Transform leverUp;
    [SerializeField] Transform leverDown;
    [SerializeField] TMP_Text leverCostText;
    Vector3 lastMousePos;
    bool isLeverDrag = false;
    private void OnMouseDown()
    {
        lastMousePos = Utils.MousePos;
        isLeverDrag = true;
    }

    private void OnMouseUp()
    {
        if (isLeverDrag)
        {
            if (this.transform.position.y <= leverDown.position.y)
            {
                ActivateRouletteUsingLever();
            }
            this.transform.DOMove(leverUp.position, leverRecoveryTime).OnComplete(() => isLeverDrag = false);
        }
    }

    private void ActivateRouletteUsingLever()
    {
        if (TurnManager.Inst.nowCost >= useCost)
        {
            RouletteManager.Inst.ActivateRoulette();
            TurnManager.Inst.IncreaseCost(-useCost);
            useCost += 1;
        }
    }

    public void ActivateLever()
    {
        Sequence actSeq = DOTween.Sequence();
        actSeq.Append(this.transform.DOMove(leverDown.position, leverRecoveryTime).OnComplete(() =>
        {
            ActivateRouletteUsingLever();
        }));
        actSeq.AppendInterval(0.1f);
        actSeq.Append(this.transform.DOMove(leverUp.position, leverRecoveryTime));
    }

    private void Start()
    {
        this.transform.SetPositionAndRotation(leverUp.position, leverUp.rotation);
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
    private void Update()
    {
        if (isLeverDrag)
        {
            float relMousePosY = Utils.MousePos.y - lastMousePos.y;
            lastMousePos = Utils.MousePos;
            Vector3 newLeverPos = this.transform.position;
            newLeverPos.y += relMousePosY * leverSensitivity;
            if (newLeverPos.y < leverDown.position.y)
            {
                newLeverPos.y = leverDown.position.y;
            }
            if (newLeverPos.y > leverUp.position.y)
            {
                newLeverPos.y = leverUp.position.y;
            }
            this.transform.SetPositionAndRotation(newLeverPos, this.transform.rotation);
        }

        leverCostText.text = "현재비용: " + useCost.ToString();
    }
}
