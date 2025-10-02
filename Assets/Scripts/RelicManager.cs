using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class RelicManager : MonoBehaviour
{
    public static RelicManager Inst { get; private set; }
    private void Awake() => Inst = this;

    public RelicSO relicSO;
    public GameObject relicUIPrefab;
    public List<RelicItem> relicList;

    public List<RelicUI> RelicItemListToRelicUIList(List<RelicItem> rItemList, Transform attachUI)
    {
        List<RelicUI> rUIList = new List<RelicUI>();
        List<RelicItem> sortedRelicList = rItemList.OrderBy(x => x.relicName).ToList();

        foreach (RelicItem rItem in sortedRelicList)
        {
            var relicObject = Instantiate(relicUIPrefab, Vector3.zero, Utils.QI);
            relicObject.transform.SetParent(attachUI);
            var relic = relicObject.GetComponent<RelicUI>();

            relic.Setup(rItem);
            rUIList.Add(relic);
        }
        
        return rUIList;
    }
    public void InitRelicList()
    {
        foreach (RelicItem rItem in relicSO.relicItems)
        {
            relicList.Add(rItem);
        }
    }

    public void ActivateRelics()
    {
        InitRelicList();
        for (int i = 0; i < relicList.Count; i++)
        {
            Action relicAction = null;
            int localIndex = i;
            switch (relicList[localIndex].affectItem)
            {
                case ERelicAffectItem.Health:
                    relicAction += () => TurnManager.Inst.TakeDmg(-relicList[localIndex].affectValue);
                    break;
                case ERelicAffectItem.Cost:
                    relicAction += () => TurnManager.Inst.nowCost += relicList[localIndex].affectValue;
                    break;
                case ERelicAffectItem.Draw:
                    relicAction += () => TurnManager.Inst.turnDraw = TurnManager.Inst.drawCardCount + relicList[localIndex].affectValue;
                    break;
            }
            switch (relicList[localIndex].type)
            {
                case ERelicActivateType.TurnBegin:
                    TurnManager.OnTurnStart += relicAction; break;
                case ERelicActivateType.TurnEnd:
                    TurnManager.OnTurnEnd += relicAction; break;
            }
        }
    }
}
