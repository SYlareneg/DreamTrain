using UnityEngine;

public class MagicianEffectTrigger : RoomTriggerObject
{
    public override void Trigger()
    {
        if(DataManager.Inst.characterSO.bossClear) return;
        RoomPlayer.Inst.isInteractable = false;
        MagicianEffectManager.Inst.StartEffect();
        Destroy(this);
    }
}
