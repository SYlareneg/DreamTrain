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
}
