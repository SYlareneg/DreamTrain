using UnityEngine;

public class MagicianEndTrigger : RoomTriggerObject
{
    public override void Trigger()
    {
        if(!DataManager.Inst.characterSO.bossClear) return;
        RoomPlayer.Inst.isInteractable = false;
        SceneChangeManager.Inst.SceneFadeOut("EncounterScene");
    }
}
