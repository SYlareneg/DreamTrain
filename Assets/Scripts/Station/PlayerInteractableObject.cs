using UnityEngine;

public abstract class PlayerInteractableObject : MonoBehaviour
{
    public bool isInteractable;
    public abstract void Interact();
}
