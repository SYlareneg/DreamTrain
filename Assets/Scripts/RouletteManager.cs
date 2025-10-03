using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class RouletteManager : MonoBehaviour
{
    public static RouletteManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] GameObject roulettePiecePrefab;
    [SerializeField] RouletteSO rouletteSO;

    public static int rouletteNum = 13;

    public RoulettePiece[] roulettePieces = new RoulettePiece[rouletteNum];
    public int playerLookat;
    public int enemyLookat;
    public int triggerPos;
    public bool isTriggerActivated;

    bool spinFlag = false;
    bool isRouletteDrag;
    bool onRouletteArea;
    Quaternion lastRotation;

    public static float spinDelay = 0.7f;
    public int spinCount;
    public int spinCount_Turn;
    public int spinDistance;
    public int spinDistance_Turn;
    public int spinDirection;

    public void Spin(bool isClockwise, int pieces)
    {
        if (!spinFlag)
        {
            spinCount++;
            spinCount_Turn++;
            spinDistance += pieces;
            spinDistance_Turn += pieces;
            spinDirection = isClockwise ? 1 : 0;
            spinFlag = true;
            var newRotation = this.transform.rotation;
            if (isClockwise)
            {
                TurnManager.OnRouletteSpinCW?.Invoke();
                pieces *= -1;
            }
            else
            {
                TurnManager.OnRouletteSpinCCW?.Invoke();
            }
            newRotation *= Quaternion.Euler(0f, 0f, 360f * pieces / rouletteNum);
            playerLookat = (playerLookat + rouletteNum - pieces) % rouletteNum;
            enemyLookat = (enemyLookat + rouletteNum - pieces) % rouletteNum;
            this.transform.DORotateQuaternion(newRotation, spinDelay).OnComplete(() => spinFlag = false);

            //TEMP: Enemy passive trigger
            TurnManager.Inst.TriggerEnemyPassive(1);
        }

    }

    public void ActivateRoulette()
    {
        TurnManager.OnRouletteActivate?.Invoke();
        roulettePieces[playerLookat].Activate(false);
        roulettePieces[enemyLookat].Activate(true);
    }

    public void TriggerRoulette()
    {
        TurnManager.OnRouletteTrigger?.Invoke();
        roulettePieces[triggerPos].Setup(rouletteSO.roulettePattern[20 - triggerPos]);
        roulettePieces[triggerPos].Trigger(true);
    }

    public void EnchantRoulettePiece(bool isEnemy, ERouletteType rType, int rValue)
    {
        RouletteItem rItem = new RouletteItem();
        rItem.type = rType;
        rItem.value = rValue;
        if(isEnemy)
        {
            roulettePieces[enemyLookat].Setup(rItem);
        }
        else
        {
            roulettePieces[playerLookat].Setup(rItem);
        }
    }

    public void InitRoulette()
    {
        playerLookat = 0;
        enemyLookat = 7;
        triggerPos = 8;
        spinCount = 0;
        spinCount_Turn = 0;
        spinDistance = 0;
        spinDistance_Turn = 0;
        isTriggerActivated = false;

        for (int i = 0; i < rouletteNum; i++)
        {
            var roulettePiece = Instantiate(roulettePiecePrefab, this.transform.position, Utils.QI);
            roulettePiece.transform.rotation *= Quaternion.Euler(0f, 0f, -180f + 360f * i / rouletteNum);
            roulettePiece.transform.SetParent(this.transform, true);
            roulettePieces[i] = roulettePiece.GetComponent<RoulettePiece>();
            roulettePieces[i].Setup(rouletteSO.roulettePattern[((i > 7) ? (20 - i) : (7 - i))]);
        }
        var tempRoulettePiece = roulettePieces[triggerPos].roulette;
        tempRoulettePiece.type = ERouletteType.None;
        roulettePieces[triggerPos].Setup(tempRoulettePiece);
    }

    public void RouletteMouseDown()
    {
        if(!spinFlag && !TurnManager.Inst.isLoading)
        {
            spinFlag = true;
            TurnManager.Inst.isLoading = true;
            isRouletteDrag = true;
            lastRotation = this.transform.rotation;
        }
    }

    public void RouletteMouseUp()
    {
        if(isRouletteDrag)
        {
            isRouletteDrag = false;
            this.transform.DORotateQuaternion(lastRotation, 0.7f).OnComplete(() =>
            {
                spinFlag = false;
                TurnManager.Inst.isLoading = false;
            });
        }
    }

    void RouletteDrag()
    {
        Vector3 mouseOrtho = new Vector3(Utils.MousePos.x, Utils.MousePos.y, this.transform.position.z);
        Quaternion mouseRotation = Quaternion.FromToRotation(this.transform.position, mouseOrtho);
        this.transform.rotation = lastRotation * mouseRotation;
    }

    void DetectRouletteArea()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
        int layer = LayerMask.NameToLayer("RouletteArea");
        onRouletteArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
        if(onRouletteArea == false)
        {
            RouletteMouseUp();
        }
    }

    private void Start()
    {
        TurnManager.OnPlayerTurnStart += () => { spinCount_Turn = 0; spinDistance_Turn = 0; };
    }

    private void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart = null;
    }

    private void Update()
    {
        if (isRouletteDrag)
        {
            RouletteDrag();
        }

        DetectRouletteArea();
    }
}
