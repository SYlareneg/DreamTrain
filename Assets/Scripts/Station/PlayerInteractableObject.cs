using UnityEngine;

public abstract class PlayerInteractableObject : MonoBehaviour
{
    public bool isInteractable;
    public bool alreadyInteracted;
    public string alreadyInteractedSpeech;
    public abstract void Interact();
}
