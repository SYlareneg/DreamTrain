using UnityEngine;

[System.Serializable]
public class DreamPiece
{
    public string name;
    public Passive persona;
    public Passive shadow;
    public Item[] cards;
}

[CreateAssetMenu(fileName = "DreamPieceSO", menuName = "Scriptable Objects/DreamPieceSO")]
public class DreamPieceSO : ScriptableObject
{
    public DreamPiece[] dreamPieces;
}
