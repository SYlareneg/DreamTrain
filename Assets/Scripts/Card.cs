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
    [SerializeField] TMP_Text textTMP;
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
            showText = showText.Replace($"{{cardValues[{i}]}}", this.item.cardValues[i].ToString());
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

    public void UseCard(bool isMine)
    {
        Debug.Log(item.name + " 사용!");

        switch (item.name)
        {
            case "Turn Card 1":
                RouletteManager.Inst.Spin(true, 3);
                break;
            case "Turn Card 2":
                RouletteManager.Inst.Spin(true, 2);
                break;
            case "Turn Card 3":
                RouletteManager.Inst.Spin(false, 3);
                break;
            case "Turn Card 4":
                RouletteManager.Inst.Spin(false, 2);
                break;
            case "Turn Card 5":
                RouletteManager.Inst.Spin(true, 1);
                break;
            case "Savior":
                TurnManager.Inst.TakeDmg(-TurnManager.Inst.maxHealth / 2);
                break;
            case "Spirit":
                TurnManager.Inst.IncreaseCost(2);
                TurnManager.Inst.TakeDmg(2);
                break;
            case "Enchant Heal":
                RouletteManager.Inst.EnchantRoulette(false, ERouletteType.Heal, 3);
                break;
            case "Vampiric Empowerment":
                RouletteManager.Inst.EnchantRoulette(true, ERouletteType.Lifesteal, 6);
                break;
            case "Blood Flow":
                RouletteManager.Inst.Spin(true, 1);
                TurnManager.Inst.TakeDmg(1);
                Item tempItem = new Item();
                tempItem.name = "Blood Flow (Vanish)";
                tempItem.cost = item.cost;
                tempItem.type = item.type;
                tempItem.element = item.element;
                tempItem.dreamPieceNum = item.dreamPieceNum;
                tempItem.isVanish = true;
                tempItem.sprite = item.sprite;
                tempItem.text = item.text;
                tempItem.num = item.num;
                CardManager.Inst.CreateCardInHand(tempItem);
                break;
            case "Blood Flow (Vanish)":
                RouletteManager.Inst.Spin(true, 1);
                TurnManager.Inst.TakeDmg(1);
                break;
            case "Blood, My Power":
                TurnManager.Inst.TakeDmg(2);
                TurnManager.Inst.IncreaseCost(1);
                break;
            case "Emergency Transfusion":
                TurnManager.Inst.TakeDmg(-2);
                break;
            case "Humanist":
                BuffManager.Inst.AddPlayerBuff(BuffManager.Inst.damageBuff, -3, 1, 1);
                break;
            case "Blue Blood":
                BuffManager.Inst.AddRouletteBuff(BuffManager.Inst.totalRouletteBuff_Lifesteal_Dmg, 0, 3, 2);
                BuffManager.Inst.AddRouletteBuff(BuffManager.Inst.totalRouletteBuff_Lifesteal_Heal, 0, 0, 2);
                break;
            case "Feast Time":
                BuffManager.Inst.AddRouletteBuff(BuffManager.Inst.totalRouletteBuff_Lifesteal_Dmg, 0, 2, 2);
                break;
            case "Crimson Wings":
                for (int i = 0; i < 4; i++)
                {
                    int tempIdx = (RouletteManager.Inst.enemyLookat + i) % RouletteManager.rouletteNum;
                    if (RouletteManager.Inst.roulettePieces[tempIdx].roulette.type == ERouletteType.Lifesteal)
                    {
                        RouletteManager.Inst.ActivateRoulettePiece(tempIdx, true);
                    }
                }
                RouletteManager.Inst.Spin(false, 4);
                break;
            case "Magic Box":
                RouletteManager.Inst.EnchantRoulette(false, ERouletteType.MagicBox, 12);
                break;
            case "Magic-Dove":
                bool checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].roulette.type == ERouletteType.MagicBox;
                TurnManager.Inst.EnemyTakeDmg(1);
                if (checkMagic)
                {
                    EnemyManager.Inst.RemoveAction(0);
                }
                break;
            case "Magic-Cards":
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.type == ERouletteType.MagicBox;
                RouletteManager.Inst.Spin(true, 1);
                if (checkMagic)
                {
                    Debug.Log("duplicate mode on");
                    CardManager.Inst.duplicateMode = true;
                }
                break;
            case "Magic-Sawing":
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.enemyLookat].roulette.type == ERouletteType.MagicBox;
                TurnManager.Inst.EnemyTakeDmg(2);
                if (checkMagic)
                {
                    TurnManager.Inst.enemyShieldHealth = 0;
                }
                break;
            case "Magic-Teleportation":
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.type == ERouletteType.MagicBox;
                RouletteManager.Inst.Spin(true, 2);
                if (checkMagic)
                {
                    TurnManager.Inst.GetShield(false, 12);
                }
                break;
            case "Magic-Prediction":
                Action aceNull = null;
                aceNull = () =>
                {
                    Item ace = CardManager.Inst.itemDeck.Find(x => x.name == "Ace");
                    BuffManager.Inst.AddSingleCardCostBuff(ace, 0, 0, 1);
                    Action fullCost = null;
                    fullCost = () =>
                    {
                        if (CardManager.Inst.selectedCard.item.name == "Ace")
                        {
                            TurnManager.Inst.SetFullCost();
                            TurnManager.OnUseCard -= fullCost;
                        }
                    };
                    TurnManager.OnUseCard += fullCost;
                    Action eraseFullCost = null;
                    eraseFullCost = () =>
                    {
                        TurnManager.OnUseCard -= fullCost;
                        TurnManager.OnPlayerTurnStart -= eraseFullCost;
                    };
                    TurnManager.OnPlayerTurnStart += eraseFullCost;
                    TurnManager.OnPlayerTurnStart -= aceNull;
                };
                TurnManager.OnPlayerTurnStart += aceNull;
                break;
            case "Sleight of Hand":
                StartCoroutine(TurnManager.Inst.Draw(1, null));
                break;
            case "ESP-Prediction":
                Action fullCost = null;
                fullCost = () =>
                {
                    TurnManager.OnPlayerTrigger += () =>
                    {
                        TurnManager.Inst.SetFullCost();
                    };
                    TurnManager.OnPlayerTurnStart -= fullCost;
                };
                TurnManager.OnPlayerTurnStart += fullCost;
                break;
            case "ESP-Telekinesis":
                RouletteManager.Inst.Spin(true, TurnManager.Inst.nowCost);
                break;
            case "Ace":
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
        }
    }

    private void Update()
    {
        ShowBuffedCost();
    }
}
