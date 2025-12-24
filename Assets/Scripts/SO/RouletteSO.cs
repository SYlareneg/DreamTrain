using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ERouletteType { None, Attack, Heal, Shield, Enemy_Special, Player_Special };

[System.Serializable]
public struct RouletteType
{
    public ERouletteType type;
    public int specialTypeIdx;

    public RouletteType(ERouletteType type, int specialTypeIdx)
    {
        this.type = type;
        this.specialTypeIdx = specialTypeIdx;
    }

    public RouletteType(ERouletteType type)
    {
        this.type = type;
        this.specialTypeIdx = -1;
    }

    public static bool operator ==(RouletteType a, RouletteType b)
    {
        return a.type == b.type && a.specialTypeIdx == b.specialTypeIdx;
    }

    public static bool operator !=(RouletteType a, RouletteType b)
    {
        return !(a == b);
    }

    public bool Equals(RouletteType other)
    {
        return type == other.type && specialTypeIdx == other.specialTypeIdx;
    }

    public override bool Equals(object obj)
    {
        return obj is RouletteType && this.Equals(obj);
    }

    public override int GetHashCode()
    {
        return (type, specialTypeIdx).GetHashCode();
    }
}

[System.Serializable]
public class SpecialRoulette
{
    public Sprite sprite;
    public string title;
    public string text;
    public int baseVal;

    public SpecialRoulette(Sprite sprite, string title, string text, int baseVal)
    {
        this.sprite = sprite;
        this.title = title;
        this.text = text;
        this.baseVal = baseVal;
    }

    public SpecialRoulette(SpecialRoulette esr)
    {
        if(esr == null) return;
        this.sprite = esr.sprite;
        this.title = esr.title;
        this.text = esr.text;
        this.baseVal = esr.baseVal;
    }
}

[System.Serializable]
public class RouletteItem
{
    [Tooltip("칸 종류")] public RouletteType rtype;
    [Tooltip("칸 계수")] public int value;
}

[CreateAssetMenu(fileName = "RouletteSO", menuName = "Scriptable Objects/RouletteSO")]
public class RouletteSO : ScriptableObject
{
    public RouletteItem[] roulettePattern;
}
