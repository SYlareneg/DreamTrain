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

    public static int rouletteNum = 12;

    public RoulettePiece[] roulettePieces = new RoulettePiece[rouletteNum];
    public int playerLookat;
    public int enemyLookat;
    public int triggerPos;
    public RouletteItem triggerPiece;
    public RouletteItem enemyTriggerPiece;
    public RouletteItem triggerPiece_None;
    public bool isTriggerActivated;

    public bool spinFlag = false;
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
            TurnManager.OnRouletteSpin?.Invoke(pieces);
            var newRotation = this.transform.rotation;
            if (isClockwise)
            {
                pieces *= -1;
            }
            newRotation *= Quaternion.Euler(0f, 0f, 360f * pieces / rouletteNum);
            playerLookat = (playerLookat + pieces + rouletteNum) % rouletteNum;
            enemyLookat = (enemyLookat + pieces + rouletteNum) % rouletteNum;
            this.transform.DORotateQuaternion(newRotation, spinDelay).OnComplete(() => spinFlag = false);
        }

    }

    public void ActivateRoulette()
    {
        TurnManager.BeforeRouletteActivate?.Invoke();
        roulettePieces[playerLookat].Activate(false);
        roulettePieces[enemyLookat].Activate(true);
        TurnManager.OnRouletteActivate?.Invoke();
    }

    public void ActivateRoulettePiece(int index, bool isEnemy)
    {
        roulettePieces[index].Activate(isEnemy);
    }

    public void TriggerRoulette()
    {
        roulettePieces[triggerPos].Setup(triggerPiece);
        roulettePieces[triggerPos].Trigger(true);
        TurnManager.OnRouletteTrigger?.Invoke();
    }

    public void EnemyTriggerRoulette()
    {
        roulettePieces[triggerPos].Setup(enemyTriggerPiece);
        roulettePieces[triggerPos].Trigger(true);
        TurnManager.OnRouletteTrigger?.Invoke();
    }

    public bool EnchantRoulette(bool isEnemy, ERouletteType rType, int rValue)
    {
        if (isEnemy)
        {
            if (enemyLookat == triggerPos) return false;
            EnchantRoulettePiece(enemyLookat, rType, rValue);
        }
        else
        {
            if (playerLookat == triggerPos) return false;
            EnchantRoulettePiece(playerLookat, rType, rValue);
        }
        return true;
    }

    public void EnchantRoulettePiece(int index, ERouletteType rType, int rValue)
    {
        if (index == triggerPos)
        {
            return;
        }
        RouletteItem rItem = new RouletteItem();
        rItem.type = rType;
        rItem.value = rValue;
        roulettePieces[index].Setup(rItem);
        TurnManager.OnRouletteEnchant?.Invoke();
    }

    public int CountRouletteType(ERouletteType rType)
    {
        int counter = 0;
        for (int i = 0; i < rouletteNum; i++)
        {
            if (i == triggerPos)
            {
                continue;
            }
            if (roulettePieces[i].roulette.type == rType)
            {
                counter++;
            }
        }
        return counter;
    }

    public void InitRoulette()
    {
        playerLookat = (rouletteNum - 1) / 2;
        enemyLookat = rouletteNum - 1;
        triggerPos = rouletteNum - 2;
        spinCount = 0;
        spinCount_Turn = 0;
        spinDistance = 0;
        spinDistance_Turn = 0;
        isTriggerActivated = false;

        for (int i = 0; i < rouletteNum; i++)
        {
            var roulettePiece = Instantiate(roulettePiecePrefab, this.transform.position, Utils.QI);
            roulettePiece.transform.rotation *= Quaternion.Euler(0f, 0f, -180f / rouletteNum - 360f * i / rouletteNum);
            roulettePiece.transform.SetParent(this.transform, true);
            roulettePieces[i] = roulettePiece.GetComponent<RoulettePiece>();
            roulettePieces[i].Setup(EnemyManager.Inst.enemy.roulettePattern[i]);
        }
        RouletteItem tempRoulettePiece = new RouletteItem();
        tempRoulettePiece.type = ERouletteType.None;
        tempRoulettePiece.value = triggerPiece.value;
        triggerPiece_None = tempRoulettePiece;
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

    void ShowBuffedPieces()
    {
        for (int i = 0; i < rouletteNum; i++)
        {
            roulettePieces[i].ShowTotalValue();
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
        ShowBuffedPieces();
    }
}
