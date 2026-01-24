using UnityEngine;
using System.Collections.Generic;

public class RoomClickableObject : MonoBehaviour
{
    public string objectName;
    public string objectState;
    public bool isInteractable;
    public virtual void Interact()
    {
        RoomPlayer.Inst.isInteractable = false;
        RoomDialogueManager.Inst.ShowDialogueList(RoomObjectManager.Inst.GetDialogueLines(objectName, objectState));
    }

    void Update()
    {
        if(isInteractable)
        {
            transform.Find("Highlight").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            transform.Find("Highlight").GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        }
    }
}
