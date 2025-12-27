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
    [SerializeField] SpriteRenderer type;
    [SerializeField] SpriteRenderer rarity;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text costTMP;
    [SerializeField] TMP_Text typeTMP;
    public TMP_Text textTMP;
    [SerializeField] Sprite[] cardTypes;
    [SerializeField] Sprite[] rarityTypes;

    public Item item;
    public PRS originPRS;
    
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
        rarity.sprite = rarityTypes[(int)this.item.rarity];
        if(rarity.sprite == null) rarity.enabled = false;
        else rarity.enabled = true;

        character.sprite = Utils.LoadSpriteByName("Cards", this.item.sprite);

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
        if (currType != SceneType.General) return;
        int buffedCost = BuffManager.Inst.GetBuffedCardCost(this.item);
        costTMP.text = buffedCost.ToString();

        if (buffedCost > this.item.cost)
        {
            costTMP.color = Color.red;
        }
        else if (buffedCost == this.item.cost)
        {
            costTMP.color = Color.white;
        }
        else
        {
            costTMP.color = Color.green;
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
            else returnString = "<color=white>" + buffedVal.ToString() + "</color>";
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
            case "회전 카드 1":
                RouletteManager.Inst.Spin(true, 1);
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
                isCardUsed = RouletteManager.Inst.EnchantRoulette(false, new RouletteType(ERouletteType.Heal), 3);
                break;
            case "흡혈 부여":
            case "흡혈 부여+":
                RouletteType bloodSteal = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                RouletteType bloodSteal_plus = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                if(item.name == "흡혈 부여")
                {
                    isCardUsed = RouletteManager.Inst.EnchantRoulette(true, bloodSteal, PassiveManager.Inst.playerSpecialRoulettes[PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0)].baseVal, enemyIdx);
                }
                else
                {
                    isCardUsed = RouletteManager.Inst.EnchantRoulette(true, bloodSteal_plus, PassiveManager.Inst.playerSpecialRoulettes[PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1)].baseVal, enemyIdx);
                    if(isCardUsed) RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].isEnhanced = true;
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
                for(int i = 0; i <= EnemyManager.Inst.subEnemies.Length; i++)
                {
                    BuffManager.Inst.AddShowBuff("주저함", EBuffAffectType.Enemy, item.cardValues[0], false, null, i);
                }
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
                    for(int j = 0; j <= EnemyManager.Inst.subEnemies.Length; j++)
                    {
                        int tempIdx = (RouletteManager.Inst.EnemyIdxSpinOffset(j) + i) % RouletteManager.rouletteNum;
                        if (RouletteManager.Inst.roulettePieces[tempIdx].roulette.rtype == new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0))
                        || RouletteManager.Inst.roulettePieces[tempIdx].roulette.rtype == new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1)))
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
                int boxVal = 12;
                if(item.name == "마술 상자+") boxVal = 15;
                isCardUsed = RouletteManager.Inst.EnchantRoulette(false, magicBox, boxVal);
                if (item.name == "마술 상자+" && isCardUsed)
                {
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].tooltip.tooltipTitle = "마술 상자+";
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].isEnhanced = true;
                }
                break;
            case "마술-비둘기":
            case "마술-비둘기+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                bool checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].roulette.rtype == magicBox;
                TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card, enemyIdx);
                if (checkMagic)
                {
                    if(enemyIdx == 0) EnemyManager.Inst.RemoveAction(0);
                    else EnemyManager.Inst.RemoveSubEnemyAction(enemyIdx - 1, 0);
                }
                break;
            case "마술-카드":
            case "마술-카드+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == magicBox;
                RouletteManager.Inst.Spin(true, item.cardValues[0]);
                if (checkMagic)
                {
                    Debug.Log("duplicate mode on");
                    CardManager.Inst.CardSelectModeTransit(ECardSelectMode.Duplicate, item.cardValues[1]);
                }
                break;
            case "마술-절단":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].roulette.rtype == magicBox;
                TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card, enemyIdx);
                if (checkMagic)
                {
                    TurnManager.Inst.GetShield(true, -TurnManager.Inst.enemyShieldHealth[enemyIdx], EDamageSource.Card);
                }
                break;
            case "마술-절단+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].roulette.rtype == magicBox;
                if (checkMagic)
                {
                    TurnManager.Inst.GetShield(true, -TurnManager.Inst.enemyShieldHealth[enemyIdx], EDamageSource.Card);
                }
                TurnManager.Inst.EnemyTakeDmg(GetBuffedVal(item.cardValues[0], ECardValueType.Damage), EDamageSource.Card, enemyIdx);
                break;
            case "마술-순간이동":
            case "마술-순간이동+":
                magicBox = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                checkMagic = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == magicBox;
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
                TurnManager.Inst.TriggerPlayerPassive(1);
                break;
            case "꽁꽁 얼리기":
            case "꽁꽁 얼리기+":
                RouletteType frozen = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                isCardUsed = RouletteManager.Inst.EnchantRoulette(false, frozen, item.cardValues[0]);
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
                StartCoroutine(TurnManager.Inst.Draw(item.cardValues[0], null));
                TurnManager.Inst.TriggerPlayerPassive(-item.cardValues[1]);
                break;
            case "데굴데굴":
            case "데굴데굴+":
                RouletteManager.Inst.Spin(true, item.cardValues[0]);
                if (item.name == "데굴데굴+")
                {
                    for(int i = 0; i <= EnemyManager.Inst.subEnemies.Length; i++)
                    {
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
                            RouletteManager.Inst.Spin(true, item.cardValues[0]);
                            if (item.name == "데굴데굴+")
                            {
                                for(int i = 0; i <= EnemyManager.Inst.subEnemies.Length; i++)
                                {
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
                isCardUsed = RouletteManager.Inst.EnchantRoulette(true, frozen, item.cardValues[0], enemyIdx);
                if (isCardUsed)
                {
                    BuffManager.Inst.AddShowBuff("과민함", EBuffAffectType.Enemy, item.cardValues[1], false, null, enemyIdx);
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
                    if (RouletteManager.Inst.roulettePieces[tempIdx].roulette.rtype == new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0)))
                    {
                        TurnManager.Inst.GetShield(false, GetBuffedVal(item.cardValues[1], ECardValueType.Shield), EDamageSource.Card);
                    }
                }
                RouletteManager.Inst.Spin(true, item.cardValues[0]);
                break;
            case "얼음 깨기":
            case "얼음 깨기+":
                for(int i = 0; i <= EnemyManager.Inst.subEnemies.Length; i++)
                {
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
                TurnManager.Inst.EnemyTakeDmg(frozenCnt * item.cardValues[0], EDamageSource.Card, enemyIdx);
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
                int frozenTimeInc = item.cardValues[0];
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
                int rltVal = 9;
                if(item.name == "발톱 세우기+") rltVal = 12;
                isCardUsed = RouletteManager.Inst.EnchantRoulette(true, claw, rltVal, enemyIdx);
                if(item.name == "발톱 세우기+" && isCardUsed)
                {
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].tooltip.tooltipTitle = "발톱+";
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(enemyIdx)].isEnhanced = true;
                }
                break;
            case "알뜰한 사냥꾼":
            case "알뜰한 사냥꾼+":
                claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                int incCost = 0;
                for(int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if(RouletteManager.Inst.roulettePieces[i].roulette.rtype == claw && RouletteManager.Inst.roulettePieces[i].roulette.value <= item.cardValues[0])
                    {
                        RouletteManager.Inst.roulettePieces[i].RouletteClear();
                        incCost += item.cardValues[1];
                    }
                }
                TurnManager.Inst.IncreaseCost(incCost);
                break;
            case "마구 할퀴기":
            case "마구 할퀴기+":
                claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                for(int i = 0; i < item.cardValues[0]; i++)
                {
                    for(int j = 0; j <= EnemyManager.Inst.subEnemies.Length; j++)
                    {
                        if(RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(j)].roulette.rtype == claw)
                        {
                            RoulettePiece enemyPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.EnemyIdxSpinOffset(j)];
                            enemyPiece.Activate(true, j);
                            enemyPiece.roulette.value -= 3;
                            if(enemyPiece.roulette.value <= 0)
                            {
                                enemyPiece.RouletteClear();
                            }
                        }
                    }
                    if(RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == claw)
                    {
                        RoulettePiece playerPiece = RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat];
                        playerPiece.Activate(false);
                        playerPiece.roulette.value -= 3;
                        if(playerPiece.roulette.value <= 0)
                        {
                            playerPiece.RouletteClear();
                        }
                    }
                }
                break;
            case "발톱 손질":
            case "발톱 손질+":
                claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                for(int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if(RouletteManager.Inst.roulettePieces[i].roulette.rtype == claw)
                    {
                        RouletteManager.Inst.roulettePieces[i].roulette.value += item.cardValues[0];
                    }
                }
                break;
            case "소심한 할퀴기":
            case "소심한 할퀴기+":
                claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                isCardUsed = RouletteManager.Inst.EnchantRoulette(true, claw, 3, enemyIdx);
                if(isCardUsed) TurnManager.Inst.EnemyTakeDmg(item.cardValues[0], EDamageSource.Card, enemyIdx);
                break;
            case "날카로운 발톱":
            case "날카로운 발톱+":
                claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                if(RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == claw)
                {
                    RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.value *=  item.cardValues[0];
                }
                else
                {
                    isCardUsed = false;
                }
                break;
            case "고양이의 시간":
            case "고양이의 시간+":
                claw = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 0));
                rltVal = 9;
                if(item.name == "고양이의 시간+") rltVal = 12;
                for(int i = 0; i < RouletteManager.rouletteNum; i++)
                {
                    if(RouletteManager.Inst.roulettePieces[i].roulette.rtype.type == ERouletteType.Attack)
                    {
                        RouletteManager.Inst.EnchantRoulettePiece(i, claw, rltVal);
                        if(item.name == "고양이의 시간+" && isCardUsed)
                        {
                            RouletteManager.Inst.roulettePieces[i].tooltip.tooltipTitle = "발톱+";
                            RouletteManager.Inst.roulettePieces[i].isEnhanced = true;
                        }
                    }
                }
                break;
            case "기민함":
            case "기민함+":
                BuffManager.Inst.AddShowBuff("환영", EBuffAffectType.Player, item.cardValues[0], false);
                break;
            case "실뭉치":
            case "실뭉치+":
                RouletteType furball = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                RouletteType furball_plus = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 2));
                if(item.name == "실뭉치") isCardUsed = RouletteManager.Inst.EnchantRoulette(false, furball, PassiveManager.Inst.playerSpecialRoulettes[PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1)].baseVal);
                else isCardUsed = RouletteManager.Inst.EnchantRoulette(false, furball_plus, PassiveManager.Inst.playerSpecialRoulettes[PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 2)].baseVal);
                break;
            case "고양이걸음":
            case "고양이걸음+":
                RouletteManager.Inst.Spin(true, item.cardValues[0]);
                StartCoroutine(TurnManager.Inst.Draw(item.cardValues[1], null));
                break;
            case "실 풀기":
            case "실 풀기+":
                furball = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                furball_plus = new RouletteType(ERouletteType.Player_Special, PassiveManager.GetSpecialRouletteIdx(TurnManager.Inst.characterSO.personaPiece.persona.dreamPieceNum == item.dreamPieceNum, 1));
                if(RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == furball || RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat].roulette.rtype == furball_plus)
                {
                    int tempVal = BuffManager.Inst.GetBuffedRouletteValue(RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat]);
                    tempVal = tempVal - tempVal / 2;
                    TurnManager.Inst.GetShield(false, tempVal, EDamageSource.Card);
                    BuffManager.AddBuffToTarget(BuffManager.Inst.roulettePieceBuff[RouletteManager.Inst.roulettePieces[RouletteManager.Inst.playerLookat]], -tempVal, 1, -1);
                }
                else
                {
                    isCardUsed = false;
                }
                break;
            case "놀이 시간":
            case "놀이 시간+":
                BuffManager.Inst.AddShowBuff("놀이 시간", EBuffAffectType.Player, item.cardValues[0], false);
                break;
            case "숨기":
            case "숨기+":
                int spinVal = 0;
                for (int i = 0; i < item.cardValues[0]; i++)
                {
                    spinVal++;
                    int tempIdx = (RouletteManager.Inst.playerLookat + RouletteManager.rouletteNum - spinVal) % RouletteManager.rouletteNum;
                    if (RouletteManager.Inst.roulettePieces[tempIdx].roulette.rtype.type == ERouletteType.Shield) break;
                }
                RouletteManager.Inst.Spin(true, spinVal);
                break;
            case "그루밍":
            case "그루밍+":
                TurnManager.Inst.GetShield(false, item.cardValues[0], EDamageSource.Card);
                BuffManager.Inst.AddShowBuff("보호", EBuffAffectType.Player, item.cardValues[1], false);
                break;
            case "간식 시간":
            case "간식 시간+":
                TurnManager.Inst.IncreaseCost(item.cardValues[0]);
                break;
        }
        if (isCardUsed)
        {
            Debug.Log(item.name + " 카드 사용!");
            TurnManager.Inst.IncreaseCost(-buffedCost);
            Utils.AllignActions(ref TurnManager.OnUseCard, typeof(ShowBuff), typeof(RelicManager));
            TurnManager.OnUseCard?.Invoke(this);
        }
        return isCardUsed;
    }

    private void Update()
    {
        if (currType == SceneType.General) ShowBuffedCost();
        ShowBuffedVal();
    }
}
