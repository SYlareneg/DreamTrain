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
    [System.NonSerialized] public int courage;
    [System.NonSerialized] public int wisdom;
    [System.NonSerialized] public int luck;

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
        switch (type)
        {
            case StatType.Courage:
                courage = Mathf.Clamp(courage + amount, 0, 9);
                break;
            case StatType.Wisdom:
                wisdom = Mathf.Clamp(wisdom + amount, 0, 9);
                break;
            case StatType.Luck:
                luck = Mathf.Clamp(luck + amount, 0, 9);
                break;
        }
        
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
}