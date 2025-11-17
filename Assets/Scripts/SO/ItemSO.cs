using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType { Turn, Enchant, Effect };
public enum ECardValueType { Damage, Heal, Shield, Special, Default };

[System.Serializable]
public class Item
{
    [Tooltip("카드 제목")] public string name;
    [Tooltip("카드 코스트")] public int cost;
    [Tooltip("카드 타입 (배경이미지 결정)")] public CardType type;
    [Tooltip("카드 속성 (공용, 페르소나, 그림자)")] public EPassiveType element;
    [Tooltip("카드가 속한 꿈 조각 번호")] public int dreamPieceNum;
    [Tooltip("휘발성")] public bool isVolatile;
    [Tooltip("소멸")] public bool isVanish;
    [Tooltip("잔류")] public bool isRemain;
    [Tooltip("카드 이미지")] public Sprite sprite;
    [Tooltip("카드 설명")] public string text;
    [Tooltip("카드 계수")] public List<(int val, ECardValueType valType)> cardValues = new List<(int val, ECardValueType valType)>();
    [Tooltip("덱 내 카드 장수")] public int num;
    [Tooltip("강화 여부")] public bool isEnhanced;

    public void SetItem(Item item)
    {
        name = item.name;
        cost = item.cost;
        type = item.type;
        element = item.element;
        dreamPieceNum = item.dreamPieceNum;
        isVolatile = item.isVolatile;
        isVanish = item.isVanish;
        isRemain = item.isRemain;
        sprite = item.sprite;
        text = item.text;
        cardValues = item.cardValues;
        num = item.num;
        isEnhanced = item.isEnhanced;
    }
}

[System.Serializable]
public class Item_Enhanceable : Item
{
    [Tooltip("강화 카드")] public Item enhancedItem;
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    public List<Item> items;
}
