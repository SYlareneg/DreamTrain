using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    public int ID;
    public string BoxLocation;    // Guest / Player
    public string Dialogue_KO;
    public string Dialogue_EN;
    public string Type;           // Normal / Branch
    public string SFX;
    public int IdToGet;           // 보상/포인트용
    public int IdPoint;
    public int NextID;
    public string Function;       // End 등
}