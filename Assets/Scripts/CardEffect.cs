using UnityEngine;

public class CardEffect : MonoBehaviour
{
    public void OnEffectEnd()
    {
        if(CardManager.Inst.cardEffectEndAction.Count > 0)
        {
            CardManager.Inst.cardEffectEndAction[0]?.Invoke();
            CardManager.Inst.cardEffectEndAction.RemoveAt(0);
            if(CardManager.Inst.cardEffectEndAction.Count > 0)
            {
                CardManager.Inst.cardCurrentEffectName = CardManager.Inst.cardEffectQueue[0];
                CardManager.Inst.cardEffectQueue.RemoveAt(0);
                CardManager.Inst.cardEffect.SetTrigger(CardManager.Inst.cardCurrentEffectName);
            }
            else
            {
                CardManager.Inst.cardCurrentEffectName = "";
                CardManager.Inst.cardEffectQueue.Clear();
            }
        }
    }
}
