using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType { Turn, Enchant, Effect };
public enum CardElement { Fire, Water, Grass };

[System.Serializable]
public class Item
{
    [Tooltip("카드 제목")] public string name;
    [Tooltip("카드 코스트")] public int cost;
    [Tooltip("카드 타입 (배경이미지 결정)")] public CardType type;
    [Tooltip("카드 속성")] public CardElement element;
    [Tooltip("카드 이미지")] public Sprite sprite;
    [Tooltip("카드 설명")] public string text;
    [Tooltip("덱 내 카드 장수")] public int num;
}

[CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemSO")]
public class ItemSO : ScriptableObject
{
    public Item[] items;
}
