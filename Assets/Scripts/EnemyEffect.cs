using UnityEngine;

public class EnemyEffect : MonoBehaviour
{
    public void OnEffectEnd()
    {
        if(EnemyManager.Inst.enemySpecialEffectEndAction.Count > 0)
        {
            EnemyManager.Inst.enemySpecialEffectEndAction[0]?.Invoke();
            EnemyManager.Inst.enemySpecialEffectEndAction.RemoveAt(0);
            if(EnemyManager.Inst.enemySpecialEffectEndAction.Count > 0)
            {
                EnemyManager.Inst.enemyCurrentSpecialEffectName = EnemyManager.Inst.enemySpecialEffectQueue[0];
                EnemyManager.Inst.enemySpecialEffectQueue.RemoveAt(0);
                EnemyManager.Inst.enemySpecialEffect.SetTrigger(EnemyManager.Inst.enemyCurrentSpecialEffectName);
            }
            else
            {
                EnemyManager.Inst.enemyCurrentSpecialEffectName = "";
                EnemyManager.Inst.enemySpecialEffectQueue.Clear();
            }
        }
    }
}
