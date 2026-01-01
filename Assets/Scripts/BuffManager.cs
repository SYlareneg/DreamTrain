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
public enum EBuffEffectType
{
    Benefit, Demerit, Neutral
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
    public int affectEnemyIdx;
    public EBuffEffectType effectType;
    public int val;
    public List<float> defaultVal = new List<float>{1f};
    [HideInInspector] public bool isSetOnEnemyTurn;
    [HideInInspector] public List<List<Buff>> targets;
    [HideInInspector] public List<Buff> affectBuffs;
    Action removeBuff;

    void AddAffectBuff(List<Buff> target, int add, float mul, int time)
    {
        targets.Add(target);
        Buff buff = new Buff();
        buff.SetBuff(add, mul, time);
        affectBuffs.Add(buff);
        target.Add(buff);
    }
    public void SetShowBuff(string name, EBuffAffectType aType, int newVal, bool isSetOnEnemyTurn, List<float> baseVal = null, int affectEnemyIdx = 0)
    {
        ShowBuff origin = BuffManager.Inst.showBuffSO.showBuffs.Find(x => x.name == name);
        if (origin == null)
        {
            Debug.LogWarning("미등록 버프!");
            return;
        }
        this.name = name;
        text = origin.text;
        icon = origin.icon;
        type = origin.type;
        effectType = origin.effectType;
        affectType = aType;
        this.affectEnemyIdx = affectEnemyIdx;
        val = newVal;
        defaultVal.Clear();
        foreach(float v in origin.defaultVal) defaultVal.Add(v);
        if(baseVal == null) baseVal = defaultVal;
        this.isSetOnEnemyTurn = isSetOnEnemyTurn;
        if(isSetOnEnemyTurn && type == EBuffType.Duration) newVal++;
        targets = new List<List<Buff>>();
        affectBuffs = new List<Buff>();
        switch (name)
        {
            case "강화":
                if (affectType == EBuffAffectType.Enemy)
                {
                    BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Add(this);
                    AddAffectBuff(BuffManager.Inst.enemyBuff_Attack[affectEnemyIdx], newVal, baseVal[0], -1);
                }
                else if (affectType == EBuffAffectType.Roulette)
                {
                    BuffManager.Inst.rouletteShowBuffs.Add(this);
                    AddAffectBuff(BuffManager.Inst.rouletteBuff_Attack, newVal, baseVal[0], -1);
                }
                break;
            case "보호":
                if (affectType == EBuffAffectType.Enemy)
                {
                    BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Add(this);
                    AddAffectBuff(BuffManager.Inst.enemyBuff_Shield[affectEnemyIdx], newVal, baseVal[0], -1);
                }
                else if (affectType == EBuffAffectType.Roulette)
                {
                    BuffManager.Inst.rouletteShowBuffs.Add(this);
                    AddAffectBuff(BuffManager.Inst.rouletteBuff_Shield, newVal, baseVal[0], -1);
                }
                else
                {
                    BuffManager.Inst.playerShowBuffs.Add(this);
                    AddAffectBuff(BuffManager.Inst.playerBuff_Shield, newVal, baseVal[0], -1);
                }
                break;
            case "활력":
                if (affectType == EBuffAffectType.Enemy)
                {
                    BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Add(this);
                    AddAffectBuff(BuffManager.Inst.enemyBuff_Heal[affectEnemyIdx], newVal, baseVal[0], -1);
                }
                else if (affectType == EBuffAffectType.Roulette)
                {
                    BuffManager.Inst.rouletteShowBuffs.Add(this);
                    AddAffectBuff(BuffManager.Inst.rouletteBuff_Heal, newVal, baseVal[0], -1);
                }
                else
                {
                    BuffManager.Inst.playerShowBuffs.Add(this);
                    AddAffectBuff(BuffManager.Inst.playerBuff_Heal, newVal, baseVal[0], -1);
                }
                break;
            case "주저함":
                if (affectType == EBuffAffectType.Enemy)
                {
                    BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Add(this);
                    AddAffectBuff(BuffManager.Inst.enemyBuff_Attack[affectEnemyIdx], 0, baseVal[0], newVal);
                    // 0.75f
                }
                else if (affectType == EBuffAffectType.Roulette)
                {
                    BuffManager.Inst.rouletteShowBuffs.Add(this);
                    AddAffectBuff(BuffManager.Inst.rouletteBuff_Attack, 0, baseVal[0], newVal);
                }
                else
                {
                    BuffManager.Inst.playerShowBuffs.Add(this);
                    AddAffectBuff(BuffManager.Inst.enemyBuff_Damage_Type[affectEnemyIdx, (int)EDamageSource.Card], 0, baseVal[0], newVal);
                }
                break;
            case "취약":
                if (affectType == EBuffAffectType.Enemy)
                {
                    BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Add(this);
                    AddAffectBuff(BuffManager.Inst.enemyBuff_Damage[affectEnemyIdx], 0, baseVal[0], newVal);
                    // 1.5f
                }
                else if (affectType == EBuffAffectType.Player)
                {
                    BuffManager.Inst.playerShowBuffs.Add(this);
                    AddAffectBuff(BuffManager.Inst.playerBuff_Damage, 0, baseVal[0], newVal);
                }
                break;
            case "블루 블러드":
                if (affectType == EBuffAffectType.Roulette)
                {
                    BuffManager.Inst.rouletteShowBuffs.Add(this);
                    AddAffectBuff(BuffManager.Inst.rouletteBuff_PlayerSpecial[PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == 0, 0)][0], 0, baseVal[0], newVal);
                    AddAffectBuff(BuffManager.Inst.rouletteBuff_PlayerSpecial[PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == 0, 0)][1], 0, baseVal[1], newVal);
                    // 3f, 1f
                }
                break;
            case "만찬 시간":
                if (affectType == EBuffAffectType.Roulette)
                {
                    BuffManager.Inst.rouletteShowBuffs.Add(this);
                    AddAffectBuff(BuffManager.Inst.rouletteBuff_PlayerSpecial[PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == 0, 0)][0], 0, baseVal[0], newVal);
                    // 2f
                }
                break;
            case "예언-준비":
                if (affectType == EBuffAffectType.Player)
                {
                    BuffManager.Inst.playerShowBuffs.Add(this);
                    Action addProphecy = null;
                    addProphecy = () =>
                    {
                        BuffManager.Inst.AddShowBuff("예언", EBuffAffectType.Player, 1, isSetOnEnemyTurn, baseVal, affectEnemyIdx);
                        if (this.val == 0)
                        {
                            BuffManager.Inst.playerShowBuffs.Remove(this);
                            TurnManager.OnPlayerTurnStart -= addProphecy;
                        }
                    };
                    TurnManager.OnPlayerTurnStart += addProphecy;
                    removeBuff = () =>
                    {
                        TurnManager.OnPlayerTurnStart -= addProphecy;
                    };
                }
                break;
            case "예언":
                if (affectType == EBuffAffectType.Player)
                {
                    BuffManager.Inst.playerShowBuffs.Add(this);
                    Action eraseFullCost = null;
                    eraseFullCost = () =>
                    {
                        BuffManager.Inst.playerShowBuffs.Remove(this);
                        TurnManager.OnPlayerTrigger -= TurnManager.Inst.SetFullCost;
                        TurnManager.OnPlayerTurnEnd -= eraseFullCost;
                    };
                    TurnManager.OnPlayerTrigger += TurnManager.Inst.SetFullCost;
                    TurnManager.OnPlayerTurnEnd += eraseFullCost;
                    removeBuff = () =>
                    {
                        TurnManager.OnPlayerTrigger -= TurnManager.Inst.SetFullCost;
                        TurnManager.OnPlayerTurnEnd -= eraseFullCost;
                    };
                }
                break;
            case "환영":
                if (affectType == EBuffAffectType.Enemy)
                {
                    BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Add(this);
                    Action<int, EDamageSource, int> noDamage = null;
                    noDamage = (value, source, enemyIdx) =>
                    {
                        TurnManager.Inst.enemyShieldHealth[affectEnemyIdx] += value;
                        BuffManager.Inst.AddShowBuff("환영", affectType, -1, isSetOnEnemyTurn, baseVal, affectEnemyIdx);
                        if (this.val == 0)
                        {
                            BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Remove(this);
                            TurnManager.OnEnemyDamaged -= noDamage;
                        }
                    };
                    TurnManager.OnEnemyDamaged += noDamage;
                    removeBuff = () =>
                    {
                        TurnManager.OnEnemyDamaged -= noDamage;
                    };
                }
                else if (affectType == EBuffAffectType.Player)
                {
                    BuffManager.Inst.playerShowBuffs.Add(this);
                    Action<int, EDamageSource> noDamage = null;
                    noDamage = (value, source) =>
                    {
                        TurnManager.Inst.shieldHealth += value;
                        BuffManager.Inst.AddShowBuff("환영", affectType, -1, isSetOnEnemyTurn, baseVal, affectEnemyIdx);
                        if (this.val == 0)
                        {
                            BuffManager.Inst.playerShowBuffs.Remove(this);
                            TurnManager.OnPlayerDamaged -= noDamage;
                        }
                    };
                    TurnManager.OnPlayerDamaged += noDamage;
                    removeBuff = () =>
                    {
                        TurnManager.OnPlayerDamaged -= noDamage;
                    };
                }
                break;
            case "과민함":
                if (affectType == EBuffAffectType.Enemy)
                {
                    BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Add(this);
                    AddAffectBuff(BuffManager.Inst.enemyBuff_Damage[affectEnemyIdx], 0, baseVal[0], -1);
                    // 1.5f
                    Action<int, EDamageSource, int> reduceCount = null;
                    reduceCount = (damage, source, enemyIdx) =>
                    {
                        BuffManager.Inst.AddShowBuff("과민함", affectType, -1, isSetOnEnemyTurn, baseVal, affectEnemyIdx);
                        if (this.val == 0)
                        {
                            BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Remove(this);
                            TurnManager.OnEnemyDamaged -= reduceCount;
                        }
                    };
                    TurnManager.OnEnemyDamaged += reduceCount;
                    removeBuff = () =>
                    {
                        TurnManager.OnEnemyDamaged -= reduceCount;
                    };
                }
                else if (affectType == EBuffAffectType.Player)
                {
                    BuffManager.Inst.playerShowBuffs.Add(this);
                    AddAffectBuff(BuffManager.Inst.playerBuff_Damage, 0, baseVal[0], -1);
                    // 1.5f
                    Action<int, EDamageSource> reduceCount = null;
                    reduceCount = (damage, source) =>
                    {
                        BuffManager.Inst.AddShowBuff("과민함", affectType, -1, isSetOnEnemyTurn, baseVal, affectEnemyIdx);
                        if (this.val == 0)
                        {
                            BuffManager.Inst.playerShowBuffs.Remove(this);
                            TurnManager.OnPlayerDamaged -= reduceCount;
                        }
                    };
                    TurnManager.OnPlayerDamaged += reduceCount;
                    removeBuff = () =>
                    {
                        TurnManager.OnPlayerDamaged -= reduceCount;
                    };
                }
                break;
            case "불쾌함":
                if (affectType == EBuffAffectType.Enemy)
                {
                    BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Add(this);
                    Action<bool, int> reduceCount = null;
                    reduceCount = (b, spin) =>
                    {
                        TurnManager.Inst.EnemyTakeDmg(3, EDamageSource.Buff);
                        BuffManager.Inst.AddShowBuff("불쾌함", affectType, -1, isSetOnEnemyTurn, baseVal, affectEnemyIdx);
                        if (this.val == 0)
                        {
                            BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Remove(this);
                            TurnManager.OnRouletteSpin -= reduceCount;
                        }
                    };
                    TurnManager.OnRouletteSpin += reduceCount;
                    removeBuff = () =>
                    {
                        TurnManager.OnRouletteSpin -= reduceCount;
                    };
                }
                else if (affectType == EBuffAffectType.Player)
                {
                    BuffManager.Inst.playerShowBuffs.Add(this);
                    Action<bool, int> reduceCount = null;
                    reduceCount = (b, spin) =>
                    {
                        TurnManager.Inst.TakeDmg(3, EDamageSource.Buff);
                        BuffManager.Inst.AddShowBuff("불쾌함", affectType, -1, isSetOnEnemyTurn, baseVal, affectEnemyIdx);
                        if (this.val == 0)
                        {
                            BuffManager.Inst.playerShowBuffs.Remove(this);
                            TurnManager.OnRouletteSpin -= reduceCount;
                        }
                    };
                    TurnManager.OnRouletteSpin += reduceCount;
                    removeBuff = () =>
                    {
                        TurnManager.OnRouletteSpin -= reduceCount;
                    };
                }
                break;
            case "회전 봉인":
                if(affectType == EBuffAffectType.Enemy)
                {
                    BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Add(this);
                    BuffManager.Inst.enemyBuff_ActionBlock[EEnemyActionType.Turn] = true;
                    Action endSpinBlock = null;
                    endSpinBlock = () =>
                    {
                        if (this.val == 0)
                        {
                            BuffManager.Inst.enemyBuff_ActionBlock[EEnemyActionType.Turn] = false;
                            BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Remove(this);
                            TurnManager.OnPlayerTurnStart -= endSpinBlock;
                        }
                    };
                    TurnManager.OnPlayerTurnStart += endSpinBlock;
                    removeBuff = () =>
                    {
                        TurnManager.OnPlayerTurnStart -= endSpinBlock;
                    };
                }
                else if(affectType == EBuffAffectType.Player)
                {
                    BuffManager.Inst.playerShowBuffs.Add(this);
                    BuffManager.Inst.allCardTypeBlockBuff[CardType.Turn] = true;
                    Action endSpinBlock = null;
                    endSpinBlock = () =>
                    {
                        if (this.val == 0)
                        {
                            BuffManager.Inst.allCardTypeBlockBuff[CardType.Turn] = false;
                            BuffManager.Inst.playerShowBuffs.Remove(this);
                            TurnManager.OnPlayerTurnStart -= endSpinBlock;
                        }
                    };
                    TurnManager.OnPlayerTurnStart += endSpinBlock;
                    removeBuff = () =>
                    {
                        BuffManager.Inst.allCardTypeBlockBuff[CardType.Turn] = false;
                        TurnManager.OnPlayerTurnStart -= endSpinBlock;
                    };
                }
                break;
            case "놀이 시간":
                if (affectType == EBuffAffectType.Player)
                {
                    BuffManager.Inst.playerShowBuffs.Add(this);
                    Action<Card> earnCost = null;
                    earnCost = (card) =>
                    {
                        if(card.item.type == CardType.Turn)
                        {
                            TurnManager.Inst.IncreaseCost(1);
                        }
                    };
                    TurnManager.OnUseCard += earnCost;
                    removeBuff = () =>
                    {
                        TurnManager.OnUseCard -= earnCost;
                    };
                }
                break;
            case "빙그르!":
                if (affectType == EBuffAffectType.Enemy)
                {
                    BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Add(this);
                    Action<bool, int> reduceCount = null;
                    reduceCount = (isClockwise, spinAmount) =>
                    {
                        if(spinAmount >= 3)
                        {
                            BuffManager.Inst.AddShowBuff("빙그르!", affectType, -1, isSetOnEnemyTurn, baseVal, affectEnemyIdx);
                            BuffManager.Inst.AddShowBuff("강화", EBuffAffectType.Enemy, 1, isSetOnEnemyTurn, null, affectEnemyIdx);
                            if (this.val == 0)
                            {
                                BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Remove(this);
                                EnemyManager.Inst.DestroySubEnemy(affectEnemyIdx - 1);
                                TurnManager.OnRouletteSpin -= reduceCount;
                            }
                        }
                    };
                    TurnManager.OnRouletteSpin += reduceCount;
                    removeBuff = () =>
                    {
                        TurnManager.OnRouletteSpin -= reduceCount;
                    };
                }
                break;
        }
    }

    public void RemoveShowBuff()
    {
        this.val = 0;

        removeBuff?.Invoke();

        if (affectType == EBuffAffectType.Enemy) BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Remove(this);
        else if (affectType == EBuffAffectType.Roulette) BuffManager.Inst.rouletteShowBuffs.Remove(this);
        else BuffManager.Inst.playerShowBuffs.Remove(this);

        for(int i=0;i<targets.Count;i++)
        {
            targets[i].Remove(affectBuffs[i]);
        }
    }
    
    public void ReduceShowBuffCounter()
    {
        if (type == EBuffType.Duration)
        {
            if (isSetOnEnemyTurn)
            {
                isSetOnEnemyTurn = false;
                return;
            }
            this.val -= 1;
            switch (this.affectType)
            {
                case EBuffAffectType.Roulette:
                    if (this.val == 0)
                    {
                        BuffManager.Inst.rouletteShowBuffs.Remove(this);
                    }
                    GameManager.Inst.SetRouletteBuffUI(); break;
                case EBuffAffectType.Enemy:
                    if (this.val == 0)
                    {
                        BuffManager.Inst.enemyShowBuffs[affectEnemyIdx].Remove(this);
                    }
                    GameManager.Inst.SetEnemyBuffUI(affectEnemyIdx); break;
                case EBuffAffectType.Player:
                    if (this.val == 0)
                    {
                        BuffManager.Inst.playerShowBuffs.Remove(this);
                    }
                    GameManager.Inst.SetPlayerBuffUI(); break;
            }
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
    public List<List<Buff>>[,] rouletteBuff_EnemySpecial = new List<List<Buff>>[Enemy.maxSubEnemyNum + 1, Enemy.enemySpecialRouletteNum];
    public List<List<Buff>>[] rouletteBuff_PlayerSpecial = new List<List<Buff>>[DreamPiece_Base.playerSpecialRouletteNum * 2];
    public List<Buff> rouletteBuff_Trigger;
    public Dictionary<RoulettePiece, List<Buff>> roulettePieceBuff = new Dictionary<RoulettePiece, List<Buff>>();

    public List<List<Buff>> playerBuffs = new List<List<Buff>>();
    public List<ShowBuff> playerShowBuffs = new List<ShowBuff>();
    public List<Buff> playerBuff_Damage;
    public List<Buff>[] playerBuff_Damage_Type = new List<Buff>[Enum.GetNames(typeof(EDamageSource)).Length];
    public List<Buff> playerBuff_Heal;
    public List<Buff>[] playerBuff_Heal_Type = new List<Buff>[Enum.GetNames(typeof(EDamageSource)).Length];
    public List<Buff> playerBuff_Shield;
    public List<Buff>[] playerBuff_Shield_Type = new List<Buff>[Enum.GetNames(typeof(EDamageSource)).Length];
    public List<Buff> playerBuff_Cost;
    public List<Buff> playerBuff_Draw;

    public List<List<Buff>> enemyBuffs = new List<List<Buff>>();
    public List<ShowBuff>[] enemyShowBuffs = new List<ShowBuff>[Enemy.maxSubEnemyNum + 1];
    public List<Buff>[] enemyBuff_Damage = new List<Buff>[Enemy.maxSubEnemyNum + 1];
    public List<Buff>[,] enemyBuff_Damage_Type = new List<Buff>[Enemy.maxSubEnemyNum + 1, Enum.GetNames(typeof(EDamageSource)).Length];
    public List<Buff>[] enemyBuff_Heal = new List<Buff>[Enemy.maxSubEnemyNum + 1];
    public List<Buff>[,] enemyBuff_Heal_Type = new List<Buff>[Enemy.maxSubEnemyNum + 1, Enum.GetNames(typeof(EDamageSource)).Length];
    public List<Buff>[] enemyBuff_Shield = new List<Buff>[Enemy.maxSubEnemyNum + 1];
    public List<Buff>[,] enemyBuff_Shield_Type = new List<Buff>[Enemy.maxSubEnemyNum + 1, Enum.GetNames(typeof(EDamageSource)).Length];
    public List<Buff>[] enemyBuff_Attack = new List<Buff>[Enemy.maxSubEnemyNum + 1];
    public List<Buff>[,] enemyBuff_Special = new List<Buff>[Enemy.maxSubEnemyNum + 1, Enemy.enemySpecialActionNum];
    public Dictionary<EEnemyActionType, bool> enemyBuff_ActionBlock = new Dictionary<EEnemyActionType, bool>();

    public List<Buff> allCardValueBuff;
    public List<Buff> allCardCostBuff;
    public Dictionary<CardType, bool> allCardTypeBlockBuff = new Dictionary<CardType, bool>();
    public Dictionary<Item, List<Buff>> singleCardCostBuff = new Dictionary<Item, List<Buff>>();

    public static Action InitSpecialRouletteBuffs;

    public ShowBuff GetShowBuff(string name, EBuffAffectType aType, int enemyIdx = 0)
    {
        ShowBuff findBuff = null;
        switch (aType)
        {
            case EBuffAffectType.Roulette:
                findBuff = rouletteShowBuffs.Find(x => x.name == name);
                break;
            case EBuffAffectType.Enemy:
                findBuff = enemyShowBuffs[enemyIdx].Find(x => x != null && x.name == name);
                break;
            case EBuffAffectType.Player:
                findBuff = playerShowBuffs.Find(x => x.name == name);
                break;
        }
        return findBuff;
    }
    public void AddShowBuff(string name, EBuffAffectType aType, int val, bool isSetOnEnemyTurn, List<float> bVal = null, int enemyIdx = 0)
    {
        ShowBuff findBuff = GetShowBuff(name, aType, enemyIdx);
        if (findBuff == null)
        {
            findBuff = new ShowBuff();
            findBuff.SetShowBuff(name, aType, val, isSetOnEnemyTurn, bVal, enemyIdx);
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
                GameManager.Inst.SetEnemyBuffUI(enemyIdx); break;
            case EBuffAffectType.Player:
                GameManager.Inst.SetPlayerBuffUI(); break;
        }
    }

    public void RemoveShowBuff(string name, EBuffAffectType aType, int enemyIdx = 0)
    {
        ShowBuff findBuff = GetShowBuff(name, aType, enemyIdx);
        if (findBuff == null) return;
        findBuff.RemoveShowBuff();
        switch (aType)
        {
            case EBuffAffectType.Roulette:
                GameManager.Inst.SetRouletteBuffUI(); break;
            case EBuffAffectType.Enemy:
                GameManager.Inst.SetEnemyBuffUI(enemyIdx); break;
            case EBuffAffectType.Player:
                GameManager.Inst.SetPlayerBuffUI(); break;
        }
    }

    public List<BuffUI> BuffListToBuffUIList(List<ShowBuff> BuffList, GameObject parent, Vector2 tooltipBasePos)
    {
        List<BuffUI> returnList = new List<BuffUI>();
        foreach (var buff in BuffList)
        {
            if (buff.val == 0) continue;
            var bUIObj = Instantiate(buffUIPrefab, parent.transform.position, Utils.QI);
            bUIObj.transform.SetParent(parent.transform, false);
            BuffUI bUI = bUIObj.GetComponent<BuffUI>();
            bUI.Setup(buff);
            bUI.tooltipBasePos = tooltipBasePos;
            returnList.Add(bUI);
        }
        return returnList;
    }

    public void AddSpecialBuffInstance(List<List<Buff>> rouletteBuff_Special)
    {
        List<Buff> buffList = new List<Buff>();
        rouletteBuff_Special.Add(buffList);
    }
    public void InitRouletteBuff()
    {
        rouletteBuff_Attack = new List<Buff>();
        rouletteBuff_Heal = new List<Buff>();
        rouletteBuff_Shield = new List<Buff>();
        for(int i = 0; i < Enemy.maxSubEnemyNum + 1; i++)
        {
            for(int j = 0; j < Enemy.enemySpecialRouletteNum; j++)
            {
                rouletteBuff_EnemySpecial[i, j] = new List<List<Buff>>();
            }
        }
        for(int i = 0; i < DreamPiece_Base.playerSpecialRouletteNum * 2; i++)
        {
            rouletteBuff_PlayerSpecial[i] = new List<List<Buff>>();
        }
        rouletteBuff_Trigger = new List<Buff>();

        InitSpecialRouletteBuffs?.Invoke();

        rouletteBuffs.Add(rouletteBuff_Attack);
        rouletteBuffs.Add(rouletteBuff_Heal);
        rouletteBuffs.Add(rouletteBuff_Shield);
        for(int i = 0; i < Enemy.maxSubEnemyNum + 1; i++)
        {
            for(int j = 0; j < Enemy.enemySpecialRouletteNum; j++)
            {
                foreach(var bl in rouletteBuff_EnemySpecial[i, j]) rouletteBuffs.Add(bl);
            }
        }
        for(int i = 0; i < DreamPiece_Base.playerSpecialRouletteNum; i++)
        {
            foreach(var bl in rouletteBuff_PlayerSpecial[i]) rouletteBuffs.Add(bl);
        }
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
        for(int i = 0; i < Enum.GetNames(typeof(EDamageSource)).Length; i++)
        {
            playerBuff_Damage_Type[i] = new List<Buff>();
            playerBuff_Heal_Type[i] = new List<Buff>();
            playerBuff_Shield_Type[i] = new List<Buff>();

            playerBuffs.Add(playerBuff_Damage_Type[i]);
            playerBuffs.Add(playerBuff_Heal_Type[i]);
            playerBuffs.Add(playerBuff_Shield_Type[i]);
        }

        playerBuffs.Add(playerBuff_Damage);
        playerBuffs.Add(playerBuff_Heal);
        playerBuffs.Add(playerBuff_Shield);
        playerBuffs.Add(playerBuff_Cost);
        playerBuffs.Add(playerBuff_Draw);
    }

    public void InitEnemyBuff()
    {
        for(int i = 0; i < Enemy.maxSubEnemyNum + 1; i++)
        {
            enemyShowBuffs[i] = new List<ShowBuff>();
            enemyBuff_Attack[i] = new List<Buff>();
            enemyBuff_Damage[i] = new List<Buff>();
            enemyBuff_Heal[i] = new List<Buff>();
            enemyBuff_Shield[i] = new List<Buff>();
            for(int j = 0; j < Enemy.enemySpecialActionNum; j++)
            {
                enemyBuff_Special[i, j] = new List<Buff>();
                enemyBuffs.Add(enemyBuff_Special[i, j]);
            }
            for(int j = 0; j < Enum.GetNames(typeof(EDamageSource)).Length; j++)
            {
                enemyBuff_Damage_Type[i, j] = new List<Buff>();
                enemyBuff_Heal_Type[i, j] = new List<Buff>();
                enemyBuff_Shield_Type[i, j] = new List<Buff>();

                enemyBuffs.Add(enemyBuff_Damage_Type[i, j]);
                enemyBuffs.Add(enemyBuff_Heal_Type[i, j]);
                enemyBuffs.Add(enemyBuff_Shield_Type[i, j]);
            }

            enemyBuffs.Add(enemyBuff_Attack[i]);
            enemyBuffs.Add(enemyBuff_Damage[i]);
            enemyBuffs.Add(enemyBuff_Heal[i]);
            enemyBuffs.Add(enemyBuff_Shield[i]);
        }

        enemyBuff_ActionBlock = new Dictionary<EEnemyActionType, bool>();
        foreach(EEnemyActionType eat in Enum.GetValues(typeof(EEnemyActionType)))
        {
            enemyBuff_ActionBlock.Add(eat, false);
        }
    }

    public void InitCardBuff()
    {
        allCardValueBuff = new List<Buff>();
        allCardCostBuff = new List<Buff>();
        allCardTypeBlockBuff = new Dictionary<CardType, bool>();
        foreach (CardType ct in Enum.GetValues(typeof(CardType)))
        {
            allCardTypeBlockBuff.Add(ct, false);
        }

        playerBuffs.Add(allCardValueBuff);
        playerBuffs.Add(allCardCostBuff);

        var keys = singleCardCostBuff.Keys.ToList();
        foreach (var key in keys)
        {
            singleCardCostBuff[key] = new List<Buff>();
            playerBuffs.Add(singleCardCostBuff[key]);
        }
    }

    public static Buff AddBuffToTarget(List<Buff> target, int add, float mul, int turns)
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
        switch (targetPiece.roulette.rtype.type)
        {
            case ERouletteType.Attack:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Attack)); break;
            case ERouletteType.Heal:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Heal)); break;
            case ERouletteType.Shield:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Shield)); break;
            case ERouletteType.Enemy_Special:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_EnemySpecial[targetPiece.roulette.rtype.enemyIdx, targetPiece.roulette.rtype.specialTypeIdx][0])); break;
            case ERouletteType.Player_Special:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_PlayerSpecial[targetPiece.roulette.rtype.specialTypeIdx][0])); break;
        }
        if (RouletteManager.Inst.isTriggerActivated)
        {
            totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Trigger));
        }
        totalBuff.AddBuff(CalcTotalBuff(roulettePieceBuff[targetPiece]));
        return (int)((targetPiece.roulette.value + totalBuff.add) * totalBuff.mul);
    }

    public int GetBuffedRouletteValue(RouletteItem rouletteItem)
    {
        Buff totalBuff = new Buff();
        totalBuff.InitBuff();
        switch (rouletteItem.rtype.type)
        {
            case ERouletteType.Attack:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Attack)); break;
            case ERouletteType.Heal:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Heal)); break;
            case ERouletteType.Shield:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Shield)); break;
            case ERouletteType.Enemy_Special:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_EnemySpecial[rouletteItem.rtype.enemyIdx, rouletteItem.rtype.enemyIdx][0])); break;
            case ERouletteType.Player_Special:
                totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_PlayerSpecial[rouletteItem.rtype.specialTypeIdx][0])); break;
        }
        if (RouletteManager.Inst.isTriggerActivated)
        {
            totalBuff.AddBuff(CalcTotalBuff(rouletteBuff_Trigger));
        }
        return (int)((rouletteItem.value + totalBuff.add) * totalBuff.mul);
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
        if(card.name == "마술-예언")
        {
            if(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == 1 && RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(true, 0))
            || TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum == 1 && RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(false, 0)))
            {
                totalBuff.add -= card.cardValues[1];
            }
        }
        return (int)((card.cost + totalBuff.add) * totalBuff.mul);
    }

    public int GetBuffedPlayerDamage(EDamageSource source, int damage)
    {
        Buff totalBuff = new Buff();
        totalBuff.InitBuff();
        totalBuff.AddBuff(CalcTotalBuff(playerBuff_Damage_Type[(int)source]));
        totalBuff.AddBuff(CalcTotalBuff(playerBuff_Damage));
        return (int)((damage + totalBuff.add) * totalBuff.mul);
    }

    public int GetBuffedPlayerHeal(EDamageSource source, int heal)
    {
        Buff totalBuff = new Buff();
        totalBuff.InitBuff();
        totalBuff.AddBuff(CalcTotalBuff(playerBuff_Heal_Type[(int)source]));
        totalBuff.AddBuff(CalcTotalBuff(playerBuff_Heal));
        return (int)((heal + totalBuff.add) * totalBuff.mul);
    }

    public int GetBuffedPlayerShield(EDamageSource source, int shield)
    {
        Buff totalBuff = new Buff();
        totalBuff.InitBuff();
        totalBuff.AddBuff(CalcTotalBuff(playerBuff_Shield_Type[(int)source]));
        totalBuff.AddBuff(CalcTotalBuff(playerBuff_Shield));
        return (int)((shield + totalBuff.add) * totalBuff.mul);
    }

    public int GetBuffedEnemyDamage(EDamageSource source, int damage, int enemyIdx = 0)
    {
        Buff totalBuff = new Buff();
        totalBuff.InitBuff();
        totalBuff.AddBuff(CalcTotalBuff(enemyBuff_Damage_Type[enemyIdx, (int)source]));
        totalBuff.AddBuff(CalcTotalBuff(enemyBuff_Damage[enemyIdx]));
        return (int)((damage + totalBuff.add) * totalBuff.mul);
    }

    public int GetBuffedEnemyHeal(EDamageSource source, int heal, int enemyIdx = 0)
    {
        Buff totalBuff = new Buff();
        totalBuff.InitBuff();
        totalBuff.AddBuff(CalcTotalBuff(enemyBuff_Heal_Type[enemyIdx, (int)source]));
        totalBuff.AddBuff(CalcTotalBuff(enemyBuff_Heal[enemyIdx]));
        return (int)((heal + totalBuff.add) * totalBuff.mul);
    }

    public int GetBuffedEnemyShield(EDamageSource source, int shield, int enemyIdx = 0)
    {
        Buff totalBuff = new Buff();
        totalBuff.InitBuff();
        totalBuff.AddBuff(CalcTotalBuff(enemyBuff_Shield_Type[enemyIdx, (int)source]));
        totalBuff.AddBuff(CalcTotalBuff(enemyBuff_Shield[enemyIdx]));
        return (int)((shield + totalBuff.add) * totalBuff.mul);
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

        for (int i = rouletteShowBuffs.Count - 1; i >= 0; i--)
        {
            rouletteShowBuffs[i].ReduceShowBuffCounter();
        }
        for (int i = playerShowBuffs.Count - 1; i >= 0; i--)
        {
            playerShowBuffs[i].ReduceShowBuffCounter();
        }
        for(int i = 0; i < Enemy.maxSubEnemyNum + 1; i++)
        {
            for (int j = enemyShowBuffs[i].Count - 1; j >= 0; j--)
            {
                enemyShowBuffs[i][j].ReduceShowBuffCounter();
            }
        }
    }

    public void InitAllBuffs()
    {
        InitRouletteBuff();
        InitPlayerBuff();
        InitEnemyBuff();
        InitCardBuff();
    }

    private void Start()
    {
        TurnManager.OnPlayerTurnStart = ReduceAllBuffCounters + TurnManager.OnPlayerTurnStart;
    }

    private void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart = null;
        InitSpecialRouletteBuffs = null;
    }
}
