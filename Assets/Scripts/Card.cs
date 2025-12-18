using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using DG.Tweening;
using System.Text.RegularExpressions;

public class Card : MonoBehaviour
{
    [SerializeField] SpriteRenderer card;
    [SerializeField] SpriteRenderer character;
    [SerializeField] SpriteRenderer element;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text costTMP;
    public TMP_Text textTMP;
    [SerializeField] Sprite[] cardTypes;
    [SerializeField] Sprite[] elementTypes;

    public Item item;
    public PRS originPRS;

    public void Setup(Item item)
    {
        if (item == null)
        {
            this.item = null;
            return;
        }

        this.item = item;

        switch (this.item.type)
        {
            case CardType.Turn:
                card.sprite = cardTypes[0]; break;
            case CardType.Enchant:
                card.sprite = cardTypes[1]; break;
            case CardType.Effect:
                card.sprite = cardTypes[2]; break;
        }

        character.sprite = Utils.LoadSpriteByName("Cards", this.item.sprite);

        switch (this.item.element)
        {
            case EPassiveType.Normal:
                element.sprite = elementTypes[0]; break;
            case EPassiveType.Persona:
                element.sprite = elementTypes[1]; break;
            case EPassiveType.Shadow:
                element.sprite = elementTypes[2]; break;
        }

        nameTMP.text = this.item.name;
        ShowBuffedCost();

        string showText = this.item.text;
        int index = 0;
        if (this.item.cardValues.Count == 0)
        {
            string itemText = Regex.Replace(this.item.text, @"(\d+)(<(피해|수비|회복|특수)>)?", match =>
            {
                ECardValueType tempType = ECardValueType.Default;
                switch(match.Groups[2].Value)
                {
                    case "피해":
                        tempType = ECardValueType.Damage; break;
                    case "수비":
                        tempType = ECardValueType.Shield; break;
                    case "회복":
                        tempType = ECardValueType.Heal; break;
                    case "특수":
                        tempType = ECardValueType.Special; break;
                    default:
                        tempType = ECardValueType.Default; break;
                }
                this.item.cardValues.Add(int.Parse(match.Groups[1].Value));
                this.item.cardValueTypes.Add(tempType);
                index++;
                return match.Value;
            });
            showText = $"{itemText}";
        }
        ShowBuffedVal();
    }

    public void ShowBuffedCost()
    {
        int buffedCost = BuffManager.Inst.GetBuffedCardCost(this.item);
        costTMP.text = buffedCost.ToString();

        if (buffedCost > this.item.cost)
        {
            costTMP.color = Color.red;
        }
        else if (buffedCost == this.item.cost)
        {
            costTMP.color = Color.black;
        }
        else
        {
            costTMP.color = Color.green;
        }
    }

    public void ShowBuffedVal()
    {
        if (this.item.cardValues.Count == 0) return;
        string showText = this.item.text;
        int index = 0;
        showText = Regex.Replace(showText, @"(\d+)(<(피해|수비|회복|특수)>)?", match => 
        {
            ECardValueType tempType = this.item.cardValueTypes[index];
            int buffedVal = GetBuffedVal(this.item.cardValues[index], tempType);
            string returnString = "NaN";
            if(buffedVal > this.item.cardValues[index]) returnString = "<color=green>" + buffedVal.ToString() + "</color>";
            else if(buffedVal < this.item.cardValues[index]) returnString = "<color=red>" + buffedVal.ToString() + "</color>";
            else returnString = "<color=black>" + buffedVal.ToString() + "</color>";
            index++;
            return returnString;
        });
        textTMP.text = $"{showText}";
    }

    public int GetBuffedVal(int originVal, ECardValueType valType = ECardValueType.Default)
    {
        int retVal = originVal;
        switch (valType)
        {
            case ECardValueType.Damage:
                retVal = BuffManager.Inst.GetBuffedEnemyDamage(EDamageSource.Card, originVal);
                break;
            case ECardValueType.Heal:
                retVal = BuffManager.Inst.GetBuffedPlayerHeal(EDamageSource.Card, originVal);
                break;
            case ECardValueType.Shield:
                retVal = BuffManager.Inst.GetBuffedPlayerShield(EDamageSource.Card, originVal);
                break;
        }
        return retVal;
    }

    private void OnMouseOver()
    {
        CardManager.Inst.CardMouseOver(this);
    }

    private void OnMouseExit()
    {
        CardManager.Inst.CardMouseExit(this);
    }

    private void OnMouseDown()
    {
        CardManager.Inst.CardMouseDown(this);
    }

    private void OnMouseUp()
    {
        CardManager.Inst.CardMouseUp(this);
    }

    public void MoveTransform(PRS prs, bool useDotween, float dotweenTime = 0)
    {
        if (useDotween)
        {
            transform.DOMove(prs.pos, dotweenTime);
            transform.DORotateQuaternion(prs.rot, dotweenTime);
            transform.DOScale(prs.scale, dotweenTime);
        }
        else
        {
            transform.position = prs.pos;
            transform.rotation = prs.rot;
            transform.localScale = prs.scale;
        }
    }

    public bool UseCard(bool isMine)
    {
        int buffedCost = BuffManager.Inst.GetBuffedCardCost(item);
        if (buffedCost > TurnManager.Inst.nowCost)
        {
            return false;
        }
        bool isCardUsed = true;
        switch (item.name)
        {
            case "회전 카드 1":
                RouletteManager.Inst.Spin(true, 1);
                //CardManager.Inst.CardSelectModeTransit(ECardSelectMode.Duplicate, 1);
                break;
            case "회전 카드 2":
                RouletteManager.Inst.Spin(true, 2);
                break;
            case "회전 카드 3":
                RouletteManager.Inst.Spin(true, 3);
                break;
            case "회전 카드 4":
                RouletteManager.Inst.Spin(false, 2);
                break;
            case "회전 카드 5":
                RouletteManager.Inst.Spin(false, 3);
                break;
            case "구원":
                TurnManager.Inst.TakeDmg(-TurnManager.Inst.maxHealth / 2, EDamageSource.Card);
                break;
            case "혼령":
                TurnManager.Inst.IncreaseCost(2);
                TurnManager.Inst.TakeDmg(2, EDamageSource.Card);
                break;
            case "회복 부여":
                isCardUsed = RouletteManager.Inst.EnchantRoulette(false, ERouletteType.Heal, 3);
                break;
            case "흡혈 부여":
            case "흡혈 부여+":
                ERouletteType bloodSteal = ERouletteType.Player_Special_1;
                if (TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum != this.item.dreamPieceNum) bloodSteal = ERouletteType.Player_Special_2;
                isCardUsed = RouletteManager.Inst.EnchantRoulette(true, bloodSteal, 6);
                if (item.name == "흡혈 부여+" && isCardUsed)
                {
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].isEnhanced = true;
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].tooltip.tooltipTxt = "값의 데미지를 주고 입힌 피해의 <color=red>50%</color>만큼 체력을 회복합니다. 뱀파이어 페르소나를 장착하고 있다면, 회복한 체력만큼 트리거 게이지를 얻습니다.";
                }
                break;
            case "혈액 순환":
            case "혈액 순환+":
                RouletteManager.Inst.Spin(true, item.cardValues[0]);
                TurnManager.Inst.TakeDmg(item.cardValues[1], EDamageSource.Card);
                Item tempItem = new Item();
                tempItem.SetItem(item);
                tempItem.name = "혈액 순환 (소멸)";
                tempItem.isVanish = true;
                for(int i = 0; i < item.cardValues[2]; i++)
                {
                    CardManager.Inst.CreateCardInHand(tempItem);
                }
                break;
            case "혈액 순환 (소멸)":
                RouletteManager.Inst.Spin(true, item.cardValues[0]);
                TurnManager.Inst.TakeDmg(item.cardValues[1], EDamageSource.Card);
                break;
            case "피는 나의 힘":
            case "피는 나의 힘+":
                TurnManager.Inst.TakeDmg(item.cardValues[0], EDamageSource.Card);
                TurnManager.Inst.IncreaseCost(item.cardValues[1]);
                break;
            case "긴급 수혈":
            case "긴급 수혈+":
                TurnManager.Inst.TakeDmg(-GetBuffedVal(item.cardValues[0], ECardValueType.Heal), EDamageSource.Card);
                break;
            case "휴머니스트":
            case "휴머니스트+":
                BuffManager.Inst.AddShowBuff("주저함", EBuffAffectType.Enemy, item.cardValues[0], false);
                BuffManager.Inst.AddShowBuff("주저함", EBuffAffectType.Roulette, item.cardValues[0], false);
                break;
            case "블루 블러드":
            case "블루 블러드+":
                BuffManager.Inst.AddShowBuff("블루 블러드", EBuffAffectType.Roulette, item.cardValues[0], false, new List<float>{(float)item.cardValues[1], 0f});
                break;
            case "만찬 시간":
            case "만찬 시간+":
                BuffManager.Inst.AddShowBuff("만찬 시간", EBuffAffectType.Roulette, item.cardValues[0], false, new List<float>{(float)item.cardValues[1]});
                break;
            case "핏빛 날개":
            case "핏빛 날개+":
                int bloodwing_spinnnum = item.cardValues[0];
                for (int i = 0; i <= bloodwing_spinnnum; i++)
                {
                    int tempIdx = (RouletteManager.Inst.enemyLookat + i) % RouletteManager.rouletteNum;
                    if (RouletteManager.Inst.roulettePieces[tempIdx].roulette.type == ERouletteType.Player_Special_2)
                    {
                        RouletteManager.Inst.ActivateRoulettePiece(tempIdx, true);
                    }
                }
                RouletteManager.Inst.Spin(false, bloodwing_spinnnum);
                break;
            case "마술 상자":
            case "마술 상자+":
                ERouletteType magicBox = ERouletteType.Player_Special_1;
                if (TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum != this.item.dreamPieceNum) magicBox = ERouletteType.Player_Special_2;
                if (item.name == "마술 상자") isCardUsed = RouletteManager.Inst.EnchantRoulette(false, magicBox, 12);
                else isCardUsed = RouletteManager.Inst.EnchantRoulette(false, magicBox, 15);
                break;
            case "마술-비둘기":
            case "마술-비둘기+":
                magicBox = ERouletteType.Player_Special_1;
                if (TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum != this.item.dreamPieceNum) magicBox = ERouletteType.Player_Special_2;
                bool checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].roulette.type == magicBox;
                TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card);
                if (checkMagic)
                {
                    EnemyManager.Inst.RemoveAction(0);
                }
                break;
            case "마술-카드":
            case "마술-카드+":
                magicBox = ERouletteType.Player_Special_1;
                if (TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum != this.item.dreamPieceNum) magicBox = ERouletteType.Player_Special_2;
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.type == magicBox;
                RouletteManager.Inst.Spin(true, item.cardValues[0]);
                if (checkMagic)
                {
                    Debug.Log("duplicate mode on");
                    CardManager.Inst.CardSelectModeTransit(ECardSelectMode.Duplicate, item.cardValues[1]);
                }
                break;
            case "마술-절단":
                magicBox = ERouletteType.Player_Special_1;
                if (TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum != this.item.dreamPieceNum) magicBox = ERouletteType.Player_Special_2;
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].roulette.type == magicBox;
                TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card);
                if (checkMagic)
                {
                    TurnManager.Inst.enemyShieldHealth = 0;
                }
                break;
            case "마술-절단+":
                magicBox = ERouletteType.Player_Special_1;
                if (TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum != this.item.dreamPieceNum) magicBox = ERouletteType.Player_Special_2;
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].roulette.type == magicBox;
                if (checkMagic)
                {
                    TurnManager.Inst.enemyShieldHealth = 0;
                }
                TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card);
                break;
            case "마술-순간이동":
            case "마술-순간이동+":
                magicBox = ERouletteType.Player_Special_1;
                if (TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum != this.item.dreamPieceNum) magicBox = ERouletteType.Player_Special_2;
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.type == magicBox;
                RouletteManager.Inst.Spin(true, item.cardValues[0]);
                if (checkMagic)
                {
                    TurnManager.Inst.GetShield(false, GetBuffedVal(item.cardValues[1], ECardValueType.Shield), EDamageSource.Card);
                }
                break;
            case "마술-예언":
            case "마술-예언+":
                BuffManager.Inst.AddShowBuff("예언-준비", EBuffAffectType.Player, 1, false);
                break;
            case "재빠른 손놀림":
            case "재빠른 손놀림+":
                StartCoroutine(TurnManager.Inst.Draw(item.cardValues[0], null));
                break;
            case "초능력-예언":
            case "초능력-예언+":
                BuffManager.Inst.AddShowBuff("예언-준비", EBuffAffectType.Player, 1, false);
                break;
            case "초능력-염력":
            case "초능력-염력+":
                RouletteManager.Inst.Spin(true, TurnManager.Inst.nowCost * item.cardValues[0]);
                if (item.name == "초능력-염력+" && TurnManager.Inst.nowCost >= item.cardValues[1]) TurnManager.Inst.GetShield(false, GetBuffedVal(item.cardValues[2], ECardValueType.Shield), EDamageSource.Card);
                break;
            case "에이스":
            case "에이스+":
                RouletteManager.Inst.TriggerRoulette();
                Action endTrigger = null;
                endTrigger = () =>
                {
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.triggerPos].Trigger(false);
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.triggerPos].Setup(RouletteManager.Inst.triggerPiece_None);
                    TurnManager.OnPlayerTurnEnd -= endTrigger;
                };
                TurnManager.OnPlayerTurnEnd += endTrigger;
                break;
            case "꽁꽁 얼리기":
            case "꽁꽁 얼리기+":
                ERouletteType frozen = ERouletteType.Player_Special_1;
                if (TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum != this.item.dreamPieceNum) frozen = ERouletteType.Player_Special_2;
                isCardUsed = RouletteManager.Inst.EnchantRoulette(false, frozen, item.cardValues[0]);
                break;
            case "얼음 방패":
            case "얼음 방패+":
                frozen = ERouletteType.Player_Special_1;
                if (TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum != this.item.dreamPieceNum) frozen = ERouletteType.Player_Special_2;
                TurnManager.Inst.GetShield(false, GetBuffedVal(item.cardValues[0], ECardValueType.Shield), EDamageSource.Card);
                bool checkFrozen = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.type == frozen;
                if (checkFrozen)
                {
                    TurnManager.Inst.GetShield(false, GetBuffedVal(item.cardValues[1], ECardValueType.Shield), EDamageSource.Card);
                }
                break;
            case "나뭇가지 손":
            case "나뭇가지 손+":
                StartCoroutine(TurnManager.Inst.Draw(item.cardValues[0], null));
                TurnManager.Inst.TriggerPlayerPassive(-item.cardValues[1]);
                break;
            case "데굴데굴":
            case "데굴데굴+":
                RouletteManager.Inst.Spin(true, item.cardValues[0]);
                if (item.name == "데굴데굴+") TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[1], ECardValueType.Damage), EDamageSource.Card);
                Action<int> repeatCard = null;
                repeatCard = (x) =>
                {
                    Sequence wait = DOTween.Sequence();
                    wait.AppendInterval(0.5f).OnComplete(() =>
                    {
                        if (TurnManager.Inst.nowCost >= buffedCost)
                        {
                            TurnManager.Inst.IncreaseCost(-buffedCost);
                            RouletteManager.Inst.Spin(true, item.cardValues[0]);
                            if (item.name == "데굴데굴+") TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[1], ECardValueType.Damage), EDamageSource.Card);
                        }
                        TurnManager.AfterRouletteSpin -= repeatCard;
                    });
                };
                TurnManager.AfterRouletteSpin += repeatCard;
                break;
            case "차가운 악수":
            case "차가운 악수+":
                frozen = ERouletteType.Player_Special_1;
                if (TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum != this.item.dreamPieceNum) frozen = ERouletteType.Player_Special_2;
                isCardUsed = RouletteManager.Inst.EnchantRoulette(true, frozen, item.cardValues[0]);
                if (isCardUsed)
                {
                    BuffManager.Inst.AddShowBuff("과민함", EBuffAffectType.Enemy, item.cardValues[1], false);
                }
                break;
            case "스노우볼링":
            case "스노우볼링+":
                RouletteManager.Inst.Spin(true, item.cardValues[0]);
                TurnManager.Inst.GetShield(false, TurnManager.Inst.shieldHealth * (item.cardValues[1] - 1), EDamageSource.Card);
                break;
            case "녹아내리기":
            case "녹아내리기+":
                TurnManager.Inst.TriggerPlayerPassive(-item.cardValues[0]);
                TurnManager.Inst.GetShield(false, GetBuffedVal(item.cardValues[1], ECardValueType.Shield), EDamageSource.Card);
                break;
            case "목도리":
            case "목도리+":
                for (int i = 0; i <= item.cardValues[0]; i++)
                {
                    int tempIdx = (RouletteManager.Inst.playerLookat + RouletteManager.rouletteNum - i) % RouletteManager.rouletteNum;
                    Debug.Log(RouletteManager.Inst.roulettePieces[tempIdx].roulette.type);
                    if (RouletteManager.Inst.roulettePieces[tempIdx].roulette.type == ERouletteType.Player_Special_1)
                    {
                        TurnManager.Inst.GetShield(false, GetBuffedVal(item.cardValues[1], ECardValueType.Shield), EDamageSource.Card);
                    }
                }
                RouletteManager.Inst.Spin(true, item.cardValues[0]);
                break;
            case "얼음 깨기":
            case "얼음 깨기+":
                if (RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].roulette.type == ERouletteType.Player_Special_1)
                {
                    TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card);
                }
                for (int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Player_Special_1)
                    {
                        PassiveManager.PlayerSpecialRoulette1Clear?.Invoke(i);
                    }
                }
                break;
            case "눈싸움":
            case "눈싸움+":
                int frozenCnt = 0;
                for (int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Player_Special_2)
                    {
                        frozenCnt++;
                    }
                }
                TurnManager.Inst.EnemyTakeDmg(frozenCnt * item.cardValues[0], EDamageSource.Card);
                break;
            case "폭설":
            case "폭설+":
                for (int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.None)
                    {
                        RouletteManager.Inst.EnchantRoulettePiece(i, ERouletteType.Player_Special_2, item.cardValues[0]);
                    }
                }
                break;
            case "끝나지 않는 겨울":
            case "끝나지 않는 겨울+":
                int frozenTimeInc = item.cardValues[0];
                for (int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if (RouletteManager.Inst.roulettePieces[i].roulette.type == ERouletteType.Player_Special_2)
                    {
                        RouletteManager.Inst.roulettePieces[i].roulette.value += frozenTimeInc;
                    }
                }
                break;
        }
        if (isCardUsed)
        {
            Debug.Log(item.name + " 카드 사용!");
            TurnManager.Inst.IncreaseCost(-buffedCost);
            Utils.AllignActions(ref TurnManager.OnUseCard, typeof(ShowBuff), typeof(RelicManager));
            TurnManager.OnUseCard?.Invoke();
        }
        return isCardUsed;
    }

    private void Update()
    {
        ShowBuffedCost();
        ShowBuffedVal();
    }
}
