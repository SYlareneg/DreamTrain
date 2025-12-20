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
    [SerializeField] GameObject rouletteArea;
    public SpriteRenderer triggerSprite;
    public Sprite playerTriggerSprite;
    public Sprite enemyTriggerSprite;

    public static int rouletteNum = 12;

    public RoulettePiece[] roulettePieces = new RoulettePiece[rouletteNum];
    public int playerLookat;
    public int enemyLookat;
    public int triggerPos;
    public RouletteItem triggerPiece;
    public RouletteItem playerTriggerPiece;
    public RouletteItem enemyTriggerPiece;
    public RouletteItem triggerPiece_None;
    public bool isTriggerActivated;
    public static Action<bool, int> PlayerTriggerActivation;
    public static Action<bool, int> EnemyTriggerActivation;
    public static Action<bool, int> TriggerActivation;

    public bool spinFlag = false;
    public bool isRouletteDrag;
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
            Utils.AllignActions<bool, int>(ref TurnManager.OnRouletteSpin, typeof(ShowBuff), typeof(RelicManager));
            TurnManager.OnRouletteSpin?.Invoke(isClockwise, pieces);
            if (isClockwise)
            {
                pieces *= -1;
            }
            Vector3 newRotation = new Vector3(0f, 0f, rouletteArea.transform.eulerAngles.z + 360f * pieces / rouletteNum);
            playerLookat = (playerLookat + pieces + rouletteNum) % rouletteNum;
            enemyLookat = (enemyLookat + pieces + rouletteNum) % rouletteNum;
            rouletteArea.transform.parent.DORotate(newRotation, spinDelay, RotateMode.FastBeyond360).OnComplete(() => {
                spinFlag = false;
                Utils.AllignActions(ref TurnManager.AfterRouletteSpin, typeof(ShowBuff), typeof(RelicManager));
                TurnManager.AfterRouletteSpin?.Invoke(pieces);
            });
        }

    }

    private int ActivationWeight(int index)
    {
        if (roulettePieces[index].roulette.type == ERouletteType.Shield) return 0;
        if (roulettePieces[index].roulette.type == ERouletteType.Heal) return 1;
        if (roulettePieces[index].roulette.type == ERouletteType.Attack) return 3;
        if (roulettePieces[index].roulette.type == ERouletteType.None) return 4;
        return 2;
    }

    public void ActivateRoulette()
    {
        if (isTriggerActivated)
        {
            int totalVal = BuffManager.Inst.GetBuffedRouletteValue(triggerPiece);
            Debug.Log("trigger value: " + totalVal.ToString());
            DeTriggerRoulette();
            TriggerActivation?.Invoke(isEnemyTrigger(), totalVal);
        }
        else
        {
            int playerWeight = ActivationWeight(playerLookat);
            int enemyWeight = ActivationWeight(enemyLookat);
            if (playerWeight < enemyWeight)
            {
                roulettePieces[playerLookat].Activate(false);
                roulettePieces[enemyLookat].Activate(true);
            }
            else
            {
                roulettePieces[enemyLookat].Activate(true);
                roulettePieces[playerLookat].Activate(false);
            }
        }

        Utils.AllignActions(ref TurnManager.OnRouletteActivate, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnRouletteActivate?.Invoke();
    }

    public void ActivateRoulettePiece(int index, bool isEnemy)
    {
        roulettePieces[index].Activate(isEnemy);
    }

    public bool isPlayerTrigger()
    {
        return triggerPiece == playerTriggerPiece;
    }

    public bool isEnemyTrigger()
    {
        return triggerPiece == enemyTriggerPiece;
    }

    public void TriggerRoulette()
    {
        triggerPiece = playerTriggerPiece;
        BuffManager.Inst.rouletteBuff_Trigger.Clear();
        isTriggerActivated = true;
        triggerSprite.sprite = playerTriggerSprite;
        triggerSprite.gameObject.SetActive(true);
        for(int i = 0; i < rouletteNum; i++)
        {
            roulettePieces[i].Trigger(true);
        }
        Utils.AllignActions(ref TurnManager.OnPlayerTrigger, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnPlayerTrigger?.Invoke();
        TriggerActivation = PlayerTriggerActivation;
        Utils.AllignActions(ref TurnManager.OnRouletteTrigger, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnRouletteTrigger?.Invoke();
    }

    public void DeTriggerRoulette()
    {
        isTriggerActivated = false;
        for (int i = 0; i < rouletteNum; i++)
        {
            roulettePieces[i].Trigger(false);
        }
        triggerSprite.gameObject.SetActive(false);
        BuffManager.Inst.rouletteBuff_Trigger.Clear();
    }

    public void EnemyTriggerRoulette()
    {
        triggerPiece = enemyTriggerPiece;
        roulettePieces[triggerPos].Setup(enemyTriggerPiece);
        BuffManager.Inst.rouletteBuff_Trigger.Clear();
        roulettePieces[triggerPos].Trigger(true);
        isTriggerActivated = true;
        Utils.AllignActions(ref TurnManager.OnEnemyTrigger, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnEnemyTrigger?.Invoke();
        TriggerActivation = EnemyTriggerActivation;
        roulettePieces[triggerPos].GetComponent<Tooltip>().tooltipTitle = EnemyManager.Inst.enemy.passive.name;
        roulettePieces[triggerPos].GetComponent<Tooltip>().tooltipTxt = EnemyManager.Inst.enemy.passive.text;
        Utils.AllignActions(ref TurnManager.OnRouletteTrigger, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnRouletteTrigger?.Invoke();
    }

    public bool EnchantRoulette(bool isEnemy, ERouletteType rType, int rValue)
    {
        bool ret = false;
        if (isEnemy)
        {
            ret = EnchantRoulettePiece(enemyLookat, rType, rValue);
        }
        else
        {
            ret = EnchantRoulettePiece(playerLookat, rType, rValue);
        }
        return ret;
    }

    public bool EnchantRoulettePiece(int index, ERouletteType rType, int rValue)
    {
        bool ret = true;
        if(TurnManager.CheckRouletteEnchantable != null)
        {
            foreach (Func<int, ERouletteType, bool> func in TurnManager.CheckRouletteEnchantable.GetInvocationList())
            {
                ret = ret && func.Invoke(index, rType);
            }
            if (ret == false) return false;
        }
        RouletteItem rItem = new RouletteItem();
        rItem.type = rType;
        rItem.value = rValue;
        roulettePieces[index].Setup(rItem);
        Utils.AllignActions(ref TurnManager.OnRouletteEnchant, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnRouletteEnchant?.Invoke(index);
        return true;
    }

    public bool EnchantRoulettePiece(RoulettePiece piece, ERouletteType rType, int rValue)
    {
        int index = Array.IndexOf(roulettePieces, piece);
        return EnchantRoulettePiece(index, rType, rValue);
    }

    public int CountRouletteType(ERouletteType rType)
    {
        int counter = 0;
        for (int i = 0; i < rouletteNum; i++)
        {
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
        spinCount = 0;
        spinCount_Turn = 0;
        spinDistance = 0;
        spinDistance_Turn = 0;
        isTriggerActivated = false;

        for (int i = 0; i < rouletteNum; i++)
        {
            var roulettePiece = Instantiate(roulettePiecePrefab, Vector3.zero, Utils.QI);
            roulettePiece.transform.rotation *= Quaternion.Euler(0f, 0f, -180f / rouletteNum - 360f * i / rouletteNum);
            roulettePiece.transform.SetParent(rouletteArea.transform.parent, false);
            roulettePieces[i] = roulettePiece.GetComponent<RoulettePiece>();
            roulettePieces[i].Setup(EnemyManager.Inst.enemy.roulettePattern[i]);
        }
        RouletteItem tempRoulettePiece = new RouletteItem();
        tempRoulettePiece.type = ERouletteType.None;
        tempRoulettePiece.value = 0;
        triggerPiece_None = tempRoulettePiece;
        // roulettePieces[triggerPos].Setup(tempRoulettePiece);
    }

    public void RouletteMouseDown()
    {
        if(!spinFlag && !TurnManager.Inst.isLoading)
        {
            spinFlag = true;
            TurnManager.Inst.isLoading = true;
            isRouletteDrag = true;
            lastRotation = rouletteArea.transform.parent.rotation;
        }
    }

    public void RouletteMouseUp()
    {
        if(isRouletteDrag)
        {
            isRouletteDrag = false;
            rouletteArea.transform.parent.DORotateQuaternion(lastRotation, 0.7f).OnComplete(() =>
            {
                spinFlag = false;
                TurnManager.Inst.isLoading = false;
            });
        }
    }

    void RouletteDrag()
    {
        Vector3 mouseOrtho = new Vector3(Utils.MousePos.x, Utils.MousePos.y, rouletteArea.transform.parent.position.z);
        Quaternion mouseRotation = Quaternion.FromToRotation(rouletteArea.transform.parent.position, mouseOrtho);
        rouletteArea.transform.parent.rotation = lastRotation * mouseRotation;
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
        PlayerTriggerActivation = null;
        EnemyTriggerActivation = null;
        TriggerActivation = null;
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
