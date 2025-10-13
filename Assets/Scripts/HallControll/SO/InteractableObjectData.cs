using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueList
{
    public int dialogueIDToPlay;
    public int prerequisiteDialogueID = -1;
}

[CreateAssetMenu(fileName = "New Interactable Object Data", menuName = "ScriptableObjects/Interactable Object Data", order = 1)]
public class InteractableObjectData : ScriptableObject
{
    [Header("Object")]
    public int ID; 
    public string Name_KO;
    public string Name_EN;
    
    [Header("Inventory")]
    public Sprite itemIcon;

    [Header("Object Dialogue")]
    public List<DialogueList> DialogueList; // 
}