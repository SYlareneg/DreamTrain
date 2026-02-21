using UnityEngine;

public class RouletteEffect_Enemy : MonoBehaviour
{
    public void OnEffectEnd()
    {
        if(RouletteManager.Inst.enemyRouletteEffectEndAction.Count > 0)
        {
            RouletteManager.Inst.enemyRouletteEffectEndAction[0]?.Invoke();
            RouletteManager.Inst.enemyRouletteEffectEndAction.RemoveAt(0);
            if(RouletteManager.Inst.enemyRouletteEffectEndAction.Count > 0)
            {
                RouletteManager.Inst.enemyRouletteCurrentEffectName = RouletteManager.Inst.enemyRouletteEffectQueue[0];
                RouletteManager.Inst.enemyRouletteEffectQueue.RemoveAt(0);
                RouletteManager.Inst.enemyRouletteEffect.SetTrigger(RouletteManager.Inst.enemyRouletteCurrentEffectName);
            }
            else
            {
                RouletteManager.Inst.enemyRouletteCurrentEffectName = "";
                RouletteManager.Inst.enemyRouletteEffectQueue.Clear();
            }
        }
    }
}
