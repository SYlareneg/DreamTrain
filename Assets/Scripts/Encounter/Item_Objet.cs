using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item_Objets 
{
    [Header("CSV Data")]
    public int id;              
    public string name_ko;      
    [TextArea]
    public string desc_ko;      
    
    public int act;             
    public CardRarity rarity;   
    
    [Header("Shop Settings")]
    public int price = 100;     
    public bool isBought;       

    [Header("Game Data")]
    public RelicItem relicData; 
}

[CreateAssetMenu(fileName = "ObjetDataList", menuName = "Scriptable Objects/Item_Objets_List")]
public class ObjetData : ScriptableObject
{
    public List<Item_Objets> ObjetItems;
}