using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DreamPiece_Base
{
    public string name;
    public Passive_Enhanceable persona;
    public Passive_Enhanceable shadow;
    public Sprite triggerSprite;
    public static int playerSpecialRouletteNum = 3;
    public SpecialRoulette[] playerSpecialRoulettes = new SpecialRoulette[playerSpecialRouletteNum];

    public void Setup(DreamPiece_Base dp)
    {
        if(dp == null) return;
        name = dp.name;
        persona = new Passive_Enhanceable();
        persona.Setup(dp.persona);
        shadow = new Passive_Enhanceable();
        shadow.Setup(dp.shadow);
        triggerSprite = dp.triggerSprite;
        playerSpecialRoulettes = new SpecialRoulette[playerSpecialRouletteNum];
        for(int i = 0; i < dp.playerSpecialRoulettes.Length; i++)
        {
            playerSpecialRoulettes[i] = new SpecialRoulette(dp.playerSpecialRoulettes[i]);
        }
    }
}

[System.Serializable]
public class DreamPiece_Data : DreamPiece_Base
{
    public List<string> cards;
    public List<Item_Data> baseCards_persona;
    public List<Item_Data> baseCards_shadow;

    public void Setup(DreamPiece_Data dp)
    {
        base.Setup(dp);
        cards = new List<string>(dp.cards);
        baseCards_persona = new List<Item_Data>(dp.baseCards_persona);
        baseCards_shadow = new List<Item_Data>(dp.baseCards_shadow);
    }
}

[System.Serializable]
public class DreamPiece_Reference : DreamPiece_Base
{
    public List<Item_Enhanceable> cards;
    public List<Item_Enhanceable> baseCards_persona;
    public List<Item_Enhanceable> baseCards_shadow;

    public void Setup(DreamPiece_Data dp, ItemDataSO cardList)
    {
        base.Setup(dp);
        cards = new List<Item_Enhanceable>();
        foreach(string cardName in dp.cards)
        {
            Item_Enhanceable item = new Item_Enhanceable(cardList.items.Find(x => x.name == cardName));
            if(item != null)
            {
                item.num = 1;
                cards.Add(item);
            }

        }
        baseCards_persona = new List<Item_Enhanceable>();
        baseCards_shadow = new List<Item_Enhanceable>();
        foreach(Item_Data cardData in dp.baseCards_persona)
        {
            Item_Enhanceable item = new Item_Enhanceable(this.cards.Find(x => x.name == cardData.cardName));
            if(item != null)
            {
                item.num = cardData.num;
                baseCards_persona.Add(item);
            }
        }
        foreach(Item_Data cardData in dp.baseCards_shadow)
        {
            Item_Enhanceable item = new Item_Enhanceable(this.cards.Find(x => x.name == cardData.cardName));
            if(item != null)
            {
                item.num = cardData.num;
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
