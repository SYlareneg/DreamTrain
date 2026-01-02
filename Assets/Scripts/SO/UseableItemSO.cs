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

    public void Setup(UseItem useItem)
    {
        this.name = useItem.name;
        this.text = useItem.text;
        this.sprite = useItem.sprite;
        this.rarity = useItem.rarity;
    }

    public UseItem() { }

    public UseItem(UseItem useItem)
    {
        this.name = useItem.name;
        this.text = useItem.text;
        this.sprite = useItem.sprite;
        this.rarity = useItem.rarity;
    }

    public UseItem(UseItem_Data useItemData)
    {
        this.name = useItemData.name;
        this.text = useItemData.text;
        this.sprite = Utils.LoadSpriteByName("Items", useItemData.sprite);
        this.rarity = useItemData.rarity;
    }
}

[System.Serializable]
public class UseItem_Data
{
    public string name;
    public string text;
    public string sprite;
    public int rarity;

    public UseItem_Data(UseItem useItem)
    {
        this.name = useItem.name;
        this.text = useItem.text;
        this.sprite = useItem.sprite != null ? useItem.sprite.name : "";
        this.rarity = useItem.rarity;
    }
}

[CreateAssetMenu(fileName = "UseableItemSO", menuName = "Scriptable Objects/UseableItemSO")]
public class UseableItemSO : ScriptableObject
{
    public List<UseItem> useableItems;
}

[CreateAssetMenu(fileName = "UseableItemDataSO", menuName = "Scriptable Objects/UseableItemDataSO")]
public class UseableItemDataSO : ScriptableObject
{
    public List<UseItem_Data> useableItems;
}
