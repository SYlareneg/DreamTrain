using UnityEngine;

public class RouletteEffect_Player : MonoBehaviour
{
    public void OnEffectEnd()
    {
        if(RouletteManager.Inst.playerRouletteEffectEndAction.Count > 0)
        {
            RouletteManager.Inst.playerRouletteEffectEndAction[0]?.Invoke();
            RouletteManager.Inst.playerRouletteEffectEndAction.RemoveAt(0);
            if(RouletteManager.Inst.playerRouletteEffectEndAction.Count > 0)
            {
                RouletteManager.Inst.playerRouletteCurrentEffectName = RouletteManager.Inst.playerRouletteEffectQueue[0];
                RouletteManager.Inst.playerRouletteEffectQueue.RemoveAt(0);
                if(RouletteManager.Inst.playerRouletteCurrentEffectName == "Claw"
                || RouletteManager.Inst.playerRouletteCurrentEffectName == "Drain")
                {
                    RouletteManager.Inst.playerRouletteEffect2.SetTrigger(RouletteManager.Inst.playerRouletteCurrentEffectName);
                }
                else
                {
                    RouletteManager.Inst.playerRouletteEffect.SetTrigger(RouletteManager.Inst.playerRouletteCurrentEffectName);
                }
            }
            else
            {
                RouletteManager.Inst.playerRouletteCurrentEffectName = "";
                RouletteManager.Inst.playerRouletteEffectQueue.Clear();
            }
        }
    }
}
