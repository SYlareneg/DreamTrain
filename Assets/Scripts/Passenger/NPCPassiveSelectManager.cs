using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class NPCPassiveSelectManager : MonoBehaviour
{
    public static NPCPassiveSelectManager Inst { get; private set; }
    void Awake() => Inst = this;

    [Header("패시브 목록 제시")]
    [SerializeField] GameObject npcPassiveScreen;
    [SerializeField] TMP_Text curPassiveTMP;
    [SerializeField] CharacterSO characterSO;
    [SerializeField] DreamPieceSO dreamPieceSO;
    [SerializeField] ItemSO normalItemSO;
    public bool curMode_isPersona;
    [Header("페르소나")]
    [SerializeField] Image curPassiveImg_Persona;
    [SerializeField] TMP_Text curPassiveName_Persona;
    [SerializeField] TMP_Text curPassiveText_Persona;
    [SerializeField] Button curPassiveSelectButton_Persona;
    public DreamPieceUI_Select curPassive_Persona;
    [Header("그림자")]
    [SerializeField] Image curPassiveImg_Shadow;
    [SerializeField] TMP_Text curPassiveName_Shadow;
    [SerializeField] TMP_Text curPassiveText_Shadow;
    [SerializeField] Button curPassiveSelectButton_Shadow;
    public DreamPieceUI_Select curPassive_Shadow;
    [Header("꿈 조각 목록")]
    [SerializeField] GameObject npcPassivePrefab;
    [SerializeField] GameObject npcPassiveListScroll;

    public void Setup()
    {
        curPassiveName_Persona.text = "";
        curPassiveText_Persona.text = "";
        curPassive_Persona = new DreamPieceUI_Select();
        curPassiveSelectButton_Persona.gameObject.SetActive(false);
        curPassiveName_Shadow.text = "";
        curPassiveText_Shadow.text = "";
        curPassive_Shadow = new DreamPieceUI_Select();
        curPassiveSelectButton_Shadow.gameObject.SetActive(false);

        foreach(Transform t in npcPassiveListScroll.transform)
        {
            Destroy(t.gameObject);
        }
        foreach (var dp in dreamPieceSO.dreamPieces)
        {
            var npcPassiveObj = Instantiate(npcPassivePrefab, npcPassiveListScroll.transform, false);
            npcPassiveObj.transform.SetParent(npcPassiveListScroll.transform);
            var npcPassive = npcPassiveObj.GetComponent<DreamPieceUI_Select>();

            npcPassive.Setup(dp);
        }

        curPassiveTMP.text = "페르소나 선택";
        curMode_isPersona = true;
    }

    public void SetPassive(DreamPieceUI_Select dp)
    {
        if(curMode_isPersona)
        {
            curPassive_Persona = dp;
            if (dp.dreamPiece.persona.isEnhanced)
            {
                curPassiveImg_Persona.sprite = dp.dreamPiece.persona.enhancedPassive.sprite;
                curPassiveName_Persona.text = dp.dreamPiece.persona.enhancedPassive.name;
                curPassiveText_Persona.text = dp.dreamPiece.persona.enhancedPassive.text;
            }
            else
            {
                curPassiveImg_Persona.sprite = dp.dreamPiece.persona.sprite;
                curPassiveName_Persona.text = dp.dreamPiece.persona.name;
                curPassiveText_Persona.text = dp.dreamPiece.persona.text;
            }
            curPassiveSelectButton_Persona.gameObject.SetActive(true);
        }
        else
        {
            curPassive_Shadow = dp;
            if (dp.dreamPiece.shadow.isEnhanced)
            {
                curPassiveImg_Shadow.sprite = dp.dreamPiece.shadow.enhancedPassive.sprite;
                curPassiveName_Shadow.text = dp.dreamPiece.shadow.enhancedPassive.name;
                curPassiveText_Shadow.text = dp.dreamPiece.shadow.enhancedPassive.text;
            }
            else
            {
                curPassiveImg_Shadow.sprite = dp.dreamPiece.shadow.sprite;
                curPassiveName_Shadow.text = dp.dreamPiece.shadow.name;
                curPassiveText_Shadow.text = dp.dreamPiece.shadow.text;
            }
            curPassiveSelectButton_Shadow.gameObject.SetActive(true);
        }
    }

    public void SelectPassive()
    {
        if(curMode_isPersona)
        {
            characterSO.personaPiece = new DreamPiece_Player();
            characterSO.personaPiece.Setup(curPassive_Persona.dreamPiece);
            characterSO.personaPiece.cards = new List<Item>();
            foreach(var card in curPassive_Persona.dreamPiece.baseCards_persona)
            {
                characterSO.personaPiece.cards.Add(card);
            }
            curPassive_Persona.Select(true, Color.blue);
            curPassiveSelectButton_Persona.gameObject.SetActive(false);
            curPassiveTMP.text = "그림자 선택";
            curMode_isPersona = false;
        }
        else
        {
            characterSO.shadowPiece = new DreamPiece_Player();
            characterSO.shadowPiece.Setup(curPassive_Shadow.dreamPiece);
            characterSO.shadowPiece.cards = new List<Item>();
            foreach(var card in curPassive_Persona.dreamPiece.baseCards_shadow)
            {
                characterSO.shadowPiece.cards.Add(card);
            }

            foreach(var card in normalItemSO.items)
            {
                characterSO.normalCards.Add(card);
            }
            HideScreen();
        }
    }

    public void ShowScreen()
    {
        PlayerManager.Inst.isLoading = true;
        Setup();
        npcPassiveScreen.SetActive(true);
    }

    public void HideScreen()
    {
        PlayerManager.Inst.isLoading = false;
        npcPassiveScreen.SetActive(false);
    }
}
