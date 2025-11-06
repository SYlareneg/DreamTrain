using UnityEngine;

public enum EPassiveType { Normal, Persona, Shadow };

[System.Serializable]
public class Passive
{
    public EPassiveType type;
    public Sprite sprite;
    public string name;
    public string text;
    public int dreamPieceNum;
}

[System.Serializable]
public class Passive_Enhanceable : Passive
{
    public bool isEnhanced;
    public Passive enhancedPassive;
}

[CreateAssetMenu(fileName = "PassiveSO", menuName = "Scriptable Objects/PassiveSO")]
public class PassiveSO : ScriptableObject
{
    public Passive[] passives;
}
