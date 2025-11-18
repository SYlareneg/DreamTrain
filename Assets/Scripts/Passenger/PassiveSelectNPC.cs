using UnityEngine;

public class PassiveSelectNPC : PlayerInteractableObject
{
    public override void Interact()
    {
        NPCPassiveSelectManager.Inst.ShowScreen();
    }
}
