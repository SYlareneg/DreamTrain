using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public enum EBuffType
{
    Value, Duration, Count
};
public enum EBuffAffectType
{
    Player, Enemy, Roulette
};

[System.Serializable]
public class Buff
{
    public int add;
    public float mul;
    public int lastingTime;

    public void InitBuff()
    {
        add = 0;
        mul = 1;
    }

    public void SetBuff(int a, float m, int time)
    {
        add = a;
        mul = m;
        lastingTime = time;
    }

    public void AddBuff(Buff buff)
    {
        add += buff.add;
        mul *= buff.mul;
        lastingTime += buff.lastingTime;
    }

    public static bool IsEqual(Buff a, Buff b)
    {
        if (a.add == b.add && a.mul == b.mul && a.lastingTime == b.lastingTime)
        {
            return true;
        }
        return false;
    }
}
[System.Serializable]
public class ShowBuff
{
    public string name;
    public string text;
    public Sprite icon;
    public EBuffType type;
    [HideInInspector] public EBuffAffectType affectType;
    [HideInInspector] public int val;
    [HideInInspector] public List<List<Buff>> targets;
    [HideInInspector] public List<Buff> affectBuffs;

    void AddAffectBuff(List<Buff> target, int add, float mul, int time)
    {
        targets.Add(target);
        Buff buff = new Buff();
        buff.SetBuff(add, mul, time);
        affectBuffs.Add(buff);
        target.Add(buff);
    }
    public void SetShowBuff(string name, EBuffAffectType aType, int newVal)
    {
        ShowBuff origin = BuffManager.Inst.showBuffSO.showBuffs.Find(x => x.name == name);
        this.name = name;
        text = origin.text;
        icon = origin.icon;
        type = origin.type;
        affectType = aType;
        val = newVal;
        targets = new List<List<Buff>>();
        affectBuffs = new List<Buff>();
        switch (name)
        {
            case "강화":
                if (affectType == EBuffAffectType.Enemy)
                {
                    AddAffectBuff(BuffManager.Inst.enemyBuff_Attack, newVal, 1, -1);
                    BuffManager.Inst.enemyShowBuffs.Add(this);
                }
                else if (affectType == EBuffAffectType.Roulette)
                {
                    AddAffectBuff(BuffManager.Inst.rouletteBuff_Attack, newVal, 1, -1);
                    BuffManager.Inst.rouletteShowBuffs.Add(this);
                }
                break;
            case "보호":
                if (affectType == EBuffAffectType.Enemy)
                {
                    AddAffectBuff(BuffManager.Inst.enemyBuff_Shield, newVal, 1, -1);
                    BuffManager.Inst.enemyShowBuffs.Add(this);
                }
                else if (affectType == EBuffAffectType.Roulette)
                {
                    AddAffectBuff(BuffManager.Inst.rouletteBuff_Shield, newVal, 1, -1);
                    BuffManager.Inst.rouletteShowBuffs.Add(this);
                }
                else
                {
                    AddAffectBuff(BuffManager.Inst.playerBuff_Shield, newVal, 1, -1);
                    BuffManager.Inst.playerShowBuffs.Add(this);
                }
                break;
            case "활력":
                if (affectType == EBuffAffectType.Enemy)
                {
                    AddAffectBuff(BuffManager.Inst.enemyBuff_Heal, newVal, 1, -1);
                    BuffManager.Inst.enemyShowBuffs.Add(this);
                }
                else if (affectType == EBuffAffectType.Roulette)
                {
                    AddAffectBuff(BuffManager.Inst.rouletteBuff_Heal, newVal, 1, -1);
                    BuffManager.Inst.rouletteShowBuffs.Add(this);
                }
                else
                {
                    AddAffectBuff(BuffManager.Inst.playerBuff_Heal, newVal, 1, -1);
                    BuffManager.Inst.playerShowBuffs.Add(this);
                }
                break;
            case "주저함":
                if (affectType == EBuffAffectType.Enemy)
                {
                    AddAffectBuff(BuffManager.Inst.enemyBuff_Attack, 0, 0.75f, newVal);
                    BuffManager.Inst.enemyShowBuffs.Add(this);
                }
                else if (affectType == EBuffAffectType.Roulette)
                {
                    AddAffectBuff(BuffManager.Inst.rouletteBuff_Attack, 0, 0.75f, newVal);
                    BuffManager.Inst.rouletteShowBuffs.Add(this);
                }
                break;
            case "취약":
                if (affectType == EBuffAffectType.Enemy)
                {
                    AddAffectBuff(BuffManager.Inst.enemyBuff_Damage, 0, 1.5f, newVal);
                    BuffManager.Inst.enemyShowBuffs.Add(this);
                }
                else if (affectType == EBuffAffectType.Player)
                {
                    AddAffectBuff(BuffManager.Inst.playerBuff_Damage, 0, 1.5f, newVal);
                    BuffManager.Inst.playerShowBuffs.Add(this);
                }
                break;
            case "블루 블러드":
                if (affectType == EBuffAffectType.Roulette)
                {
                    AddAffectBuff(BuffManager.Inst.rouletteBuff_Lifesteal_Dmg, 0, 3f, newVal);
                    AddAffectBuff(BuffManager.Inst.rouletteBuff_Lifesteal_Heal, 0, 0f, newVal);
                    BuffManager.Inst.rouletteShowBuffs.Add(this);
                }
                break;
            case "만찬 시간":
                if (affectType == EBuffAffectType.Roulette)
                {
                    AddAffectBuff(BuffManager.Inst.rouletteBuff_Lifesteal_Dmg, 0, 2f, newVal);
                    BuffManager.Inst.rouletteShowBuffs.Add(this);
                }
                break;
            case "예언-준비":
                if (affectType == EBuffAffectType.Player)
                {
                    BuffManager.Inst.playerShowBuffs.Add(this);
                    Action addProphecy = null;
                    addProphecy = () =>
                    {
                        BuffManager.Inst.AddShowBuff("예언", EBuffAffectType.Player, 1);
                        if (this.val == 0)
                        {
                            TurnManager.OnPlayerTurnStart -= addProphecy;
                        }
                    };
                    TurnManager.OnPlayerTurnStart += addProphecy;
                }
                break;
            case "예언":
                if (affectType == EBuffAffectType.Player)
                {
                    BuffManager.Inst.playerShowBuffs.Add(this);
                    Action eraseFullCost = null;
                    eraseFullCost = () =>
                    {
                        TurnManager.OnPlayerTrigger -= TurnManager.Inst.SetFullCost;
                        TurnManager.OnPlayerTurnEnd -= eraseFullCost;
                    };
                    TurnManager.OnPlayerTrigger += TurnManager.Inst.SetFullCost;
                    TurnManager.OnPlayerTurnEnd += eraseFullCost;
                }
                break;
        }
        if(type == EBuffType.Duration)
        {
            Action ReduceShowBuffCounter = () =>
            {
                this.AddShowBuff(-1);
                switch (this.affectType)
                {
                    case EBuffAffectType.Roulette:
                        if(this.val == 0)
                        {
                            BuffManager.Inst.rouletteShowBuffs.Remove(this);
                        }
                        GameManager.Inst.SetRouletteBuffUI();
                        break;
                    case EBuffAffectType.Enemy:
                        if(this.val == 0)
                        {
                            BuffManager.Inst.enemyShowBuffs.Remove(this);
                        }
                        GameManager.Inst.SetEnemyBuffUI(); break;
                    case EBuffAffectType.Player:
                        if(this.val == 0)
                        {
                            BuffManager.Inst.playerShowBuffs.Remove(this);
                        }
                        GameManager.Inst.SetPlayerBuffUI(); break;
                }
            };
            TurnManager.OnPlayerTurnStart = ReduceShowBuffCounter + TurnManager.OnPlayerTurnStart;
        }
    }

    public void AddShowBuff(int addVal)
    {
        val += addVal;
        Buff addBuff = new Buff();
        int affectBuffNum = affectBuffs.Count;
        for(int i = 0; i < affectBuffNum; i++)
        {
            switch(type)
            {
                case EBuffType.Value:
                    addBuff.SetBuff(addVal, 1, 0);
                    affectBuffs[i].AddBuff(addBuff);
                    break;
                case EBuffType.Duration:
                    addBuff.SetBuff(0, 1, addVal);
                    affectBuffs[i].AddBuff(addBuff);
                    break;
                case EBuffType.Count:
                    for (int j = 0; j < addVal; j++)
                    {
                        AddAffectBuff(targets[i], affectBuffs[i].add, affectBuffs[i].mul, affectBuffs[i].lastingTime);
                    }
                    break;
            }
        }
    }
}
public class BuffManager : MonoBehaviour
{
    public static BuffManager Inst { get; private set; }
    private void Awake() => Inst = this;
    [SerializeField] GameObject buffUIPrefab;
    public ShowBuffSO showBuffSO;

    public List<List<Buff>> rouletteBuffs = new List<List<Buff>>();
    public List<ShowBuff> rouletteShowBuffs = new List<ShowBuff>();
    public List<Buff> rouletteBuff_Attack;
    public List<Buff> rouletteBuff_Heal;
    public List<Buff> rouletteBuff_Shield;
    public List<Buff> rouletteBuff_Charge;
    public List<Buff> rouletteBuff_Lifesteal_Dmg;
    public List<Buff> rouletteBuff_Lifesteal_Heal;
    public List<Buff> rouletteBuff_Drain_Dmg;
    public List<Buff> rouletteBuff_Drain_Heal;
    public List<Buff> rouletteBuff_Trigger;
    public Dictionary<RoulettePiece, List<Buff>> roulettePieceBuff = new Dictionary<RoulettePiece, List<Buff>>();

    public List<List<Buff>> playerBuffs = new List<List<Buff>>();
    public List<ShowBuff> playerShowBuffs = new List<ShowBuff>();
    public List<Buff> playerBuff_Damage;
    public List<Buff> playerBuff_Heal;
    public List<Buff> playerBuff_Shield;
    public List<Buff> playerBuff_Cost;
    public List<Buff> playerBuff_Draw;

    public List<List<Buff>> enemyBuffs = new List<List<Buff>>();
    public List<ShowBuff> enemyShowBuffs = new List<ShowBuff>();
    public List<Buff> enemyBuff_Damage;
    public List<Buff> enemyBuff_Heal;
    public List<Buff> enemyBuff_Shield;
    public List<Buff> enemyBuff_Attack;
    public List<Buff> enemyBuff_Drain;

    public List<Buff> allCardValueBuff;
    public List<Buff> allCardCostBuff;
    public Dictionary<Item, List<Buff>> singleCardCostBuff = new Dictionary<Item, List<Buff>>();

    public void AddShowBuff(string name, EBuffAffectType aType, int val)
    {
        ShowBuff findBuff = null;
        switch (aType)
        {
            case EBuffAffectType.Roulette:
                findBuff = rouletteShowBuffs.Find(x => x.name == name);
                break;
            case EBuffAffectType.Enemy:
                findBuff = enemyShowBuffs.Find(x => x.name == name);
                break;
            case EBuffAffectType.Player:
                findBuff = playerShowBuffs.Find(x => x.name == name);
                break;
        }
        if (findBuff == null)
        {
            findBuff = new ShowBuff();
            findBuff.SetShowBuff(name, aType, val);
        }
        else
        {
            findBuff.AddShowBuff(val);
        }
        switch (aType)
        {
            case EBuffAffectType.Roulette:
                GameManager.Inst.SetRouletteBuffUI(); break;
            case EBuffAffectType.Enemy:
                GameManager.Inst.SetEnemyBuffUI(); break;
            case EBuffAffectType.Player:
                GameManager.Inst.SetPlayerBuffUI(); break;
        }
    }

    public List<BuffUI> BuffListToBuffUIList(List<ShowBuff> BuffList, GameObject parent)
    {
        List<BuffUI> returnList = new List<BuffUI>();
        foreach (var buff in BuffList)
        {
            if (buff.val == 0) continue;
            var bUIObj = Instantiate(buffUIPrefab, parent.transform.position, Utils.QI);
            bUIObj.transform.SetParent(parent.transform);
            BuffUI bUI = bUIObj.GetComponent<BuffUI>();
            bUI.Setup(buff);
            returnList.Add(bUI);
        }
        return returnList;
    }

    public void InitRouletteBuff()
    {
        rouletteBuff_Attack = new List<Buff>();
        rouletteBuff_Heal = new List<Buff>();
        rouletteBuff_Shield = new List<Buff>();
        rouletteBuff_Charge = new List<Buff>();
        rouletteBuff_Lifesteal_Dmg = new List<Buff>();
        rouletteBuff_Lifesteal_Heal = new List<Buff>();
        rouletteBuff_Drain_Dmg = new List<Buff>();
        rouletteBuff_Drain_Heal = new List<Buff>();
        rouletteBuff_Trigger = new List<Buff>();

        rouletteBuffs.Add(rouletteBuff_Attack);
        rouletteBuffs.Add(rouletteBuff_Heal);
        rouletteBuffs.Add(rouletteBuff_Shield);
        rouletteBuffs.Add(rouletteBuff_Charge);
        rouletteBuffs.Add(rouletteBuff_Lifesteal_Dmg);
        rouletteBuffs.Add(rouletteBuff_Lifesteal_Heal);
        rouletteBuffs.Add(rouletteBuff_Drain_Dmg);
        rouletteBuffs.Add(rouletteBuff_Drain_Heal);
        rouletteBuffs.Add(rouletteBuff_Trigger);

        roulettePieceBuff = new Dictionary<RoulettePiece, List<Buff>>();
        for (int i = 0; i < RouletteManager.rouletteNum; i++)
        {
            List<Buff> buffList = new List<Buff>();
            roulettePieceBuff.Add(RouletteManager.Inst.roulettePieces[i], buffList);
            rouletteBuffs.Add(roulettePieceBuff[RouletteManager.Inst.roulettePieces[i]]);
        }
    }

    public void InitPlayerBuff()
    {
        playerBuff_Damage = new List<Buff>();
        playerBuff_Heal = new List<Buff>();
        playerBuff_Shield = new List<Buff>();
        playerBuff_Cost = new List<Buff>();
        playerBuff_Draw = new List<Buff>();

        playerBuffs.Add(playerBuff_Damage);
        playerBuffs.Add(playerBuff_Heal);
        playerBuffs.Add(playerBuff_Shield);
        playerBuffs.Add(playerBuff_Cost);
        playerBuffs.Add(playerBuff_Draw);
    }

    public void InitEnemyBuff()
    {
        enemyBuff_Attack = new List<Buff>();
        enemyBuff_Damage = new List<Buff>();
        enemyBuff_Heal = new List<Buff>();
        enemyBuff_Shield = new List<Buff>();
        enemyBuff_Drain = new List<Buff>();

        enemyBuffs.Add(enemyBuff_Attack);
        enemyBuffs.Add(enemyBuff_Damage);
        enemyBuffs.Add(enemyBuff_Heal);
        enemyBuffs.Add(enemyBuff_Shield);
        enemyBuffs.Add(enemyBuff_Drain);
    }

    public void InitCardBuff()
    {
        allCardValueBuff = new List<Buff>();
        allCardCostBuff = new List<Buff>();

        playerBuffs.Add(allCardValueBuff);
        playerBuffs.Add(allCardCostBuff);

        var keys = singleCardCostBuff.Keys.ToList();
        foreach (var key in keys)
        {
            singleCardCostBuff[key] = new List<Buff>();
            playerBuffs.Add(singleCardCostBuff[key]);
        }
    }

    public static Buff AddBuffToTarget(List<Buff> target, int add, int mul, int turns)
    {
        Buff rb = new Buff();
        rb.SetBuff(add, mul, turns);
        target.Add(rb);
        return rb;
    }

    public void AddSingleCardCostBuff(Item card, int add, int mul, int turns)
    {
        if (singleCardCostBuff.ContainsKey(card) == false)
        {
            singleCardCostBuff[card] = new List<Buff>();
            playerBuffs.Add(singleCardCostBuff[card]);
        }
        AddBuffToTarget(singleCardCostBuff[card], add, mul, turns);
    }

    public static Buff CalcTotalBuff(List<Buff> target)
    {
        Buff totalBuff = new Buff();
        totalBuff.InitBuff();
        foreach (Buff buff in target)
        {
            if (buff.lastingTime != 0)
            {
                totalBuff.add += buff.add;
                totalBuff.mul *= buff.mul;
            }
        }
        return totalBuff;
    }

    public static int GetTargetBuffedValue(List<Buff> target, int value)
    {
        Buff totalBuff = new Buff();
        totalBuff.InitBuff();
        totalBuff.AddBuff(CalcTotalBuff(target));
        return (int)((value + totalBuff.add) * totalBuff.mul);
    }
    
    public int GetBuffedRouletteValue(RoulettePiece targetPiece)
    {
        Buff totalBuff = new Buff();
        totalBuff.InitBuff();
        switch (targetPiece.roulette.type)
        {
            case ERouletteType.Attack:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Attack)); break;
            case ERouletteType.Heal:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Heal)); break;
            case ERouletteType.Shield:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Shield)); break;
            case ERouletteType.Charge:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Charge)); break;
            case ERouletteType.Lifesteal:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Lifesteal_Dmg)); break;
            case ERouletteType.Drain:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Drain_Dmg)); break;
        }
        if (targetPiece.isTriggered)
        {
            totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Trigger));
        }
        totalBuff.AddBuff(CalcTotalBuff(roulettePieceBuff[targetPiece]));
        return (int)((targetPiece.roulette.value + totalBuff.add) * totalBuff.mul);
    }

    public int GetBuffedCardCost(Item card)
    {
        Buff totalBuff = new Buff();
        totalBuff.InitBuff();
        totalBuff.AddBuff(CalcTotalBuff(allCardCostBuff));
        if (singleCardCostBuff.ContainsKey(card) == true)
        {
            totalBuff.AddBuff(CalcTotalBuff(singleCardCostBuff[card]));
        }
        if(card.name == "마술-예언" && RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.type == ERouletteType.MagicBox)
        {
            totalBuff.add -= 2;
        }
        return (int)((card.cost + totalBuff.add) * totalBuff.mul);
    }

    public void ReduceBuffCounter(List<Buff> target)
    {
        for (int i = target.Count - 1; i >= 0; i--)
        {
            if(target[i].lastingTime > 0)
            {
                target[i].lastingTime--;
            }
            if (target[i].lastingTime == 0)
            {
                target.RemoveAt(i);
            }
        }
    }

    public void ReduceAllBuffCounters()
    {
        foreach (var buffList in rouletteBuffs)
        {
            ReduceBuffCounter(buffList);
        }
        foreach (var buffList in playerBuffs)
        {
            ReduceBuffCounter(buffList);
        }
        foreach (var buffList in enemyBuffs)
        {
            ReduceBuffCounter(buffList);
        }
    }

    private void Start()
    {
        InitRouletteBuff();
        InitPlayerBuff();
        InitEnemyBuff();
        InitCardBuff();
        TurnManager.OnPlayerTurnStart = ReduceAllBuffCounters + TurnManager.OnPlayerTurnStart;
    }

    private void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart = null;
    }
}
