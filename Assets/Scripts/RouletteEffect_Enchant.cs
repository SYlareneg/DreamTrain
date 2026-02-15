using UnityEngine;
using System;

public class RouletteEffect_Enchant : MonoBehaviour
{
    public Action OnEffectEndAction;
    
    public void OnEffectEnd()
    {
        OnEffectEndAction?.Invoke();
        OnEffectEndAction = null;
    }
}
