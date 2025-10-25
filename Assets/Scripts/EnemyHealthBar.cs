using UnityEngine;

public class EnemyHealthBar : MonoBehaviour
{
    float originSizeX;
    float originPosX;
    void Start()
    {
        originSizeX = this.transform.localScale.x;
        originPosX = this.transform.localPosition.x;
    }

    // Update is called once per frame
    void Update()
    {
        float newSizeX = originSizeX * TurnManager.Inst.enemyCurHealth / TurnManager.Inst.enemyMaxHealth;
        Vector3 newScale = this.transform.localScale;
        newScale.x = newSizeX;
        this.transform.localScale = newScale;
        Vector3 newPos = this.transform.localPosition;
        newPos.x = originPosX - (originSizeX - newSizeX) / 2;
        this.transform.localPosition = newPos;
    }
}
