using UnityEngine;

public class RoomDoor : PlayerInteractableObject
{
    public int roomNum;
    public string passengerName;
    public override void Interact()
    {
        StageManager.Inst.characterSO.enemyName = passengerName;
        SceneChangeManager.Inst.SceneFadeOut("Room" + roomNum.ToString());
    }
}
