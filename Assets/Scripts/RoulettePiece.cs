using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Text.RegularExpressions;
using System;
using DG.Tweening;

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

    public void Setup(RouletteItem rlt, bool isEnhanced = false, bool setSprite = true, bool setTooltip = true)
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
                originalTooltipTitle = "수비 룰렛";
                originalTooltipText = "방어도를 값만큼 부여합니다.";
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
            if (setSprite) SetRoulettePieceSprite(originalSprite);
            if (setTooltip) SetRoulettePieceTooltip(originalTooltipTitle, originalTooltipText);
        }
    }

    public void RouletteClear()
    {
        tooltip.HideTooltip();
        int index = Array.IndexOf(RouletteManager.Inst.roulettePieces, this);
        bool eraseFlag = roulette.rtype.type != ERouletteType.None;
        switch (roulette.rtype.type)
        {
            case ERouletteType.Player_Special:
                PassiveManager.playerSpecialRouletteClear[roulette.rtype.specialTypeIdx]?.Invoke(index);
                break;
            default:
                // RouletteManager.Inst.EnchantRoulettePiece(index, new RouletteType(ERouletteType.None, 0), 0);
                RouletteItem rItem = new RouletteItem();
                rItem.rtype = new RouletteType(ERouletteType.None, 0);
                rItem.value = 0;
                Setup(rItem);
                break;
        }
        if(eraseFlag)
        {
            Utils.AllignActions(ref TurnManager.OnRouletteErase, typeof(ShowBuff), typeof(RelicManager));
            TurnManager.OnRouletteErase?.Invoke(index);
        }
    }

    public void Enhance()
    {
        Debug.Log("룰렛 강화 시도: " + roulette.rtype.type.ToString());
        if(roulette.rtype.type != ERouletteType.Player_Special || roulette.rtype.type == ERouletteType.Enemy_Special || isEnhanced) return;
        isEnhanced = true;
        roulette.value = PassiveManager.Inst.playerSpecialRoulettes[roulette.rtype.specialTypeIdx].baseVal_enhanced;
        Setup(roulette, true, false, true);
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
            rouletteValueTMP.color = new Color(0.3020f, 0.2941f, 0.1882f, 1f);
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
        rouletteValueTMP.color = new Color(0.3020f, 0.2941f, 0.1882f, 1f);
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

    Sequence activateSeq;

    public void Activate(bool isEnemy, int enemyIdx = 0)
    {
        int totalVal = BuffManager.Inst.GetBuffedRouletteValue(this);
        switch (roulette.rtype.type)
        {
            case ERouletteType.Attack:
                if(totalVal < 0) totalVal = 0;
                if (isEnemy)
                {
                    RouletteManager.Inst.enemyRouletteEffectEndAction.Add(() =>
                    {
                        TurnManager.Inst.EnemyTakeDmg(totalVal, EDamageSource.Roulette, enemyIdx);
                    });
                    if(RouletteManager.Inst.enemyRouletteCurrentEffectName == "")
                    {
                        RouletteManager.Inst.enemyRouletteCurrentEffectName = "Attack";
                        RouletteManager.Inst.enemyRouletteEffect.SetTrigger("Attack");
                    }
                    else
                    {
                        RouletteManager.Inst.enemyRouletteEffectQueue.Add("Attack");
                    }
                }
                else
                {
                    RouletteManager.Inst.playerRouletteEffectEndAction.Add(() =>
                    {
                        TurnManager.Inst.TakeDmg(totalVal, EDamageSource.Roulette);
                    });
                    if(RouletteManager.Inst.playerRouletteCurrentEffectName == "")
                    {
                        RouletteManager.Inst.playerRouletteCurrentEffectName = "Attack";
                        RouletteManager.Inst.playerRouletteEffect.SetTrigger("Attack");
                    }
                    else
                    {
                        RouletteManager.Inst.playerRouletteEffectQueue.Add("Attack");
                    }
                }
                break;
            case ERouletteType.Heal:
                if(totalVal < 0) totalVal = 0;
                if (isEnemy)
                {
                    RouletteManager.Inst.enemyRouletteEffectEndAction.Add(() =>
                    {
                        TurnManager.Inst.EnemyTakeDmg(-totalVal, EDamageSource.Roulette, enemyIdx);
                    });
                    if(RouletteManager.Inst.enemyRouletteCurrentEffectName == "")
                    {
                        RouletteManager.Inst.enemyRouletteCurrentEffectName = "Heal";
                        RouletteManager.Inst.enemyRouletteEffect.SetTrigger("Heal");
                    }
                    else
                    {
                        RouletteManager.Inst.enemyRouletteEffectQueue.Add("Heal");
                    }
                    // TurnManager.Inst.EnemyTakeDmg(-totalVal, EDamageSource.Roulette, enemyIdx);
                }
                else
                {
                    RouletteManager.Inst.playerRouletteEffectEndAction.Add(() =>
                    {
                        TurnManager.Inst.TakeDmg(-totalVal, EDamageSource.Roulette);
                    });
                    if(RouletteManager.Inst.playerRouletteCurrentEffectName == "")
                    {
                        RouletteManager.Inst.playerRouletteCurrentEffectName = "Heal";
                        RouletteManager.Inst.playerRouletteEffect.SetTrigger("Heal");
                    }
                    else
                    {
                        RouletteManager.Inst.playerRouletteEffectQueue.Add("Heal");
                    }
                }
                break;
            case ERouletteType.Shield:
                if (isEnemy)
                {
                    RouletteManager.Inst.enemyRouletteEffectEndAction.Add(() =>
                    {
                        TurnManager.Inst.GetShield(true, totalVal, EDamageSource.Roulette, enemyIdx);
                    });
                    if(RouletteManager.Inst.enemyRouletteCurrentEffectName == "")
                    {
                        RouletteManager.Inst.enemyRouletteCurrentEffectName = "Shield";
                        RouletteManager.Inst.enemyRouletteEffect.SetTrigger("Shield");
                    }
                    else
                    {
                        RouletteManager.Inst.enemyRouletteEffectQueue.Add("Shield");
                    }
                    // TurnManager.Inst.GetShield(true, totalVal, EDamageSource.Roulette, enemyIdx);
                }
                else
                {
                    RouletteManager.Inst.playerRouletteEffectEndAction.Add(() =>
                    {
                        TurnManager.Inst.GetShield(false, totalVal, EDamageSource.Roulette);
                    });
                    if(RouletteManager.Inst.playerRouletteCurrentEffectName == "")
                    {
                        RouletteManager.Inst.playerRouletteCurrentEffectName = "Shield";
                        RouletteManager.Inst.playerRouletteEffect.SetTrigger("Shield");
                    }
                    else
                    {
                        RouletteManager.Inst.playerRouletteEffectQueue.Add("Shield");
                    }
                }
                break;
            case ERouletteType.Enemy_Special:
                if(enemyIdx == 0)
                {
                    if(isEnemy)
                    {
                        RouletteManager.Inst.enemyRouletteEffectEndAction.Add(() =>
                        {
                            EnemyManager.enemySpecialRouletteActivation[roulette.rtype.specialTypeIdx]?.Invoke(this, isEnemy, totalVal, 0, isEnhanced);
                        });
                        if(RouletteManager.Inst.enemyRouletteCurrentEffectName == "")
                        {
                            switch(EnemyManager.Inst.enemySpecialRoulettes[roulette.rtype.specialTypeIdx].title)
                            {
                                case "흡혈":
                                case "흡혈+":
                                    RouletteManager.Inst.enemyRouletteCurrentEffectName = "Drain";
                                    RouletteManager.Inst.enemyRouletteEffect.SetTrigger(RouletteManager.Inst.enemyRouletteCurrentEffectName);
                                    break;
                                default:
                                    RouletteManager.Inst.enemyRouletteCurrentEffectName = "Default";
                                    RouletteManager.Inst.enemyRouletteEffect.SetTrigger(RouletteManager.Inst.enemyRouletteCurrentEffectName);
                                    break;
                            }
                        }
                        else
                        {
                            switch(EnemyManager.Inst.enemySpecialRoulettes[roulette.rtype.specialTypeIdx].title)
                            {
                                case "흡혈":
                                case "흡혈+":
                                    RouletteManager.Inst.enemyRouletteEffectQueue.Add("Drain");
                                    break;
                                default:
                                    RouletteManager.Inst.enemyRouletteEffectQueue.Add("Default");
                                    break;
                            }
                        }
                    }
                    else
                    {
                        RouletteManager.Inst.playerRouletteEffectEndAction.Add(() =>
                        {
                            EnemyManager.enemySpecialRouletteActivation[roulette.rtype.specialTypeIdx]?.Invoke(this, isEnemy, totalVal, 0, isEnhanced);
                        });
                        if(RouletteManager.Inst.playerRouletteCurrentEffectName == "")
                        {
                            switch(EnemyManager.Inst.enemySpecialRoulettes[roulette.rtype.specialTypeIdx].title)
                            {
                                case "흡혈":
                                case "흡혈+":
                                    RouletteManager.Inst.playerRouletteCurrentEffectName = "Drain";
                                    RouletteManager.Inst.playerRouletteEffect2.SetTrigger(RouletteManager.Inst.playerRouletteCurrentEffectName);
                                    break;
                                default:
                                    RouletteManager.Inst.playerRouletteCurrentEffectName = "Default";
                                    RouletteManager.Inst.playerRouletteEffect.SetTrigger(RouletteManager.Inst.playerRouletteCurrentEffectName);
                                    break;
                            }
                        }
                        else
                        {
                            switch(EnemyManager.Inst.enemySpecialRoulettes[roulette.rtype.specialTypeIdx].title)
                            {
                                case "흡혈":
                                case "흡혈+":
                                    RouletteManager.Inst.playerRouletteEffectQueue.Add("Drain");
                                    break;
                                default:
                                    RouletteManager.Inst.playerRouletteEffectQueue.Add("Default");
                                    break;
                            }
                        }
                    }
                }
                else EnemyManager.subEnemySpecialRouletteActivation[enemyIdx - 1, roulette.rtype.specialTypeIdx]?.Invoke(this, isEnemy, totalVal, enemyIdx, isEnhanced);
                break;
            case ERouletteType.Player_Special:
                if(isEnemy)
                {
                    RouletteManager.Inst.enemyRouletteEffectEndAction.Add(() =>
                    {
                        PassiveManager.playerSpecialRouletteActivation[roulette.rtype.specialTypeIdx]?.Invoke(isEnemy, totalVal, enemyIdx, isEnhanced);
                    });
                    if(RouletteManager.Inst.enemyRouletteCurrentEffectName == "")
                    {
                        switch(PassiveManager.Inst.playerSpecialRoulettes[roulette.rtype.specialTypeIdx].title)
                        {
                            case "발톱":
                            case "발톱+":
                                RouletteManager.Inst.enemyRouletteCurrentEffectName = "Claw";
                                break;
                            case "실뭉치":
                            case "실뭉치+":
                                RouletteManager.Inst.enemyRouletteCurrentEffectName = "Furball";
                                break;
                            default:
                                RouletteManager.Inst.enemyRouletteCurrentEffectName = "Default";
                                break;
                        }
                        RouletteManager.Inst.enemyRouletteEffect.SetTrigger(RouletteManager.Inst.enemyRouletteCurrentEffectName);
                    }
                    else
                    {
                        switch(PassiveManager.Inst.playerSpecialRoulettes[roulette.rtype.specialTypeIdx].title)
                        {
                            case "발톱":
                            case "발톱+":
                                RouletteManager.Inst.enemyRouletteEffectQueue.Add("Claw");
                                break;
                            case "실뭉치":
                            case "실뭉치+":
                                RouletteManager.Inst.enemyRouletteEffectQueue.Add("Furball");
                                break;
                            default:
                                RouletteManager.Inst.enemyRouletteEffectQueue.Add("Default");
                                break;
                        }
                    }
                }
                else
                {
                    RouletteManager.Inst.playerRouletteEffectEndAction.Add(() =>
                    {
                        PassiveManager.playerSpecialRouletteActivation[roulette.rtype.specialTypeIdx]?.Invoke(isEnemy, totalVal, enemyIdx, isEnhanced);
                    });
                    if(RouletteManager.Inst.playerRouletteCurrentEffectName == "")
                    {
                        switch(PassiveManager.Inst.playerSpecialRoulettes[roulette.rtype.specialTypeIdx].title)
                        {
                            case "발톱":
                            case "발톱+":
                                RouletteManager.Inst.playerRouletteCurrentEffectName = "Claw";
                                RouletteManager.Inst.playerRouletteEffect2.SetTrigger(RouletteManager.Inst.playerRouletteCurrentEffectName);
                                break;
                            case "실뭉치":
                            case "실뭉치+":
                                RouletteManager.Inst.playerRouletteCurrentEffectName = "Furball";
                                RouletteManager.Inst.playerRouletteEffect.SetTrigger(RouletteManager.Inst.playerRouletteCurrentEffectName);
                                break;
                            default:
                                RouletteManager.Inst.playerRouletteCurrentEffectName = "Default";
                                RouletteManager.Inst.playerRouletteEffect.SetTrigger(RouletteManager.Inst.playerRouletteCurrentEffectName);
                                break;
                        }
                    }
                    else
                    {
                        switch(PassiveManager.Inst.playerSpecialRoulettes[roulette.rtype.specialTypeIdx].title)
                        {
                            case "발톱":
                            case "발톱+":
                                RouletteManager.Inst.playerRouletteEffectQueue.Add("Claw");
                                break;
                            case "실뭉치":
                            case "실뭉치+":
                                RouletteManager.Inst.playerRouletteEffectQueue.Add("Furball");
                                break;
                            default:
                                RouletteManager.Inst.playerRouletteEffectQueue.Add("Default");
                                break;
                        }
                    }
                }
                break;
        }

        if(activateSeq != null && activateSeq.IsActive())
        {
            activateSeq.Kill();
        }
        activateSeq = DOTween.Sequence();
        Transform highlight = transform.Find("RouletteHighlight");
        if (highlight != null)
        {
            highlight.gameObject.SetActive(true);
            var highlightSR = highlight.GetComponent<SpriteRenderer>();
            highlightSR.color = new Color(1f, 1f, 1f, 0f);
            activateSeq.Append(highlightSR.DOFade(1f, 0.1f));
            activateSeq.AppendInterval(0.2f);
            activateSeq.Append(highlightSR.DOFade(0f, 0.1f));
            activateSeq.OnComplete(() => { highlight.gameObject.SetActive(false); });
        }
    }

    public void EnchantAnim(Action onComplete = null)
    {
        Transform enchantEffect = transform.Find("EnchantEffect");
        if (enchantEffect != null)
        {
            var enchantAnim = enchantEffect.GetComponent<Animator>();
            enchantAnim.SetTrigger("Enchant");
            enchantAnim.GetComponent<RouletteEffect_Enchant>().OnEffectEndAction = () =>
            {
                onComplete?.Invoke();
            };
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    // private void OnMouseDown()
    // {
    //     RouletteManager.Inst.RouletteMouseDown();
    // }

    // private void OnMouseUp()
    // {
    //     RouletteManager.Inst.RouletteMouseUp();
    // }
}
