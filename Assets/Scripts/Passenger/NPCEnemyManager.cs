using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class NPCEnemyManager : MonoBehaviour
{
    public static NPCEnemyManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] CharacterSO characterSO;
    [SerializeField] EnemySO enemySO;
    [SerializeField] StageSO stageSO;

    public void EncounterEnemy(EnemyNPC npc)
    {
        if(enemySO.enemies.Find(x => x.name == npc.stageEnemy.enemyName) == null)
        {
            Debug.LogError("wrong enemy name!!");
            return;
        }

        characterSO.enemyName = npc.stageEnemy.enemyName;
        SceneChangeManager.Inst.SceneFadeOut("BattleScene");
    }
}
