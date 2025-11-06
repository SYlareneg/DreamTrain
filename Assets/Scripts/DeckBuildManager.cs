using System;
using System.Collections;
using System.Linq;
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
    [SerializeField] GameObject itemScrollView;

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
    [SerializeField] GameObject playerDeckScrollView;
    [SerializeField] GameObject playerDeckListScroll;
    [SerializeField] GameObject showCardUIPrefab;
    public List<CardUI> playerDeckList;
    public TMP_Text playerDeckNum;
    [SerializeField] GameObject playerRelicScrollView;
    [SerializeField] GameObject playerRelicListScroll;
    public List<RelicUI> playerRelicList;

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

    public bool isLoading;
    public static bool IsDeckBuildOpen = false;

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
            cardListTitleTMP.text = "카드 풀";
        }
        else
        {
            cardListTitleTMP.text = newDP.name + " 카드 풀";
        }

        if (newDP == null || newDP.cards == null) return;

        Item_Enhanceable[] itemList = newDP.cards;
        foreach (Item_Enhanceable item in itemList)
        {
            if (item.element == pType || item.element == EPassiveType.Normal)
            {
                GameObject cardObject = null;
                CardUI_Draggable card = null;
                if (item.num > 0)
                {
                    cardObject = Instantiate(draggableCardUIPrefab, cardListScroll.transform.position, Utils.QI);
                    cardObject.transform.SetParent(cardListScroll.transform);
                    card = cardObject.GetComponent<CardUI_Draggable>();
                    card.Setup((Item)item);
                    card.raycaster = canvas.GetComponent<GraphicRaycaster>();
                    availableCardList.Add(card);
                }

                if (item.enhancedItem.num > 0)
                {
                    cardObject = Instantiate(draggableCardUIPrefab, cardListScroll.transform.position, Utils.QI);
                    cardObject.transform.SetParent(cardListScroll.transform);
                    card = cardObject.GetComponent<CardUI_Draggable>();
                    card.Setup(item.enhancedItem);
                    card.raycaster = canvas.GetComponent<GraphicRaycaster>();
                    availableCardList.Add(card);
                }
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

        cardListTitleTMP.text = "공용 카드 풀";
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
                personaName.text = dp.name;
                if (dp.persona.isEnhanced)
                {
                    personaButton.sprite = dp.persona.enhancedPassive.sprite;
                    personaText.text = dp.persona.enhancedPassive.name + ":\n" + dp.persona.enhancedPassive.text;
                }
                else
                {
                    personaButton.sprite = dp.persona.sprite;
                    personaText.text = dp.persona.name + ":\n" + dp.persona.text;
                }

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
                shadowName.text = dp.name;
                if (dp.shadow.isEnhanced)
                {
                    shadowButton.sprite = dp.shadow.enhancedPassive.sprite;
                    shadowText.text = dp.shadow.enhancedPassive.name + ":\n" + dp.shadow.enhancedPassive.text;
                }
                else
                {
                    shadowButton.sprite = dp.shadow.sprite;
                    shadowText.text = dp.shadow.name + ":\n" + dp.shadow.text;
                }

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
                if(deckCard.item.dreamPieceNum < 0)
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
                    changeCardNum(deckCard.item, 1);
                    deckCard.Setup(null);
                }
            }
        }
    }

    public void ShowPersona()
    {
        if (passiveScrollView.activeSelf == true)
        {
            itemScrollView.SetActive(true);
            shadowShow.SetActive(true);
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
            itemScrollView.SetActive(false);
            shadowShow.SetActive(false);
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
            itemScrollView.SetActive(true);
            personaShow.SetActive(true);
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
            itemScrollView.SetActive(false);
            personaShow.SetActive(false);
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
                var existItem = CardManager.Inst.playerDeckSO.items.Find(x => x.name == card.item.name);
                if (existItem == null)
                {
                    Item tempItem = new Item();
                    tempItem.SetItem(card.item);
                    tempItem.num = 1;
                    CardManager.Inst.playerDeckSO.items.Add(tempItem);
                }
                else
                {
                    existItem.num++;
                }
                card.item.num++;
            }
            characterSO.personaPiece = selectedPersona;
            characterSO.shadowPiece = selectedShadow;
            EndDeckBuildUI();
        }
    }

    public void PlayerDeckList()
    {
        if (playerDeckScrollView.activeSelf == false)
        {
            foreach (CardUI card in playerDeckList)
            {
                Destroy(card.gameObject);
            }

            playerDeckList = new List<CardUI>();
            List<Item> sortedItemList = CardManager.Inst.playerDeckSO.items.OrderBy(x => x.name).ToList();
            Vector3 standardListPosition = playerDeckListScroll.transform.position;

            foreach (Item item in sortedItemList)
            {
                for (int i = 0; i < item.num; i++)
                {
                    var cardObject = Instantiate(showCardUIPrefab, standardListPosition, Utils.QI);
                    cardObject.transform.SetParent(playerDeckListScroll.transform);
                    var card = cardObject.GetComponent<CardUI>();

                    card.Setup(item);
                    playerDeckList.Add(card);
                }
            }
            Canvas.ForceUpdateCanvases();

            playerDeckScrollView.SetActive(true);
        }
        else
        {
            playerDeckScrollView.SetActive(false);
        }
    }
    
    public void PlayerRelicList()
    {
        if (playerRelicScrollView.activeSelf == false)
        {
            foreach (RelicUI relic in playerRelicList)
            {
                Destroy(relic.gameObject);
            }

            playerRelicList = RelicManager.Inst.RelicItemListToRelicUIList(RelicManager.Inst.relicList, playerRelicListScroll.transform);
            Canvas.ForceUpdateCanvases();

            playerRelicScrollView.SetActive(true);
        }
        else
        {
            playerRelicScrollView.SetActive(false);
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
        backgroundPanel.SetActive(true);
        IsDeckBuildOpen = true;
    }

    public void EndDeckBuildUI()
    {
        // TODO. exit UI or something.
        //SceneManager.LoadScene("BattleScene");
        backgroundPanel.SetActive(false);
        IsDeckBuildOpen = false;
    }

    void Start()
    {
        RelicList();
    }

    private void Update()
    {
        int num = 0;
        foreach(var item in CardManager.Inst.playerDeckSO.items)
        {
            num += item.num;
        }
        playerDeckNum.text = num.ToString();
        playerDeckNum.color = Color.white;
    }
}
