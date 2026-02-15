using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Inst;
    void Awake()
    {
        if(Inst != null && Inst != this)
        {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(this.gameObject);
        Inst = this;
    }

    public AudioClip playerTurnStartSFX;
    public AudioClip enemyTurnStartSFX;
    public AudioClip cardDrawSFX;
    public AudioClip enemyDamageSFX;
    public AudioClip enemyTriggerDamageSFX;
    public AudioClip playerDamageSFX;
    public AudioClip healSFX;
    public AudioClip shieldSFX;
    public AudioClip rouletteRotateSFX;
    public AudioClip rouletteButtonSFX;
    public AudioClip rouletteEnchantSFX;
    public AudioClip specialRouletteSFX_Claw;
    public AudioClip UISelectSFX;
    public AudioClip coinGetSFX;

    [SerializeField] AudioClip[] bgmClips;
    public AudioSource bgmSource;

    public void PlayBGM(int actNum)
    {
        if(bgmSource.isPlaying) return;
        bgmSource.clip = bgmClips[Mathf.Clamp(actNum, 0, bgmClips.Length - 1)];
        bgmSource.time = 0;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }
}
