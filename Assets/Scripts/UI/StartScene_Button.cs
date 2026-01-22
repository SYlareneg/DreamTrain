using UnityEngine;
using UnityEngine.EventSystems;

public class StartScene_Button : MonoBehaviour, IPointerEnterHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (gameObject.name == "NewGame")
        {
            StartCanvasController.Inst.SelectNewGame();
        }
        else if (gameObject.name == "LoadGame")
        {
            StartCanvasController.Inst.SelectLoadGame();
        }
        else if (gameObject.name == "Option")
        {
            StartCanvasController.Inst.SelectOption();
        }
        else if (gameObject.name == "Exit")
        {
            StartCanvasController.Inst.SelectExit();
        }
    }
}
