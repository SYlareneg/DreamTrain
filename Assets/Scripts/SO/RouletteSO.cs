using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ERouletteType { None, Attack, Heal, Shield, Enemy_Special, Player_Special };

[System.Serializable]
public struct RouletteType
{
    public ERouletteType type;
    public int specialTypeIdx;
    public int enemyIdx;

    public RouletteType(ERouletteType type, int specialTypeIdx, int enemyIdx)
    {
        this.type = type;
        this.specialTypeIdx = specialTypeIdx;
        this.enemyIdx = enemyIdx;
    }

    public RouletteType(ERouletteType type, int specialTypeIdx)
    {
        this.type = type;
        this.specialTypeIdx = specialTypeIdx;
        this.enemyIdx = 0;
    }

    public RouletteType(ERouletteType type)
    {
        this.type = type;
        this.specialTypeIdx = -1;
        this.enemyIdx = 0;
    }

    public static bool operator ==(RouletteType a, RouletteType b)
    {
        return a.type == b.type && a.specialTypeIdx == b.specialTypeIdx && a.enemyIdx == b.enemyIdx;
    }

    public static bool operator !=(RouletteType a, RouletteType b)
    {
        return !(a == b);
    }

    public bool Equals(RouletteType other)
    {
        return type == other.type && specialTypeIdx == other.specialTypeIdx && enemyIdx == other.enemyIdx;
    }

    public override bool Equals(object obj)
    {
        return obj is RouletteType && this.Equals(obj);
    }

    public override int GetHashCode()
    {
        return (type, specialTypeIdx, enemyIdx).GetHashCode();
    }
}

[System.Serializable]
public class SpecialRoulette
{
    public Sprite sprite;
    public string title;
    public string text;
    public int baseVal;
    public string title_enhanced;
    public string text_enhanced;
    public int baseVal_enhanced;

    public SpecialRoulette(Sprite sprite, string title, string text, int baseVal, int baseVal_enhanced, string title_enhanced, string text_enhanced)
    {
        this.sprite = sprite;
        this.title = title;
        this.text = text;
        this.baseVal = baseVal;
        this.baseVal_enhanced = baseVal_enhanced;
        this.title_enhanced = title_enhanced;
        this.text_enhanced = text_enhanced;
    }

    public SpecialRoulette(SpecialRoulette esr)
    {
        if(esr == null) return;
        this.sprite = esr.sprite;
        this.title = esr.title;
        this.text = esr.text;
        this.baseVal = esr.baseVal;
        this.baseVal_enhanced = esr.baseVal_enhanced;
        this.title_enhanced = esr.title_enhanced;
        this.text_enhanced = esr.text_enhanced;
    }

    public SpecialRoulette(SpecialRoulette_Data esr)
    {
        if(esr == null) return;
        this.sprite = Utils.LoadSpriteByName("SpecialRoulette", esr.sprite);
        this.title = esr.title;
        this.text = esr.text;
        this.baseVal = esr.baseVal;
        this.baseVal_enhanced = esr.baseVal_enhanced;
        this.title_enhanced = esr.title_enhanced;
        this.text_enhanced = esr.text_enhanced;
    }
}

[System.Serializable]
public class SpecialRoulette_Data
{
    public string sprite;
    public string title;
    public string text;
    public int baseVal;
    public string title_enhanced;
    public string text_enhanced;
    public int baseVal_enhanced;

    public SpecialRoulette_Data(string sprite, string title, string text, int baseVal, string title_enhanced, string text_enhanced, int baseVal_enhanced)
    {
        this.sprite = sprite;
        this.title = title;
        this.text = text;
        this.baseVal = baseVal;
        this.title_enhanced = title_enhanced;
        this.text_enhanced = text_enhanced;
        this.baseVal_enhanced = baseVal_enhanced;
    }

    public SpecialRoulette_Data(SpecialRoulette_Data esr)
    {
        if(esr == null) return;
        this.sprite = esr.sprite;
        this.title = esr.title;
        this.text = esr.text;
        this.baseVal = esr.baseVal;
        this.title_enhanced = esr.title_enhanced;
        this.text_enhanced = esr.text_enhanced;
        this.baseVal_enhanced = esr.baseVal_enhanced;
    }

    public SpecialRoulette_Data(SpecialRoulette esr)
    {
        if(esr == null) return;
        this.sprite = esr.sprite.name;
        this.title = esr.title;
        this.text = esr.text;
        this.baseVal = esr.baseVal;
        this.title_enhanced = esr.title_enhanced;
        this.text_enhanced = esr.text_enhanced;
        this.baseVal_enhanced = esr.baseVal_enhanced;
    }
}

[System.Serializable]
public class RouletteItem
{
    [Tooltip("칸 종류")] public RouletteType rtype;
    [Tooltip("칸 계수")] public int value;

    public RouletteItem(RouletteItem rouletteItem)
    {
        rtype = rouletteItem.rtype;
        value = rouletteItem.value;
    }

    public RouletteItem()
    {
        rtype = new RouletteType(ERouletteType.None);
        value = 0;
    }

    public RouletteItem(RouletteType rtype, int value)
    {
        this.rtype = rtype;
        this.value = value;
    }
}

[CreateAssetMenu(fileName = "RouletteSO", menuName = "Scriptable Objects/RouletteSO")]
public class RouletteSO : ScriptableObject
{
    public RouletteItem[] roulettePattern;
}
