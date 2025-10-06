using UnityEngine;

public class PassiveManager : MonoBehaviour
{
    public static PassiveManager Inst { get; private set; }
    private void Awake() => Inst = this;
    public void SetPersona()
    {
        switch (TurnManager.Inst.characterSO.personaPiece.persona.name)
        {
            case "Thicker than Water":
                TurnManager.Inst.playerTriggerMaxCnt = 3;
                TurnManager.OnRouletteActivate += () =>
                {
                    if (TurnManager.Inst.characterSO.personaPiece.persona.name == "Thicker than Water")
                    {
                        var playerPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat];
                        var enemyPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat];
                        if (enemyPiece.roulette.type == ERouletteType.Attack)
                        {
                            int damage = BuffManager.Inst.GetBuffedRouletteValue(enemyPiece);
                            damage = BuffManager.Inst.GetPlayerBuffValue(BuffManager.Inst.damageBuff, damage);
                            TurnManager.Inst.TakeDmg(-damage / 3);
                            TurnManager.Inst.TriggerPlayerPassive(1);
                        }
                        if (playerPiece.roulette.type == ERouletteType.Attack)
                        {
                            int damage = BuffManager.Inst.GetBuffedRouletteValue(playerPiece);
                            damage = BuffManager.Inst.GetPlayerBuffValue(BuffManager.Inst.damageBuff, damage);
                            TurnManager.Inst.TakeDmg(-damage / 3);
                            TurnManager.Inst.TriggerPlayerPassive(1);
                        }
                    }
                    else if (TurnManager.Inst.characterSO.personaPiece.persona.name == "Cursed Blood")
                    {
                        if (RouletteManager.Inst.playerLookat == RouletteManager.Inst.triggerPos || RouletteManager.Inst.enemyLookat == RouletteManager.Inst.triggerPos)
                        {
                            TurnManager.Inst.characterSO.personaPiece.persona.name = "Thicker than Water";
                            TurnManager.Inst.turnCost++;
                        }
                    }
                };
                TurnManager.OnRouletteTrigger += () =>
                {
                    TurnManager.Inst.characterSO.personaPiece.persona.name = "Cursed Blood";
                    TurnManager.Inst.turnCost--;
                };
                break;
        }
    }
    public void SetShadow()
    {
        switch (TurnManager.Inst.characterSO.shadowPiece.name)
        {
            case "Vampire's Dream":
                RouletteItem rItem = new RouletteItem();
                rItem.type = ERouletteType.Attack;
                rItem.value = 8;
                RouletteManager.Inst.triggerPiece = rItem;
                TurnManager.OnPlayerTurnEnd += () =>
                {
                    if (RouletteManager.Inst.roulettePieces[RouletteManager.Inst.triggerPos].isTriggered == true)
                    {
                        BuffManager.Inst.AddRouletteBuff(BuffManager.Inst.singleRouletteBuff_Trigger, TurnManager.Inst.nowCost * 8, 1, 1);
                    }
                };
                break;
        }
    }
}
