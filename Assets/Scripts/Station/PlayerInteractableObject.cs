using UnityEngine;

public abstract class PlayerInteractableObject : MonoBehaviour
{
    public bool isInteractable;
    public bool alreadyInteracted;
    public abstract void Interact();
}
