using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System;

public class RoulettePiece : MonoBehaviour
{
    [SerializeField] SpriteRenderer roulettePiece;
    [SerializeField] TMP_Text rouletteValueTMP;
    [SerializeField] Sprite[] rouletteTypeSprites;
    
    public RouletteItem roulette;
    public bool isTriggered;
    public bool isEnhanced;
    public Tooltip tooltip;

    public void SetRoulettePieceSprite(Sprite sprite)
    {
        roulettePiece.sprite = sprite;
    }

    public void SetRoulettePieceTooltip(string title, string text)
    {
        if (tooltip)
        {
            tooltip.tooltipTitle = title;
            tooltip.tooltipTxt = text;
            tooltip.tooltipDisable = false;
        }
    }

    public void Setup(RouletteItem rlt)
    {
        tooltip = GetComponent<Tooltip>();
        switch(rlt.type)
        {
            case ERouletteType.Attack:
                SetRoulettePieceSprite(rouletteTypeSprites[1]);
                SetRoulettePieceTooltip("공격 룰렛", "피해를 값만큼 줍니다.");
                break;
            case ERouletteType.Heal:
                SetRoulettePieceSprite(rouletteTypeSprites[2]);
                SetRoulettePieceTooltip("회복 룰렛", "체력을 값만큼 회복합니다.");
                break;
            case ERouletteType.Shield:
                SetRoulettePieceSprite(rouletteTypeSprites[3]);
                SetRoulettePieceTooltip("실드 룰렛", "실드를 값만큼 획득합니다.");
                break;
            case ERouletteType.Enemy_Special_1:
                SetRoulettePieceSprite(EnemyManager.Inst.EnemySpecialRoulette1Sprite);
                SetRoulettePieceTooltip(EnemyManager.Inst.EnemySpecialRoulette1Title, EnemyManager.Inst.EnemySpecialRoulette1Text);
                break;
            case ERouletteType.Enemy_Special_2:
                SetRoulettePieceSprite(EnemyManager.Inst.EnemySpecialRoulette2Sprite);
                SetRoulettePieceTooltip(EnemyManager.Inst.EnemySpecialRoulette2Title, EnemyManager.Inst.EnemySpecialRoulette2Text);
                break;
            case ERouletteType.Player_Special_1:
                SetRoulettePieceSprite(PassiveManager.Inst.PlayerSpecialRoulette1Sprite);
                SetRoulettePieceTooltip(PassiveManager.Inst.PlayerSpecialRoulette1Title, PassiveManager.Inst.PlayerSpecialRoulette1Text);
                break;
            case ERouletteType.Player_Special_2:
                SetRoulettePieceSprite(PassiveManager.Inst.PlayerSpecialRoulette2Sprite);
                SetRoulettePieceTooltip(PassiveManager.Inst.PlayerSpecialRoulette2Title, PassiveManager.Inst.PlayerSpecialRoulette2Text);
                break;
            default:
                roulettePiece.sprite = rouletteTypeSprites[0];
                if (tooltip)
                {
                    if (this == RouletteManager.Inst.roulettePieces[RouletteManager.Inst.triggerPos])
                    {
                        tooltip.tooltipTitle = "트리거 룰렛";
                        tooltip.tooltipTxt = "현재 활성화되어 있지 않습니다.";
                    }
                    else
                    {
                        tooltip.tooltipDisable = true;
                    }
                }
                break;
        }

        roulette = rlt;
        HideTotalValue();
        Trigger(false);
    }

    public void ShowTotalValue()
    {
        int totalVal = BuffManager.Inst.GetBuffedRouletteValue(this);

        if (totalVal > roulette.value)
        {
            rouletteValueTMP.color = Color.green;
        }
        else if (totalVal == roulette.value)
        {
            rouletteValueTMP.color = Color.white;
        }
        else
        {
            rouletteValueTMP.color = Color.red;
        }
        rouletteValueTMP.text = totalVal.ToString();
        if (roulette.type == ERouletteType.None && totalVal == 0)
        {
            rouletteValueTMP.text = "";
        }

        tooltip.tooltipTxt = Regex.Replace(tooltip.tooltipTxt, @"값|<\d+>", match =>
        {
            string replacement = $"<{totalVal}>";
            return replacement;
        });
    }

    public void HideTotalValue()
    {
        rouletteValueTMP.color = Color.white;
        if (roulette.value != 0)
        {
            rouletteValueTMP.text = roulette.value.ToString();
        }
        else
        {
            rouletteValueTMP.text = "";
        }
    }

    public void Trigger(bool triggerState)
    {
        isTriggered = triggerState;
        if (isTriggered)
        {
            roulettePiece.color = Color.green;
        }
        else
        {
            roulettePiece.color = Color.white;
        }
    }

    public void Activate(bool isEnemy)
    {
        int totalVal = BuffManager.Inst.GetBuffedRouletteValue(this);
        if (isTriggered == true)
        {
            RouletteManager.TriggerActivation?.Invoke(isEnemy, totalVal);
            Trigger(false);
            RouletteManager.Inst.isTriggerActivated = false;
            this.Setup(RouletteManager.Inst.triggerPiece_None);
            BuffManager.Inst.rouletteBuff_Trigger.Clear();
            return;
        }
        switch (roulette.type)
        {
            case ERouletteType.Attack:
                if (isEnemy)
                {
                    TurnManager.Inst.EnemyTakeDmg(totalVal);
                }
                else
                {
                    TurnManager.Inst.TakeDmg(totalVal);
                }
                break;
            case ERouletteType.Heal:
                if (isEnemy)
                {
                    TurnManager.Inst.EnemyTakeDmg(-totalVal);
                }
                else
                {
                    TurnManager.Inst.TakeDmg(-totalVal);
                }
                break;
            case ERouletteType.Shield:
                if (isEnemy)
                {
                    TurnManager.Inst.GetShield(true, totalVal);
                }
                else
                {
                    TurnManager.Inst.GetShield(false, totalVal);
                }
                break;
            case ERouletteType.Enemy_Special_1:
                EnemyManager.EnemySpecialRoulette1Activation?.Invoke(this, isEnemy, totalVal);
                break;
            case ERouletteType.Enemy_Special_2:
                EnemyManager.EnemySpecialRoulette2Activation?.Invoke(this, isEnemy, totalVal);
                break;
            case ERouletteType.Player_Special_1:
                PassiveManager.PlayerSpecialRoulette1Activation?.Invoke(isEnemy, totalVal);
                break;
            case ERouletteType.Player_Special_2:
                PassiveManager.PlayerSpecialRoulette2Activation?.Invoke(isEnemy, totalVal);
                break;
        }
    }

    private void OnMouseDown()
    {
        RouletteManager.Inst.RouletteMouseDown();
    }

    private void OnMouseUp()
    {
        RouletteManager.Inst.RouletteMouseUp();
    }
}
