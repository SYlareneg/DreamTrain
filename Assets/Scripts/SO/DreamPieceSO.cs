using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DreamPiece_Base
{
    public string name;
    public Passive_Enhanceable persona;
    public Passive_Enhanceable shadow;
    public Sprite specialRouletteSprite;
    public string specialRouletteTitle;
    public string specialRouletteText;

    public void Setup(DreamPiece_Base dp)
    {
        name = dp.name;
        persona = new Passive_Enhanceable();
        persona.Setup(dp.persona);
        shadow = new Passive_Enhanceable();
        shadow.Setup(dp.shadow);
        specialRouletteSprite = dp.specialRouletteSprite;
        specialRouletteTitle = dp.specialRouletteTitle;
        specialRouletteText = dp.specialRouletteText;
    }
}

[System.Serializable]
public class DreamPiece_Data : DreamPiece_Base
{
    public List<Item_Enhanceable> cards;
    public List<string> baseCards_persona;
    public List<string> baseCards_shadow;

    public void Setup(DreamPiece_Data dp)
    {
        base.Setup(dp);
        cards = new List<Item_Enhanceable>(dp.cards);
        baseCards_persona = new List<string>(dp.baseCards_persona);
        baseCards_shadow = new List<string>(dp.baseCards_shadow);
    }
}

[System.Serializable]
public class DreamPiece_Reference : DreamPiece_Base
{
    public List<Item_Enhanceable> cards;
    public List<Item_Enhanceable> baseCards_persona;
    public List<Item_Enhanceable> baseCards_shadow;

    public void Setup(DreamPiece_Data dp)
    {
        base.Setup(dp);
        cards = new List<Item_Enhanceable>(dp.cards);
        baseCards_persona = new List<Item_Enhanceable>();
        baseCards_shadow = new List<Item_Enhanceable>();
        foreach(string cardName in dp.baseCards_persona)
        {
            Item_Enhanceable item = this.cards.Find(x => x.name == cardName);
            Debug.Log(item.name);
            if(item != null)
            {
                item.num = 1;
                baseCards_persona.Add(item);
            }
        }
        foreach(string cardName in dp.baseCards_shadow)
        {
            Item_Enhanceable item = this.cards.Find(x => x.name == cardName);
            Debug.Log(item.name);
            if(item != null)
            {
                item.num = 1;
                baseCards_shadow.Add(item);
            }
        }
    }
}

[System.Serializable]
public class DreamPiece_Player : DreamPiece_Base
{
    public List<Item> cards;

    public void Setup(DreamPiece_Player dp)
    {
        base.Setup(dp);
        cards = new List<Item>(dp.cards);
    }
}

[CreateAssetMenu(fileName = "DreamPieceSO", menuName = "Scriptable Objects/DreamPieceSO")]
public class DreamPieceSO : ScriptableObject
{
    public List<DreamPiece_Reference> dreamPieces = new List<DreamPiece_Reference>();
}

[CreateAssetMenu(fileName = "DreamPieceDataSO", menuName = "Scriptable Objects/DreamPieceDataSO")]
public class DreamPieceDataSO : ScriptableObject
{
    public List<DreamPiece_Data> dreamPieces = new List<DreamPiece_Data>();
}
