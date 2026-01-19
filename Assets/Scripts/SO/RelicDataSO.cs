using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RelicDataSO", menuName = "Scriptable Objects/RelicDataSO")]
public class RelicDataSO : ScriptableObject
{
    public List<RelicItem_Data> relicItems;
}