using UnityEngine;

public class RestDoor : PlayerInteractableObject
{
    public override void Interact()
    {
        StageManager.Inst.stageSO.playerSpawn = EPlayerSpawn.Rest;
        SceneChangeManager.Inst.SceneFadeOut("RestScene");
    }
}
