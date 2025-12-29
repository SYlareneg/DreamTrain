using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType { Turn, Enchant, Skill, Dream };
public enum CardRarity { Normal, Rare };
public enum ECardValueType { Damage, Heal, Shield, Special, Default };

[System.Serializable]
public class Item
{
    [Tooltip("카드 제목")] public string name;
    [Tooltip("카드 코스트")] public int cost;
    [Tooltip("카드 타입")] public CardType type;
    [Tooltip("카드 희귀도")] public CardRarity rarity;
    [Tooltip("카드가 속한 꿈 조각 번호")] public int dreamPieceNum;
    [Tooltip("휘발성")] public bool isVolatile;
    [Tooltip("소멸")] public bool isVanish;
    [Tooltip("잔류")] public bool isRemain;
    [Tooltip("단일기")] public bool isSingleTarget;
    [Tooltip("카드 이미지")] public Sprite sprite;
    [Tooltip("카드 설명")] public string text;
    [Tooltip("카드 계수")] public List<int> cardValues = new List<int>();
    [Tooltip("카드 계수 타입")] public List<ECardValueType> cardValueTypes = new List<ECardValueType>();
    [Tooltip("덱 내 카드 장수")] public int num;
    [Tooltip("강화 여부")] public bool isEnhanced;

    public void SetItem(Item item)
    {
        name = item.name;
        cost = item.cost;
        type = item.type;
        rarity = item.rarity;
        dreamPieceNum = item.dreamPieceNum;
        isVolatile = item.isVolatile;
        isVanish = item.isVanish;
        isRemain = item.isRemain;
        sprite = item.sprite;
        text = item.text;
        cardValues.Clear();
        foreach(var v in item.cardValues) cardValues.Add(v);
        cardValueTypes.Clear();
        foreach(var v in item.cardValueTypes) cardValueTypes.Add(v);
        num = item.num;
        isEnhanced = item.isEnhanced;
    }
}

[System.Serializable]
public class Item_Enhanceable : Item
{
    [Tooltip("강화 카드")] public Item enhancedItem;

    public Item_Enhanceable(Item_Enhanceable item)
    {
        if(item == null) return;
        this.SetItem((Item)item);
        this.enhancedItem = new Item();
        this.enhancedItem.SetItem(item.enhancedItem);
    }
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    public List<Item> items;
}

[System.Serializable]
public class Item_Data
{
    public string cardName;
    public int num;

    public Item_Data(Item_Data data)
    {
        cardName = data.cardName;
        num = data.num;
    }
}

[CreateAssetMenu(fileName = "ItemDataSO", menuName = "Scriptable Objects/ItemDataSO")]
public class ItemDataSO : ScriptableObject
{
    public List<Item_Enhanceable> items;
}
