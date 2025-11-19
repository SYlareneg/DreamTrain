using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PassiveManager : MonoBehaviour
{
    public static PassiveManager Inst { get; private set; }
    private void Awake() => Inst = this;
    
    public static Action<bool, int> PlayerSpecialRoulette1Activation;
    public static Action<int> PlayerSpecialRoulette1Clear;
    public static Action<bool, int> PlayerSpecialRoulette2Activation;
    public static Action<int> PlayerSpecialRoulette2Clear;
    public Sprite PlayerSpecialRoulette1Sprite;
    public string PlayerSpecialRoulette1Title;
    public string PlayerSpecialRoulette1Text;
    public Sprite PlayerSpecialRoulette2Sprite;
    public string PlayerSpecialRoulette2Title;
    public string PlayerSpecialRoulette2Text;

    public void SetPersona()
    {
        if (TurnManager.Inst.characterSO.personaPiece == null) return;
        PlayerSpecialRoulette1Sprite = TurnManager.Inst.characterSO.personaPiece.specialRouletteSprite;
        PlayerSpecialRoulette1Title = TurnManager.Inst.characterSO.personaPiece.specialRouletteTitle;
        PlayerSpecialRoulette1Text = TurnManager.Inst.characterSO.personaPiece.specialRouletteText;
        string personaName = "";
        if (TurnManager.Inst.characterSO.personaPiece.persona.isEnhanced) personaName = TurnManager.Inst.characterSO.personaPiece.persona.enhancedPassive.name;
        else personaName = TurnManager.Inst.characterSO.personaPiece.persona.name;
        switch (personaName)
        {
            case "물보다 진한 피":
            case "물보다 진한 피+":
                TurnManager.Inst.playerTriggerMaxCnt = 12;
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial1.Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_PlayerSpecial1.Add(new List<Buff>());
                };
                PlayerSpecialRoulette1Activation = (isEnemy, value) =>
                {
                    int trueDamage = 0;
                    bool isEnhanced = false;
                    if (isEnemy)
                    {
                        trueDamage = TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette);
                        isEnhanced = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].isEnhanced;
                    }
                    else
                    {
                        trueDamage = TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                        isEnhanced = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].isEnhanced;
                    }
                    int healVal = 0;
                    if (isEnhanced) healVal = trueDamage / 2;
                    else healVal = trueDamage / 3;
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_PlayerSpecial1[1], healVal);
                    TurnManager.Inst.TakeDmg(-totalVal_Heal, EDamageSource.Roulette);
                    TurnManager.Inst.TriggerPlayerPassive(totalVal_Heal);
                };
                TurnManager.OnRouletteActivate += () =>
                {
                    var playerPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat];
                    var enemyPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat];
                    if (enemyPiece.roulette.type == ERouletteType.Attack)
                    {
                        int damage = BuffManager.Inst.GetBuffedRouletteValue(enemyPiece);
                        int heal = 0;
                        if (personaName == "물보다 진한 피") heal = damage / 3;
                        else if (personaName == "물보다 진한 피+") heal = damage / 2;
                        TurnManager.Inst.TakeDmg(-heal, EDamageSource.Passive);
                        TurnManager.Inst.TriggerPlayerPassive(heal);
                    }
                    if (playerPiece.roulette.type == ERouletteType.Attack)
                    {
                        int damage = BuffManager.Inst.GetBuffedRouletteValue(playerPiece);
                        int heal = 0;
                        if (personaName == "물보다 진한 피") heal = damage / 3;
                        else if (personaName == "물보다 진한 피+") heal = damage / 2;
                        Debug.Log(heal);
                        TurnManager.Inst.TakeDmg(-heal, EDamageSource.Passive);
                        TurnManager.Inst.TriggerPlayerPassive(heal);
                    }
                };
                break;
            case "카드 숨기기":
            case "카드 숨기기+":
                TurnManager.Inst.playerTriggerMaxCnt = 99;
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial1.Add(new List<Buff>());
                };
                TurnManager.OnGameStart += () =>
                {
                    Item ace = new Item();
                    ace.name = "에이스";
                    if (personaName == "카드 숨기기+") ace.name += "+";
                    ace.cost = 1;
                    ace.type = CardType.Effect;
                    ace.element = EPassiveType.Normal;
                    ace.dreamPieceNum = -1;
                    ace.isVolatile = false;
                    ace.isVanish = false;
                    if (personaName == "카드 숨기기+") ace.isRemain = true;
                    else if (personaName == "카드 숨기기") ace.isRemain = false;
                    ace.text = "트리거 게이지를 최대로 얻습니다. 이번 턴이 종료될 때 12번 슬롯을 비활성화합니다.";
                    if (personaName == "카드 숨기기+") ace.text += " <color=red>잔류</color>";
                    ace.cardValues = new List<(int, ECardValueType)>();
                    ace.num = 1;
                    CardManager.Inst.itemDeck.Add(ace);
                    CardManager.Inst.itemDraw.Add(ace);
                    CardManager.Inst.ShuffleDeck();
                };
                TurnManager.OnRouletteSpin += (x) =>
                {
                    if (RouletteManager.Inst.spinDirection == 1)
                    {
                        for (int i = 0; i < RouletteManager.rouletteNum; i++)
                        {
                            if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Player_Special_1)
                            {
                                RouletteManager.Inst.roulettePieces[i].roulette.value--;
                                int val = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                                if (val == 0)
                                {
                                    PlayerSpecialRoulette1Clear?.Invoke(i);
                                }
                            }
                        }
                    }
                };
                PlayerSpecialRoulette1Clear = (index) =>
                {
                    RouletteItem tempItem = new RouletteItem();
                    tempItem.type = ERouletteType.None;
                    tempItem.value = 0;
                    RouletteManager.Inst.roulettePieces[index].Setup(tempItem);
                };
                break;
            case "순환하는 계절":
            case "순환하는 계절+":
                TurnManager.Inst.playerTriggerMaxCnt = 12;
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial1.Add(new List<Buff>());
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if (TurnManager.Inst.turnNum % 4 == 0)
                    {
                        if (personaName == "순환하는 계절") TurnManager.Inst.TriggerPlayerPassive(6);
                        else TurnManager.Inst.TriggerPlayerPassive(8);
                    }
                };
                List<(int rIdx, RouletteItem rItem, GameObject frozenIcon)> frozenRoulettes = new List<(int, RouletteItem, GameObject)>();
                TurnManager.CheckRouletteEnchantable += (index, type) =>
                {
                    var frozenChk = frozenRoulettes.Find(x => x.rIdx == index);
                    if (frozenChk.rItem != null && RouletteManager.Inst.roulettePieces[index].roulette.type == ERouletteType.Player_Special_1)
                    {
                        return false;
                    }
                    if(frozenChk.rItem == null && type == ERouletteType.Player_Special_1)
                    {
                        GameObject frozenSprite = new GameObject("FrozenIcon");
                        frozenRoulettes.Add((index, RouletteManager.Inst.roulettePieces[index].roulette, frozenSprite));
                        frozenSprite.transform.SetParent(RouletteManager.Inst.roulettePieces[index].transform);
                        frozenSprite.transform.localPosition = Vector3.zero;
                        frozenSprite.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
                        SpriteRenderer frozenSpriteRenderer = frozenSprite.AddComponent<SpriteRenderer>();
                        frozenSpriteRenderer.sortingOrder = RouletteManager.Inst.roulettePieces[index].GetComponent<SpriteRenderer>().sortingOrder + 1;
                        frozenSpriteRenderer.sprite = TurnManager.Inst.characterSO.personaPiece.specialRouletteSprite;
                        PlayerSpecialRoulette1Sprite = RouletteManager.Inst.roulettePieces[index].roulettePiece.sprite;
                    }
                    return true;
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    for (int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Player_Special_1)
                        {
                            RouletteManager.Inst.roulettePieces[i].roulette.value--;
                            int val = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                            if (val == 0)
                            {
                                PlayerSpecialRoulette1Clear?.Invoke(i);
                            }
                        }
                    }
                };
                PlayerSpecialRoulette1Clear = (index) =>
                {
                    var frozenChk = frozenRoulettes.Find(x => x.rIdx == index);
                    if (frozenChk.rItem != null)
                    {
                        RouletteManager.Inst.roulettePieces[index].Setup(frozenChk.rItem);
                        Destroy(frozenChk.frozenIcon);
                        frozenRoulettes.Remove(frozenChk);
                    }
                    else
                    {
                        RouletteItem tempItem = new RouletteItem();
                        tempItem.type = ERouletteType.None;
                        tempItem.value = 0;
                        RouletteManager.Inst.roulettePieces[index].Setup(tempItem);
                    }
                };
                break;
        }
    }
    public void SetShadow()
    {
        if (TurnManager.Inst.characterSO.shadowPiece == null) return;
        PlayerSpecialRoulette2Sprite = TurnManager.Inst.characterSO.shadowPiece.specialRouletteSprite;
        PlayerSpecialRoulette2Title = TurnManager.Inst.characterSO.shadowPiece.specialRouletteTitle;
        PlayerSpecialRoulette2Text = TurnManager.Inst.characterSO.shadowPiece.specialRouletteText;
        RouletteItem rItem = new RouletteItem();
        string shadowName = "";
        if (TurnManager.Inst.characterSO.shadowPiece.shadow.isEnhanced) shadowName = TurnManager.Inst.characterSO.shadowPiece.shadow.enhancedPassive.name;
        else shadowName = TurnManager.Inst.characterSO.shadowPiece.shadow.name;
        switch (shadowName)
        {
            case "해방된 본능":
            case "해방된 본능+":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial2.Add(new List<Buff>());
                    BuffManager.Inst.rouletteBuff_PlayerSpecial2.Add(new List<Buff>());
                };
                PlayerSpecialRoulette2Activation = (isEnemy, value) =>
                {
                    int trueDamage = 0;
                    bool isEnhanced = false;
                    if (isEnemy)
                    {
                        trueDamage = TurnManager.Inst.EnemyTakeDmg(value, EDamageSource.Roulette);
                        isEnhanced = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].isEnhanced;
                    }
                    else
                    {
                        trueDamage = TurnManager.Inst.TakeDmg(value, EDamageSource.Roulette);
                        isEnhanced = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].isEnhanced;
                    }
                    int healVal = 0;
                    if (isEnhanced) healVal = trueDamage / 2;
                    else healVal = trueDamage / 3;
                    int totalVal_Heal = BuffManager.GetTargetBuffedValue(BuffManager.Inst.rouletteBuff_PlayerSpecial2[1], healVal);
                    TurnManager.Inst.TakeDmg(-totalVal_Heal, EDamageSource.Roulette);
                };
                rItem.type = ERouletteType.Attack;
                rItem.value = 8;
                RouletteManager.Inst.triggerPiece = rItem;
                RouletteManager.PlayerTriggerActivation = (isEnemy, totalVal) =>
                {
                    if (isEnemy)
                    {
                        TurnManager.Inst.EnemyTakeDmg(totalVal, EDamageSource.Roulette);
                    }
                    else
                    {
                        TurnManager.Inst.TakeDmg(totalVal, EDamageSource.Roulette);
                    }
                };
                TurnManager.OnPlayerTrigger += () =>
                {
                    BuffManager.Inst.rouletteBuff_Trigger.Clear();
                    if (shadowName == "해방된 본능") BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, TurnManager.Inst.nowCost * 8, 1, -1);
                    else if (shadowName == "해방된 본능+")  BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, TurnManager.Inst.nowCost * 12, 1, -1);
                };
                TurnManager.OnCostChange += (x) =>
                {
                    if (RouletteManager.Inst.isTriggerActivated && RouletteManager.Inst.isPlayerTrigger())
                    {
                        if (shadowName == "해방된 본능") BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, TurnManager.Inst.nowCost * 8, 1, -1);
                        else if (shadowName == "해방된 본능+")  BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, TurnManager.Inst.nowCost * 12, 1, -1);
                    }
                };
                break;
            case "마술 해체":
            case "마술 해체+":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial2.Add(new List<Buff>());
                };
                rItem.type = ERouletteType.Attack;
                rItem.value = 0;
                RouletteManager.Inst.triggerPiece = rItem;
                RouletteManager.PlayerTriggerActivation = (isEnemy, totalVal) =>
                {
                    if (isEnemy)
                    {
                        TurnManager.Inst.EnemyTakeDmg(totalVal, EDamageSource.Roulette);
                    }
                    else
                    {
                        TurnManager.Inst.TakeDmg(totalVal, EDamageSource.Roulette);
                    }
                };
                int counter = 0;
                TurnManager.OnPlayerTrigger += () =>
                {
                    BuffManager.Inst.rouletteBuff_Trigger.Clear();
                    counter = RouletteManager.rouletteNum - 1;
                    counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.None);
                    counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.Attack);
                    counter -= RouletteManager.Inst.CountRouletteType(ERouletteType.Shield);
                    if (shadowName == "마술 해체+" && counter >= 6) BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, counter * 7, 1.5f, -1);
                    else BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, counter * 7, 1, -1);
                };
                TurnManager.OnRouletteEnchant += (x) =>
                {
                    if (RouletteManager.Inst.isTriggerActivated && RouletteManager.Inst.isPlayerTrigger())
                    {
                        int newCnt = RouletteManager.rouletteNum - 1;
                        newCnt -= RouletteManager.Inst.CountRouletteType(ERouletteType.None);
                        newCnt -= RouletteManager.Inst.CountRouletteType(ERouletteType.Attack);
                        newCnt -= RouletteManager.Inst.CountRouletteType(ERouletteType.Shield);
                        if (newCnt != counter)
                        {
                            if (shadowName == "마술 해체+" && counter >= 6 && newCnt < 6) BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, (newCnt - counter) * 7, 2.0f / 3, -1);
                            else if (shadowName == "마술 해체+" && counter < 6 && newCnt >= 6) BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, (newCnt - counter) * 7, 1.5f, -1);
                            else BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, (newCnt - counter) * 7, 1, -1);
                            counter = newCnt;
                        }
                    }
                };
                TurnManager.BeforeRouletteActivate += () =>
                {
                    if (RouletteManager.Inst.isTriggerActivated && RouletteManager.Inst.isPlayerTrigger())
                    {
                        if (RouletteManager.Inst.playerLookat == RouletteManager.Inst.triggerPos || RouletteManager.Inst.enemyLookat == RouletteManager.Inst.triggerPos)
                        {
                            for (int i = 0; i < RouletteManager.rouletteNum; i++)
                            {
                                if(i != RouletteManager.Inst.triggerPos && RouletteManager.Inst.roulettePieces[i].roulette.type != ERouletteType.None && RouletteManager.Inst.roulettePieces[i].roulette.type != ERouletteType.Attack && RouletteManager.Inst.roulettePieces[i].roulette.type != ERouletteType.Shield)
                                {
                                    RouletteManager.Inst.EnchantRoulettePiece(i, ERouletteType.None, 0);
                                    BuffManager.AddBuffToTarget(BuffManager.Inst.rouletteBuff_Trigger, 7, 1, -1);
                                }
                            }
                        }
                    }
                };
                TurnManager.OnRouletteSpin += (x) =>
                {
                    if (RouletteManager.Inst.spinDirection == 1)
                    {
                        for (int i = 0; i < RouletteManager.rouletteNum; i++)
                        {
                            if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Player_Special_2)
                            {
                                RouletteManager.Inst.roulettePieces[i].roulette.value--;
                                int val = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                                if (val == 0)
                                {
                                    RouletteItem tempItem = new RouletteItem();
                                    tempItem.type = ERouletteType.None;
                                    tempItem.value = 0;
                                    RouletteManager.Inst.roulettePieces[i].Setup(tempItem);
                                }
                            }
                        }
                    }
                };
                break;
            case "겨울 바람":
            case "겨울 바람+":
                BuffManager.InitSpecialRouletteBuffs += () =>
                {
                    BuffManager.Inst.rouletteBuff_PlayerSpecial2.Add(new List<Buff>());
                };
                rItem.type = ERouletteType.None;
                rItem.value = 0;
                if(shadowName == "겨울 바람+") 
                {
                    rItem.type = ERouletteType.Attack;
                    rItem.value = 12;
                }
                RouletteManager.Inst.triggerPiece = rItem;
                RouletteManager.PlayerTriggerActivation = (isEnemy, totalVal) =>
                {
                    for(int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if(RouletteManager.Inst.roulettePieces[i].roulette.type != ERouletteType.None)
                        {
                            RouletteManager.Inst.EnchantRoulettePiece(i, ERouletteType.Player_Special_2, 2);
                        }
                    }
                    for(int i = 0; i < EnemyManager.Inst.actionList.Count; i++)
                    {
                        EnemyManager.Inst.RemoveAction(i);
                    }
                    if(shadowName == "겨울 바람+")
                    {
                        if (isEnemy)
                        {
                            TurnManager.Inst.EnemyTakeDmg(totalVal, EDamageSource.Roulette);
                        }
                        else
                        {
                            TurnManager.Inst.TakeDmg(totalVal, EDamageSource.Roulette);
                        }
                    }
                };
                List<(int rIdx, RouletteItem rItem, GameObject frozenIcon)> frozenRoulettes = new List<(int, RouletteItem, GameObject)>();
                TurnManager.CheckRouletteEnchantable += (index, type) =>
                {
                    var frozenChk = frozenRoulettes.Find(x => x.rIdx == index);
                    if (frozenChk.rItem != null && RouletteManager.Inst.roulettePieces[index].roulette.type == ERouletteType.Player_Special_2)
                    {
                        return false;
                    }
                    if(frozenChk.rItem == null && type == ERouletteType.Player_Special_2)
                    {
                        GameObject frozenSprite = new GameObject("FrozenIcon");
                        frozenRoulettes.Add((index, RouletteManager.Inst.roulettePieces[index].roulette, frozenSprite));
                        frozenSprite.transform.SetParent(RouletteManager.Inst.roulettePieces[index].transform);
                        frozenSprite.transform.localPosition = Vector3.zero;
                        frozenSprite.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
                        SpriteRenderer frozenSpriteRenderer = frozenSprite.AddComponent<SpriteRenderer>();
                        frozenSpriteRenderer.sortingOrder = RouletteManager.Inst.roulettePieces[index].GetComponent<SpriteRenderer>().sortingOrder + 1;
                        frozenSpriteRenderer.sprite = TurnManager.Inst.characterSO.shadowPiece.specialRouletteSprite;
                        PlayerSpecialRoulette2Sprite = RouletteManager.Inst.roulettePieces[index].roulettePiece.sprite;
                    }
                    return true;
                };
                TurnManager.OnPlayerTurnStart += () =>
                {
                    for (int i = 0; i < RouletteManager.rouletteNum; i++)
                    {
                        if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Player_Special_2)
                        {
                            RouletteManager.Inst.roulettePieces[i].roulette.value--;
                            int val = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                            if (val == 0)
                            {
                                PlayerSpecialRoulette2Clear?.Invoke(i);
                            }
                        }
                    }
                };
                PlayerSpecialRoulette2Clear = (index) =>
                {
                    var frozenChk = frozenRoulettes.Find(x => x.rIdx == index);
                    if (frozenChk.rItem != null)
                    {
                        RouletteManager.Inst.roulettePieces[index].Setup(frozenChk.rItem);
                        Destroy(frozenChk.frozenIcon);
                        frozenRoulettes.Remove(frozenChk);
                    }
                    else
                    {
                        RouletteItem tempItem = new RouletteItem();
                        tempItem.type = ERouletteType.None;
                        tempItem.value = 0;
                        RouletteManager.Inst.roulettePieces[index].Setup(tempItem);
                    }
                };
                break;
        }
    }

    private void OnDestroy()
    {
        PlayerSpecialRoulette1Activation = null;
        PlayerSpecialRoulette2Activation = null;
    }
}
