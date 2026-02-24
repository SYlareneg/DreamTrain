using UnityEngine;
using UnityEngine.Events;

public enum StatType
{
    Courage, 
    Wisdom, 
    Luck  
}

[CreateAssetMenu(fileName = "PlayerStatsSO", menuName = "Scriptable Objects/PlayerStatsSO")]
public class PlayerStatsSo : ScriptableObject
{
    [Header("Runtime Stats")]
    public int courage;
    public int wisdom;
    public int luck;

    public UnityAction OnStatChanged;

    public void Initialize(int startCourage, int startWisdom, int startLuck)
    {
        courage = Mathf.Clamp(startCourage, 0, 9);
        wisdom = Mathf.Clamp(startWisdom, 0, 9);
        luck = Mathf.Clamp(startLuck, 0, 9);
        
        OnStatChanged?.Invoke();
    }
    
    public void ModifyStat(StatType type, int amount)
    {
        bool hasEternalSmile = false;
        if (RelicManager.Inst != null && RelicManager.Inst.relicSO != null)
        {
            hasEternalSmile = RelicManager.Inst.relicSO.relicItems.Exists(r => 
                (r.relicName == "영원한 웃음" || (r.enhancedRelicItem != null && r.enhancedRelicItem.relicName == "영원한 웃음")));
        }
        
        switch (type)
        {
            case StatType.Courage:
                courage = Mathf.Clamp(courage + amount, 0, 9);
                break;
            case StatType.Wisdom:
                if (hasEternalSmile)
                {
                    wisdom = 1;
                    Debug.Log("[Stat] 영원한 웃음 효과로 인해 지혜가 1로 고정됩니다.");
                }
                else
                {
                    wisdom = Mathf.Clamp(wisdom + amount, 0, 9);
                }
                break;
            case StatType.Luck:
                luck = Mathf.Clamp(luck + amount, 0, 9);
                break;
        }
        Debug.Log($"[Stat] {type} changed by {amount}. Current: {GetStat(type)}");
        OnStatChanged?.Invoke();
    }

    public int GetStat(StatType type)
    {
        switch (type)
        {
            case StatType.Courage: return courage;
            case StatType.Wisdom: return wisdom;
            case StatType.Luck: return luck;
            default: return 0;
        }
    }

    public bool IsAutoFail(StatType type) => GetStat(type) == 0;
    public int GetBattleEndHealAmount() => (courage >= 4 && courage <= 6) ? 3 : 0;
    public int GetStartBattleShield() => (wisdom >= 4 && wisdom <= 6) ? 5 : 0;
    public float GetRareCardChanceMultiplier() => (luck >= 4 && luck <= 6) ? 2.0f : 1.0f;
    public float GetMerchantRareChanceMultiplier() => (luck >= 7 && luck <= 8) ? 2.0f : 1.0f;
    public int GetExtraActionPoints() => (courage >= 9) ? 1 : 0;
    public int GetExtraDrawCount() => (wisdom >= 9) ? 1 : 0;
    
    public void OnEnterDangerousBattle()
    {
        if (courage >= 7 && courage <= 8)
        {
            //dreamFragments += 1;
            Debug.Log("용기 보너스: 꿈 파편 1 획득");
        }
    }
    public void OnRestRemoveCard()
    {
        if (wisdom >= 7 && wisdom <= 8)
        {
            Debug.Log("지혜 보너스: 꿈 파편 1 획득");
        }
    }
    
    public void EvaluateBattleResult(int turnsTaken, bool tookDamage)
    {
        if (turnsTaken <= 4)
        {
            ModifyStat(StatType.Courage, 1);
            Debug.Log("조건 달성: 용기 증가 (4턴 이내 승리)");
        }

        if (!tookDamage)
        {
            ModifyStat(StatType.Wisdom, 1);
            Debug.Log("조건 달성: 지혜 증가 (노 히트 클리어)");
        }
    }

    public void EvaluateRouletteResult(RouletteResultType result)
    {
        if (result == RouletteResultType.GreatSuccess)
        {
            ModifyStat(StatType.Luck, 1);
            Debug.Log("조건 달성: 행운 증가 (대성공)");
        }
        else if (result == RouletteResultType.Fail)
        {
            if (Random.value < 0.5f)
            {
                ModifyStat(StatType.Luck, -1);
                Debug.Log("조건 발동: 행운 감소 (실패 패널티)");
            }
        }
    }
}