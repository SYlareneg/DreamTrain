using UnityEngine;

public class CardEffect2 : MonoBehaviour
{
    public void OnEffectEnd()
    {
        if(CardManager.Inst.cardEffectEndAction2.Count > 0)
        {
            CardManager.Inst.cardEffectEndAction2[0]?.Invoke();
            CardManager.Inst.cardEffectEndAction2.RemoveAt(0);
            if(CardManager.Inst.cardEffectEndAction2.Count > 0)
            {
                CardManager.Inst.cardCurrentEffectName2 = CardManager.Inst.cardEffectQueue2[0];
                CardManager.Inst.cardEffectQueue2.RemoveAt(0);
                CardManager.Inst.cardEffect2.SetTrigger(CardManager.Inst.cardCurrentEffectName2);
            }
            else
            {
                CardManager.Inst.cardCurrentEffectName2 = "";
                CardManager.Inst.cardEffectQueue2.Clear();
            }
        }
    }
}
