using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TurnEnd : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Button button;
    [SerializeField] GameObject highlight;

     public void OnPointerEnter(PointerEventData eventData)
    {
        highlight.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        highlight.SetActive(false);
    }

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    void Update()
    {
        if(TurnManager.Inst.isLoading && (TurnManager.Inst.characterSO.isTutorial == false || TutorialManager.Inst.endTurnActivate == false))
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }
    }
}
