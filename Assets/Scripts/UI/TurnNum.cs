using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurnNum : MonoBehaviour
{
    int shownTurnNum = 0;
    [SerializeField] TMP_Text turnNumTMP;

    void Update()
    {
        if(TurnManager.Inst.turnNum != shownTurnNum)
        {
            shownTurnNum = TurnManager.Inst.turnNum;
            turnNumTMP.text = shownTurnNum.ToString();
        }
    }
}
