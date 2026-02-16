using UnityEngine;

public class RoomTriggerObject : MonoBehaviour
{
    public string objectName;
    public string objectState;

    public virtual void Trigger()
    {
        RoomPlayer.Inst.isInteractable = false;
        RoomDialogueManager.Inst.ShowDialogueList(RoomObjectManager.Inst.GetDialogueLines(objectName, objectState));
    }
}
