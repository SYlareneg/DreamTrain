using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShowBuff_Data
{
    public string name;
    public string text;
    public string icon;
    public EBuffType type; // value, duration, count
    public EBuffEffectType effectType; // beneficial, harmful, neutral
    public List<int> defaultVal;

    public ShowBuff_Data(ShowBuff showBuff)
    {
        if(showBuff == null) return;
        name = showBuff.name;
        text = showBuff.text;
        icon = showBuff.icon.name;
        type = showBuff.type;
        effectType = showBuff.effectType;
        defaultVal = new List<int>(showBuff.defaultVal);
    }
}

[CreateAssetMenu(fileName = "ShowBuffSO", menuName = "Scriptable Objects/ShowBuffSO")]
public class ShowBuffSO : ScriptableObject
{
    public List<ShowBuff> showBuffs;
}
