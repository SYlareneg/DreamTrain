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

    public void Setup(Passive p)
    {
        type = p.type;
        sprite = p.sprite;
        name = p.name;
        text = p.text;
        dreamPieceNum = p.dreamPieceNum;
    }
}

[System.Serializable]
public class Passive_Enhanceable : Passive
{
    public bool isEnhanced;
    public Passive enhancedPassive;

    public void Setup(Passive_Enhanceable p)
    {
        base.Setup(p);
        isEnhanced = p.isEnhanced;
        enhancedPassive = new Passive();
        enhancedPassive.Setup(p.enhancedPassive);
    }
}

[CreateAssetMenu(fileName = "PassiveSO", menuName = "Scriptable Objects/PassiveSO")]
public class PassiveSO : ScriptableObject
{
    public Passive[] passives;
}
