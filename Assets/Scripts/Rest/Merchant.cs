using UnityEngine;

public class Merchant : PlayerInteractableObject
{
    public override void Interact()
    {
        NPCMerchantManager.Inst.merchant = this;
        NPCMerchantManager.Inst.ShowMerchantUI();
    }
}
