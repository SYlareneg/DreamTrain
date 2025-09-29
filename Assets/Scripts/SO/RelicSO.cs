using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ERelicActivateType { TurnBegin, TurnEnd };
public enum ERelicAffectItem { Health, Cost, Draw };

[System.Serializable]
public class RelicItem
{
    public string relicName;
    public Sprite relicSprite;
    public ERelicActivateType type;
    public ERelicAffectItem affectItem;
    public int affectValue;
    public string relicTxt;
}

[CreateAssetMenu(fileName = "RelicSO", menuName = "Scriptable Objects/RelicSO")]
public class RelicSO : ScriptableObject
{
    public RelicItem[] relicItems;
}
