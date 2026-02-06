using UnityEngine;
using UnityEngine.UI;

public class TurnNum : MonoBehaviour
{
    int shownTurnNum = 0;
    [SerializeField] GameObject turnNumDigitPrefab;
    [SerializeField] Sprite[] digitSprites;
    void Update()
    {
        if(TurnManager.Inst.turnNum != shownTurnNum)
        {
            shownTurnNum = TurnManager.Inst.turnNum;
            foreach(Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            string turnNumStr = shownTurnNum.ToString();
            for(int i = 0; i < turnNumStr.Length; i++)
            {
                GameObject digitObj = Instantiate(turnNumDigitPrefab, transform);
                int digit = int.Parse(turnNumStr[i].ToString());
                digitObj.GetComponent<Image>().sprite = digitSprites[digit];
            }
        }
    }
}
