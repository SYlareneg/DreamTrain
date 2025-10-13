using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class HallSceneControll : MonoBehaviour
{
    private InputSystem_Actions input;
    private DoorControll currentDoor;

    private void Awake()
    {
        input = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.EnterRoom.performed += OnEnterRoomPerformed;
    }

    private void OnDisable()
    {
        input.Player.Disable();
        input.Player.EnterRoom.performed -= OnEnterRoomPerformed;
    }

    public void SetCurrentDoor(DoorControll door)
    {
        currentDoor = door;
        Debug.Log($"[HallScene] Player approached {door.name}");
    }

    public void ClearCurrentDoor(DoorControll door)
    {
        if (currentDoor == door)
        {
            Debug.Log($"[HallScene] Player left {door.name}");
            currentDoor = null;
           
        }
    }

    private void OnEnterRoomPerformed(InputAction.CallbackContext context)
    {
        if (currentDoor != null)
        {
            int deckCount = 0;
            foreach(var item in CardManager.Inst.playerDeckSO.items)
            {
                deckCount += item.num;
            }
            if (deckCount == DeckBuildManager.deckCardNum)
            {
                Debug.Log($"Entering room: {currentDoor.roomSceneName}");
                SceneManager.LoadScene(currentDoor.roomSceneName);
            }
            else
            {
                Debug.Log("Player deck is not ready.");
            }
        }
        else
        {
            Debug.Log("No door nearby.");
        }
    }
}