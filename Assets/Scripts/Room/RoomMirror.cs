using UnityEngine;

public class RoomMirror : RoomClickableObject
{
    public override void Interact()
    {
        if(objectState == "default") return;
        RoomPlayer.Inst.isInteractable = false;
        StartCoroutine(DataManager.Inst.LoadPlayerData(true));
        RoomDPManager.Inst.InitDPUI();
    }
}
