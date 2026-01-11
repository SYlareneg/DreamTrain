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
    public bool isEnhanced = false;
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

    public void Setup(RouletteItem rlt, bool isEnhanced = false)
    {
        roulette = rlt;
        tooltip = GetComponent<Tooltip>();
        switch(rlt.rtype.type)
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
            case ERouletteType.Enemy_Special:
                if(rlt.rtype.enemyIdx == 0)
                {
                    originalSprite = EnemyManager.Inst.enemySpecialRoulettes[rlt.rtype.specialTypeIdx].sprite;
                    if(isEnhanced)
                    {
                        originalTooltipTitle = EnemyManager.Inst.enemySpecialRoulettes[rlt.rtype.specialTypeIdx].title_enhanced;
                        originalTooltipText = EnemyManager.Inst.enemySpecialRoulettes[rlt.rtype.specialTypeIdx].text_enhanced;
                    }
                    else
                    {
                        originalTooltipTitle = EnemyManager.Inst.enemySpecialRoulettes[rlt.rtype.specialTypeIdx].title;
                        originalTooltipText = EnemyManager.Inst.enemySpecialRoulettes[rlt.rtype.specialTypeIdx].text;
                    }
                }
                else
                {
                    originalSprite = EnemyManager.Inst.subEnemySpecialRoulettes[rlt.rtype.enemyIdx - 1][rlt.rtype.specialTypeIdx].sprite;
                    if(isEnhanced)
                    {
                        originalTooltipTitle = EnemyManager.Inst.subEnemySpecialRoulettes[rlt.rtype.enemyIdx - 1][rlt.rtype.specialTypeIdx].title_enhanced;
                        originalTooltipText = EnemyManager.Inst.subEnemySpecialRoulettes[rlt.rtype.enemyIdx - 1][rlt.rtype.specialTypeIdx].text_enhanced;
                    }
                    else
                    {
                        originalTooltipTitle = EnemyManager.Inst.subEnemySpecialRoulettes[rlt.rtype.enemyIdx - 1][rlt.rtype.specialTypeIdx].title;
                        originalTooltipText = EnemyManager.Inst.subEnemySpecialRoulettes[rlt.rtype.enemyIdx - 1][rlt.rtype.specialTypeIdx].text;
                    }
                }
                break;
            case ERouletteType.Player_Special:
                originalSprite = PassiveManager.Inst.playerSpecialRoulettes[rlt.rtype.specialTypeIdx].sprite;
                if (isEnhanced)
                {
                    originalTooltipTitle = PassiveManager.Inst.playerSpecialRoulettes[rlt.rtype.specialTypeIdx].title_enhanced;
                    originalTooltipText = PassiveManager.Inst.playerSpecialRoulettes[rlt.rtype.specialTypeIdx].text_enhanced;
                }
                else
                {
                    originalTooltipTitle = PassiveManager.Inst.playerSpecialRoulettes[rlt.rtype.specialTypeIdx].title;
                    originalTooltipText = PassiveManager.Inst.playerSpecialRoulettes[rlt.rtype.specialTypeIdx].text;
                }
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
        switch (roulette.rtype.type)
        {
            case ERouletteType.Player_Special:
                PassiveManager.playerSpecialRouletteClear[roulette.rtype.specialTypeIdx]?.Invoke(index);
                break;
            default:
                RouletteManager.Inst.EnchantRoulettePiece(index, new RouletteType(ERouletteType.None), 0);
                break;
        }
    }

    public void Enhance()
    {
        if(roulette.rtype.type != ERouletteType.Player_Special || roulette.rtype.type == ERouletteType.Enemy_Special || isEnhanced) return;
        isEnhanced = true;
        roulette.value = PassiveManager.Inst.playerSpecialRoulettes[roulette.rtype.specialTypeIdx].baseVal_enhanced;
        Setup(roulette, true);
    }

    public void ShowTotalValue()
    {
        int totalVal = BuffManager.Inst.GetBuffedRouletteValue(this);
        ERouletteType curType = roulette.rtype.type;
        int curVal = roulette.value;

        if (RouletteManager.Inst.isTriggerActivated)
        {
            totalVal = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.triggerPiece);
            curType = RouletteManager.Inst.triggerPiece.rtype.type;
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

    public void Activate(bool isEnemy, int enemyIdx = 0)
    {
        int totalVal = BuffManager.Inst.GetBuffedRouletteValue(this);
        switch (roulette.rtype.type)
        {
            case ERouletteType.Attack:
                if (isEnemy)
                {
                    TurnManager.Inst.EnemyTakeDmg(totalVal, EDamageSource.Roulette, enemyIdx);
                }
                else
                {
                    TurnManager.Inst.TakeDmg(totalVal, EDamageSource.Roulette);
                }
                break;
            case ERouletteType.Heal:
                if (isEnemy)
                {
                    TurnManager.Inst.EnemyTakeDmg(-totalVal, EDamageSource.Roulette, enemyIdx);
                }
                else
                {
                    TurnManager.Inst.TakeDmg(-totalVal, EDamageSource.Roulette);
                }
                break;
            case ERouletteType.Shield:
                if (isEnemy)
                {
                    TurnManager.Inst.GetShield(true, totalVal, EDamageSource.Roulette, enemyIdx);
                }
                else
                {
                    TurnManager.Inst.GetShield(false, totalVal, EDamageSource.Roulette);
                }
                break;
            case ERouletteType.Enemy_Special:
                if(enemyIdx == 0) EnemyManager.enemySpecialRouletteActivation[roulette.rtype.specialTypeIdx]?.Invoke(this, isEnemy, totalVal, 0, isEnhanced);
                else EnemyManager.subEnemySpecialRouletteActivation[enemyIdx - 1, roulette.rtype.specialTypeIdx]?.Invoke(this, isEnemy, totalVal, enemyIdx, isEnhanced);
                break;
            case ERouletteType.Player_Special:
                PassiveManager.playerSpecialRouletteActivation[roulette.rtype.specialTypeIdx]?.Invoke(isEnemy, totalVal, enemyIdx, isEnhanced);
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
