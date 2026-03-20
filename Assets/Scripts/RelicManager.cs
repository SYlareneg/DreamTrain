using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using DG.Tweening;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Inst { get; private set; }
    private void Awake() => Inst = this;

    public RelicSO relicSO;
    public GameObject relicUIPrefab;
    public List<RelicItem_Enhanceable> relicList;
    public List<RelicItem> relicActivationList;
    [SerializeField][Tooltip("이드 발동 효과 표시 위치")] RectTransform relicActivateEffectPos;
    [SerializeField][Tooltip("이드 발동 효과 표시 시간")] float relicActivateEffectTime = 1.5f;

    public List<RelicUI> RelicItemListToRelicUIList(List<RelicItem_Enhanceable> rItemList, Transform attachUI)
    {
        List<RelicUI> rUIList = new List<RelicUI>();
        List<RelicItem_Enhanceable> sortedRelicList = rItemList.OrderBy(x => x.relicOwner).ToList();

        for(int i = 0; i < Mathf.Min(sortedRelicList.Count, 5); i++)
        {
            var relicObject = Instantiate(relicUIPrefab, Vector3.zero, Utils.QI);
            relicObject.transform.SetParent(attachUI);
            var relic = relicObject.GetComponent<RelicUI>();

            if (i < sortedRelicList.Count - 1 && sortedRelicList[i + 1].relicOwner == sortedRelicList[i].relicOwner)
            {
                var relic1 = sortedRelicList[i].isEnhanced ? sortedRelicList[i].enhancedRelicItem : sortedRelicList[i];
                var relic2 = sortedRelicList[i].isEnhanced ? sortedRelicList[i + 1].enhancedRelicItem : sortedRelicList[i + 1];
                relic.Setup(relic1, relic2);
                rUIList.Add(relic);
                i++;
            }
            else
            {
                var relic1 = sortedRelicList[i].isEnhanced ? sortedRelicList[i].enhancedRelicItem : sortedRelicList[i];
                relic.Setup(relic1, null);
                rUIList.Add(relic);
            }
        }
        return rUIList;
    }
    public void InitRelicList()
    {
        relicList.Clear();
        foreach (RelicItem_Enhanceable rItem in relicSO.relicItems)
        {
            relicList.Add(rItem);
        }
        if (GameManager.Inst != null)
        {
            GameManager.Inst.RelicList();
        }
    }

    Sequence relicActivateEffectSeq;
    public void RelicActivateEffect()
    {
        // if(relicActivateEffectPos.gameObject.activeSelf == false && relicActivationList.Count > 0)
        // {
        //     relicActivateEffectPos.gameObject.SetActive(true);
        //     for(int i = relicActivationList.Count - 1; i >= 0; i--)
        //     {
        //         var relicUIObj = Instantiate(relicUIPrefab, relicActivateEffectPos.transform);
        //         var relicUI = relicUIObj.GetComponent<RelicUI>();
        //         relicUI.Setup(relicActivationList[i], null);
        //         relicActivationList.RemoveAt(i);
        //     }
        //     Sequence seq = DOTween.Sequence();
        //     seq.Append(DOTween.To(() => relicActivateEffectPos.pivot, x => relicActivateEffectPos.pivot = x, new Vector2(-1.1f, relicActivateEffectPos.pivot.y), relicActivateEffectTime * 0.33f).SetEase(Ease.InOutQuad));
        //     seq.AppendInterval(relicActivateEffectTime * 0.33f);
        //     seq.Append(DOTween.To(() => relicActivateEffectPos.pivot, x => relicActivateEffectPos.pivot = x, new Vector2(1f, relicActivateEffectPos.pivot.y), relicActivateEffectTime * 0.33f).SetEase(Ease.InOutQuad));
        //     seq.OnComplete(() =>
        //     {
        //         foreach (Transform child in relicActivateEffectPos)
        //         {
        //             Destroy(child.gameObject);
        //         }
        //         relicActivateEffectPos.gameObject.SetActive(false);
        //     });
        //     seq.Play();
        // }
        if(relicActivateEffectSeq != null && relicActivateEffectSeq.IsPlaying()) return;
        relicActivateEffectSeq = DOTween.Sequence();
        List<RelicUI> relicUIs = GameManager.Inst.relicListScroll.GetComponentsInChildren<RelicUI>().ToList();
        for(int i = relicActivationList.Count - 1; i >= 0; i--)
        {
            RelicItem relicItem = relicActivationList[i];
            RelicUI relicUI = relicUIs.Find(x => (x.relicItem1 != null && x.relicItem1.relicName == relicItem.relicName) || (x.relicItem2 != null && x.relicItem2.relicName == relicItem.relicName));
            if(relicUI != null)
            {
                relicActivationList.Remove(relicItem);
                Image[] images = relicUI.GetComponentsInChildren<Image>();
                foreach(Image img in images)
                {
                    img.color = Color.white;
                }
                relicUI.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, -15f));
                Sequence seq = DOTween.Sequence();
                seq.Append(relicUI.transform.DOLocalRotate(new Vector3(0, 0, 30f), relicActivateEffectTime * 0.166f).SetEase(Ease.InOutSine).SetLoops(6, LoopType.Yoyo));
                seq.AppendCallback(() =>
                {
                    relicUI.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
                    // foreach(Image img in images)
                    // {
                    //     img.color = Color.gray;
                    // }
                });
                relicActivateEffectSeq.Join(seq);
            }
        }
        relicActivateEffectSeq.AppendCallback(() => relicActivateEffectSeq = null);
        relicActivateEffectSeq.Play();
    }
    
    public void ActivateRelic(RelicItem relicItem)
    {
        // 특수 이드
        switch(relicItem.relicName)
        {
            case "순진무구":
            case "순진무구+":
                float damageMul = 1.5f;
                if(relicItem.relicName == "순진무구+") damageMul = 2f;
                bool cardUsed = false;
                TurnManager.OnPlayerTurnStart += () =>
                {
                    cardUsed = false;
                    BuffManager.AddBuffToTarget(BuffManager.Inst.playerBuff_Damage_Type[(int)EDamageSource.Roulette], 0, damageMul, -1);
                    BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Damage_Type[0, (int)EDamageSource.Roulette], 0, damageMul, -1);
                };
                TurnManager.OnUseCard += (x, enemyIdx) =>
                {
                    if(cardUsed == false)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.playerBuff_Damage_Type[(int)EDamageSource.Roulette], 0, 1f / damageMul, -1);
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Damage_Type[0, (int)EDamageSource.Roulette], 0, 1f / damageMul, -1);
                        cardUsed = true;
                    }
                };
                TurnManager.OnPlayerTurnEnd += () =>
                {
                    if(cardUsed == false)
                    {
                        BuffManager.AddBuffToTarget(BuffManager.Inst.playerBuff_Damage_Type[(int)EDamageSource.Roulette], 0, 1f / damageMul, -1);
                        BuffManager.AddBuffToTarget(BuffManager.Inst.enemyBuff_Damage_Type[0, (int)EDamageSource.Roulette], 0, 1f / damageMul, -1);
                        cardUsed = true;
                    }
                };
                TurnManager.OnPlayerDamaged += (x, s) =>
                {
                    if (cardUsed == false && s == EDamageSource.Roulette)
                    {
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                TurnManager.OnEnemyDamaged += (x, s, i) =>
                {
                    if (cardUsed == false && s == EDamageSource.Roulette)
                    {
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                break;
            case "박쥐 날개":
            case "박쥐 날개+":
                int shieldMul = relicItem.relicVal[0];
                TurnManager.OnPlayerTurnEnd += () =>
                {
                    if (TurnManager.Inst.nowCost != 0)
                    {
                        TurnManager.Inst.GetShield(false, TurnManager.Inst.nowCost * shieldMul, EDamageSource.Relic);
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                return;
            case "저주 인형":
            case "저주 인형+":
                Action<int, EDamageSource> curseDollActivate = null;
                curseDollActivate = (x, s) =>
                {
                    TurnManager.Inst.shieldHealth += x;
                    relicItem.relicVal[0] -= x;
                    if(relicItem.relicVal[0] <= 0)
                    {
                        relicItem.relicVal[0] = 0;
                        RelicItem_Enhanceable curseDollRelic = relicSO.relicItems.Find(r => r.relicName == relicItem.relicName || r.enhancedRelicItem.relicName == relicItem.relicName);
                        relicSO.relicItems.Remove(curseDollRelic);
                    }
                    TurnManager.OnPlayerDamaged -= curseDollActivate;
                    Debug.Log("Relic Activate: " + relicItem.relicName);
                    if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                };
                TurnManager.OnPlayerDamaged += curseDollActivate;
                return;
            case "꿀":
            case "꿀+":
                float healMul = (float)relicItem.relicVal[0] / 100f;
                TurnManager.BeforePlayerTurnStart += () =>
                {
                    int healVal = (int)(TurnManager.Inst.shieldHealth * healMul);
                    TurnManager.Inst.TakeDmg(-healVal, EDamageSource.Relic);
                    Debug.Log("Relic Activate: " + relicItem.relicName);
                    if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                };
                return;
            case "영원한 웃음":
            case "영원한 웃음+":
                if (DataManager.Inst.playerStatsSO != null)
                {
                    DataManager.Inst.playerStatsSO.wisdom = 1;
                }
                TurnManager.OnGameStart += () =>
                {
                    BuffManager.Inst.AddShowBuff("보호", EBuffAffectType.Player, relicItem.relicVal[1], false);
                    Debug.Log("Relic Activate: " + relicItem.relicName);
                    int currentWisdom = DataManager.Inst.playerStatsSO.GetStat(StatType.Wisdom);
                    if(currentWisdom != 1) DataManager.Inst.playerStatsSO.wisdom = 1;

                    if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                };
                return;
            case "붉은 꽃잎":
            case "붉은 꽃잎+":
                int threshold = (int)(TurnManager.Inst.maxHealth * (float)relicItem.relicVal[0] / 100f);
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if (TurnManager.Inst.curHealth <= threshold)
                    {
                        TurnManager.Inst.IncreaseCost(1);
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                return;
            case "와인의 눈물":
            case "와인의 눈물+":
                threshold = (int)(TurnManager.Inst.maxHealth * (float)relicItem.relicVal[0] / 100f);
                TurnManager.OnGameStart += () =>
                {
                    if (TurnManager.Inst.curHealth <= threshold)
                    {
                        BuffManager.Inst.AddShowBuff("활력", EBuffAffectType.Player, relicItem.relicVal[1], false);
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                TurnManager.OnPlayerHealthChange += (x) =>
                {
                    if (TurnManager.Inst.curHealth <= threshold && TurnManager.Inst.curHealth + x > threshold)
                    {
                        BuffManager.Inst.AddShowBuff("활력", EBuffAffectType.Player, relicItem.relicVal[1], false);
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                    else if (TurnManager.Inst.curHealth + x <= threshold && TurnManager.Inst.curHealth > threshold)
                    {
                        BuffManager.Inst.AddShowBuff("활력", EBuffAffectType.Player, -relicItem.relicVal[1], false);
                    }
                };
                return;
            case "찻잔":
            case "찻잔+":
                TurnManager.OnPlayerTurnStart += () =>
                {
                    TurnManager.Inst.TakeDmg(-relicItem.relicVal[0], EDamageSource.Relic);
                    Debug.Log("Relic Activate: " + relicItem.relicName);
                    if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                };
                return;
            case "잃어버린 노래":
            case "잃어버린 노래+":
                TurnManager.OnVanishCard += () =>
                {
                    TurnManager.Inst.IncreaseCost(relicItem.relicVal[0]);
                    Debug.Log("Relic Activate: " + relicItem.relicName);
                    if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                };
                return;
            case "모두의 우승":
            case "모두의 우승+":
                TurnManager.OnGameStart += () =>
                {
                    BuffManager.Inst.AddShowBuff("강화", EBuffAffectType.Roulette, relicItem.relicVal[0], false);
                    BuffManager.Inst.AddShowBuff("강화", EBuffAffectType.Enemy, relicItem.relicVal[0], false);
                    Debug.Log("Relic Activate: " + relicItem.relicName);
                    if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                };
                return;
            case "낙하산":
            case "낙하산+":
                bool shieldedThisGame = false;
                TurnManager.OnGameStart += () =>
                {
                    BuffManager.Inst.AddShowBuff("보호", EBuffAffectType.Player, relicItem.relicVal[0], false);
                    Debug.Log("Relic Activate: " + relicItem.relicName);
                    if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                };
                TurnManager.OnPlayerDamaged += (x, s) =>
                {
                    if (shieldedThisGame == false && x > TurnManager.Inst.shieldHealth)
                    {
                        BuffManager.Inst.AddShowBuff("보호", EBuffAffectType.Player, -relicItem.relicVal[0], false);
                        shieldedThisGame = true;
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                return;
            case "자가수리 키트":
            case "자가수리 키트+":
                TurnManager.OnPlayerHealed += (damage, s) =>
                {
                    if(s == EDamageSource.Roulette)
                    {
                        damage = (int)(damage * ((float)relicItem.relicVal[0] / 100f));
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                        if (TurnManager.Inst.curHealth + damage > TurnManager.Inst.maxHealth)
                        {
                            damage = TurnManager.Inst.maxHealth - TurnManager.Inst.curHealth;
                        }
                        
                        TurnManager.Inst.curHealth += damage;
                    }
                };
                return;
            case "사건의 지평선":
            case "사건의 지평선+":
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if(TurnManager.Inst.turnNum == relicItem.relicVal[0])
                    {
                        BuffManager.Inst.AddShowBuff("강화", EBuffAffectType.Roulette, relicItem.relicVal[1], false);
                        BuffManager.Inst.AddShowBuff("보호", EBuffAffectType.Roulette, relicItem.relicVal[1], false);
                        BuffManager.Inst.AddShowBuff("활력", EBuffAffectType.Roulette, relicItem.relicVal[1], false);
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                return;
            case "깃펜":
            case "깃펜+":
                TurnManager.OnUseCard += (card, enemyIdx) =>
                {
                    if(Random.value <= (float)relicItem.relicVal[0] / 100f)
                    {
                        StartCoroutine(TurnManager.Inst.Draw(relicItem.relicVal[1], null));
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                return;
            case "일기":
            case "일기+":
                TurnManager.OnGameEnd += (isWin) =>
                {
                    if(Random.value <= (float)relicItem.relicVal[0] / 100f)
                    {
                        int randIdx = Random.Range(0, 3);
                        switch (randIdx)
                        {
                            case 0:
                                DataManager.Inst.playerStatsSO.courage += relicItem.relicVal[1];
                                break;
                            case 1:
                                DataManager.Inst.playerStatsSO.wisdom += relicItem.relicVal[1];
                                break;
                            case 2:
                                DataManager.Inst.playerStatsSO.luck += relicItem.relicVal[1];
                                break;
                        }
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                return;
            case "낡은 장갑":
            case "낡은 장갑+":
                TurnManager.OnRouletteActivate += () =>
                {
                    TurnManager.Inst.GetShield(false, relicItem.relicVal[0], EDamageSource.Relic);
                    Debug.Log("Relic Activate: " + relicItem.relicName);
                    if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                };
                return;
            case "빗자루":
            case "빗자루+":
                TurnManager.OnRouletteErase += (index) =>
                {
                    if(relicItem.relicVal[0] <= 0) return;
                    relicItem.relicVal[0]--;
                    TurnManager.Inst.IncreaseCost(relicItem.relicVal[1]);
                    Debug.Log("Relic Activate: " + relicItem.relicName);
                    if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                };
                TurnManager.OnPlayerTurnEnd += () =>
                {
                    relicItem.relicVal[0] = 1;
                };
                TurnManager.OnGameEnd += (isWin) =>
                {
                    relicItem.relicVal[0] = 1;
                };
                return;
            case "파랑 구두":
            case "파랑 구두+":
                TurnManager.OnPlayerShielded += (x, s) =>
                {
                    BuffManager.Inst.AddShowBuff("강화", EBuffAffectType.Roulette, relicItem.relicVal[0], false);
                    Debug.Log("Relic Activate: " + relicItem.relicName);
                    if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                };
                return;
            case "머리끈":
            case "머리끈+":
                TurnManager.OnPlayerTurnStart += () =>
                {
                    if(TurnManager.Inst.turnNum == 1)
                    {
                        TurnManager.Inst.IncreaseCost(relicItem.relicVal[0]);
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                return;
            case "새장":
            case "새장+":
                TurnManager.OnGameStart += () =>
                {
                    BuffManager.Inst.AddShowBuff("보호", EBuffAffectType.Player, relicItem.relicVal[0], false);
                    Debug.Log("Relic Activate: " + relicItem.relicName);
                    if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                };
                TurnManager.OnRouletteSpin += (isClockwise, spinCount) =>
                {
                    if(spinCount >= relicItem.relicVal[1])
                    {
                        BuffManager.Inst.AddShowBuff("보호", EBuffAffectType.Player, -relicItem.relicVal[2], false);
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                return;
            case "모닥불":
            case "모닥불+":
                TurnManager.OnRouletteSpin += (isClockwise, spinCount) =>
                {
                    if(spinCount >= relicItem.relicVal[0])
                    {
                        TurnManager.Inst.EnemyTakeDmg(relicItem.relicVal[1], EDamageSource.Relic);
                        Debug.Log("Relic Activate: " + relicItem.relicName);
                        if(relicActivationList.Find(x => x == relicItem) == null) relicActivationList.Add(relicItem);
                    }
                };
                return;
        }
    }

    public void ActivateRelics()
    {
        InitRelicList();
        for (int i = 0; i < relicList.Count; i++)
        {
            if (relicList[i].isEnhanced) ActivateRelic(relicList[i].enhancedRelicItem);
            else ActivateRelic(relicList[i]);
        }
    }

    private void LateUpdate()
    {
        RelicActivateEffect();
    }

    private void OnDestroy()
    {
        TurnManager.OnPlayerTurnStart = null;
        TurnManager.OnPlayerTurnEnd = null;
        TurnManager.OnEnemyTurnStart = null;
        TurnManager.OnEnemyTurnEnd = null;
        TurnManager.OnGameStart = null;
        TurnManager.OnGameEnd = null;
        TurnManager.OnUseCard = null;
        TurnManager.OnAddCard = null;
        TurnManager.OnDiscardCard = null;
        TurnManager.OnPlayerDamaged = null;
        TurnManager.OnPlayerHealed = null;
        TurnManager.OnPlayerShielded = null;
        TurnManager.OnPlayerTrigger = null;
        TurnManager.OnPlayerTriggerIncrease = null;
        TurnManager.OnPlayerTriggerDecrease = null;
        TurnManager.OnEnemyDamaged = null;
        TurnManager.OnEnemyHealed = null;
        TurnManager.OnEnemyShielded = null;
        TurnManager.OnEnemyTrigger = null;
        TurnManager.OnEnemyTriggerIncrease = null;
        TurnManager.OnEnemyTriggerDecrease = null;
        TurnManager.OnEnemyAction = null;
        TurnManager.OnRouletteSpin = null;
        TurnManager.OnRouletteTrigger = null;
        TurnManager.OnRouletteEnchant = null;
        TurnManager.OnRouletteActivate = null;
    }
}

