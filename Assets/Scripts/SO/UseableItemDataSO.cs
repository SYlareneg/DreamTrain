using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "UseableItemDataSO", menuName = "Scriptable Objects/UseableItemDataSO")]
public class UseableItemDataSO : ScriptableObject
{
    public List<UseItem_Data> useableItems;
}