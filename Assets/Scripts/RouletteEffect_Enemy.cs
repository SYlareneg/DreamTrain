using UnityEngine;

public class RouletteEffect_Enemy : MonoBehaviour
{
    public void OnEffectEnd()
    {
        RouletteManager.Inst.enemyRouletteEffectEndAction?.Invoke();
        RouletteManager.Inst.enemyRouletteEffectEndAction = null;
    }
}
