using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class UseItem
{
    public string name;
    public string text;
    public Sprite sprite;
    public int rarity;
    public bool isSingleTarget;
    public List<int> useValue;

    public void Setup(string name, string text, Sprite sprite, int rarity, bool isSingleTarget, List<int> useValue)
    {
        this.name = name;
        this.text = text;
        this.sprite = sprite;
        this.rarity = rarity;
        this.isSingleTarget = isSingleTarget;
        this.useValue = new List<int>(useValue);
    }

    public void Setup(UseItem useItem)
    {
        this.name = useItem.name;
        this.text = useItem.text;
        this.sprite = useItem.sprite;
        this.rarity = useItem.rarity;
        this.isSingleTarget = useItem.isSingleTarget;
        this.useValue = new List<int>(useItem.useValue);
    }

    public UseItem() { }

    public UseItem(UseItem useItem)
    {
        this.name = useItem.name;
        this.text = useItem.text;
        this.sprite = useItem.sprite;
        this.rarity = useItem.rarity;
        this.isSingleTarget = useItem.isSingleTarget;
        this.useValue = new List<int>(useItem.useValue);
    }

    public UseItem(UseItem_Data useItemData)
    {
        this.name = useItemData.name;
        this.text = useItemData.text;
        this.sprite = Utils.LoadSpriteByName("Items", useItemData.sprite);
        this.rarity = useItemData.rarity;
        this.isSingleTarget = useItemData.isSingleTarget;
        this.useValue = new List<int>(useItemData.useValue);
    }
}

[System.Serializable]
public class UseItem_Data
{
    public string name;
    public string text;
    public string sprite;
    public int rarity;
    public bool isSingleTarget;
    public List<int> useValue;

    public UseItem_Data(UseItem useItem)
    {
        this.name = useItem.name;
        this.text = useItem.text;
        this.sprite = useItem.sprite != null ? useItem.sprite.name : "";
        this.rarity = useItem.rarity;
        this.isSingleTarget = useItem.isSingleTarget;
        this.useValue = new List<int>(useItem.useValue);
    }
}

[CreateAssetMenu(fileName = "UseableItemSO", menuName = "Scriptable Objects/UseableItemSO")]
public class UseableItemSO : ScriptableObject
{
    public List<UseItem> useableItems;
}
