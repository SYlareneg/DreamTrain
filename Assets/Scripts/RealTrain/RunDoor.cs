using UnityEngine;
using UnityEngine.SceneManagement;

public class RunDoor : PlayerInteractableObject
{
    public override void Interact()
    {
        if(SceneManager.GetActiveScene().name == "RealTrainScene")
        {
            SceneChangeManager.Inst.SceneFadeOut("RunScene_Real");
        }
        else if(SceneManager.GetActiveScene().name == "RunScene_Real")
        {
            SceneChangeManager.Inst.SceneFadeOut("RealTrainScene");
        }
    }
}
