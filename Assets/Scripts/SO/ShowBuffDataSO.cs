using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShowBuffDataSO", menuName = "Scriptable Objects/ShowBuffDataSO")]
public class ShowBuffDataSO : ScriptableObject
{
    public List<ShowBuff_Data> showBuffs;
}