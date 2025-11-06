using UnityEngine;

[System.Serializable]
public class DreamPiece
{
    public string name;
    public Passive_Enhanceable persona;
    public Passive_Enhanceable shadow;
    public Item_Enhanceable[] cards;
    public Sprite specialRouletteSprite;
    public string specialRouletteTitle;
    public string specialRouletteText;
}

[CreateAssetMenu(fileName = "DreamPieceSO", menuName = "Scriptable Objects/DreamPieceSO")]
public class DreamPieceSO : ScriptableObject
{
    public DreamPiece[] dreamPieces;
}
