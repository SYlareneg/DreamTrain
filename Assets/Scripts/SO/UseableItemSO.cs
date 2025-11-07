using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class UseItem
{
    public string name;
    public string text;
    public Sprite sprite;
    public int rarity;

    public void Setup(string name, string text, Sprite sprite, int rarity)
    {
        this.name = name;
        this.text = text;
        this.sprite = sprite;
        this.rarity = rarity;
    }
}

[CreateAssetMenu(fileName = "UseableItemSO", menuName = "Scriptable Objects/UseableItemSO")]
public class UseableItemSO : ScriptableObject
{
    public List<UseItem> useableItems;
}
