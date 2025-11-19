using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableObject : MonoBehaviour 
{
    public InteractableObjectData objectData;
    private bool isCollected = false; 
    public bool isInteractionEnabled = true;
    public int objectID;

    private void OnMouseEnter()
    {
        //if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!isInteractionEnabled || isCollected || EventSystem.current.IsPointerOverGameObject()) return;
    }
    
    public void OnCollectionComplete()
    {
        isCollected = true;
        gameObject.SetActive(false);
    }
}