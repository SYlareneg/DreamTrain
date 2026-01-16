using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnEnd : MonoBehaviour
{
    Button button;

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
