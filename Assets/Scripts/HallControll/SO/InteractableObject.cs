using UnityEngine;
using UnityEngine.EventSystems;

public class InteractableObject : MonoBehaviour 
{
    public InteractableObjectData objectData;
    private bool isCollected = false; 
    public bool isInteractionEnabled = true;

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

    private void OnMouseDown()
    {
        //if (EventSystem.current.IsPointerOverGameObject()) return;
        if (!isInteractionEnabled || isCollected || EventSystem.current.IsPointerOverGameObject()) return;
        if (objectData != null && objectData.DialogueList.Count > 0)
        {
            DialogueManager.Instance.StartDialogueFromObject(objectData, this);
        }
        else
        {
            Debug.LogWarning($"'{gameObject.name}' No data or No dialogue");
        }       
        
    }
    public void OnCollectionComplete()
    {
        isCollected = true;
        gameObject.SetActive(false);
    }
}