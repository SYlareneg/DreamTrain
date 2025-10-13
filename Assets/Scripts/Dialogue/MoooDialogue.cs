using UnityEngine;
[RequireComponent(typeof(InteractableObject))]
public class MoooDialogue : MonoBehaviour
{
    private InteractableObject interactable;
    private void Awake()
    {
        interactable = GetComponent<InteractableObject>();
        interactable.isInteractionEnabled = false;
    }

    private void OnEnable()
    {
        InventoryManager.OnInventoryFull += EnableInteraction;
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryFull -= EnableInteraction;
    }

    private void EnableInteraction()
    {
        interactable.isInteractionEnabled = true;
        Debug.Log("Mooo is now interactable.");
    }
}
