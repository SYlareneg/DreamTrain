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
        DialogueUI.Instance.ShowObjectName(objectData.Name_KO);
    }

    private void OnMouseExit()
    {
        DialogueUI.Instance.HideObjectName();
    }

    void OnMouseDown()
    {
        //DialogueBundleSelector.Inst.ShowBundleChoices(objectID);
    }
    
    
    public void OnCollectionComplete()
    {
        isCollected = true;
        gameObject.SetActive(false);
    }
}