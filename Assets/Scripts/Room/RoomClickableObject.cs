using UnityEngine;
using System.Collections.Generic;

public class RoomClickableObject : MonoBehaviour
{
    public string objectName;
    public string objectState;
    public bool isInteractable;
    public virtual void Interact()
    {
        RoomPlayer.Inst.isInteractable = false;
        RoomDialogueManager.Inst.ShowDialogueList(RoomObjectManager.Inst.GetDialogueLines(objectName, objectState));

        if(objectName == "고양이" && objectState == "awake")
        {
            RoomDialogueManager.OnDialogueEnd += () =>
            {
                RoomObjectManager.Inst.roomClickableObjects.Find(obj => obj.objectName == "거울").objectState = "awake";
            };
        }
    }
}
