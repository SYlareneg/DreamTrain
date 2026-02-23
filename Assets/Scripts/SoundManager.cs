using UnityEngine;
using DG.Tweening;

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

    [Header("BGM Clips")]
    [SerializeField] public AudioClip titleBGM;
    [SerializeField] AudioClip[] bgmClips;
    [SerializeField] public AudioClip magicianBGM;
    [SerializeField] public AudioClip magicianBattleBGM;
    public AudioSource bgmSource;

    public void PlayBGM(int actNum)
    {
        if (bgmSource.isPlaying)
        {
            if(bgmSource.clip == bgmClips[Mathf.Clamp(actNum, 0, bgmClips.Length - 1)])
            {
                return;
            }
            DOTween.To(() => bgmSource.volume, x => bgmSource.volume = x, 0, 2.5f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                bgmSource.Stop();
                bgmSource.clip = bgmClips[Mathf.Clamp(actNum, 0, bgmClips.Length - 1)];
                bgmSource.time = 0;
                bgmSource.volume = 0;
                DOTween.To(() => bgmSource.volume, x => bgmSource.volume = x, 1, 2.5f).SetEase(Ease.InOutSine);
                bgmSource.Play();
                });
        }
        else
        {
            bgmSource.clip = bgmClips[Mathf.Clamp(actNum, 0, bgmClips.Length - 1)];
            bgmSource.time = 0;
            bgmSource.volume = 0;
            DOTween.To(() => bgmSource.volume, x => bgmSource.volume = x, 1, 2.5f).SetEase(Ease.InOutSine);
            bgmSource.Play();
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if(bgmSource.isPlaying)
        {
            DOTween.To(() => bgmSource.volume, x => bgmSource.volume = x, 0, 2.5f).SetEase(Ease.InOutSine).OnComplete(() =>
            {
                bgmSource.Stop();
                bgmSource.clip = clip;
                bgmSource.time = 0;
                bgmSource.volume = 0;
                DOTween.To(() => bgmSource.volume, x => bgmSource.volume = x, 1, 2.5f).SetEase(Ease.InOutSine);
                bgmSource.Play();
            });
        }
        else
        {
            bgmSource.clip = clip;
            bgmSource.time = 0;
            bgmSource.volume = 0;
            DOTween.To(() => bgmSource.volume, x => bgmSource.volume = x, 1, 2.5f).SetEase(Ease.InOutSine);
            bgmSource.Play();
        }
    }

    public void StopBGM()
    {
        DOTween.To(() => bgmSource.volume, x => bgmSource.volume = x, 0, 2.5f).SetEase(Ease.InOutSine).OnComplete(() => bgmSource.Stop());
    }
}
