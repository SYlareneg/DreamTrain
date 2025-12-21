using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System;

public class RoulettePiece : MonoBehaviour
{
    public SpriteRenderer roulettePiece;
    [SerializeField] TMP_Text rouletteValueTMP;
    [SerializeField] public Sprite[] rouletteTypeSprites;

    public Sprite originalSprite;
    public string originalTooltipTitle;
    public string originalTooltipText;
    
    public RouletteItem roulette;
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
            if(title == null || title == "" || text == null || text == "")
            {
                tooltip.tooltipDisable = true;
                return;
            }
            tooltip.tooltipTitle = title;
            tooltip.tooltipTxt = text;
            tooltip.tooltipDisable = false;
        }
    }

    public void Setup(RouletteItem rlt)
    {
        roulette = rlt;
        tooltip = GetComponent<Tooltip>();
        switch(rlt.type)
        {
            case ERouletteType.Attack:
                originalSprite = rouletteTypeSprites[1];
                originalTooltipTitle = "공격 룰렛";
                originalTooltipText = "피해를 값만큼 줍니다.";
                break;
            case ERouletteType.Heal:
                originalSprite = rouletteTypeSprites[2];
                originalTooltipTitle = "회복 룰렛";
                originalTooltipText = "체력을 값만큼 회복합니다.";
                break;
            case ERouletteType.Shield:
                originalSprite = rouletteTypeSprites[3];
                originalTooltipTitle = "실드 룰렛";
                originalTooltipText = "실드를 값만큼 획득합니다.";
                break;
            case ERouletteType.Enemy_Special_1:
                originalSprite = EnemyManager.Inst.enemySpecialRoulettes[0].sprite;
                originalTooltipTitle = EnemyManager.Inst.enemySpecialRoulettes[0].title;
                originalTooltipText = EnemyManager.Inst.enemySpecialRoulettes[0].text;
                break;
            case ERouletteType.Enemy_Special_2:
                originalSprite = EnemyManager.Inst.enemySpecialRoulettes[1].sprite;
                originalTooltipTitle = EnemyManager.Inst.enemySpecialRoulettes[1].title;
                originalTooltipText = EnemyManager.Inst.enemySpecialRoulettes[1].text;
                break;
            case ERouletteType.Player_Special_1:
                originalSprite = PassiveManager.Inst.PlayerSpecialRoulette1Sprite;
                originalTooltipTitle = PassiveManager.Inst.PlayerSpecialRoulette1Title;
                originalTooltipText = PassiveManager.Inst.PlayerSpecialRoulette1Text;
                break;
            case ERouletteType.Player_Special_2:
                originalSprite = PassiveManager.Inst.PlayerSpecialRoulette2Sprite;
                originalTooltipTitle = PassiveManager.Inst.PlayerSpecialRoulette2Title;
                originalTooltipText = PassiveManager.Inst.PlayerSpecialRoulette2Text;
                break;
            default:
                originalSprite = rouletteTypeSprites[0];
                originalTooltipTitle = null;
                originalTooltipText = null;
                break;
        }

        HideTotalValue();

        if (RouletteManager.Inst.isTriggerActivated == false)
        {
            SetRoulettePieceSprite(originalSprite);
            SetRoulettePieceTooltip(originalTooltipTitle, originalTooltipText);
        }
    }

    public void RouletteClear()
    {
        int index = Array.IndexOf(RouletteManager.Inst.roulettePieces, this);
        switch (roulette.type)
        {
            case ERouletteType.Player_Special_1:
                PassiveManager.PlayerSpecialRoulette1Clear?.Invoke(index);
                break;
            case ERouletteType.Player_Special_2:
                PassiveManager.PlayerSpecialRoulette2Clear?.Invoke(index);
                break;
            default:
                RouletteManager.Inst.EnchantRoulettePiece(index, ERouletteType.None, 0);
                break;
        }
    }

    public void ShowTotalValue()
    {
        int totalVal = BuffManager.Inst.GetBuffedRouletteValue(this);
        ERouletteType curType = roulette.type;
        int curVal = roulette.value;

        if (RouletteManager.Inst.isTriggerActivated)
        {
            totalVal = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.triggerPiece);
            curType = RouletteManager.Inst.triggerPiece.type;
            curVal = RouletteManager.Inst.triggerPiece.value;
        }

        if (totalVal > curVal)
        {
            rouletteValueTMP.color = Color.green;
        }
        else if (totalVal == curVal)
        {
            rouletteValueTMP.color = Color.white;
        }
        else
        {
            rouletteValueTMP.color = Color.red;
        }
        rouletteValueTMP.text = totalVal.ToString();
        if (curType == ERouletteType.None && totalVal == 0)
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
        if (triggerState)
        {
            originalSprite = roulettePiece.sprite;
            if (tooltip)
            {
                originalTooltipTitle = tooltip.tooltipTitle;
                originalTooltipText = tooltip.tooltipTxt;
            }
            SetRoulettePieceSprite(rouletteTypeSprites[4]);
            SetRoulettePieceTooltip(TurnManager.Inst.characterSO.personaPiece.persona.name, TurnManager.Inst.characterSO.personaPiece.persona.text);
            Transform frozenIcon = transform.Find("FrozenIcon");
            if(frozenIcon != null)
            {
                frozenIcon.gameObject.SetActive(false);
            }
        }
        else
        {
            SetRoulettePieceSprite(originalSprite);
            SetRoulettePieceTooltip(originalTooltipTitle, originalTooltipText);
            Transform frozenIcon = transform.Find("FrozenIcon");
            if(frozenIcon != null)
            {
                frozenIcon.gameObject.SetActive(true);
            }
            roulettePiece.color = Color.white;
        }
    }

    public void Activate(bool isEnemy)
    {
        int totalVal = BuffManager.Inst.GetBuffedRouletteValue(this);
        switch (roulette.type)
        {
            case ERouletteType.Attack:
                if (isEnemy)
                {
                    TurnManager.Inst.EnemyTakeDmg(totalVal, EDamageSource.Roulette);
                }
                else
                {
                    TurnManager.Inst.TakeDmg(totalVal, EDamageSource.Roulette);
                }
                break;
            case ERouletteType.Heal:
                if (isEnemy)
                {
                    TurnManager.Inst.EnemyTakeDmg(-totalVal, EDamageSource.Roulette);
                }
                else
                {
                    TurnManager.Inst.TakeDmg(-totalVal, EDamageSource.Roulette);
                }
                break;
            case ERouletteType.Shield:
                if (isEnemy)
                {
                    TurnManager.Inst.GetShield(true, totalVal, EDamageSource.Roulette);
                }
                else
                {
                    TurnManager.Inst.GetShield(false, totalVal, EDamageSource.Roulette);
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
