using UnityEngine;
using UnityEngine.UI;

public class CardSelectButton : MonoBehaviour
{
    Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    void Update()
    {
        if(CardManager.Inst.cardSelectNum == 0)
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }
    }
}
