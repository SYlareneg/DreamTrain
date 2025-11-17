using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainDoor : PlayerInteractableObject
{
    public override void Interact()
    {
        SceneChangeManager.Inst.SceneFadeOut("PassengerScene");
    }
}
