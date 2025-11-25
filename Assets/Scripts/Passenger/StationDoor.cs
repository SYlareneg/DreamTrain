using UnityEngine;

public class StationDoor : PlayerInteractableObject
{
    public override void Interact()
    {
        StageManager.Inst.stageSO.currentStage++;
        if(StageManager.Inst.stageSO.currentStage > StageManager.Inst.stageSO.stageList.Length)
        {
            StageManager.Inst.stageSO.currentStage = StageManager.Inst.stageSO.stageList.Length;
        }
        SceneChangeManager.Inst.SceneFadeOut("StationScene");
    }
}
