using UnityEngine;
using System;
using System.Collections.Generic;

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
}
public class BuffManager : MonoBehaviour
{
    public static BuffManager Inst { get; private set; }
    private void Awake() => Inst = this;

    public List<Buff> rouletteBuffs;
    public Buff totalRouletteBuff_Attack;
    public Buff totalRouletteBuff_Heal;
    public Buff totalRouletteBuff_Shield;
    public Buff totalRouletteBuff_Charge;
    public Buff totalRouletteBuff_Lifesteal_Dmg;
    public Buff totalRouletteBuff_Lifesteal_Heal;
    public Buff singleRouletteBuff_Trigger;
    public Dictionary<RoulettePiece, Buff> roulettePieceBuff;

    public List<Buff> playerBuffs;
    public Buff damageBuff;
    public Buff healBuff;
    public Buff shieldBuff;
    public Buff costBuff;

    public void InitRouletteBuff()
    {
        totalRouletteBuff_Attack.InitBuff();
        totalRouletteBuff_Heal.InitBuff();
        totalRouletteBuff_Shield.InitBuff();
        totalRouletteBuff_Charge.InitBuff();
        totalRouletteBuff_Lifesteal_Dmg.InitBuff();
        totalRouletteBuff_Lifesteal_Heal.InitBuff();
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

    public void AddRouletteBuff(Buff target, int add, int mul, int turns)
    {
        Buff rb = new Buff();
        rb.target = target;
        rb.add = add;
        rb.mul = mul;
        rb.lastingTime = turns;
        rouletteBuffs.Add(rb);
    }

    public void AddPlayerBuff(Buff target, int add, int mul, int turns)
    {
        Buff rb = new Buff();
        rb.target = target;
        rb.add = add;
        rb.mul = mul;
        rb.lastingTime = turns;
        playerBuffs.Add(rb);
    }

    public void CalcTotalRouletteBuff()
    {
        InitRouletteBuff();
        foreach (Buff rb in rouletteBuffs)
        {
            if (rb.lastingTime > 0)
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
            if (pb.lastingTime > 0)
            {
                pb.target.add += pb.add;
                pb.target.mul *= pb.mul;
            }
        }
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
    }

    public void ReduceBuffCounter()
    {
        ReducePlayerBuffCounter();
        ReduceRouletteBuffCounter();
    }

    private void Start()
    {
        TurnManager.OnPlayerTurnStart += ReduceBuffCounter;
    }

    private void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart = null;
    }
}
