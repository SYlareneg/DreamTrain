using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NPCPassiveManager : MonoBehaviour
{
    public static NPCPassiveManager Inst { get; private set; }
    void Awake() => Inst = this;

    [Header("패시브 목록 제시")]
    [SerializeField] GameObject npcPassiveScreen;
    public EPassiveType npcType;
    [SerializeField] TMP_Text curPassiveTMP;
    [SerializeField] Image curPassiveImg;
    [SerializeField] TMP_Text curPassiveName;
    [SerializeField] TMP_Text curPassiveText;
    [SerializeField] GameObject npcPassivePrefab;
    [SerializeField] GameObject npcPassiveListScroll;
    [SerializeField] TMP_Text npcPassiveListTMP;
    [SerializeField] CharacterSO characterSO;
    [SerializeField] DreamPieceSO dreamPieceSO;
    public PassiveUI_Select selectedPersona;
    public PassiveUI_Select selectedShadow;
    public PassiveUI_Select selectedPassive;
    public PassiveUI_Select curPassive;
    [SerializeField] Button npcPassiveEnhanceButton;
    [SerializeField] Button npcPassiveChangeButton;

    [Header("패시브 변화 확인")]
    [SerializeField] GameObject npcPassiveChangeScreen;
    [SerializeField] TMP_Text passiveTMP_before;
    [SerializeField] Image passiveImg_before;
    [SerializeField] TMP_Text passiveName_before;
    [SerializeField] TMP_Text passiveText_before;
    [SerializeField] TMP_Text passiveTMP_after;
    [SerializeField] Image passiveImg_after;
    [SerializeField] TMP_Text passiveName_after;
    [SerializeField] TMP_Text passiveText_after;
    [SerializeField] int enhanceCost;
    [SerializeField] int changeCost;
    [SerializeField] Button enhanceButton;
    [SerializeField] Button changeButton;

    public void Setup(EPassiveType pType)
    {
        npcType = pType;
        foreach(Transform t in npcPassiveListScroll.transform)
        {
            Destroy(t.gameObject);
        }
        foreach (var dp in dreamPieceSO.dreamPieces)
        {
            var npcPassiveObj = Instantiate(npcPassivePrefab, npcPassiveListScroll.transform, false);
            npcPassiveObj.transform.SetParent(npcPassiveListScroll.transform);
            var npcPassive = npcPassiveObj.GetComponent<PassiveUI_Select>();

            npcPassive.Setup(dp.persona);

            if (dp.name == characterSO.personaPiece.name)
            {
                npcPassive.Select(true, Color.blue);
                selectedPersona = npcPassive;
                if (pType == EPassiveType.Persona) 
                {
                    selectedPassive = npcPassive;
                    SetPassive(npcPassive);
                }
            }
            else if (dp.name == characterSO.shadowPiece.name)
            {
                npcPassive.Select(true, Color.red);
                selectedShadow = npcPassive;
                if(pType == EPassiveType.Shadow)
                {
                    selectedPassive = npcPassive;
                    SetPassive(npcPassive);
                }
            }
        }
    }

    public void SetPassive(PassiveUI_Select p)
    {
        curPassive = p;
        if (p.passive.isEnhanced)
        {
            curPassiveImg.sprite = p.passive.enhancedPassive.sprite;
            curPassiveName.text = p.passive.enhancedPassive.name;
            curPassiveText.text = p.passive.enhancedPassive.text;
        }
        else
        {
            curPassiveImg.sprite = p.passive.sprite;
            curPassiveName.text = p.passive.name;
            curPassiveText.text = p.passive.text;
        }
    }

    public void ShowConfirmScreen(bool isChange)
    {
        npcPassiveScreen.SetActive(false);
        npcPassiveChangeScreen.SetActive(true);

        if (isChange)
        {
            // 이전 패시브: selectedPassive
            if (selectedPassive.passive.isEnhanced)
            {
                passiveImg_before.sprite = selectedPassive.passive.enhancedPassive.sprite;
                passiveName_before.text = selectedPassive.passive.enhancedPassive.name;
                passiveText_before.text = selectedPassive.passive.enhancedPassive.text;
            }
            else
            {
                passiveImg_before.sprite = selectedPassive.passive.sprite;
                passiveName_before.text = selectedPassive.passive.name;
                passiveText_before.text = selectedPassive.passive.text;
            }
            // 바꿀 패시브: curPassive
            passiveImg_after.sprite = curPassiveImg.sprite;
            passiveName_after.text = curPassiveName.text;
            passiveText_after.text = curPassiveText.text;
            // 결정 버튼 세팅
            enhanceButton.gameObject.SetActive(false);
            changeButton.gameObject.SetActive(true);
            if (characterSO.dreamDust < changeCost)
            {
                changeButton.interactable = false;
            }
            else
            {
                changeButton.interactable = true;
            }
        }
        else
        {
            // 강화 전 패시브
            passiveImg_before.sprite = curPassive.passive.sprite;
            passiveName_before.text = curPassive.passive.name;
            passiveText_before.text = curPassive.passive.text;
            // 강화 후 패시브
            passiveImg_after.sprite = curPassive.passive.enhancedPassive.sprite;
            passiveName_after.text = curPassive.passive.enhancedPassive.name;
            passiveText_after.text = curPassive.passive.enhancedPassive.text;
            // 결정 버튼 세팅
            enhanceButton.gameObject.SetActive(true);
            changeButton.gameObject.SetActive(false);
            if (characterSO.dreamDust < enhanceCost)
            {
                enhanceButton.interactable = false;
            }
            else
            {
                enhanceButton.interactable = true;
            }
        }
    }

    public void HideConfirmScreen()
    {
        npcPassiveChangeScreen.SetActive(false);
        if (npcType == EPassiveType.Persona) ShowPersonaScreen();
        else if (npcType == EPassiveType.Shadow) ShowShadowScreen();
    }

    public void EnhancePassive()
    {
        ShowConfirmScreen(false);
    }

    public void EnhancePassiveConfirm()
    {
        characterSO.dreamDust -= enhanceCost;
        curPassive.passive.isEnhanced = true;
        curPassive.Setup(curPassive.passive);
        HideConfirmScreen();
    }

    public void ChangePassive()
    {
        ShowConfirmScreen(true);
    }
    public void ChangePassiveConfirm()
    {
        characterSO.dreamDust -= changeCost;
        selectedPassive.Select(false, Color.white);
        selectedPassive = curPassive;
        if (npcType == EPassiveType.Persona)
        {
            curPassive.Select(true, Color.blue);
            characterSO.personaPiece = new DreamPiece_Player();
            characterSO.personaPiece.Setup(dreamPieceSO.dreamPieces[curPassive.passive.dreamPieceNum]);
            characterSO.personaPiece.cards = new List<Item>();
            foreach(var card in dreamPieceSO.dreamPieces[curPassive.passive.dreamPieceNum].baseCards_persona)
            {
                characterSO.personaPiece.cards.Add(card);
            }
        }
        else if (npcType == EPassiveType.Shadow)
        {
            curPassive.Select(true, Color.red);
            characterSO.shadowPiece = new DreamPiece_Player();
            characterSO.shadowPiece.Setup(dreamPieceSO.dreamPieces[curPassive.passive.dreamPieceNum]);
            characterSO.shadowPiece.cards = new List<Item>();
            foreach(var card in dreamPieceSO.dreamPieces[curPassive.passive.dreamPieceNum].baseCards_shadow)
            {
                characterSO.shadowPiece.cards.Add(card);
            }
        }
        HideConfirmScreen();
    }

    public void ShowPersonaScreen()
    {
        PlayerManager.Inst.isLoading = true;
        Setup(EPassiveType.Persona);
        curPassiveTMP.text = "현재 페르소나";
        npcPassiveListTMP.text = "보유 페르소나";
        passiveTMP_before.text = "현재 페르소나";
        passiveTMP_after.text = "변경 페르소나";
        npcPassiveScreen.SetActive(true);
    }

    public void ShowShadowScreen()
    {
        PlayerManager.Inst.isLoading = true;
        Setup(EPassiveType.Shadow);
        curPassiveTMP.text = "현재 그림자";
        npcPassiveListTMP.text = "보유 그림자";
        passiveTMP_before.text = "현재 그림자";
        passiveTMP_after.text = "변경 그림자";
        npcPassiveScreen.SetActive(true);
    }

    public void HideScreen()
    {
        npcPassiveScreen.SetActive(false);
        PlayerManager.Inst.isLoading = false;
    }

    private void Start()
    {
        //ShowShadowScreen();
    }
    
    private void Update()
    {
        if (curPassive == null) return;
        
        if (curPassive.passive.isEnhanced) npcPassiveEnhanceButton.gameObject.SetActive(false);
        else  npcPassiveEnhanceButton.gameObject.SetActive(true);

        if (curPassive == selectedPersona || curPassive == selectedShadow) npcPassiveChangeButton.gameObject.SetActive(false);
        else npcPassiveChangeButton.gameObject.SetActive(true);
    }
}
