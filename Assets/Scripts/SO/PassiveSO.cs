using UnityEngine;
using System.Collections.Generic;

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

    public Passive() { }

    public Passive(Passive p)
    {
        type = p.type;
        sprite = p.sprite;
        name = p.name;
        text = p.text;
        dreamPieceNum = p.dreamPieceNum;
    }

    public Passive(Passive_Data pd)
    {
        type = pd.type;
        sprite = Utils.LoadSpriteByName("Passives", pd.sprite);
        name = pd.name;
        text = pd.text;
        dreamPieceNum = pd.dreamPieceNum;
    }
}

[System.Serializable]
public class Passive_Data
{
    public EPassiveType type;
    public int dreamPieceNum;
    public string sprite;
    public string name;
    public string text;
    public string name_enhanced;
    public string text_enhanced;

    public Passive_Data() { }

    public Passive_Data(Passive_Data pd)
    {
        type = pd.type;
        dreamPieceNum = pd.dreamPieceNum;
        sprite = pd.sprite;
        name = pd.name;
        text = pd.text;
        name_enhanced = pd.name_enhanced;
        text_enhanced = pd.text_enhanced;
    }

    public Passive_Data(Passive_Enhanceable p)
    {
        type = p.type;
        dreamPieceNum = p.dreamPieceNum;
        sprite = p.sprite != null ? p.sprite.name : "";
        name = p.name;
        text = p.text;
        name_enhanced = p.enhancedPassive != null ? p.enhancedPassive.name : "";
        text_enhanced = p.enhancedPassive != null ? p.enhancedPassive.text : "";
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
        enhancedPassive = new Passive(p.enhancedPassive);
    }

    public Passive_Enhanceable() { }

    public Passive_Enhanceable(Passive_Enhanceable pe) : base(pe)
    {
        isEnhanced = pe.isEnhanced;
        enhancedPassive = new Passive(pe.enhancedPassive);
    }

    public Passive_Enhanceable(Passive_Data pd) : base(pd)
    {
        isEnhanced = false;
        enhancedPassive = new Passive();
        if(pd != null)
        {
            enhancedPassive.type = pd.type;
            enhancedPassive.sprite = Utils.LoadSpriteByName("Passives", pd.sprite);
            enhancedPassive.name = pd.name_enhanced;
            enhancedPassive.text = pd.text_enhanced;
            enhancedPassive.dreamPieceNum = pd.dreamPieceNum;
        }
    }
}

[CreateAssetMenu(fileName = "PassiveSO", menuName = "Scriptable Objects/PassiveSO")]
public class PassiveSO : ScriptableObject
{
    public Passive[] passives;
}
