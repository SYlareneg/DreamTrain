using UnityEngine;

public class RouletteEffect_Trigger : MonoBehaviour
{
    public int triggerVal;

    public void OnEffectEnd()
    {
        RouletteManager.TriggerActivation.Invoke(RouletteManager.Inst.isEnemyTrigger(), triggerVal);
    }
}
