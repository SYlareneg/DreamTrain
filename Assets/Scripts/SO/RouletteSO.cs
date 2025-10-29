using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ERouletteType { None, Attack, Heal, Shield, Enemy_Special_1, Enemy_Special_2, Player_Special_1, Player_Special_2 };

[System.Serializable]
public class RouletteItem
{
    [Tooltip("칸 종류")] public ERouletteType type;
    [Tooltip("칸 계수")] public List<int> value = new List<int>();
}

[CreateAssetMenu(fileName = "RouletteSO", menuName = "Scriptable Objects/RouletteSO")]
public class RouletteSO : ScriptableObject
{
    public RouletteItem[] roulettePattern;
}
