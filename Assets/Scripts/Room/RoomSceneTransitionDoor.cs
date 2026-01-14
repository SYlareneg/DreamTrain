using UnityEngine;

public class RoomSceneTransitionDoor : PlayerInteractableObject
{
    public override void Interact()
    {
        if(alreadyInteracted)
        {
            return;
        }

        var colliders = GetComponents<BoxCollider2D>();
        foreach(var collider in colliders)
        {
            collider.enabled = false;
        }
        GetComponent<SpriteRenderer>().sortingLayerName = "Default";
        alreadyInteracted = true;
    }
}
