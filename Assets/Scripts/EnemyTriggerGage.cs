using UnityEngine;

public class EnemyTriggerGage : MonoBehaviour
{
    float originPosY;
    void Start()
    {
        originPosY = this.transform.localPosition.y;
    }

    void Update()
    {
        Vector3 newPos = this.transform.localPosition;
        newPos.y = originPosY + this.transform.localScale.y * (float)TurnManager.Inst.enemyTriggerCnt / TurnManager.Inst.enemyTriggerMaxCnt;
        this.transform.localPosition = newPos;
    }
}
