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
    public GameObject rouletteInnerFrame;
    public Animator triggerEffect;
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
    public int spinOffset;
    public Vector3 curRotation;
    Tween rotateTween;

    public float curZ;
    public float targetZ;
    public int pendingPieces;

    public void Spin(bool isClockwise, int pieces)
    {
        spinCount++;
        spinCount_Turn++;
        spinDistance += pieces;
        spinDistance_Turn += pieces;
        spinDirection = isClockwise ? 1 : 0;
        Utils.AllignActions<bool, int>(ref TurnManager.OnRouletteSpin, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnRouletteSpin?.Invoke(isClockwise, pieces);
        if (isClockwise)
        {
            pieces *= -1;
        }
        playerLookat = (playerLookat + pieces + rouletteNum) % rouletteNum;
        enemyLookat = (enemyLookat + pieces + rouletteNum) % rouletteNum;
        spinOffset = (spinOffset + pieces + rouletteNum) % rouletteNum;

        targetZ += 360f * pieces / rouletteNum;
        pendingPieces += pieces;

        if(rotateTween != null && rotateTween.IsActive() && rotateTween.IsPlaying()) return;
        spinFlag = true;
        rotateTween = DOTween.To(
            () => curZ,
            z =>
            {
                curZ = z;
                var e = rouletteArea.transform.parent.eulerAngles;
                e.z = curZ;
                rouletteArea.transform.parent.eulerAngles = e;
            },
            targetZ,
            spinDelay
        )
        .OnComplete(() =>
        {
            rotateTween = null;
            int donePieces = pendingPieces;
            pendingPieces = 0;

            Utils.AllignActions(ref TurnManager.AfterRouletteSpin, typeof(ShowBuff), typeof(RelicManager));
            TurnManager.AfterRouletteSpin?.Invoke(donePieces);

            if(targetZ != curZ) Spin(isClockwise, 0);
            else spinFlag = false;
        });

        GetComponent<AudioSource>().PlayOneShot(SoundManager.Inst.rouletteRotateSFX);
    }

    public int EnemyIdxSpinOffset(int enemyIdx)
    {
        if(enemyIdx == 0) return enemyLookat;
        return (EnemyManager.Inst.subEnemies[enemyIdx - 1].roulettePos + spinOffset + rouletteNum) % rouletteNum;
    }

    private int ActivationWeight(int index)
    {
        if (roulettePieces[index].roulette.rtype.type == ERouletteType.Shield) return 0;
        if (roulettePieces[index].roulette.rtype.type == ERouletteType.Heal) return 1;
        if (roulettePieces[index].roulette.rtype.type == ERouletteType.Attack) return 3;
        if (roulettePieces[index].roulette.rtype.type == ERouletteType.None) return 4;
        return 2;
    }

    private int ActivationWeight(RoulettePiece roulettePiece)
    {
        if (roulettePiece.roulette.rtype.type == ERouletteType.Shield) return 0;
        if (roulettePiece.roulette.rtype.type == ERouletteType.Heal) return 1;
        if (roulettePiece.roulette.rtype.type == ERouletteType.Attack) return 3;
        if (roulettePiece.roulette.rtype.type == ERouletteType.None) return 4;
        return 2;
    }

    public void ActivateRoulette()
    {
        if (isTriggerActivated)
        {
            int totalVal = BuffManager.Inst.GetBuffedRouletteValue(triggerPiece);
            Debug.Log("trigger value: " + totalVal.ToString());
            TriggerActivation?.Invoke(isEnemyTrigger(), totalVal);
            Utils.AllignActions(ref TurnManager.OnRouletteActivate, typeof(ShowBuff), typeof(RelicManager));
            TurnManager.OnRouletteActivate?.Invoke();
            DeTriggerRoulette();
        }
        else
        {
            List<(RoulettePiece rp, int owner)> activateRoulettePieces = new List<(RoulettePiece, int)>();
            activateRoulettePieces.Add((roulettePieces[playerLookat], -1));
            activateRoulettePieces.Add((roulettePieces[enemyLookat], 0));
            for(int i = 0; i < Enemy.maxSubEnemyNum; i++)
            {
                if(EnemyManager.Inst.subEnemies[i] == null || EnemyManager.Inst.subEnemies[i].name == null) continue;
                activateRoulettePieces.Add((roulettePieces[EnemyIdxSpinOffset(i + 1)], i + 1));
            }
            activateRoulettePieces = activateRoulettePieces.OrderBy(x => ActivationWeight(x.rp)).ThenBy(x => (Array.FindIndex(roulettePieces, y => y == x.rp) + spinOffset + rouletteNum) % rouletteNum).ToList();
            foreach(var rp_owner in activateRoulettePieces)
            {
                if(rp_owner.owner == -1) roulettePieces[playerLookat].Activate(false);
                else if(rp_owner.owner == 0) roulettePieces[enemyLookat].Activate(true);
                else rp_owner.rp.Activate(true, rp_owner.owner);
            }
            Utils.AllignActions(ref TurnManager.OnRouletteActivate, typeof(ShowBuff), typeof(RelicManager));
            TurnManager.OnRouletteActivate?.Invoke();
        }
    }

    public void ActivateRoulettePiece(int index, bool isEnemy, int enemyIdx = 0)
    {
        roulettePieces[index].Activate(isEnemy, enemyIdx);
    }

    public bool isPlayerTrigger()
    {
        return triggerPiece == playerTriggerPiece;
    }

    public bool isEnemyTrigger()
    {
        return triggerPiece == enemyTriggerPiece;
    }

    public IEnumerator TriggerRoulette()
    {
        triggerPiece = playerTriggerPiece;
        BuffManager.Inst.rouletteBuff_Trigger.Clear();
        triggerEffect.gameObject.SetActive(true);
        TurnManager.Inst.isLoading = true;
        yield return new WaitForSeconds(2f);
        isTriggerActivated = true;
        triggerSprite.sprite = playerTriggerSprite;
        triggerSprite.gameObject.SetActive(true);
        // rouletteInnerFrame.SetActive(false);
        for(int i = 0; i < rouletteNum; i++)
        {
            roulettePieces[i].Trigger(true);
        }
        Utils.AllignActions(ref TurnManager.OnPlayerTrigger, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnPlayerTrigger?.Invoke();
        TriggerActivation = PlayerTriggerActivation;
        Utils.AllignActions(ref TurnManager.OnRouletteTrigger, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnRouletteTrigger?.Invoke();
        yield return new WaitForSeconds(2.2f);
        TurnManager.Inst.isLoading = false;
        triggerEffect.gameObject.SetActive(false);
    }

    public void DeTriggerRoulette()
    {
        isTriggerActivated = false;
        for (int i = 0; i < rouletteNum; i++)
        {
            roulettePieces[i].Trigger(false);
        }
        triggerSprite.gameObject.SetActive(false);
        // rouletteInnerFrame.SetActive(true);
        BuffManager.Inst.rouletteBuff_Trigger.Clear();
    }

    public bool EnchantRoulette(bool isEnemy, RouletteType rType, int rValue, int enemyIdx = 0)
    {
        bool ret = false;
        if (isEnemy)
        {
            ret = EnchantRoulettePiece(EnemyIdxSpinOffset(enemyIdx), rType, rValue);
        }
        else
        {
            ret = EnchantRoulettePiece(playerLookat, rType, rValue);
        }
        return ret;
    }

    public bool IsRouletteEnchantable(int index, RouletteType rType)
    {
        if(isTriggerActivated) return false;
        bool ret = true;
        if(TurnManager.CheckRouletteEnchantable != null)
        {
            foreach (Func<int, RouletteType, bool> func in TurnManager.CheckRouletteEnchantable.GetInvocationList())
            {
                ret = ret && func.Invoke(index, rType);
            }
        }
        return ret;
    }

    public bool IsRouletteEnchantable(bool isEnemy, RouletteType rType, int enemyIdx = 0)
    {
        if(isTriggerActivated) return false;
        bool ret = true;
        if(TurnManager.CheckRouletteEnchantable != null)
        {
            foreach (Func<int, RouletteType, bool> func in TurnManager.CheckRouletteEnchantable.GetInvocationList())
            {
                if(isEnemy)
                {
                    ret = ret && func.Invoke(EnemyIdxSpinOffset(enemyIdx), rType);
                }
                else
                {
                    ret = ret && func.Invoke(playerLookat, rType);
                }
            }
        }
        return ret;
    }

    public bool EnchantRoulettePiece(int index, RouletteType rType, int rValue)
    {
        if(isTriggerActivated) return false;
        bool ret = true;
        if(TurnManager.CheckRouletteEnchantable != null)
        {
            foreach (Func<int, RouletteType, bool> func in TurnManager.CheckRouletteEnchantable.GetInvocationList())
            {
                ret = ret && func.Invoke(index, rType);
            }
            if (ret == false) return false;
        }
        if(roulettePieces[index].roulette.rtype.type != ERouletteType.None)
        {
            Utils.AllignActions(ref TurnManager.OnRouletteErase, typeof(ShowBuff), typeof(RelicManager));
            TurnManager.OnRouletteErase?.Invoke(index);
        }
        RouletteItem rItem = new RouletteItem();
        rItem.rtype = rType;
        rItem.value = rValue;
        roulettePieces[index].Setup(rItem);
        Utils.AllignActions(ref TurnManager.OnRouletteEnchant, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnRouletteEnchant?.Invoke(index);

        roulettePieces[index].GetComponent<AudioSource>().PlayOneShot(SoundManager.Inst.rouletteEnchantSFX);
        return true;
    }

    public bool EnchantRoulettePiece(RoulettePiece piece, RouletteType rType, int rValue)
    {
        int index = Array.IndexOf(roulettePieces, piece);
        return EnchantRoulettePiece(index, rType, rValue);
    }

    public bool EnhanceRoulette(bool isEnemy, int enemyIdx = 0)
    {
        int index;
        if (isEnemy)
        {
            index = EnemyIdxSpinOffset(enemyIdx);
        }
        else
        {
            index = playerLookat;
        }
        if((roulettePieces[index].roulette.rtype.type != ERouletteType.Player_Special && roulettePieces[index].roulette.rtype.type != ERouletteType.Enemy_Special) || roulettePieces[index].isEnhanced)
        {
            return false;
        }
        roulettePieces[index].Enhance();
        Utils.AllignActions(ref TurnManager.OnRouletteEnhance, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnRouletteEnhance?.Invoke(index);
        return true;
    }

    public bool EnhanceRoulettePiece(int index)
    {
        if(roulettePieces[index].roulette.rtype.type != ERouletteType.Player_Special || roulettePieces[index].isEnhanced)
        {
            return false;
        }
        roulettePieces[index].Enhance();
        Utils.AllignActions(ref TurnManager.OnRouletteEnhance, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnRouletteEnhance?.Invoke(index);
        return true;
    }

    public bool EnhanceRoulettePiece(RoulettePiece piece)
    {
        int index = Array.IndexOf(roulettePieces, piece);
        if(roulettePieces[index].roulette.rtype.type != ERouletteType.Player_Special || roulettePieces[index].isEnhanced)
        {
            return false;
        }
        roulettePieces[index].Enhance();
        Utils.AllignActions(ref TurnManager.OnRouletteEnhance, typeof(ShowBuff), typeof(RelicManager));
        TurnManager.OnRouletteEnhance?.Invoke(index);
        return true;
    }

    public int CountRouletteType(RouletteType rType)
    {
        int counter = 0;
        for (int i = 0; i < rouletteNum; i++)
        {
            if (roulettePieces[i].roulette.rtype == rType)
            {
                counter++;
            }
        }
        return counter;
    }

    public int CountRouletteType(ERouletteType type)
    {
        int counter = 0;
        for (int i = 0; i < rouletteNum; i++)
        {
            if (roulettePieces[i].roulette.rtype.type == type)
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
        spinOffset = 0;
        isTriggerActivated = false;

        for (int i = 0; i < rouletteNum; i++)
        {
            var roulettePiece = Instantiate(roulettePiecePrefab, new Vector3(1.89f * Mathf.Sin((30 + 360f * i / rouletteNum) * Mathf.Deg2Rad), 1.89f * Mathf.Cos((30 + 360f * i / rouletteNum) * Mathf.Deg2Rad), 0f), Utils.QI);
            roulettePiece.transform.rotation *= Quaternion.Euler(0f, 0f, -30 - 360f * i / rouletteNum);
            roulettePiece.transform.SetParent(rouletteArea.transform.parent, false);
            roulettePieces[i] = roulettePiece.GetComponent<RoulettePiece>();
            roulettePieces[i].Setup(EnemyManager.Inst.enemy.roulettePattern[i]);
        }
        RouletteItem tempRoulettePiece = new RouletteItem();
        tempRoulettePiece.rtype = new RouletteType(ERouletteType.None);
        tempRoulettePiece.value = 0;
        triggerPiece_None = tempRoulettePiece;
        // roulettePieces[triggerPos].Setup(tempRoulettePiece);
    }

    // public void RouletteMouseDown()
    // {
    //     if(!spinFlag && !TurnManager.Inst.isLoading)
    //     {
    //         spinFlag = true;
    //         TurnManager.Inst.isLoading = true;
    //         isRouletteDrag = true;
    //         lastRotation = rouletteArea.transform.parent.rotation;
    //     }
    // }

    // public void RouletteMouseUp()
    // {
    //     if(isRouletteDrag)
    //     {
    //         isRouletteDrag = false;
    //         rouletteArea.transform.parent.DORotateQuaternion(lastRotation, 0.7f).OnComplete(() =>
    //         {
    //             spinFlag = false;
    //             TurnManager.Inst.isLoading = false;
    //         });
    //     }
    // }

    // void RouletteDrag()
    // {
    //     Vector3 mouseOrtho = new Vector3(Utils.MousePos.x, Utils.MousePos.y, rouletteArea.transform.parent.position.z);
    //     Quaternion mouseRotation = Quaternion.FromToRotation(rouletteArea.transform.parent.position, mouseOrtho);
    //     rouletteArea.transform.parent.rotation = lastRotation * mouseRotation;
    // }

    // void DetectRouletteArea()
    // {
    //     RaycastHit2D[] hits = Physics2D.RaycastAll(Utils.MousePos, Vector3.forward);
    //     int layer = LayerMask.NameToLayer("RouletteArea");
    //     onRouletteArea = Array.Exists(hits, x => x.collider.gameObject.layer == layer);
    //     if(onRouletteArea == false)
    //     {
    //         RouletteMouseUp();
    //     }
    // }

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
        // if (isRouletteDrag)
        // {
        //     RouletteDrag();
        // }

        // DetectRouletteArea();
        ShowBuffedPieces();
    }
}
