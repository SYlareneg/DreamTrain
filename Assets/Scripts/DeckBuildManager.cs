using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    public ItemSO normalItemSO;
    [SerializeField] TMP_Text cardListTitleTMP;
    [SerializeField] GameObject cardListScroll;
    public List<CardUI_Draggable> availableCardList;
    [SerializeField] GameObject draggableCardUIPrefab;
    public GameObject draggingCardUI;

    public DreamPiece selectedPersona;
    [SerializeField] GameObject personaShow;
    [SerializeField] Image personaButton;
    [SerializeField] TMP_Text personaName;
    [SerializeField] TMP_Text personaText;
    public DreamPiece selectedShadow;
    [SerializeField] GameObject shadowShow;
    [SerializeField] Image shadowButton;
    [SerializeField] TMP_Text shadowName;
    [SerializeField] TMP_Text shadowText;

    [SerializeField] GameObject passiveScrollView;
    [SerializeField] GameObject passiveListScroll;
    public List<PassiveUI> passiveList;
    [SerializeField] GameObject passivePrefab;
    public PassiveSO passiveSO;
    public DreamPieceSO dreamPieceSO;
    public CharacterSO characterSO;

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

        foreach (Item item in CardManager.Inst.playerDeckSO.items)
        {
            Item found = null;
            if (item.dreamPieceNum < 0)
            {
                found = normalItemSO.items.Find(x => x.name == item.name);
            }
            else
            {
                found = Array.Find(dreamPieceSO.dreamPieces[item.dreamPieceNum].cards, x => x.name == item.name);
            }
            if (found != null)
            {
                changeCardNum(found, item.num);
            }
        }
        CardManager.Inst.playerDeckSO.items.Clear();

        for (int i = 0; i < deckCardNum; i++)
        {
            var cardObject = Instantiate(cardUIPrefab, deckListScroll.transform.position, Utils.QI);
            cardObject.transform.SetParent(deckListScroll.transform);
            var card = cardObject.GetComponent<CardUI_DeckBuild>();

            card.Setup(null);
            card.raycaster = canvas.GetComponent<GraphicRaycaster>();
            deckList.Add(card);
        }
    }

    public void setCardNum(Item item, int newNum)
    {
        item.num = newNum;
    }

    public void changeCardNum(Item item, int changeNum)
    {
        item.num += changeNum;
    }

    public CardUI_Draggable FindInCardListByName(string name)
    {
        foreach (CardUI_Draggable card in availableCardList)
        {
            if (card.item.name == name)
            {
                return card;
            }
        }
        return null;
    }

    public void CardListSet(DreamPiece newDP, EPassiveType pType)
    {
        foreach (CardUI_Draggable card in availableCardList)
        {
            if (card != null)
            {
                Destroy(card.gameObject);
            }
        }
        availableCardList.Clear();

        if (newDP == null || newDP.name == null)
        {
            cardListTitleTMP.text = "Available Cards";
        }
        else
        {
            cardListTitleTMP.text = "Available Cards for " + newDP.name;
        }

        if (newDP == null) return;

        Item[] itemList = newDP.cards;
        foreach (Item item in itemList)
        {
            if (item.element == pType)
            {
                var cardObject = Instantiate(draggableCardUIPrefab, cardListScroll.transform.position, Utils.QI);
                cardObject.transform.SetParent(cardListScroll.transform);
                var card = cardObject.GetComponent<CardUI_Draggable>();

                card.Setup(item);
                card.raycaster = canvas.GetComponent<GraphicRaycaster>();
                availableCardList.Add(card);
            }
        }
    }

    public void NormalCardListSet()
    {
        foreach(CardUI_Draggable card in availableCardList)
        {
            if(card != null)
            {
                Destroy(card.gameObject);
            }
        }
        availableCardList.Clear();

        foreach(Item item in normalItemSO.items)
        {
            var cardObject = Instantiate(draggableCardUIPrefab, cardListScroll.transform.position, Utils.QI);
            cardObject.transform.SetParent(cardListScroll.transform);
            var card = cardObject.GetComponent<CardUI_Draggable>();

            card.Setup(item);
            card.raycaster = canvas.GetComponent<GraphicRaycaster>();
            availableCardList.Add(card);
        }
        if (selectedPersona != null)
        {
            foreach (Item item in selectedPersona.cards)
            {
                if (item.element == EPassiveType.Normal)
                {
                    var cardObject = Instantiate(draggableCardUIPrefab, cardListScroll.transform.position, Utils.QI);
                    cardObject.transform.SetParent(cardListScroll.transform);
                    var card = cardObject.GetComponent<CardUI_Draggable>();

                    card.Setup(item);
                    card.raycaster = canvas.GetComponent<GraphicRaycaster>();
                    availableCardList.Add(card);
                }
            }
        }
        if (selectedShadow != null && selectedShadow != selectedPersona)
        {
            foreach (Item item in selectedShadow.cards)
            {
                if (item.element == EPassiveType.Normal)
                {
                    var cardObject = Instantiate(draggableCardUIPrefab, cardListScroll.transform.position, Utils.QI);
                    cardObject.transform.SetParent(cardListScroll.transform);
                    var card = cardObject.GetComponent<CardUI_Draggable>();

                    card.Setup(item);
                    card.raycaster = canvas.GetComponent<GraphicRaycaster>();
                    availableCardList.Add(card);
                }
            }
        }

        cardListTitleTMP.text = "Available Cards for Prim";
    }

    public void PassiveList(EPassiveType pType)
    {
        foreach (DreamPiece dp in dreamPieceSO.dreamPieces)
        {
            var pObject = Instantiate(passivePrefab, passiveListScroll.transform.position, Utils.QI);
            pObject.transform.SetParent(passiveListScroll.transform);

            var passive = pObject.GetComponent<PassiveUI>();
            if (pType == EPassiveType.Persona)
            {
                passive.Setup(dp, EPassiveType.Persona);
                if (selectedPersona == dp)
                {
                    passive.Select(true);
                }
            }
            else if (pType == EPassiveType.Shadow)
            {
                passive.Setup(dp, EPassiveType.Shadow);
                if (selectedShadow == dp)
                {
                    passive.Select(true);
                }
            }
            passiveList.Add(passive);
        }
    }

    public void SelectPassive(DreamPiece dp, EPassiveType pType)
    {
        foreach (PassiveUI pUI in passiveList)
        {
            if (pUI.dreamPiece == dp)
            {
                pUI.Select(true);
            }
            else
            {
                pUI.Select(false);
            }
        }

        CardListSet(dp, pType);
        
        if(dp != null)
        {
            if (pType == EPassiveType.Persona)
            {
                selectedPersona = dp;
                personaButton.sprite = dp.persona.sprite;
                personaName.text = dp.persona.name;
                personaText.text = dp.persona.text;

                if (selectedShadow == dp)
                {
                    selectedShadow = null;
                    shadowButton.sprite = null;
                    shadowName.text = "";
                    shadowText.text = "";
                }
            }
            else if (pType == EPassiveType.Shadow)
            {
                selectedShadow = dp;
                shadowButton.sprite = dp.shadow.sprite;
                shadowName.text = dp.shadow.name;
                shadowText.text = dp.shadow.text;

                if (selectedPersona == dp)
                {
                    selectedPersona = null;
                    personaButton.sprite = null;
                    personaName.text = "";
                    personaText.text = "";
                }
            }
        }
        
        foreach (CardUI_DeckBuild deckCard in deckList)
        {
            if(deckCard.item != null)
            {
                if(deckCard.item.element == EPassiveType.Normal)
                {
                    continue;
                }
                else if(selectedPersona != null && (selectedPersona.name == dreamPieceSO.dreamPieces[deckCard.item.dreamPieceNum].name))
                {
                    continue;
                }
                else if(selectedShadow != null && (selectedShadow.name == dreamPieceSO.dreamPieces[deckCard.item.dreamPieceNum].name))
                {
                    continue;
                }
                else
                {
                    deckCard.Setup(null);
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

            NormalCardListSet();
        }
        else
        {
            PassiveList(EPassiveType.Persona);
            SelectPassive(selectedPersona, EPassiveType.Persona);
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

            NormalCardListSet();
        }
        else
        {
            var temp = shadowShow.transform.position;
            shadowShow.transform.position = personaShow.transform.position;
            personaShow.transform.position = temp;
            PassiveList(EPassiveType.Shadow);
            SelectPassive(selectedShadow, EPassiveType.Shadow);
            passiveScrollView.GetComponent<Image>().color = shadowShow.GetComponent<Image>().color;
            passiveScrollView.SetActive(true);
        }
    }

    public void FinishDeckBuild()
    {
        bool isReady = true;
        foreach (CardUI_DeckBuild card in deckList)
        {
            if (card.item == null || card.item.name == null)
            {
                isReady = false;
            }
        }
        if (isReady == true)
        {
            CardManager.Inst.playerDeckSO.items.Clear();
            foreach (CardUI_DeckBuild card in deckList)
            {
                Item existItem = CardManager.Inst.playerDeckSO.items.Find(x => x.name == card.item.name);
                if (existItem == null)
                {
                    Item tempItem = new Item();
                    tempItem.name = card.item.name;
                    tempItem.cost = card.item.cost;
                    tempItem.type = card.item.type;
                    tempItem.element = card.item.element;
                    tempItem.dreamPieceNum = card.item.dreamPieceNum;
                    tempItem.isVolatile = card.item.isVolatile;
                    tempItem.isVanish = card.item.isVanish;
                    tempItem.isRemain = card.item.isRemain;
                    tempItem.sprite = card.item.sprite;
                    tempItem.text = card.item.text;
                    tempItem.num = 1;
                    CardManager.Inst.playerDeckSO.items.Add(tempItem);
                }
                else
                {
                    existItem.num++;
                }
            }
            characterSO.personaPiece = selectedPersona;
            characterSO.shadowPiece = selectedShadow;
            EndDeckBuildUI();
        }
    }

    public void InitializeDeckBuildUI()
    {
        selectedPersona = null;
        selectedShadow = null;
        isLoading = false;
        RelicList();
        DeckListInit();
        NormalCardListSet();
    }

    public void EndDeckBuildUI()
    {
        // TODO. exit UI or something.
        SceneManager.LoadScene("BattleScene");
    }

    void Start()
    {
        InitializeDeckBuildUI();
    }
}
