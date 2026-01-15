using UnityEngine;

public class RoomMirror : RoomClickableObject
{
    public override void Interact()
    {
        if(objectState == "default") return;
        RoomPlayer.Inst.isInteractable = false;
        DataManager.Inst.InitPlayerData();
        RoomDPManager.Inst.InitDPUI();
        // SceneChangeManager.Inst.SceneFadeOut("MapScene");
    }
}
