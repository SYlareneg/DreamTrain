using UnityEngine;

public class RestDoor : PlayerInteractableObject
{
    public override void Interact()
    {
        SceneChangeManager.Inst.SceneFadeOut("RestScene");
    }
}
