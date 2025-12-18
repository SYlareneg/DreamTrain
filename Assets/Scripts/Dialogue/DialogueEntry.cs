using UnityEngine;

[System.Serializable]
public class DialogueEntry
{
    public int ID;
    public string BoxLocation;   
    public string Dialogue_KO;
    public string Dialogue_EN;
    public string Type;    
    public string SFX;
    public int IdToGet;
    public int IdPoint;
    public int NextID;
    public string Function; 
    public FeelingType feelingType = FeelingType.None;
}