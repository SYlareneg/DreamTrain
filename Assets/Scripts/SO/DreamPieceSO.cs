using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DreamPiece_Base
{
    public string name;
    public string description;
    public int courageStat;
    public int wisdomStat;
    public int luckStat;
    public Passive_Enhanceable persona;
    public Passive_Enhanceable shadow;
    public Sprite triggerSprite;
    public List<Sprite> cardBackgrounds;
    public Color[] textColors = new Color[3]; 
    public static int playerSpecialRouletteNum = 3;
    public SpecialRoulette[] playerSpecialRoulettes = new SpecialRoulette[playerSpecialRouletteNum];

    public void Setup(DreamPiece_Base dp)
    {
        if(dp == null) return;
        name = dp.name;
        description = dp.description;
        courageStat = dp.courageStat;
        wisdomStat = dp.wisdomStat;
        luckStat = dp.luckStat;
        persona = new Passive_Enhanceable(dp.persona);
        shadow = new Passive_Enhanceable(dp.shadow);
        triggerSprite = dp.triggerSprite;
        cardBackgrounds = new List<Sprite>(dp.cardBackgrounds);
        textColors = new Color[dp.textColors.Length];
        for(int i = 0; i < dp.textColors.Length; i++)
        {
            textColors[i] = dp.textColors[i];
        }
        playerSpecialRoulettes = new SpecialRoulette[playerSpecialRouletteNum];
        for(int i = 0; i < dp.playerSpecialRoulettes.Length; i++)
        {
            playerSpecialRoulettes[i] = new SpecialRoulette(dp.playerSpecialRoulettes[i]);
        }
    }
}

[System.Serializable]
public class DreamPiece_Data
{
    public string name;
    public string description;
    public int courageStat;
    public int wisdomStat;
    public int luckStat;
    public Passive_Data persona;
    public Passive_Data shadow;
    public string triggerSprite;
    public List<string> cardBackgrounds;
    public Color[] textColors = new Color[3];
    public static int playerSpecialRouletteNum = 3;
    public SpecialRoulette_Data[] playerSpecialRoulettes = new SpecialRoulette_Data[playerSpecialRouletteNum];
    public List<string> cards;
    public List<Item_Num> baseCards_persona;
    public List<Item_Num> baseCards_shadow;

    public DreamPiece_Data()
    {
        cards = new List<string>();
        baseCards_persona = new List<Item_Num>();
        baseCards_shadow = new List<Item_Num>();
    }

    public DreamPiece_Data(DreamPiece_Data dp)
    {
        name = dp.name;
        description = dp.description;
        courageStat = dp.courageStat;
        wisdomStat = dp.wisdomStat;
        luckStat = dp.luckStat;
        persona = dp.persona;
        shadow = dp.shadow;
        triggerSprite = dp.triggerSprite;
        cardBackgrounds = new List<string>(dp.cardBackgrounds);
        textColors = new Color[dp.textColors.Length];
        for(int i = 0; i < dp.textColors.Length; i++)
        {
            textColors[i] = dp.textColors[i];
        }
        playerSpecialRoulettes = new SpecialRoulette_Data[playerSpecialRouletteNum];
        for(int i = 0; i < dp.playerSpecialRoulettes.Length; i++)
        {
            playerSpecialRoulettes[i] = new SpecialRoulette_Data(dp.playerSpecialRoulettes[i]);
        }
        cards = new List<string>(dp.cards);
        baseCards_persona = new List<Item_Num>(dp.baseCards_persona);
        baseCards_shadow = new List<Item_Num>(dp.baseCards_shadow);
    }

    public DreamPiece_Data(DreamPiece_Reference dp)
    {
        name = dp.name;
        description = dp.description;
        courageStat = dp.courageStat;
        wisdomStat = dp.wisdomStat;
        luckStat = dp.luckStat;
        persona = new Passive_Data(dp.persona);
        shadow = new Passive_Data(dp.shadow);
        triggerSprite = dp.triggerSprite != null ? dp.triggerSprite.name : "";
        cardBackgrounds = new List<string>();
        foreach(Sprite bg in dp.cardBackgrounds)
        {
            cardBackgrounds.Add(bg == null ? "" : bg.name);
        }
        textColors = new Color[dp.textColors.Length];
        for(int i = 0; i < dp.textColors.Length; i++)
        {
            textColors[i] = dp.textColors[i];
        }
        playerSpecialRoulettes = new SpecialRoulette_Data[playerSpecialRouletteNum];
        for(int i = 0; i < dp.playerSpecialRoulettes.Length; i++)
        {
            playerSpecialRoulettes[i] = new SpecialRoulette_Data(dp.playerSpecialRoulettes[i]);
        }
        cards = new List<string>();
        foreach(Item_Enhanceable item in dp.cards)
        {
            cards.Add(item.name);
        }
        baseCards_persona = new List<Item_Num>();
        foreach(Item_Enhanceable item in dp.baseCards_persona)
        {
            baseCards_persona.Add(new Item_Num(item.name, item.num));
        }
        baseCards_shadow = new List<Item_Num>();
        foreach(Item_Enhanceable item in dp.baseCards_shadow)
        {
            baseCards_shadow.Add(new Item_Num(item.name, item.num));
        }
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
        if(dp == null) return;
        name = dp.name;
        description = dp.description;
        courageStat = dp.courageStat;
        wisdomStat = dp.wisdomStat;
        luckStat = dp.luckStat;
        persona = new Passive_Enhanceable(dp.persona);
        shadow = new Passive_Enhanceable(dp.shadow);
        triggerSprite = Utils.LoadSpriteByName("TriggerRoulette", dp.triggerSprite);
        cardBackgrounds = new List<Sprite>();
        foreach(string bgName in dp.cardBackgrounds)
        {
            Sprite bgSprite = Utils.LoadSpriteByName("CardBackgrounds", bgName);
            if(bgSprite != null)
            {
                cardBackgrounds.Add(bgSprite);
            }
        }
        textColors = new Color[dp.textColors.Length];
        for(int i = 0; i < dp.textColors.Length; i++)
        {
            textColors[i] = dp.textColors[i];
        }
        playerSpecialRoulettes = new SpecialRoulette[playerSpecialRouletteNum];
        for(int i = 0; i < dp.playerSpecialRoulettes.Length; i++)
        {
            playerSpecialRoulettes[i] = new SpecialRoulette(dp.playerSpecialRoulettes[i]);
        }
        cards = new List<Item_Enhanceable>();
        foreach(string cardName in dp.cards)
        {
            Item_Data item_Data = cardList.items.Find(x => x.name == cardName);
            if(item_Data != null)
            {
                Item_Enhanceable item = new Item_Enhanceable(item_Data);
                item.num = 1;
                cards.Add(item);
            }

        }
        baseCards_persona = new List<Item_Enhanceable>();
        baseCards_shadow = new List<Item_Enhanceable>();
        foreach(Item_Num cardData in dp.baseCards_persona)
        {
            Item_Enhanceable item = this.cards.Find(x => x.name == cardData.cardName);
            if(item != null)
            {
                Item_Enhanceable newItem = new Item_Enhanceable(item);
                newItem.num = cardData.num;
                baseCards_persona.Add(newItem);
            }
        }
        foreach(Item_Num cardData in dp.baseCards_shadow)
        {
            Item_Enhanceable item = this.cards.Find(x => x.name == cardData.cardName);
            if(item != null)
            {
                Item_Enhanceable newItem = new Item_Enhanceable(item);
                newItem.num = cardData.num;
                baseCards_shadow.Add(newItem);
            }
        }
    }
}

[System.Serializable]
public class DreamPiece_Player : DreamPiece_Base
{
    public List<Item> cards;

    public DreamPiece_Player() { }

    public DreamPiece_Player(DreamPiece_Player dp)
    {
        Setup(dp);
    }

    public DreamPiece_Player(DreamPiece_Reference dp)
    {
        base.Setup(dp);
        cards = new List<Item>();
    }

    public DreamPiece_Player(string dpName, bool personaEnhanced, bool shadowEnhanced, List<Item_Num> cardNameNums, DreamPieceDataSO dpDataSO, ItemDataSO itemDataSO)
    {
        this.persona = new Passive_Enhanceable();
        this.shadow = new Passive_Enhanceable();
        this.cardBackgrounds = new List<Sprite>();
        this.textColors = new Color[3];
        this.playerSpecialRoulettes = new SpecialRoulette[playerSpecialRouletteNum];
        this.cards = new List<Item>();
        DreamPiece_Data dreamPiece_Data = dpDataSO.dreamPieces.Find(x => x.name == dpName);
        if (dreamPiece_Data == null) return;
        this.name = dreamPiece_Data.name;
        this.description = dreamPiece_Data.description;
        this.courageStat = dreamPiece_Data.courageStat;
        this.wisdomStat = dreamPiece_Data.wisdomStat;
        this.luckStat = dreamPiece_Data.luckStat;
        this.persona = new Passive_Enhanceable(dreamPiece_Data.persona);
        this.persona.isEnhanced = personaEnhanced;
        this.shadow = new Passive_Enhanceable(dreamPiece_Data.shadow);
        this.shadow.isEnhanced = shadowEnhanced;
        this.triggerSprite = Utils.LoadSpriteByName("TriggerRoulette", dreamPiece_Data.triggerSprite);
        this.cardBackgrounds = new List<Sprite>();
        foreach(string bgName in dreamPiece_Data.cardBackgrounds)
        {
            Sprite bgSprite = Utils.LoadSpriteByName("CardBackgrounds", bgName);
            if(bgSprite != null)
            {
                this.cardBackgrounds.Add(bgSprite);
            }
        }
        this.textColors = new Color[dreamPiece_Data.textColors.Length];
        for(int i = 0; i < dreamPiece_Data.textColors.Length; i++)
        {
            this.textColors[i] = dreamPiece_Data.textColors[i];
        }
        this.playerSpecialRoulettes = new SpecialRoulette[playerSpecialRouletteNum];
        for (int i = 0; i < dreamPiece_Data.playerSpecialRoulettes.Length; i++)
        {
            this.playerSpecialRoulettes[i] = new SpecialRoulette(dreamPiece_Data.playerSpecialRoulettes[i]);
        }
        this.cards = new List<Item>();
        foreach (Item_Num card in cardNameNums)
        {
            Item_Data item_Data = itemDataSO.items.Find(x => x.name == card.cardName || x.name_enhanced == card.cardName);
            if (item_Data != null)
            {
                Item item = new Item(item_Data, item_Data.name_enhanced == card.cardName);
                item.num = card.num;
                this.cards.Add(item);
            }
        }
    }

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

