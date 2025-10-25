using DG.Tweening;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public static Lever Inst { get; private set; }
    void Awake() => Inst = this;

    public float leverSensitivity = 1.0f;
    public float leverRecoveryTime = 0.5f;
    public int useCost;
    [SerializeField] Transform leverUp;
    [SerializeField] Transform leverDown;
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
                if (TurnManager.Inst.nowCost >= useCost)
                {
                    TurnManager.Inst.IncreaseCost(-useCost);
                    RouletteManager.Inst.ActivateRoulette();
                }
            }
            this.transform.DOMove(leverUp.position, leverRecoveryTime).OnComplete(() => isLeverDrag = false);
        }
    }

    public void ActivateLever()
    {
        Sequence actSeq = DOTween.Sequence();
        actSeq.Append(this.transform.DOMove(leverDown.position, leverRecoveryTime).OnComplete(() =>
        {
            if (this.transform.position.y <= leverDown.position.y)
            {
                if (TurnManager.Inst.nowCost >= useCost)
                {
                    TurnManager.Inst.IncreaseCost(-useCost);
                    RouletteManager.Inst.ActivateRoulette();
                }
            }
        }));
        actSeq.AppendInterval(0.1f);
        actSeq.Append(this.transform.DOMove(leverUp.position, leverRecoveryTime));
    }

    private void Start()
    {
        this.transform.SetPositionAndRotation(leverUp.position, leverUp.rotation);
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
    }
}
