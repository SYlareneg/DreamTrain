using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Inst;
    void Awake() => Inst = this;

    public AudioClip cardDrawSFX;
    public AudioClip enemyDamageSFX;
    public AudioClip enemyTriggerDamageSFX;
    public AudioClip playerDamageSFX;
    public AudioClip rouletteRotateSFX;
    public AudioClip rouletteButtonSFX;
    public AudioClip rouletteEnchantSFX;
    public AudioClip UISelectSFX;
    public AudioClip coinGetSFX;
}
