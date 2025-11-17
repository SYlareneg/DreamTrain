using UnityEngine;

public class EnemyNPC : PlayerInteractableObject
{
    public StageEnemy stageEnemy;
    public void Setup(StageEnemy se)
    {
        stageEnemy = se;
        GetComponent<SpriteRenderer>().sprite = stageEnemy.enemySprite;
        transform.position = stageEnemy.enemyPos;
    }
    public override void Interact()
    {
        NPCEnemyManager.Inst.EncounterEnemy(this);
    }
}
