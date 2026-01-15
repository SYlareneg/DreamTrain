using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RoomObject
{
    public string objectName;
    public Sprite objectSprite;
    public List<ObjectState> objectStates;
}

[System.Serializable]
public class ObjectState
{
    public string stateName;
    public List<DialogueLine> dialogueLines;
}

[CreateAssetMenu(fileName = "RoomObjectSO", menuName = "Scriptable Objects/RoomObjectSO")]
public class RoomObjectSO : ScriptableObject
{
    public List<RoomObject> roomObjects;
}
