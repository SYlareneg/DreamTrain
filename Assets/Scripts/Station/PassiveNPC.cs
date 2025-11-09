using UnityEngine;

public class PassiveNPC : PlayerInteractableObject
{
    public EPassiveType npcType;
    public override void Interact()
    {
        if (npcType == EPassiveType.Persona) NPCPassiveManager.Inst.ShowPersonaScreen();
        else if (npcType == EPassiveType.Shadow) NPCPassiveManager.Inst.ShowShadowScreen();

        Destroy(this);
    }
}
