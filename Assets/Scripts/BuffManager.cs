using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class Buff
{
    public Buff target;
    public int add;
    public int mul;
    public int lastingTime;

    public void InitBuff()
    {
        add = 0;
        mul = 1;
    }

    public void SetBuff(int a, int m, int time)
    {
        target = this;
        add = a;
        mul = m;
        lastingTime = time;
    }

    public static bool IsEqual(Buff a, Buff b)
    {
        if (a.target == b.target && a.add == b.add && a.mul == b.mul && a.lastingTime == b.lastingTime)
        {
            return true;
        }
        return false;
    }
}
public class BuffManager : MonoBehaviour
{
    public static BuffManager Inst { get; private set; }
    private void Awake() => Inst = this;
    [SerializeField] GameObject buffUIPrefab;

    public List<Buff> rouletteBuffs;
    public Buff totalRouletteBuff_Attack;
    public Buff totalRouletteBuff_Heal;
    public Buff totalRouletteBuff_Shield;
    public Buff totalRouletteBuff_Charge;
    public Buff totalRouletteBuff_Lifesteal_Dmg;
    public Buff totalRouletteBuff_Lifesteal_Heal;
    public Buff totalRouletteBuff_Drain_Dmg;
    public Buff totalRouletteBuff_Drain_Heal;
    public Buff singleRouletteBuff_Trigger;
    public Dictionary<RoulettePiece, Buff> roulettePieceBuff = new Dictionary<RoulettePiece, Buff>();

    public List<Buff> playerBuffs;
    public Buff damageBuff;
    public Buff healBuff;
    public Buff shieldBuff;
    public Buff costBuff;
    public List<Buff> enemyBuffs;
    public Buff enemyAttackBuff;
    public Buff enemyHealBuff;
    public Buff enemyShieldBuff;
    public Buff enemyDrainBuff;

    public List<Buff> cardBuffs;
    public Buff allCardValueBuff;
    public Buff allCardCostBuff;
    public Dictionary<Item, Buff> singleCardCostBuff = new Dictionary<Item, Buff>();

    public List<BuffUI> BuffListToBuffUIList(List<Buff> BuffList, GameObject parent)
    {
        List<BuffUI> returnList = new List<BuffUI>();
        foreach(var b in BuffList)
        {
            var bUIObj = Instantiate(buffUIPrefab, parent.transform.position, Utils.QI);
            bUIObj.transform.SetParent(parent.transform);
            BuffUI bUI = bUIObj.GetComponent<BuffUI>();
            bUI.Setup(b);
            returnList.Add(bUI);
        }
        return returnList;
    }

    public void InitRouletteBuff()
    {
        totalRouletteBuff_Attack.InitBuff();
        totalRouletteBuff_Heal.InitBuff();
        totalRouletteBuff_Shield.InitBuff();
        totalRouletteBuff_Charge.InitBuff();
        totalRouletteBuff_Lifesteal_Dmg.InitBuff();
        totalRouletteBuff_Lifesteal_Heal.InitBuff();
        totalRouletteBuff_Drain_Dmg.InitBuff();
        totalRouletteBuff_Drain_Heal.InitBuff();
        singleRouletteBuff_Trigger.InitBuff();

        roulettePieceBuff = new Dictionary<RoulettePiece, Buff>();
        for (int i = 0; i < RouletteManager.rouletteNum; i++)
        {
            Buff rBuff = new Buff();
            rBuff.InitBuff();
            roulettePieceBuff.Add(RouletteManager.Inst.roulettePieces[i], rBuff);
        }
    }

    public void InitPlayerBuff()
    {
        damageBuff.InitBuff();
        healBuff.InitBuff();
        shieldBuff.InitBuff();
        costBuff.InitBuff();
    }

    public void InitEnemyBuff()
    {
        enemyAttackBuff.InitBuff();
        enemyHealBuff.InitBuff();
        enemyShieldBuff.InitBuff();
        enemyDrainBuff.InitBuff();
    }

    public void InitCardBuff()
    {
        allCardValueBuff.InitBuff();
        allCardCostBuff.InitBuff();

        var keys = singleCardCostBuff.Keys.ToList();
        foreach (var key in keys)
        {
            singleCardCostBuff[key].InitBuff();
        }
    }

    public void AddRouletteBuff(Buff target, int add, int mul, int turns)
    {
        Buff rb = new Buff();
        rb.target = target;
        rb.add = add;
        rb.mul = mul;
        rb.lastingTime = turns;
        rouletteBuffs.Add(rb);
        GameManager.Inst.SetRouletteBuffUI();
    }

    public void AddPlayerBuff(Buff target, int add, int mul, int turns)
    {
        Buff rb = new Buff();
        rb.target = target;
        rb.add = add;
        rb.mul = mul;
        rb.lastingTime = turns;
        playerBuffs.Add(rb);
        GameManager.Inst.SetPlayerBuffUI();
    }

    public void AddEnemyBuff(Buff target, int add, int mul, int turns)
    {
        Buff rb = new Buff();
        rb.target = target;
        rb.add = add;
        rb.mul = mul;
        rb.lastingTime = turns;
        enemyBuffs.Add(rb);
        GameManager.Inst.SetEnemyBuffUI();
    }

    public void AddCardBuff(Buff target, int add, int mul, int turns)
    {
        Buff rb = new Buff();
        rb.target = target;
        rb.add = add;
        rb.mul = mul;
        rb.lastingTime = turns;
        cardBuffs.Add(rb);
        GameManager.Inst.SetPlayerBuffUI();
    }

    public void AddSingleCardCostBuff(Item card, int add, int mul, int turns)
    {
        Buff rb = new Buff();
        if (singleCardCostBuff.ContainsKey(card) == false)
        {
            Buff temp = new Buff();
            temp.InitBuff();
            singleCardCostBuff[card] = temp;
        }
        rb.target = singleCardCostBuff[card];
        rb.add = add;
        rb.mul = mul;
        rb.lastingTime = turns;
        cardBuffs.Add(rb);
    }

    public void CalcTotalRouletteBuff()
    {
        InitRouletteBuff();
        foreach (Buff rb in rouletteBuffs)
        {
            if (rb.lastingTime != 0)
            {
                rb.target.add += rb.add;
                rb.target.mul *= rb.mul;
            }
        }

        for (int i = 0; i < RouletteManager.rouletteNum; i++)
        {
            RoulettePiece roulettePiece = RouletteManager.Inst.roulettePieces[i];
            RouletteItem rouletteItem = RouletteManager.Inst.roulettePieces[i].roulette;
            switch (rouletteItem.type)
            {
                case ERouletteType.Attack:
                    roulettePieceBuff[roulettePiece].add += totalRouletteBuff_Attack.add;
                    roulettePieceBuff[roulettePiece].mul *= totalRouletteBuff_Attack.mul;
                    break;
                case ERouletteType.Heal:
                    roulettePieceBuff[roulettePiece].add += totalRouletteBuff_Heal.add;
                    roulettePieceBuff[roulettePiece].mul *= totalRouletteBuff_Heal.mul;
                    break;
                case ERouletteType.Shield:
                    roulettePieceBuff[roulettePiece].add += totalRouletteBuff_Shield.add;
                    roulettePieceBuff[roulettePiece].mul *= totalRouletteBuff_Shield.mul;
                    break;
                case ERouletteType.Charge:
                    roulettePieceBuff[roulettePiece].add += totalRouletteBuff_Charge.add;
                    roulettePieceBuff[roulettePiece].mul *= totalRouletteBuff_Charge.mul;
                    break;
                case ERouletteType.Lifesteal:
                    roulettePieceBuff[roulettePiece].add += totalRouletteBuff_Lifesteal_Dmg.add;
                    roulettePieceBuff[roulettePiece].mul *= totalRouletteBuff_Lifesteal_Dmg.mul;
                    break;
                case ERouletteType.Drain:
                    roulettePieceBuff[roulettePiece].add += totalRouletteBuff_Drain_Dmg.add;
                    roulettePieceBuff[roulettePiece].mul *= totalRouletteBuff_Drain_Dmg.mul;
                    break;
            }
            if (i == RouletteManager.Inst.triggerPos)
            {
                roulettePieceBuff[roulettePiece].add += singleRouletteBuff_Trigger.add;
                roulettePieceBuff[roulettePiece].mul *= singleRouletteBuff_Trigger.mul;
            }
        }
    }

    public void CalcTotalPlayerBuff()
    {
        InitPlayerBuff();
        foreach (Buff pb in playerBuffs)
        {
            if (pb.lastingTime != 0)
            {
                pb.target.add += pb.add;
                pb.target.mul *= pb.mul;
            }
        }
    }

    public void CalcTotalEnemyBuff()
    {
        InitEnemyBuff();
        foreach (Buff eb in enemyBuffs)
        {
            if (eb.lastingTime != 0)
            {
                eb.target.add += eb.add;
                eb.target.mul *= eb.mul;
            }
        }
    }

    public void CalcTotalCardBuff()
    {
        InitCardBuff();
        foreach (Buff cb in cardBuffs)
        {
            if (cb.lastingTime != 0)
            {
                cb.target.add += cb.add;
                cb.target.mul *= cb.mul;
            }
        }
    }

    public void CalcSingleCardCostBuff(Item card)
    {
        CalcTotalCardBuff();
        if (singleCardCostBuff.ContainsKey(card) == false)
        {
            Buff temp = new Buff();
            temp.InitBuff();
            singleCardCostBuff[card] = temp;
        }
        singleCardCostBuff[card].add += allCardCostBuff.add;
        singleCardCostBuff[card].mul *= allCardCostBuff.mul;
    }

    public int GetBuffedRouletteValue(RoulettePiece targetPiece)
    {
        CalcTotalRouletteBuff();
        return (targetPiece.roulette.value + roulettePieceBuff[targetPiece].add) * roulettePieceBuff[targetPiece].mul;
    }

    public int GetTotalRouletteBuffValue(Buff rBuff, int value)
    {
        CalcTotalRouletteBuff();
        return (value + rBuff.add) * rBuff.mul;
    }

    public int GetPlayerBuffValue(Buff pBuff, int value)
    {
        CalcTotalPlayerBuff();
        return (value + pBuff.add) * pBuff.mul;
    }

    public int GetEnemyBuffValue(Buff eBuff, int value)
    {
        CalcTotalEnemyBuff();
        return (value + eBuff.add) * eBuff.mul;
    }

    public int GetBuffedCardCost(Item targetCard)
    {
        CalcSingleCardCostBuff(targetCard);
        return (targetCard.cost + singleCardCostBuff[targetCard].add) * singleCardCostBuff[targetCard].mul;
    }

    public int GetCardBuffValue(Buff cBuff, int value)
    {
        CalcTotalCardBuff();
        return (value + cBuff.add) * cBuff.mul;
    }

    public void ReduceRouletteBuffCounter()
    {
        for (int i = rouletteBuffs.Count - 1; i >= 0; i--)
        {
            rouletteBuffs[i].lastingTime--;
            if (rouletteBuffs[i].lastingTime == 0)
            {
                rouletteBuffs.RemoveAt(i);
            }
        }
        GameManager.Inst.SetRouletteBuffUI();
    }

    public void ReducePlayerBuffCounter()
    {
        for (int i = playerBuffs.Count - 1; i >= 0; i--)
        {
            playerBuffs[i].lastingTime--;
            if (playerBuffs[i].lastingTime == 0)
            {
                playerBuffs.RemoveAt(i);
            }
        }
        GameManager.Inst.SetPlayerBuffUI();
    }

    public void ReduceEnemyBuffCounter()
    {
        for (int i = enemyBuffs.Count - 1; i >= 0; i--)
        {
            enemyBuffs[i].lastingTime--;
            if (enemyBuffs[i].lastingTime == 0)
            {
                enemyBuffs.RemoveAt(i);
            }
        }
        GameManager.Inst.SetEnemyBuffUI();
    }

    public void ReduceCardBuffCounter()
    {
        for (int i = cardBuffs.Count - 1; i >= 0; i--)
        {
            cardBuffs[i].lastingTime--;
            if (cardBuffs[i].lastingTime == 0)
            {
                cardBuffs.RemoveAt(i);
            }
        }
        GameManager.Inst.SetPlayerBuffUI();
    }

    public void ReduceBuffCounter()
    {
        ReducePlayerBuffCounter();
        ReduceEnemyBuffCounter();
        ReduceRouletteBuffCounter();
        ReduceCardBuffCounter();
    }

    private void Start()
    {
        TurnManager.OnPlayerTurnStart = ReduceBuffCounter + TurnManager.OnPlayerTurnStart;
    }

    private void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart = null;
    }
}
