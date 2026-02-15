using UnityEngine;
using UnityEngine.UI;

public class CostBeads : MonoBehaviour
{
    int totalBeads = 0;
    int fullBeads = 0;
    [SerializeField] GameObject beadPrefab;
    [SerializeField] Sprite fullBeadSprite;
    [SerializeField] Sprite emptyBeadSprite;
    [SerializeField] Sprite tieSprite;

    void Update()
    {
        if(totalBeads != TurnManager.Inst.turnCost + TurnManager.Inst.extraCost || fullBeads != TurnManager.Inst.nowCost - TurnManager.Inst.extraCost)
        {
            Debug.Log("Updating Cost Beads: " + TurnManager.Inst.nowCost + "/" + TurnManager.Inst.turnCost);
            totalBeads = TurnManager.Inst.turnCost + TurnManager.Inst.extraCost;
            fullBeads = TurnManager.Inst.nowCost - TurnManager.Inst.extraCost;
            foreach(Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            for(int i = 0; i < fullBeads; i++)
            {
                GameObject beadObj = Instantiate(beadPrefab, transform);
                beadObj.GetComponent<Image>().sprite = fullBeadSprite;
            }
            int emptyBeads = TurnManager.Inst.turnCost - fullBeads;
            for(int i = 0; i < emptyBeads; i++)
            {
                GameObject beadObj = Instantiate(beadPrefab, transform);
                beadObj.GetComponent<Image>().sprite = emptyBeadSprite;
            }
            GameObject tieObj = Instantiate(beadPrefab, transform);
            tieObj.GetComponent<Image>().sprite = tieSprite;
            for(int i = 0; i < TurnManager.Inst.extraCost; i++)
            {
                GameObject beadObj = Instantiate(beadPrefab, transform);
                beadObj.GetComponent<Image>().sprite = fullBeadSprite;
            }
            // for(int i = 0; i < totalBeads; i++)
            // {
            //     GameObject beadObj = Instantiate(beadPrefab, transform);
            //     if(i < emptyBeads)
            //     {
            //         beadObj.GetComponent<Image>().sprite = emptyBeadSprite;
            //     }
            //     else
            //     {
            //         beadObj.GetComponent<Image>().sprite = fullBeadSprite;
            //     }
            // }
        }
    }
}
