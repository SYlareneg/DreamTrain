using UnityEngine;
using UnityEngine.UI;

public class CostBeads : MonoBehaviour
{
    int totalBeads = 0;
    int fullBeads = 0;
    [SerializeField] GameObject beadPrefab;
    [SerializeField] Sprite fullBeadSprite;
    [SerializeField] Sprite emptyBeadSprite;

    void Update()
    {
        if(Mathf.Max(TurnManager.Inst.nowCost, TurnManager.Inst.turnCost) != totalBeads || TurnManager.Inst.nowCost != fullBeads)
        {
            Debug.Log("Updating Cost Beads: " + TurnManager.Inst.nowCost + "/" + TurnManager.Inst.turnCost);
            totalBeads = Mathf.Max(TurnManager.Inst.nowCost, TurnManager.Inst.turnCost);
            fullBeads = TurnManager.Inst.nowCost;
            foreach(Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            int emptyBeads = totalBeads - fullBeads;
            for(int i = 0; i < totalBeads; i++)
            {
                GameObject beadObj = Instantiate(beadPrefab, transform);
                if(i < emptyBeads)
                {
                    beadObj.GetComponent<Image>().sprite = emptyBeadSprite;
                }
                else
                {
                    beadObj.GetComponent<Image>().sprite = fullBeadSprite;
                }
            }
        }
    }
}
