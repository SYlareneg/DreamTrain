using UnityEngine;

public class RoomCat : RoomClickableObject
{
    public override void Interact()
    {
        base.Interact();

        if(objectState == "awake")
        {
            RoomDialogueManager.OnDialogueEnd += () =>
            {
                RoomObjectManager.Inst.roomClickableObjects.Find(obj => obj.objectName == "거울").objectState = "awake";
            };
        }
        else if(objectState == "tutorial")
        {
            RoomDialogueManager.OnDialogueEnd += () =>
            {
                SceneChangeManager.Inst.SceneFadeOut("MapScene");
            };
        }
    }
}
