using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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

    void Start()
    {
        RoomClickableObject cat = roomClickableObjects.OfType<RoomCat>().FirstOrDefault();
        if(cat != null)
        {
            if(DataManager.Inst.characterSO.isTutorial)
            {
                cat.objectState = "tutorial";
            }
            else
            {
                cat.objectState = "awake";
            }
        }

        RoomClickableObject mirror = roomClickableObjects.OfType<RoomMirror>().FirstOrDefault();
        if(mirror != null)
        {
            if(DataManager.Inst.characterSO.isTutorial)
            {
                mirror.objectState = "default";
            }
            else
            {
                mirror.objectState = "awake";
            }
        }
    }
}
