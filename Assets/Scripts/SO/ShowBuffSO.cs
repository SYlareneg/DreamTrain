using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShowBuffSO", menuName = "Scriptable Objects/ShowBuffSO")]
public class ShowBuffSO : ScriptableObject
{
    public List<ShowBuff> showBuffs;
}
