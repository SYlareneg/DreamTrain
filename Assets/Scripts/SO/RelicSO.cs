using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RelicItem
{
    public int relicOwner;
    public Sprite relicSprite;
    public string relicName;
    public string relicTxt;
    public List<int> relicVal;
    public bool isEnhanced;
    public int relicAct;
    public CardRarity rarity; 
    public int cost = 100;
    public int sellCost = 50;

    public RelicItem() { }
    public RelicItem(RelicItem relicItem)
    {
        relicOwner = relicItem.relicOwner;
        relicSprite = relicItem.relicSprite;
        relicName = relicItem.relicName;
        relicTxt = relicItem.relicTxt;
        relicVal = new List<int>(relicItem.relicVal);
        isEnhanced = relicItem.isEnhanced;
        relicAct = relicItem.relicAct;
        rarity = relicItem.rarity;
        cost = relicItem.cost;
        sellCost = relicItem.sellCost;
    }
}

[System.Serializable]
public class RelicItem_Enhanceable : RelicItem
{
    public RelicItem enhancedRelicItem;

    public RelicItem_Enhanceable(RelicItem_Enhanceable relicItem_Enhanceable) : base((RelicItem)relicItem_Enhanceable)
    {
        enhancedRelicItem = new RelicItem(relicItem_Enhanceable.enhancedRelicItem);
    }

    public RelicItem_Enhanceable(RelicItem_Data relicItem_Data) 
    {
        relicOwner = relicItem_Data.relicOwner;
        relicSprite = Utils.LoadSpriteByName("Relics", relicItem_Data.relicSprite);
        relicName = relicItem_Data.relicName;
        relicTxt = relicItem_Data.relicTxt;
        relicVal = new List<int>(relicItem_Data.relicVal);
        relicAct = relicItem_Data.relicAct;
        rarity = relicItem_Data.rarity;
        cost = relicItem_Data.cost;
        sellCost = relicItem_Data.sellCost;
        
        isEnhanced = false;
        enhancedRelicItem = new RelicItem();
        enhancedRelicItem.relicOwner = relicItem_Data.relicOwner;
        enhancedRelicItem.relicSprite = relicSprite;
        enhancedRelicItem.relicName = relicItem_Data.relicName_enhanced;
        enhancedRelicItem.relicTxt = relicItem_Data.relicTxt_enhanced;
        enhancedRelicItem.relicVal = new List<int>(relicItem_Data.relicVal_enhanced);
        enhancedRelicItem.isEnhanced = true;
        
    }
}

[System.Serializable]
public class RelicItem_Data
{
    public int relicOwner;
    public string relicSprite;
    public string relicName;
    public string relicTxt;
    public List<int> relicVal;
    public int relicAct;
    public CardRarity rarity; 
    public int cost = 100;
    public int sellCost = 50;
    public string relicName_enhanced;
    public string relicTxt_enhanced;
    public List<int> relicVal_enhanced;

    public RelicItem_Data(RelicItem_Data relicItem_Data)
    {
        if(relicItem_Data == null) return;
        relicOwner = relicItem_Data.relicOwner;
        relicSprite = relicItem_Data.relicSprite;
        relicName = relicItem_Data.relicName;
        relicTxt = relicItem_Data.relicTxt;
        relicVal = new List<int>(relicItem_Data.relicVal);
        relicAct = relicItem_Data.relicAct;
        rarity = relicItem_Data.rarity;
        cost = relicItem_Data.cost;
        sellCost = relicItem_Data.sellCost;
        relicName_enhanced = relicItem_Data.relicName_enhanced;
        relicTxt_enhanced = relicItem_Data.relicTxt_enhanced;
        relicVal_enhanced = new List<int>(relicItem_Data.relicVal_enhanced);
    }

    public RelicItem_Data(RelicItem_Enhanceable relicItem)
    {
        if(relicItem == null) return;
        relicOwner = relicItem.relicOwner;
        relicSprite = (relicItem.relicSprite != null)? relicItem.relicSprite.name : "";
        relicName = relicItem.relicName;
        relicTxt = relicItem.relicTxt;
        relicVal = new List<int>(relicItem.relicVal);
        relicAct = relicItem.relicAct;
        rarity = relicItem.rarity;
        cost = relicItem.cost;
        sellCost = relicItem.sellCost;
        relicName_enhanced = relicItem.enhancedRelicItem.relicName;
        relicTxt_enhanced = relicItem.enhancedRelicItem.relicTxt;
        relicVal_enhanced = new List<int>(relicItem.enhancedRelicItem.relicVal);
    }
}

[CreateAssetMenu(fileName = "RelicSO", menuName = "Scriptable Objects/RelicSO")]
public class RelicSO : ScriptableObject
{
    public List<RelicItem_Enhanceable> relicItems;
}
