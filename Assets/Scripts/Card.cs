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
    public SpriteRenderer character;
    public SpriteRenderer highlight;
    public SpriteRenderer specialHighlight;
    [SerializeField] SpriteRenderer type;
    [SerializeField] SpriteRenderer rarity;
    [SerializeField] SpriteRenderer cost;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text costTMP;
    [SerializeField] TMP_Text typeTMP;
    public TMP_Text textTMP;
    public AudioSource cardSound;
    [SerializeField] Sprite[] cardTypes;
    [SerializeField] Sprite[] rarityTypes;
    [SerializeField] Sprite[] costTypes;

    public Item item;
    public PRS originPRS;
    bool tooltipCreated = false;
    [SerializeField] GameObject cardUITooltipPrefab;
    [SerializeField] Transform tooltipPos;
    List<GameObject> activeTooltips = new List<GameObject>();
    Canvas tooltipCanvas;

    
    public Action OnCardClicked; 
    public enum SceneType { Dialogue, Emotion, General };
    
    SceneType currType = SceneType.General;
    public void Setup(Item item)
    {
        currType = SceneType.General;
        if (item == null)
        {
            this.item = null;
            return;
        }

        this.item = item;

        type.sprite = cardTypes[(int)this.item.type];
        if(type.sprite == null) type.enabled = false;
        else type.enabled = true;
        switch(this.item.type)
        {
            case CardType.Turn:
                typeTMP.text = "회전";
                break;
            case CardType.Enchant:
                typeTMP.text = "부여";
                break;
            case CardType.Skill:
                typeTMP.text = "스킬";
                break;
            case CardType.Dream:
                typeTMP.text = "몽상";
                break;
        }
        
        if(this.item.dreamPieceNum >= 0 && this.item.dreamPieceNum == TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum)
        {
            card.sprite = TurnManager.Inst.characterSO.personaPiece.cardBackgrounds[(int)this.item.rarity];
            typeTMP.color = TurnManager.Inst.characterSO.personaPiece.textColors[0];
            nameTMP.color = TurnManager.Inst.characterSO.personaPiece.textColors[1];
            textTMP.color = TurnManager.Inst.characterSO.personaPiece.textColors[2];
        }
        else if(this.item.dreamPieceNum >= 0 && this.item.dreamPieceNum == TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum)
        {
            card.sprite = TurnManager.Inst.characterSO.shadowPiece.cardBackgrounds[(int)this.item.rarity];
            typeTMP.color = TurnManager.Inst.characterSO.shadowPiece.textColors[0];
            nameTMP.color = TurnManager.Inst.characterSO.shadowPiece.textColors[1];
            textTMP.color = TurnManager.Inst.characterSO.shadowPiece.textColors[2];
        }
        else
        {
            card.sprite = rarityTypes[(int)this.item.rarity];
        }
        rarity.sprite = rarityTypes[(int)this.item.rarity];
        if(rarity.sprite == null) rarity.enabled = false;
        else rarity.enabled = true;

        character.sprite = this.item.sprite;
        highlight.enabled = false;
        specialHighlight.enabled = false;

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

    public void SetCostImg(int costVal)
    {
        if(costVal < 0 || costVal > 9) return;
        cost.sprite = costTypes[costVal];
    }

    public void ShowBuffedCost()
    {
        if (currType != SceneType.General) return;
        int buffedCost = BuffManager.Inst.GetBuffedCardCost(this.item);
        costTMP.text = buffedCost.ToString();
        // SetCostImg(buffedCost);

        if (buffedCost > this.item.cost)
        {
            costTMP.color = Color.red;
            // cost.color = Color.red;
        }
        else if (buffedCost == this.item.cost)
        {
            costTMP.color = new Color(60f/255f, 60f/255f, 80f/255f);
            // cost.color = Color.white;
        }
        else
        {
            costTMP.color = Color.green;
            // cost.color = Color.green;
        }
    }

    public void ShowBuffedVal()
    {
        if (this.item.cardValues.Count == 0)
        {
            textTMP.text = this.item.text;
            return;
        }
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

    
    public void SetupDialogue(string title, string description, Sprite charSprite, Action onClick)
    {
        currType = SceneType.Dialogue;
        OnCardClicked = onClick;

        nameTMP.text = title;
        textTMP.text = description;
        if (charSprite != null) character.sprite = charSprite;
        else character.color = Color.clear;
        
        costTMP.text = "";
    }
    
    public void SetupEmotion(string title, string description, Sprite charSprite, Action onClick)
    {
        currType = SceneType.Emotion;
        OnCardClicked = onClick;

        nameTMP.text = title;
        textTMP.text = description;
        if (charSprite != null) character.sprite = charSprite;
        else character.color = Color.clear;
        
        costTMP.text = "";
    }
    
    private void OnMouseOver()
    {
        switch (currType)
        {
            case SceneType.Dialogue:
                if (originPRS != null) 
                    transform.localScale = originPRS.scale * 1.2f;
                break;
            case  SceneType.Emotion:
                if (originPRS != null) 
                    transform.localScale = originPRS.scale * 1.2f;
                break;
            case  SceneType.General:
                CardManager.Inst.CardMouseOver(this);
                break;
        }

        if (item != null && textTMP != null && !tooltipCreated && !CardManager.Inst.isMyCardDrag)
        {
            int tooltipCount = 0;
            foreach(Keyword keyword in DataManager.Inst.keywordSO.keywords)
            {
                if(textTMP.text.Contains(keyword.word))
                {
                    var keywordTooltipObj = Instantiate(cardUITooltipPrefab, tooltipPos.position, Utils.QI);
                    keywordTooltipObj.transform.SetParent(tooltipCanvas.transform, false);
                    keywordTooltipObj.transform.SetAsLastSibling();
                    keywordTooltipObj.transform.position = Camera.main.WorldToScreenPoint(tooltipPos.position);
                    activeTooltips.Add(keywordTooltipObj);

                    CardTooltip keywordTooltip = keywordTooltipObj.GetComponent<CardTooltip>();
                    keywordTooltip.SetTooltip(keyword.word, keyword.explanation);
                    tooltipCreated = true;
                    tooltipCount++;
                }
            }
        }
    }

    private void OnMouseExit()
    {
        switch (currType)
        {
            case SceneType.Dialogue:
                transform.localScale = originPRS.scale; 
                break;
            case  SceneType.Emotion:
                transform.localScale = originPRS.scale; 
                OnCardClicked?.Invoke();
                break;
            case  SceneType.General:
                CardManager.Inst.CardMouseExit(this);
                break;
        }

        if (tooltipCreated)
        {
            foreach(GameObject tooltipObj in activeTooltips)
            {
                Destroy(tooltipObj);
            }
            activeTooltips.Clear();
            tooltipCreated = false;
        }
    }

    private void OnMouseDown()
    {
        switch (currType)
        {
            case SceneType.Dialogue:
                OnCardClicked?.Invoke();
                break;
            case  SceneType.Emotion:
                break;
            case  SceneType.General:
                CardManager.Inst.CardMouseDown(this);
                break;
        }

        if (tooltipCreated)
        {
            foreach(GameObject tooltipObj in activeTooltips)
            {
                Destroy(tooltipObj);
            }
            activeTooltips.Clear();
            tooltipCreated = false;
        }
    }

    private void OnMouseUp()
    {
        switch (currType)
        {
            case SceneType.Dialogue:
                break;
            case  SceneType.Emotion:
                break;
            case  SceneType.General:
                CardManager.Inst.CardMouseUp(this);
                break;
        }

        if (tooltipCreated)
        {
            foreach(GameObject tooltipObj in activeTooltips)
            {
                Destroy(tooltipObj);
            }
            activeTooltips.Clear();
            tooltipCreated = false;
        }
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

    public bool IsCardUseable(int enemyIdx)
    {
        int buffedCost = BuffManager.Inst.GetBuffedCardCost(item);
        if (buffedCost > TurnManager.Inst.nowCost)
        {
            return false;
        }
        if(BuffManager.Inst.allCardTypeBlockBuff[item.type] == true)
        {
            Debug.Log(item.type + " 카드 사용이 차단되었습니다!");
            return false;
        }
        switch(item.name)
        {
            case "흡혈 부여":
            case "흡혈 부여+":
                RouletteType bloodSteal = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                if(!RouletteManager.Inst.IsRouletteEnchantable(true, bloodSteal, enemyIdx)) return false;
                break;
            case "마술 상자":
            case "마술 상자+":
                RouletteType magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                if(!RouletteManager.Inst.IsRouletteEnchantable(false, magicBox)) return false;
                break;
            case "마술 카드":
            case "마술 카드+":
                RouletteType magicCard = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                if(!RouletteManager.Inst.IsRouletteEnchantable(false, magicCard)) return false;
                break;
            case "수트 체인지":
            case "수트 체인지+":
                magicCard = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                if(RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype != magicCard) return false;
                break;
            case "그랜드 피날레":
            case "그랜드 피날레+":
                magicCard = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                bool skillFound = false;
                bool enchantFound = false;
                bool turnFound = false;
                bool dreamFound = false;
                for(int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if(RouletteManager.Inst.roulettePieces[i].roulette.rtype == magicCard)
                    {
                        Debug.Log("그랜드 피날레 룰렛 타입: " + PassiveManager.Inst.playerSpecialRoulette_lastCardType[i].ToString());
                        switch(PassiveManager.Inst.playerSpecialRoulette_lastCardType[i])
                        {
                            case CardType.Skill:
                                skillFound = true; break;
                            case CardType.Enchant:
                                enchantFound = true; break;
                            case CardType.Turn:
                                turnFound = true; break;
                            case CardType.Dream:
                                dreamFound = true; break;
                        }
                    }
                }
                if((skillFound && enchantFound && turnFound && dreamFound) == false) return false;
                break;
            case "꽁꽁 얼리기":
            case "꽁꽁 얼리기+":
                RouletteType frozen = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                if(!RouletteManager.Inst.IsRouletteEnchantable(false, frozen)) return false;
                break;
            case "차가운 악수":
            case "차가운 악수+":
                frozen = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                if(!RouletteManager.Inst.IsRouletteEnchantable(true, frozen, enemyIdx)) return false;
                break;
            case "발톱 세우기":
            case "발톱 세우기+":
                RouletteType claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                if(!RouletteManager.Inst.IsRouletteEnchantable(true, claw, enemyIdx)) return false;
                break;
            case "소심한 할퀴기":
            case "소심한 할퀴기+":
                claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                if(!RouletteManager.Inst.IsRouletteEnchantable(true, claw, enemyIdx)) return false;
                break;
            case "날카로운 발톱":
            case "날카로운 발톱+":
                claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                RouletteType claw2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 0));
                if(!RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype.Equals(claw) && !RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype.Equals(claw2)) return false;
                break;
            case "실뭉치":
            case "실뭉치+":
                RouletteType furball = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                if(!RouletteManager.Inst.IsRouletteEnchantable(false, furball)) return false;
                break;
        }
        return true;
    }

    public bool IsCardSpecialEffect(int enemyIdx)
    {
        switch(item.name)
        {
            case "비둘기 마술":
            case "비둘기 마술+":
                RouletteType magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                return RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].roulette.rtype == magicBox;
            case "복제 마술":
            case "복제 마술+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                return RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == magicBox;
            case "절단 마술":
            case "절단 마술+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                return RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].roulette.rtype == magicBox;
            case "환영 마술+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                return RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == magicBox;
            case "토끼 마술":
            case "토끼 마술+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                return RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == magicBox;
            case "폭발 마술":
            case "폭발 마술+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                return RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].roulette.rtype == magicBox;
            case "마술 준비+":
                if (TurnManager.Inst.nowCost >= GetBuffedVal(item.cardValues[1], ECardValueType.Special)) return true;
                break;
        }
        return false;
    }

    public bool UseCard(int enemyIdx)
    {
        int buffedCost = BuffManager.Inst.GetBuffedCardCost(item);
        if (buffedCost > TurnManager.Inst.nowCost)
        {
            return false;
        }
        if(BuffManager.Inst.allCardTypeBlockBuff[item.type] == true)
        {
            Debug.Log(item.type + " 카드 사용이 차단되었습니다!");
            return false;
        }
        bool isCardUsed = true;
        switch (item.name)
        {
            case "1칸 회전":
                RouletteManager.Inst.Spin(true, 1);
                break;
            case "2칸 회전":
                RouletteManager.Inst.Spin(true, 2);
                break;
            case "3칸 회전":
                RouletteManager.Inst.Spin(true, 3);
                break;
            case "1칸 역회전":
                RouletteManager.Inst.Spin(false, 1);
                break;
            case "2칸 역회전":
                RouletteManager.Inst.Spin(false, 2);
                break;
            case "3칸 역회전":
                RouletteManager.Inst.Spin(false, 3);
                break;
            case "구원":
                TurnManager.Inst.TakeDmg(-TurnManager.Inst.maxHealth / 2, EDamageSource.Card);
                break;
            case "혼령":
                TurnManager.Inst.IncreaseCost(2);
                TurnManager.Inst.TakeDmg(2, EDamageSource.Card);
                break;
            case "공격 부여":
                isCardUsed = RouletteManager.Inst.EnchantRoulette(true, new RouletteType(ERouletteType.Attack), GetBuffedVal(item.cardValues[0], ECardValueType.Special), enemyIdx);
                break;
            case "수비 부여":
                isCardUsed = RouletteManager.Inst.EnchantRoulette(false, new RouletteType(ERouletteType.Shield), GetBuffedVal(item.cardValues[0], ECardValueType.Special));
                break;
            case "흡혈 부여":
            case "흡혈 부여+":
                RouletteType bloodSteal = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                // RouletteType bloodSteal_plus = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                isCardUsed = RouletteManager.Inst.EnchantRoulette(true, bloodSteal, PassiveManager.Inst.playerSpecialRoulettes[bloodSteal.specialTypeIdx].baseVal, enemyIdx);
                if(isCardUsed && item.name == "흡혈 부여+")
                {
                    RouletteManager.Inst.EnhanceRoulette(true, enemyIdx);
                }
                break;
            case "혈액 순환":
            case "혈액 순환+":
                RouletteManager.Inst.Spin(true, GetBuffedVal(item.cardValues[0], ECardValueType.Special));
                TurnManager.Inst.TakeDmg(GetBuffedVal(item.cardValues[1], ECardValueType.Damage), EDamageSource.Card);
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
                RouletteManager.Inst.Spin(true, GetBuffedVal(item.cardValues[0], ECardValueType.Special));
                TurnManager.Inst.TakeDmg(GetBuffedVal(item.cardValues[1], ECardValueType.Damage), EDamageSource.Card);
                break;
            case "피는 나의 힘":
            case "피는 나의 힘+":
                TurnManager.Inst.TakeDmg(GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card);
                TurnManager.Inst.IncreaseCost(GetBuffedVal(item.cardValues[1], ECardValueType.Special));
                break;
            case "긴급 수혈":
            case "긴급 수혈+":
                TurnManager.Inst.TakeDmg(-GetBuffedVal(item.cardValues[0], ECardValueType.Heal), EDamageSource.Card);
                break;
            case "휴머니스트":
            case "휴머니스트+":
                for(int i = 0; i <= EnemyManager.Inst.subEnemies.Length; i++)
                {
                    if(i != 0 && (EnemyManager.Inst.subEnemies[i - 1] == null || EnemyManager.Inst.subEnemies[i - 1].name == null)) continue;
                    BuffManager.Inst.AddShowBuff("주저함", EBuffAffectType.Enemy, GetBuffedVal(item.cardValues[0], ECardValueType.Special), false, null, i);
                }
                BuffManager.Inst.AddShowBuff("주저함", EBuffAffectType.Roulette, GetBuffedVal(item.cardValues[0], ECardValueType.Special), false);
                break;
            case "블루 블러드":
            case "블루 블러드+":
                BuffManager.Inst.AddShowBuff("블루 블러드", EBuffAffectType.Roulette, GetBuffedVal(item.cardValues[0], ECardValueType.Special), false, new List<int>{GetBuffedVal(item.cardValues[1], ECardValueType.Special), 0});
                break;
            case "만찬 시간":
            case "만찬 시간+":
                BuffManager.Inst.AddShowBuff("만찬 시간", EBuffAffectType.Roulette, GetBuffedVal(item.cardValues[0], ECardValueType.Special), false, new List<int>{GetBuffedVal(item.cardValues[1], ECardValueType.Special)});
                break;
            case "핏빛 날개":
            case "핏빛 날개+":
                int bloodwing_spinnnum = GetBuffedVal(item.cardValues[0], ECardValueType.Special);
                for (int i = 0; i <= bloodwing_spinnnum; i++)
                {
                    for(int j = 0; j <= EnemyManager.Inst.subEnemies.Length; j++)
                    {
                        if(i != 0 && (EnemyManager.Inst.subEnemies[i - 1] == null || EnemyManager.Inst.subEnemies[i - 1].name == null)) continue;
                        int tempIdx = (RouletteManager.Inst.EnemyIdxSpinOffset(j) + i) % RouletteManager.rouletteNum;
                        if (RouletteManager.Inst.roulettePieces[tempIdx].roulette.rtype == new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0)))
                        // || RouletteManager.Inst.roulettePieces[tempIdx].roulette.rtype == new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1)))
                        {
                            RouletteManager.Inst.ActivateRoulettePiece(tempIdx, true, j);
                        }
                    }
                }
                RouletteManager.Inst.Spin(false, bloodwing_spinnnum);
                break;
            case "마술 상자":
            case "마술 상자+":
                RouletteType magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                RouletteType magicBox2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 0));
                // int boxVal = 12;
                // if(item.name == "마술 상자+") boxVal = 15;
                isCardUsed = RouletteManager.Inst.EnchantRoulette(false, magicBox, PassiveManager.Inst.playerSpecialRoulettes[magicBox.specialTypeIdx].baseVal);
                if (item.name == "마술 상자+" && isCardUsed)
                {
                    // RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].tooltip.tooltipTitle = "마술 상자+";
                    // RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].isEnhanced = true;
                    RouletteManager.Inst.EnhanceRoulette(false);
                }
                break;
            case "비둘기 마술":
            case "비둘기 마술+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                magicBox2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 0));
                bool checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].roulette.rtype == magicBox || RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].roulette.rtype == magicBox2;
                CardManager.Inst.cardEffectEndAction.Add(() => 
                {
                    TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card, enemyIdx);
                    if (checkMagic)
                    {
                        if(enemyIdx == 0) EnemyManager.Inst.RemoveAction(0);
                        else EnemyManager.Inst.RemoveSubEnemyAction(enemyIdx - 1, 0);
                    }
                });
                if(CardManager.Inst.cardCurrentEffectName == "")
                {
                    CardManager.Inst.cardCurrentEffectName = "Pigeon";
                    CardManager.Inst.cardEffect.SetTrigger(CardManager.Inst.cardCurrentEffectName);
                }
                else
                {
                    CardManager.Inst.cardEffectQueue.Add("Pigeon");
                }
                break;
            case "복제 마술":
            case "복제 마술+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                magicBox2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 0));
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == magicBox || RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == magicBox2;
                CardManager.Inst.CardSelectModeTransit(ECardSelectMode.Duplicate, GetBuffedVal(item.cardValues[0], ECardValueType.Special));
                if (checkMagic)
                {
                    TurnManager.Inst.IncreaseCost(GetBuffedVal(item.cardValues[1], ECardValueType.Special));
                }
                break;
            case "절단 마술":
            case "절단 마술+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                magicBox2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 0));
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].roulette.rtype == magicBox || RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].roulette.rtype == magicBox2;
                CardManager.Inst.cardEffectEndAction.Add(() => 
                {
                    TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card, enemyIdx);
                    if (checkMagic)
                    {
                        TurnManager.Inst.GetShield(true, -TurnManager.Inst.enemyShieldHealth[enemyIdx], EDamageSource.Card);
                    }
                });
                if(CardManager.Inst.cardCurrentEffectName == "")
                {
                    CardManager.Inst.cardCurrentEffectName = "Cut";
                    CardManager.Inst.cardEffect.SetTrigger(CardManager.Inst.cardCurrentEffectName);
                }
                else
                {
                    CardManager.Inst.cardEffectQueue.Add("Cut");
                }
                break;
            case "순간이동 마술":
            case "순간이동 마술+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                magicBox2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 0));
                int spinVal_teleport = 0;
                for (int i = 0; i < GetBuffedVal(item.cardValues[0], ECardValueType.Special); i++)
                {
                    spinVal_teleport++;
                    int tempIdx = (RouletteManager.Inst.playerLookat + RouletteManager.rouletteNum + spinVal_teleport) % RouletteManager.rouletteNum;
                    if (RouletteManager.Inst.roulettePieces[tempIdx].roulette.rtype == magicBox || RouletteManager.Inst.roulettePieces[tempIdx].roulette.rtype == magicBox2) break;
                }
                RouletteManager.Inst.Spin(false, spinVal_teleport);
                break;
            case "환영 마술":
            case "환영 마술+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                magicBox2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 0));
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == magicBox || RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == magicBox2;
                BuffManager.Inst.AddShowBuff("환영", EBuffAffectType.Player, GetBuffedVal(item.cardValues[0], ECardValueType.Special), false);
                if (item.name == "환영 마술+" && checkMagic)
                {
                    TurnManager.Inst.IncreaseCost(GetBuffedVal(item.cardValues[1], ECardValueType.Special));
                }
                break;
            case "토끼 마술":
            case "토끼 마술+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                magicBox2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 0));
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == magicBox || RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == magicBox2;
                RouletteManager.Inst.Spin(true, GetBuffedVal(item.cardValues[0], ECardValueType.Special));
                if (checkMagic)
                {
                    RouletteManager.Inst.Spin(true, GetBuffedVal(item.cardValues[1], ECardValueType.Special));
                }
                break;
            case "폭발 마술":
            case "폭발 마술+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                magicBox2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 0));
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].roulette.rtype == magicBox || RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].roulette.rtype == magicBox2;
                CardManager.Inst.cardEffectEndAction.Add(() => 
                {
                    Debug.Log("폭발 마술 데미지: " + GetBuffedVal(item.cardValues[0], ECardValueType.Damage));
                    TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card, enemyIdx);
                });
                if(CardManager.Inst.cardCurrentEffectName == "")
                {
                    CardManager.Inst.cardCurrentEffectName = "Explosion";
                    CardManager.Inst.cardEffect.SetTrigger(CardManager.Inst.cardCurrentEffectName);
                }
                else
                {
                    CardManager.Inst.cardEffectQueue.Add("Explosion");
                }
                
                if (checkMagic)
                {
                    CardManager.Inst.cardEffectEndAction2.Add(() => 
                    {
                        CardManager.Inst.cardEffectEndAction.Add(() => 
                        {
                            TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[1], ECardValueType.Damage), EDamageSource.Card, enemyIdx);
                        });
                        if(CardManager.Inst.cardCurrentEffectName == "")
                        {
                            CardManager.Inst.cardCurrentEffectName = "Explosion";
                            CardManager.Inst.cardEffect.SetTrigger(CardManager.Inst.cardCurrentEffectName);
                        }
                        else
                        {
                            CardManager.Inst.cardEffectQueue.Add("Explosion");
                        }
                        RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].RouletteClear();
                    });
                    if(CardManager.Inst.cardCurrentEffectName2 == "")
                    {
                        CardManager.Inst.cardCurrentEffectName2 = "Explosion";
                        CardManager.Inst.cardEffect2.SetTrigger(CardManager.Inst.cardCurrentEffectName2);
                    }
                    else
                    {
                        CardManager.Inst.cardEffectQueue2.Add("Explosion");
                    }
                }
                break;
            case "재빠른 손놀림":
            case "재빠른 손놀림+":
                TurnManager.Inst.StartDraw(GetBuffedVal(item.cardValues[0], ECardValueType.Special), null);
                break;
            case "배니싱":
            case "배니싱+":
                CardManager.Inst.CardSelectModeTransit(ECardSelectMode.Vanish, 1);
                TurnManager.OnSelectCardDone += () =>
                {
                    if(CardManager.Inst.selectedCardList.Count > 0)
                    {
                        TurnManager.Inst.IncreaseCost(CardManager.Inst.selectedCardList[0].GetComponent<CardUI>().item.cost);
                    }
                };
                // 구현예정
                break;
            case "마술 준비":
            case "마술 준비+":
                RouletteManager.Inst.Spin(true, TurnManager.Inst.nowCost * GetBuffedVal(item.cardValues[0], ECardValueType.Special));
                if (item.name == "마술 준비+") BuffManager.Inst.AddShowBuff("강화", EBuffAffectType.Roulette, TurnManager.Inst.nowCost, false);
                break;
            case "커튼콜":
            case "커튼콜+":
                BuffManager.Inst.AddShowBuff("커튼콜", EBuffAffectType.Player, 1, true);
                break;
            case "마술 카드":
            case "마술 카드+":
                RouletteType magicCard = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                RouletteType magicCard2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 1));
                isCardUsed = RouletteManager.Inst.EnchantRoulette(false, magicCard, PassiveManager.Inst.playerSpecialRoulettes[magicCard.specialTypeIdx].baseVal);
                if (item.name == "마술 카드+" && isCardUsed)
                {
                    RouletteManager.Inst.EnhanceRoulette(false);
                }
                if (PassiveManager.Inst.lastCardType == CardType.Enchant)
                {
                    BuffManager.AddBuffToTarget(BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat]], -1, 1, -1);
                }
                break;
            case "수트 체인지":
            case "수트 체인지+":
                magicCard = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                magicCard2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 1));
                isCardUsed = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == magicCard || RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == magicCard2;
                if (isCardUsed)
                {
                    string title = "";
                    string text = "";
                    switch(PassiveManager.Inst.playerSpecialRoulette_lastCardType[RouletteManager.Inst.playerLookat])
                    {
                        case CardType.Skill:
                            PassiveManager.Inst.playerSpecialRoulette_lastCardType[RouletteManager.Inst.playerLookat] = CardType.Enchant;
                            if (RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].isEnhanced)
                            {
                                title = "다이아몬드 룰렛+";
                                text = "방어도를 값만큼 부여합니다. 부여 카드가 사용될 때마다 값이 6 증가합니다.";
                            }
                            else
                            {
                                title = "다이아몬드 룰렛";
                                text = "방어도를 값만큼 부여합니다. 부여 카드가 사용될 때마다 값이 4 증가합니다.";
                            }
                            break;
                        case CardType.Enchant:
                            PassiveManager.Inst.playerSpecialRoulette_lastCardType[RouletteManager.Inst.playerLookat] = CardType.Skill;
                            if (RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].isEnhanced)
                            {
                                title = "클로버 룰렛+";
                                text = "값 피해를 줍니다. 스킬 카드가 사용될 때마다 값이 6 증가합니다.";
                            }
                            else
                            {
                                title = "클로버 룰렛";
                                text = "값 피해를 줍니다. 스킬 카드가 사용될 때마다 값이 4 증가합니다.";
                            }
                            break;
                        case CardType.Turn:
                            PassiveManager.Inst.playerSpecialRoulette_lastCardType[RouletteManager.Inst.playerLookat] = CardType.Dream;
                            if (RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].isEnhanced)
                            {
                                title = "스페이드 룰렛+";
                                text = "행동력을 값만큼 회복합니다. 몽상 카드가 사용될 때마다 값이 1 증가합니다.";
                            }
                            else
                            {
                                title = "스페이드 룰렛";
                                text = "행동력을 값만큼 회복합니다. 몽상 카드가 사용될 때마다 값이 1 증가합니다.";
                            }
                            break;
                        case CardType.Dream:
                            PassiveManager.Inst.playerSpecialRoulette_lastCardType[RouletteManager.Inst.playerLookat] = CardType.Turn;
                            if (RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].isEnhanced)
                            {
                                title = "하트 룰렛+";
                                text = "체력을 값만큼 회복합니다. 회전 카드가 사용될 때마다 값이 1 증가합니다.";
                            }
                            else
                            {
                                title = "하트 룰렛";
                                text = "체력을 값만큼 회복합니다. 회전 카드가 사용될 때마다 값이 1 증가합니다.";
                            }
                            break;
                    }
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].originalTooltipTitle = title;
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].originalTooltipText = text;
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].SetRoulettePieceTooltip(title, text);
                }
                break;
            case "현혹":
            case "현혹+":
                BuffManager.Inst.AddShowBuff("현혹", EBuffAffectType.Player, GetBuffedVal(item.cardValues[0], ECardValueType.Special), false, new List<int>(1){GetBuffedVal(item.cardValues[1], ECardValueType.Special)}, enemyIdx);
                break;
            case "그랜드 피날레":
            case "그랜드 피날레+":
                magicCard = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                magicCard2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 1));
                bool skillFound = false;
                bool enchantFound = false;
                bool turnFound = false;
                bool dreamFound = false;
                for(int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if(RouletteManager.Inst.roulettePieces[i].roulette.rtype == magicCard || RouletteManager.Inst.roulettePieces[i].roulette.rtype == magicCard2)
                    {
                        Debug.Log("그랜드 피날레 룰렛 타입: " + PassiveManager.Inst.playerSpecialRoulette_lastCardType[i].ToString());
                        switch(PassiveManager.Inst.playerSpecialRoulette_lastCardType[i])
                        {
                            case CardType.Skill:
                                skillFound = true; break;
                            case CardType.Enchant:
                                enchantFound = true; break;
                            case CardType.Turn:
                                turnFound = true; break;
                            case CardType.Dream:
                                dreamFound = true; break;
                        }
                    }
                }
                isCardUsed = skillFound && enchantFound && turnFound && dreamFound;
                if (isCardUsed)
                {
                    CardManager.Inst.cardEffectFadeOut.SetActive(true);
                    CardManager.Inst.cardEffectEndAction.Add(() => 
                    {
                        for(int i = 0; i <= EnemyManager.Inst.subEnemies.Length; i++)
                        {
                            if(i != 0 && (EnemyManager.Inst.subEnemies[i - 1] == null || EnemyManager.Inst.subEnemies[i - 1].name == null)) continue;
                            TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card, i);
                        }
                        for(int i = 0; i < RouletteManager.rouletteNum; i++)
                        {
                            if(RouletteManager.Inst.roulettePieces[i].roulette.rtype == magicCard || RouletteManager.Inst.roulettePieces[i].roulette.rtype == magicCard2)
                            {
                                PassiveManager.playerSpecialRouletteClear[PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1)]?.Invoke(i);
                            }
                        }
                        CardManager.Inst.cardEffectFadeOut.SetActive(false);
                    });
                    if(CardManager.Inst.cardCurrentEffectName == "")
                    {
                        CardManager.Inst.cardCurrentEffectName = "Grand";
                        CardManager.Inst.cardEffect.SetTrigger(CardManager.Inst.cardCurrentEffectName);
                    }
                    else
                    {
                        CardManager.Inst.cardEffectQueue.Add("Grand");
                    }
                }
                break;
            case "에이스":
            case "에이스+":
                TurnManager.Inst.TriggerPlayerPassive(1);
                break;
            case "꽁꽁 얼리기":
            case "꽁꽁 얼리기+":
                RouletteType frozen = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                isCardUsed = RouletteManager.Inst.EnchantRoulette(false, frozen, PassiveManager.Inst.playerSpecialRoulettes[frozen.specialTypeIdx].baseVal);
                if(isCardUsed && item.name == "꽁꽁 얼리기+")
                {
                    RouletteManager.Inst.EnhanceRoulette(false);
                }
                break;
            case "얼음 방패":
            case "얼음 방패+":
                frozen = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                TurnManager.Inst.GetShield(false, GetBuffedVal(item.cardValues[0], ECardValueType.Shield), EDamageSource.Card);
                bool checkFrozen = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == frozen;
                if (checkFrozen)
                {
                    TurnManager.Inst.GetShield(false, GetBuffedVal(item.cardValues[1], ECardValueType.Shield), EDamageSource.Card);
                }
                break;
            case "나뭇가지 손":
            case "나뭇가지 손+":
                TurnManager.Inst.StartDraw(GetBuffedVal(item.cardValues[0], ECardValueType.Special), null);
                TurnManager.Inst.TriggerPlayerPassive(-GetBuffedVal(item.cardValues[1], ECardValueType.Special));
                break;
            case "데굴데굴":
            case "데굴데굴+":
                RouletteManager.Inst.Spin(true, GetBuffedVal(item.cardValues[0], ECardValueType.Special));
                if (item.name == "데굴데굴+")
                {
                    for(int i = 0; i <= EnemyManager.Inst.subEnemies.Length; i++)
                    {
                        if(i != 0 && (EnemyManager.Inst.subEnemies[i - 1] == null || EnemyManager.Inst.subEnemies[i - 1].name == null)) continue;
                        TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[1], ECardValueType.Damage), EDamageSource.Card, i);
                    }
                }
                Action<int> repeatCard = null;
                repeatCard = (x) =>
                {
                    Sequence wait = DOTween.Sequence();
                    wait.AppendInterval(0.5f).OnComplete(() =>
                    {
                        if (TurnManager.Inst.nowCost >= buffedCost)
                        {
                            TurnManager.Inst.IncreaseCost(-buffedCost);
                            RouletteManager.Inst.Spin(true, GetBuffedVal(item.cardValues[0], ECardValueType.Special));
                            if (item.name == "데굴데굴+")
                            {
                                for(int i = 0; i <= EnemyManager.Inst.subEnemies.Length; i++)
                                {
                                    if(i != 0 && (EnemyManager.Inst.subEnemies[i - 1] == null || EnemyManager.Inst.subEnemies[i - 1].name == null)) continue;
                                    TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[1], ECardValueType.Damage), EDamageSource.Card, i);
                                }
                            }
                        }
                        TurnManager.AfterRouletteSpin -= repeatCard;
                    });
                };
                TurnManager.AfterRouletteSpin += repeatCard;
                break;
            case "차가운 악수":
            case "차가운 악수+":
                frozen = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                isCardUsed = RouletteManager.Inst.EnchantRoulette(true, frozen, GetBuffedVal(item.cardValues[0], ECardValueType.Special), enemyIdx);
                if (isCardUsed)
                {
                    BuffManager.Inst.AddShowBuff("과민함", EBuffAffectType.Enemy, GetBuffedVal(item.cardValues[1], ECardValueType.Special), false, null, enemyIdx);
                }
                break;
            case "스노우볼링":
            case "스노우볼링+":
                RouletteManager.Inst.Spin(true, GetBuffedVal(item.cardValues[0], ECardValueType.Special));
                TurnManager.Inst.GetShield(false, TurnManager.Inst.shieldHealth * (GetBuffedVal(item.cardValues[1], ECardValueType.Special) - 1), EDamageSource.Card);
                break;
            case "녹아내리기":
            case "녹아내리기+":
                TurnManager.Inst.TriggerPlayerPassive(-GetBuffedVal(item.cardValues[0], ECardValueType.Special));
                TurnManager.Inst.GetShield(false, GetBuffedVal(item.cardValues[1], ECardValueType.Shield), EDamageSource.Card);
                break;
            case "목도리":
            case "목도리+":
                for (int i = 0; i <= GetBuffedVal(item.cardValues[0], ECardValueType.Special); i++)
                {
                    int tempIdx = (RouletteManager.Inst.playerLookat + RouletteManager.rouletteNum - i) % RouletteManager.rouletteNum;
                    if (RouletteManager.Inst.roulettePieces[tempIdx].roulette.rtype == new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0)))
                    {
                        TurnManager.Inst.GetShield(false, GetBuffedVal(item.cardValues[1], ECardValueType.Shield), EDamageSource.Card);
                    }
                }
                RouletteManager.Inst.Spin(true, GetBuffedVal(item.cardValues[0], ECardValueType.Special));
                break;
            case "얼음 깨기":
            case "얼음 깨기+":
                for(int i = 0; i <= EnemyManager.Inst.subEnemies.Length; i++)
                {
                    if(i != 0 && (EnemyManager.Inst.subEnemies[i - 1] == null || EnemyManager.Inst.subEnemies[i - 1].name == null)) continue;
                    if (RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].roulette.rtype == new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0)))
                    {
                        TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card, i);
                    }
                }
                for (int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if (RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0)))
                    {
                        PassiveManager.playerSpecialRouletteClear[PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0)]?.Invoke(i);
                    }
                }
                break;
            case "눈싸움":
            case "눈싸움+":
                int frozenCnt = 0;
                for (int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if (RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0)))
                    {
                        frozenCnt++;
                    }
                }
                TurnManager.Inst.EnemyTakeDmg(frozenCnt * GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card, enemyIdx);
                break;
            case "폭설":
            case "폭설+":
                for (int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if (RouletteManager.Inst.roulettePieces[i].roulette.rtype.type == ERouletteType.None)
                    {
                        RouletteManager.Inst.EnchantRoulettePiece(i, new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0)), item.cardValues[0]);
                    }
                }
                break;
            case "끝나지 않는 겨울":
            case "끝나지 않는 겨울+":
                int frozenTimeInc = GetBuffedVal(item.cardValues[0], ECardValueType.Special);
                for (int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if (RouletteManager.Inst.roulettePieces[i].roulette.rtype == new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0)))
                    {
                        RouletteManager.Inst.roulettePieces[i].roulette.value += frozenTimeInc;
                    }
                }
                break;
            case "발톱 세우기":
            case "발톱 세우기+":
                RouletteType claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                RouletteType claw2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 0));
                // int rltVal = 9;
                // if(item.name == "발톱 세우기+") rltVal = 12;
                isCardUsed = RouletteManager.Inst.EnchantRoulette(true, claw, PassiveManager.Inst.playerSpecialRoulettes[claw.specialTypeIdx].baseVal, enemyIdx);
                if(item.name == "발톱 세우기+" && isCardUsed)
                {
                    Debug.Log("발톱 세우기 룰렛 강화!");
                    // RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].tooltip.tooltipTitle = "발톱+";
                    // RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].isEnhanced = true;
                    RouletteManager.Inst.EnhanceRoulette(true, enemyIdx);
                }
                break;
            case "알뜰한 사냥꾼":
            case "알뜰한 사냥꾼+":
                claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                claw2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 0));
                int incCost = 0;
                for(int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if((RouletteManager.Inst.roulettePieces[i].roulette.rtype == claw || RouletteManager.Inst.roulettePieces[i].roulette.rtype == claw2) && BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]) <= GetBuffedVal(item.cardValues[0], ECardValueType.Special))
                    {
                        RouletteManager.Inst.roulettePieces[i].RouletteClear();
                        incCost += GetBuffedVal(item.cardValues[1], ECardValueType.Special);
                    }
                }
                TurnManager.Inst.IncreaseCost(incCost);
                break;
            case "마구 할퀴기":
            case "마구 할퀴기+":
                claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                claw2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 0));
                int totalDmg_claw = 0;
                for(int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if(RouletteManager.Inst.roulettePieces[i].roulette.rtype == claw || RouletteManager.Inst.roulettePieces[i].roulette.rtype == claw2)
                    {
                        int tempVal = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                        tempVal = tempVal - tempVal / 2;
                        totalDmg_claw += tempVal;
                        BuffManager.AddBuffToTarget(BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[i]], -tempVal, 1, -1);
                    }
                }
                if(item.name == "마구 할퀴기+") totalDmg_claw = (int)(totalDmg_claw * 1.5f);
                if(totalDmg_claw > 0)
                {
                    TurnManager.Inst.EnemyTakeDmg(totalDmg_claw, EDamageSource.Card, enemyIdx);
                }
                break;
            case "발톱 손질":
            case "발톱 손질+":
                claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                claw2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 0));
                for(int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if(RouletteManager.Inst.roulettePieces[i].roulette.rtype == claw || RouletteManager.Inst.roulettePieces[i].roulette.rtype == claw2)
                    {
                        RouletteManager.Inst.roulettePieces[i].roulette.value += GetBuffedVal(item.cardValues[0], ECardValueType.Special);
                    }
                }
                break;
            case "소심한 할퀴기":
            case "소심한 할퀴기+":
                claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                isCardUsed = RouletteManager.Inst.EnchantRoulette(true, claw, GetBuffedVal(item.cardValues[1], ECardValueType.Special), enemyIdx);
                if(isCardUsed) TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card, enemyIdx);
                break;
            case "날카로운 발톱":
            case "날카로운 발톱+":
                claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                claw2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 0));
                if(RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype.Equals(claw) || RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype.Equals(claw2))
                {
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.value *= GetBuffedVal(item.cardValues[0], ECardValueType.Special);
                }
                else
                {
                    isCardUsed = false;
                }
                break;
            case "고양이의 시간":
            case "고양이의 시간+":
                claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                // rltVal = 9;
                // if(item.name == "고양이의 시간+") rltVal = 12;
                for(int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if(RouletteManager.Inst.roulettePieces[i].roulette.rtype.type == ERouletteType.Attack)
                    {
                        RouletteManager.Inst.EnchantRoulettePiece(i, claw, PassiveManager.Inst.playerSpecialRoulettes[claw.specialTypeIdx].baseVal);
                        if(item.name == "고양이의 시간+" && isCardUsed)
                        {
                            // RouletteManager.Inst.roulettePieces[i].tooltip.tooltipTitle = "발톱+";
                            // RouletteManager.Inst.roulettePieces[i].isEnhanced = true;
                            RouletteManager.Inst.EnhanceRoulettePiece(i);
                        }
                    }
                }
                break;
            case "기민함":
            case "기민함+":
                BuffManager.Inst.AddShowBuff("환영", EBuffAffectType.Player, GetBuffedVal(item.cardValues[0], ECardValueType.Special), false);
                break;
            case "실뭉치":
            case "실뭉치+":
                RouletteType furball = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                RouletteType furball2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 1));
                // RouletteType furball_plus = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 2));
                isCardUsed = RouletteManager.Inst.EnchantRoulette(false, furball, PassiveManager.Inst.playerSpecialRoulettes[furball.specialTypeIdx].baseVal);
                if(isCardUsed && item.name == "실뭉치+")
                {
                    RouletteManager.Inst.EnhanceRoulette(false);
                }
                // if(item.name == "실뭉치") isCardUsed = RouletteManager.Inst.EnchantRoulette(false, furball, PassiveManager.Inst.playerSpecialRoulettes[PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1)].baseVal);
                // else isCardUsed = RouletteManager.Inst.EnchantRoulette(false, furball_plus, PassiveManager.Inst.playerSpecialRoulettes[PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 2)].baseVal);
                break;
            case "고양이걸음":
            case "고양이걸음+":
                RouletteManager.Inst.Spin(true, GetBuffedVal(item.cardValues[0], ECardValueType.Special));
                TurnManager.Inst.StartDraw(GetBuffedVal(item.cardValues[1], ECardValueType.Special), null);
                break;
            case "실 풀기":
            case "실 풀기+":
                furball = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                furball2 = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum != item.dreamPieceNum, 1));
                // furball_plus = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                int totalShield_furball = 0;
                for(int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if(RouletteManager.Inst.roulettePieces[i].roulette.rtype.Equals(furball) || RouletteManager.Inst.roulettePieces[i].roulette.rtype.Equals(furball2))
                    {
                        int tempVal = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[i]);
                        tempVal = tempVal - tempVal / 2;
                        totalShield_furball += tempVal;
                        BuffManager.AddBuffToTarget(BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[i]], -tempVal, 1, -1);
                    }
                }
                if(item.name == "실 풀기+") totalShield_furball = (int)(totalShield_furball * 1.5f);
                if(totalShield_furball > 0)
                {
                    TurnManager.Inst.GetShield(false, totalShield_furball, EDamageSource.Card);
                }
                break;
            case "놀이 시간":
            case "놀이 시간+":
                BuffManager.Inst.AddShowBuff("놀이 시간", EBuffAffectType.Player, GetBuffedVal(item.cardValues[0], ECardValueType.Special), false);
                break;
            case "숨기":
            case "숨기+":
                int spinVal = 0;
                for (int i = 0; i < GetBuffedVal(item.cardValues[0], ECardValueType.Special); i++)
                {
                    spinVal++;
                    int tempIdx = (RouletteManager.Inst.playerLookat + RouletteManager.rouletteNum - spinVal) % RouletteManager.rouletteNum;
                    if (RouletteManager.Inst.roulettePieces[tempIdx].roulette.rtype.type == ERouletteType.Shield) break;
                }
                RouletteManager.Inst.Spin(true, spinVal);
                break;
            case "그루밍":
            case "그루밍+":
                TurnManager.Inst.GetShield(false, GetBuffedVal(item.cardValues[0], ECardValueType.Shield), EDamageSource.Card);
                BuffManager.Inst.AddShowBuff("보호", EBuffAffectType.Player, GetBuffedVal(item.cardValues[1], ECardValueType.Special), false);
                break;
            case "간식 시간":
            case "간식 시간+":
                TurnManager.Inst.IncreaseCost(GetBuffedVal(item.cardValues[0], ECardValueType.Special));
                break;
        }
        if (isCardUsed)
        {
            Debug.Log(item.name + " 카드 사용!");
            TurnManager.Inst.IncreaseCost(-buffedCost);
            Utils.AllignActions(ref TurnManager.OnUseCard, typeof(ShowBuff), typeof(RelicManager));
            TurnManager.OnUseCard?.Invoke(this, enemyIdx);
            PassiveManager.Inst.lastCardType = item.type;
        }
        return isCardUsed;
    }

    private void Start()
    {
        tooltipCanvas = GameObject.FindGameObjectWithTag("MainCanvas").GetComponent<Canvas>();
    }

    private void Update()
    {
        if (currType == SceneType.General) ShowBuffedCost();
        ShowBuffedVal();

        highlight.enabled = false;
        for(int i = 0; i <= EnemyManager.Inst.subEnemies.Length; i++)
        {
            if(i > 0 && (EnemyManager.Inst.subEnemies[i - 1] == null || EnemyManager.Inst.subEnemies[i - 1].name == null)) continue;
            if (IsCardUseable(i))
            {
                highlight.enabled = true;
                break;
            }
        }

        specialHighlight.enabled = false;
        if(highlight.enabled)
        {
            for(int i = 0; i <= EnemyManager.Inst.subEnemies.Length; i++)
            {
                if(i > 0 && (EnemyManager.Inst.subEnemies[i - 1] == null || EnemyManager.Inst.subEnemies[i - 1].name == null)) continue;
                if (IsCardSpecialEffect(i))
                {
                    highlight.enabled = false;
                    specialHighlight.enabled = true;
                    break;
                }
            }
        }

        if (tooltipCreated)
        {
            Vector3 offset = Vector3.zero;
            for(int i = 0; i < activeTooltips.Count; i++)
            {
                Vector3 screenPoint = Camera.main.WorldToScreenPoint(tooltipPos.position) - offset;
                activeTooltips[i].transform.position = screenPoint;
                offset.y += activeTooltips[i].GetComponent<RectTransform>().rect.height + 10;
            }
        }
    }

    private void OnDestroy()
    {
        if(activeTooltips != null && activeTooltips.Count > 0)
        {
            for(int i = 0; i < activeTooltips.Count; i++)
            {
                Destroy(activeTooltips[i]);
            }
            activeTooltips.Clear();
        }
    }
}
