using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class RouletteBuff
{
    public RouletteBuff target;
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

    public List<RouletteBuff> rouletteBuffs;
    public RouletteBuff totalRouletteBuff_Attack;
    public RouletteBuff totalRouletteBuff_Heal;
    public RouletteBuff totalRouletteBuff_Shield;
    public RouletteBuff totalRouletteBuff_Charge;
    public RouletteBuff totalRouletteBuff_Lifesteal_Dmg;
    public RouletteBuff totalRouletteBuff_Lifesteal_Heal;

    public void InitRouletteBuff()
    {
        totalRouletteBuff_Attack.InitBuff();
        totalRouletteBuff_Heal.InitBuff();
        totalRouletteBuff_Shield.InitBuff();
        totalRouletteBuff_Charge.InitBuff();
        totalRouletteBuff_Lifesteal_Dmg.InitBuff();
        totalRouletteBuff_Lifesteal_Heal.InitBuff();
    }

    public void AddRouletteBuff(RouletteBuff target, int add, int mul, int turns)
    {
        RouletteBuff rb = new RouletteBuff();
        rb.target = target;
        rb.add = add;
        rb.mul = mul;
        rb.lastingTime = turns;
        rouletteBuffs.Add(rb);
    }

    public void CalcTotalRouletteBuff()
    {
        InitRouletteBuff();
        foreach (RouletteBuff rb in rouletteBuffs)
        {
            if (rb.lastingTime > 0)
            {
                rb.target.add += rb.add;
                rb.target.mul *= rb.mul;
            }
        }
    }

    public int GetBuffedRouletteValue(RouletteBuff target, int value)
    {
        CalcTotalRouletteBuff();
        return (value + target.add) * target.mul;
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

    private void Start()
    {
        TurnManager.OnPlayerTurnStart += ReduceRouletteBuffCounter;
    }

    private void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart = null;
    }
}
