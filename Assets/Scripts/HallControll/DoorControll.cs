using UnityEngine;

public class DoorControll : MonoBehaviour
{
    public string roomSceneName = "Room1";
    private HallSceneControll hallManager;

    private void Start()
    {
        hallManager = FindObjectOfType<HallSceneControll>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            hallManager.SetCurrentDoor(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            hallManager.ClearCurrentDoor(this);
        }
    }
}