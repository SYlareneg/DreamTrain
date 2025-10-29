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

[CreateAssetMenu(fileName = "PassiveSO", menuName = "Scriptable Objects/PassiveSO")]
public class PassiveSO : ScriptableObject
{
    public Passive[] passives;
}
