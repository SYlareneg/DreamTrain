using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DreamPieceDataSO", menuName = "Scriptable Objects/DreamPieceDataSO")]
public class DreamPieceDataSO : ScriptableObject
{
    public List<DreamPiece_Data> dreamPieces;
}