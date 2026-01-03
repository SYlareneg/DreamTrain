using UnityEngine;

public class StatManager : MonoBehaviour
{
    public static StatManager Inst;
    void Awake() => Inst = this;

    [SerializeField] PlayerStatsSo playerStatsSO;

    public void SetStatEffect()
    {
        TurnManager.OnGameStart += () =>
        {
            TurnManager.Inst.GetShield(false, playerStatsSO.GetStartBattleShield(), EDamageSource.Stats);
            TurnManager.Inst.turnCost += playerStatsSO.GetExtraActionPoints();
            TurnManager.Inst.drawCardCount += playerStatsSO.GetExtraDrawCount();
        };
        TurnManager.OnGameEnd += (isWin) =>
        {
            if (isWin)
            {
                TurnManager.Inst.TakeDmg(-playerStatsSO.GetBattleEndHealAmount(), EDamageSource.Stats);
                GameManager.Inst.rewardCardWeights[(int)CardRarity.Rare + 1] *= playerStatsSO.GetRareCardChanceMultiplier();
            }
        };
    }
}
