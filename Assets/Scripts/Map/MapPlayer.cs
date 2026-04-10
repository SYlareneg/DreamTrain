using UnityEngine;

public class MapPlayer : MonoBehaviour
{
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    
    public void WalkSound()
    {
        audioSource.PlayOneShot(audioSource.clip);
    }

    void Start()
    {
        if(DataManager.Inst.characterSO.isTutorial)
        {
            transform.localScale *= 1.3f;
        }
    }
}
