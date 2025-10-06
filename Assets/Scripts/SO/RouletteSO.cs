using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ERouletteType { None, Attack, Heal, Shield, Charge, Lifesteal, MagicBox };

[System.Serializable]
public class RouletteItem
{
    [Tooltip("칸 종류")] public ERouletteType type;
    [Tooltip("칸 계수")] public int value;
}

[CreateAssetMenu(fileName = "RouletteSO", menuName = "Scriptable Objects/RouletteSO")]
public class RouletteSO : ScriptableObject
{
    public RouletteItem[] roulettePattern;
}
