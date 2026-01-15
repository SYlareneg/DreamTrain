using UnityEngine;
using System.Collections.Generic;

public class RoomObjectManager : MonoBehaviour
{
    public static RoomObjectManager Inst;

    void Awake()
    {
        Inst = this;
    }

    public RoomObjectSO roomObjectSO;
    public List<RoomClickableObject> roomClickableObjects;

    public List<DialogueLine> GetDialogueLines(string objectName, string dialogueSetName)
    {
        foreach (var roomObject in roomObjectSO.roomObjects)
        {
            if (roomObject.objectName == objectName)
            {
                foreach (var dialogueSet in roomObject.objectStates)
                {
                    if (dialogueSet.stateName == dialogueSetName)
                    {
                        return dialogueSet.dialogueLines;
                    }
                }
                return null;
            }
        }
        return null;
    }
}
