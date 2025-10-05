using UnityEngine;

[System.Serializable]
public class DreamPiece
{
    [SerializeField] string name;
    public Passive persona;
    public Passive shadow;
    public Item[] normalCards;
    public Item[] personaCards;
    public Item[] shadowCards;
}

[CreateAssetMenu(fileName = "DreamPieceSO", menuName = "Scriptable Objects/DreamPieceSO")]
public class DreamPieceSO : ScriptableObject
{
    public DreamPiece[] dreamPieces;
}
