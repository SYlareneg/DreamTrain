using UnityEngine;

public class RoomMirror : RoomClickableObject
{
    public override void Interact()
    {
        if(objectState == "default") return;
        DataManager.Inst.InitPlayerData();
        SceneChangeManager.Inst.SceneFadeOut("MapScene");
    }
}
