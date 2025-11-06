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

        character.sprite = this.item.sprite;

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

        if (this.item.cardValues.Count == 0)
        {
            int index = 0;
            string itemText = Regex.Replace(this.item.text, @"\d+", match =>
            {
                this.item.cardValues.Add(int.Parse(match.Value));
                string replacement = $"{{cardValues[{index}]}}";
                index++;
                return replacement;
            });
            this.item.text = $"{itemText}";
        }
        string showText = this.item.text;
        for (int i = 0; i < this.item.cardValues.Count; i++)
        {
            showText = Regex.Replace(showText, @"\{cardValues\[" + i + @"\]\}", this.item.cardValues[i].ToString());
        }
        textTMP.text = $"{showText}";
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
        bool isCardUsed = true;
        switch (item.name)
        {
            case "회전 카드 1":
                RouletteManager.Inst.Spin(true, 3);
                break;
            case "회전 카드 2":
                RouletteManager.Inst.Spin(true, 2);
                break;
            case "회전 카드 3":
                RouletteManager.Inst.Spin(false, 3);
                break;
            case "회전 카드 4":
                RouletteManager.Inst.Spin(false, 2);
                break;
            case "회전 카드 5":
                RouletteManager.Inst.Spin(true, 1);
                break;
            case "구원":
                TurnManager.Inst.TakeDmg(-TurnManager.Inst.maxHealth / 2);
                break;
            case "혼령":
                TurnManager.Inst.IncreaseCost(2);
                TurnManager.Inst.TakeDmg(2);
                break;
            case "회복 부여":
                isCardUsed = RouletteManager.Inst.EnchantRoulette(false, ERouletteType.Heal, 3);
                break;
            case "흡혈 부여":
            case "흡혈 부여+":
                isCardUsed = RouletteManager.Inst.EnchantRoulette(true, ERouletteType.Player_Special_1, 6);
                if (item.name == "흡혈 부여+")
                {
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].isEnhanced = true;
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].tooltip.tooltipTxt = "값의 데미지를 주고 입힌 피해의 <color=red>50%</color>만큼 체력을 회복합니다. 뱀파이어 페르소나를 장착하고 있다면, 회복한 체력만큼 트리거 게이지를 얻습니다.";
                }
                break;
            case "혈액 순환":
            case "혈액 순환+":
                RouletteManager.Inst.Spin(true, 1);
                TurnManager.Inst.TakeDmg(1);
                Item tempItem = new Item();
                tempItem.SetItem(item);
                tempItem.name = "혈액 순환 (소멸)";
                tempItem.isVanish = true;
                CardManager.Inst.CreateCardInHand(tempItem);
                if(item.name == "혈액 순환+") CardManager.Inst.CreateCardInHand(tempItem);
                break;
            case "혈액 순환 (소멸)":
                RouletteManager.Inst.Spin(true, 1);
                TurnManager.Inst.TakeDmg(1);
                break;
            case "피는 나의 힘":
                TurnManager.Inst.TakeDmg(2);
                TurnManager.Inst.IncreaseCost(1);
                break;
            case "피는 나의 힘+":
                TurnManager.Inst.TakeDmg(1);
                TurnManager.Inst.IncreaseCost(1);
                break;
            case "긴급 수혈":
                TurnManager.Inst.TakeDmg(-2);
                break;
            case "긴급 수혈+":
                TurnManager.Inst.TakeDmg(-3);
                break;
            case "휴머니스트":
                BuffManager.Inst.AddShowBuff("주저함", EBuffAffectType.Enemy, 1);
                BuffManager.Inst.AddShowBuff("주저함", EBuffAffectType.Roulette, 1);
                break;
            case "휴머니스트+":
                BuffManager.Inst.AddShowBuff("주저함", EBuffAffectType.Enemy, 2);
                BuffManager.Inst.AddShowBuff("주저함", EBuffAffectType.Roulette, 2);
                break;
            case "블루 블러드":
            case "블루 블러드+":
                BuffManager.Inst.AddShowBuff("블루 블러드", EBuffAffectType.Roulette, 2);
                break;
            case "만찬 시간":
            case "만찬 시간+":
                BuffManager.Inst.AddShowBuff("만찬 시간", EBuffAffectType.Roulette, 2);
                break;
            case "핏빛 날개":
            case "핏빛 날개+":
                int bloodwing_spinnnum = 4;
                if(item.name == "핏빛 날개+") bloodwing_spinnnum = 5;
                for (int i = 0; i < bloodwing_spinnnum; i++)
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
                if (item.name == "핏빛 날개") isCardUsed = RouletteManager.Inst.EnchantRoulette(false, magicBox, 12);
                else isCardUsed = RouletteManager.Inst.EnchantRoulette(false, magicBox, 15);
                break;
            case "마술-비둘기":
            case "마술-비둘기+":
                magicBox = ERouletteType.Player_Special_1;
                if (TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum != this.item.dreamPieceNum) magicBox = ERouletteType.Player_Special_2;
                bool checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].roulette.type == magicBox;
                if (item.name == "마술-비둘기") TurnManager.Inst.EnemyTakeDmg(1);
                else TurnManager.Inst.EnemyTakeDmg(3);
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
                RouletteManager.Inst.Spin(true, 1);
                if (checkMagic)
                {
                    Debug.Log("duplicate mode on");
                    CardManager.Inst.CardSelectModeTransit(ECardSelectMode.Duplicate, 1);
                }
                break;
            case "마술-절단":
                magicBox = ERouletteType.Player_Special_1;
                if (TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum != this.item.dreamPieceNum) magicBox = ERouletteType.Player_Special_2;
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].roulette.type == magicBox;
                TurnManager.Inst.EnemyTakeDmg(2);
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
                TurnManager.Inst.EnemyTakeDmg(4);
                break;
            case "마술-순간이동":
            case "마술-순간이동+":
                magicBox = ERouletteType.Player_Special_1;
                if (TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum != this.item.dreamPieceNum) magicBox = ERouletteType.Player_Special_2;
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.type == magicBox;
                RouletteManager.Inst.Spin(true, 2);
                if (checkMagic)
                {
                    if (item.name == "마술-순간이동") TurnManager.Inst.GetShield(false, 12);
                    else TurnManager.Inst.GetShield(false, 16);
                }
                break;
            case "마술-예언":
            case "마술-예언+":
                BuffManager.Inst.AddShowBuff("예언-준비", EBuffAffectType.Player, 1);
                break;
            case "재빠른 손놀림":
            case "재빠른 손놀림+":
                StartCoroutine(TurnManager.Inst.Draw(1, null));
                if (item.name == "재빠른 손놀림+") StartCoroutine(TurnManager.Inst.Draw(1, null));
                break;
            case "초능력-예언":
            case "초능력-예언+":
                BuffManager.Inst.AddShowBuff("예언-준비", EBuffAffectType.Player, 1);
                break;
            case "초능력-염력":
            case "초능력-염력+":
                RouletteManager.Inst.Spin(true, TurnManager.Inst.nowCost);
                if (item.name == "초능력-염력+" && TurnManager.Inst.nowCost >= 3) TurnManager.Inst.GetShield(false, 5);
                break;
            case "에이스":
            case "에이스+":
                RouletteManager.Inst.TriggerRoulette();
                TurnManager.OnPlayerTrigger?.Invoke();
                Action endTrigger = null;
                endTrigger = () =>
                {
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.triggerPos].Trigger(false);
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.triggerPos].Setup(RouletteManager.Inst.triggerPiece_None);
                    TurnManager.OnRouletteActivate -= endTrigger;
                };
                TurnManager.OnRouletteActivate += endTrigger;
                break;
        }
        if (isCardUsed)
        {
            Debug.Log(item.name + " 카드 사용!");
        }
        return isCardUsed;
    }

    private void Update()
    {
        ShowBuffedCost();
    }
}
