using UnityEngine;
using UnityEngine.SceneManagement;

public class TrainDoor : PlayerInteractableObject
{
    public override void Interact()
    {
        SceneManager.LoadScene("HallScene");
    }
}
