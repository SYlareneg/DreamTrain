using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckBuildManager : MonoBehaviour
{
    public static DeckBuildManager Inst { get; private set; }
    private void Awake() => Inst = this;

    public Canvas canvas;
    public GameObject backgroundPanel;

    [SerializeField] GameObject relicListScroll;
    public List<RelicUI> relicList;

    [SerializeField] GameObject deckListScroll;
    public List<CardUI_DeckBuild> deckList;
    public static int deckCardNum = 12;
    [SerializeField] GameObject cardUIPrefab;
    public ItemSO itemSO;
    [SerializeField] TMP_Text cardListTitleTMP;
    [SerializeField] GameObject cardListScroll;
    public List<CardUI_Draggable> availableCardList;
    [SerializeField] GameObject draggableCardUIPrefab;
    public GameObject draggingCardUI;

    public Passive selectedPersona;
    [SerializeField] GameObject personaShow;
    [SerializeField] Image personaButton;
    [SerializeField] TMP_Text personaName;
    [SerializeField] TMP_Text personaText;
    public Passive selectedShadow;
    [SerializeField] GameObject shadowShow;
    [SerializeField] Image shadowButton;
    [SerializeField] TMP_Text shadowName;
    [SerializeField] TMP_Text shadowText;

    [SerializeField] GameObject passiveScrollView;
    [SerializeField] GameObject passiveListScroll;
    public List<PassiveUI> passiveList;
    [SerializeField] GameObject passivePrefab;
    public PassiveSO passiveSO;

    [SerializeField] public Image tooltip;
    public TMP_Text tooltipTxt;

    public bool isLoading;

    public void RelicList()
    {
        RelicManager.Inst.InitRelicList();
        foreach (RelicUI relic in relicList)
        {
            Destroy(relic.gameObject);
        }

        relicList = RelicManager.Inst.RelicItemListToRelicUIList(RelicManager.Inst.relicList, relicListScroll.transform);
        Canvas.ForceUpdateCanvases();
    }

    public void DeckListInit()
    {
        CardManager.Inst.InitializeItemBuffer();
        foreach (CardUI_DeckBuild deckCard in deckList)
        {
            Destroy(deckCard.gameObject);
        }
        deckList.Clear();

        for (int i = 0; i < deckCardNum; i++)
        {
            var cardObject = Instantiate(cardUIPrefab, deckListScroll.transform.position, Utils.QI);
            cardObject.transform.SetParent(deckListScroll.transform);
            var card = cardObject.GetComponent<CardUI_DeckBuild>();

            card.Setup(null);
            card.raycaster = canvas.GetComponent<GraphicRaycaster>();
            card.availableNum = 0;
            deckList.Add(card);
        }
    }

    public void setCardNum(Item item, int newNum)
    {
        int itemIdx = Array.FindIndex(itemSO.items, x => x.name == item.name);
        if(itemIdx != -1)
        {
            itemSO.items[itemIdx].num = newNum;
        }
    }

    public void changeCardNum(Item item, int changeNum)
    {
        int itemIdx = Array.FindIndex(itemSO.items, x => x.name == item.name);
        if(itemIdx != -1)
        {
            itemSO.items[itemIdx].num += changeNum;
        }
    }

    public void CardListSet(Passive newP)
    {
        foreach(CardUI_Draggable card in availableCardList)
        {
            if(card != null)
            {
                setCardNum(card.item, card.availableNum);
                Destroy(card.gameObject);
            }
        }
        availableCardList.Clear();

        foreach(Item item in itemSO.items)
        {
            if(newP != null && (newP.name == passiveSO.passives[item.passiveNum].name))
            {
                var cardObject = Instantiate(draggableCardUIPrefab, cardListScroll.transform.position, Utils.QI);
                cardObject.transform.SetParent(cardListScroll.transform);
                var card = cardObject.GetComponent<CardUI_Draggable>();

                card.Setup(item);
                card.raycaster = canvas.GetComponent<GraphicRaycaster>();
                card.availableNum = item.num;
                availableCardList.Add(card);
            }
        }

        if(newP.name == "")
        {
            cardListTitleTMP.text = "Available Cards";
        }
        else
        {
            cardListTitleTMP.text = "Available Cards for " + newP.name;
        }
    }

    public void PassiveList(EPassiveType pType)
    {
        foreach (Passive p in passiveSO.passives)
            {
                if (p.type == pType)
                {
                    var pObject = Instantiate(passivePrefab, passiveListScroll.transform.position, Utils.QI);
                    pObject.transform.SetParent(passiveListScroll.transform);

                    var passive = pObject.GetComponent<PassiveUI>();
                    passive.Setup(p);
                    if ((pType == EPassiveType.Persona && selectedPersona == p) || (pType == EPassiveType.Shadow && selectedShadow == p))
                    {
                        passive.Select(true);
                    }
                    passiveList.Add(passive);
                }
            }
    }

    public void SelectPassive(Passive p)
    {
        foreach (PassiveUI pUI in passiveList)
        {
            if (pUI.passive == p)
            {
                pUI.Select(true);
            }
            else
            {
                pUI.Select(false);
            }
        }

        CardListSet(p);
        
        if (p.type == EPassiveType.Persona)
        {
            selectedPersona = p;
            personaButton.sprite = p.sprite;
            personaName.text = p.name;
            personaText.text = p.text;
        }
        else if (p.type == EPassiveType.Shadow)
        {
            selectedShadow = p;
            shadowButton.sprite = p.sprite;
            shadowName.text = p.name;
            shadowText.text = p.text;
        }
        
        foreach (CardUI_DeckBuild deckCard in deckList)
        {
            if(deckCard.item != null)
            {
                if(selectedPersona != null && (selectedPersona.name == passiveSO.passives[deckCard.item.passiveNum].name))
                {
                    continue;
                }
                else if(selectedShadow != null && (selectedShadow.name == passiveSO.passives[deckCard.item.passiveNum].name))
                {
                    continue;
                }
                else
                {
                    changeCardNum(deckCard.item, deckCard.availableNum);
                    deckCard.Setup(null);
                    deckCard.availableNum = 0;
                }
            }
        }
    }

    public void ShowPersona()
    {
        if (passiveScrollView.activeSelf == true)
        {
            passiveScrollView.SetActive(false);
            foreach (PassiveUI pUI in passiveList)
            {
                if (pUI)
                {
                    Destroy(pUI.gameObject);
                }
            }
            passiveList.Clear();
        }
        else
        {
            PassiveList(EPassiveType.Persona);
            if(selectedPersona != null)
            {
                SelectPassive(selectedPersona);
            }
            passiveScrollView.GetComponent<Image>().color = personaShow.GetComponent<Image>().color;
            passiveScrollView.SetActive(true);
        }
    }

    public void ShowShadow()
    {
        if (passiveScrollView.activeSelf == true)
        {
            var temp = shadowShow.transform.position;
            shadowShow.transform.position = personaShow.transform.position;
            personaShow.transform.position = temp;
            passiveScrollView.SetActive(false);
            foreach (PassiveUI pUI in passiveList)
            {
                if (pUI)
                {
                    Destroy(pUI.gameObject);
                }
            }
            passiveList.Clear();
        }
        else
        {
            var temp = shadowShow.transform.position;
            shadowShow.transform.position = personaShow.transform.position;
            personaShow.transform.position = temp;
            PassiveList(EPassiveType.Shadow);
            if(selectedShadow != null)
            {
                SelectPassive(selectedShadow);
            }
            passiveScrollView.GetComponent<Image>().color = shadowShow.GetComponent<Image>().color;
            passiveScrollView.SetActive(true);
        }
    }

    void Start()
    {
        isLoading = false;
        RelicList();
        DeckListInit();
    }
}
