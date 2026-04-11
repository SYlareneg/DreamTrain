using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public class RoomDPManager : MonoBehaviour
{
    public static RoomDPManager Inst;
    private void Awake()
    {
        Inst = this;
    }

    [SerializeField] CharacterSO playerCharacterSO;
    [SerializeField] DreamPieceSO dreamPieceListSO;
    [SerializeField] PlayerStatsSo playerStatsSO;
    [SerializeField] Transform dpIconParent;
    [SerializeField] GameObject dpIconPrefab;
    [SerializeField] GameObject dpPanel;
    [SerializeField] GameObject dpDetailPanel;
    [SerializeField] Image fadeoutScreen;
    [SerializeField] GameObject initView;
    [SerializeField] TMP_Text dpName;
    [SerializeField] TMP_Text dpDescription;
    [SerializeField] Image dpImage;
    [SerializeField] GameObject courageStat;
    [SerializeField] GameObject wisdomStat;
    [SerializeField] GameObject luckStat;
    [SerializeField] TMP_Text passiveText;
    [SerializeField] Button dpSelectButton;
    [SerializeField] RoomDPSlider backSlider;
    [SerializeField] RoomDPMarker dpMarker1;
    [SerializeField] RoomDPMarker dpMarker2;
    [SerializeField] RoomDPChain startSlider;
    [SerializeField] Sprite[] roomDPBackground;
    [SerializeField] Image roomBackgroundImage;
    public List<RoomDPIcon> roomDPIcons = new List<RoomDPIcon>();

    public bool isShowingPersona = true;
    public DreamPiece_Reference currentDreamPiece;
    public bool isInit = true;

    public void ShowAllDreamPieces()
    {
        foreach(Transform child in dpIconParent)
        {
            Destroy(child.gameObject);
        }
        roomDPIcons.Clear();
        foreach(var dp in dreamPieceListSO.dreamPieces)
        {
            var newDPIcon = Instantiate(dpIconPrefab, dpIconParent);
            newDPIcon.GetComponent<RoomDPIcon>().Setup(dp);
            roomDPIcons.Add(newDPIcon.GetComponent<RoomDPIcon>());
        }
    }

    public void SelectDreamPiece()
    {
        if(isShowingPersona)
        {
            playerCharacterSO.personaPiece = new DreamPiece_Player(currentDreamPiece);
            playerCharacterSO.personaPiece.cards = new List<Item>(currentDreamPiece.baseCards_persona);
            dpMarker1.SetDPIcon(currentDreamPiece.persona.sprite);
            dpMarker1.DeActivate();
            isShowingPersona = false;
            dpSelectButton.GetComponentInChildren<TMP_Text>().text = "두번째 꿈 조각 선택";
            ClearDreamPieceView();
            initView.SetActive(true);
            isInit = true;
        }
        else
        {
            playerCharacterSO.shadowPiece = new DreamPiece_Player(currentDreamPiece);
            playerCharacterSO.shadowPiece.cards = new List<Item>(currentDreamPiece.baseCards_shadow);
            dpMarker2.SetDPIcon(currentDreamPiece.shadow.sprite);
            dpMarker2.DeActivate();
            startSlider.Activate();
        }
    }

    public void SetDreamPieceView(DreamPiece_Reference dp)
    {
        if(currentDreamPiece == null)
        {
            if (isShowingPersona)
            {
                dpMarker1.Activate();
            }
            else
            {
                dpMarker2.Activate();
            }
        }
        roomBackgroundImage.sprite = roomDPBackground[dp.persona.dreamPieceNum + 1];
        currentDreamPiece = dp;
        dpName.text = dp.name;
        dpDescription.text = dp.description;
        dpImage.sprite = dp.triggerSprite;
        if (dp.courageStat > 0)
        {
            courageStat.SetActive(true);
            courageStat.GetComponentInChildren<TMP_Text>().text = "+" + dp.courageStat.ToString();
        }
        else
        {
            courageStat.SetActive(false);
        }
        if (dp.wisdomStat > 0)
        {
            wisdomStat.SetActive(true);
            wisdomStat.GetComponentInChildren<TMP_Text>().text = "+" + dp.wisdomStat.ToString();
        }
        else
        {
            wisdomStat.SetActive(false);
        }
        if (dp.luckStat > 0)
        {
            luckStat.SetActive(true);
            luckStat.GetComponentInChildren<TMP_Text>().text = "+" + dp.luckStat.ToString();
        }
        else
        {
            luckStat.SetActive(false);
        }
        if(isShowingPersona)
        {
            passiveText.text = dp.persona.name + ": " + dp.persona.text;
        }
        else
        {
            passiveText.text = dp.shadow.name + ": " + dp.shadow.text;
        }
        dpDetailPanel.SetActive(true);
        initView.SetActive(false);
        isInit = false;
    }

    public void ClearDreamPieceView()
    {
        currentDreamPiece = null;
        dpDetailPanel.SetActive(false);
    }

    public void InitDPUI()
    {
        RoomPlayer.Inst.isInteractable = false;
        RoomDialogueManager.OnDialogueEnd = () =>
        {
            RoomPlayer.Inst.isInteractable = false;
        };
        fadeoutScreen.color = new Color(Color.black.r, Color.black.g, Color.black.b, 0f);
        fadeoutScreen.gameObject.SetActive(true);
        Sequence fadeout = DOTween.Sequence();
        fadeout.Append(fadeoutScreen.DOFade(1f, 1f).OnComplete(() =>
        {
            dpPanel.SetActive(true);
            isShowingPersona = true;
            dpSelectButton.GetComponentInChildren<TMP_Text>().text = "첫번째 꿈 조각 선택";
            ClearDreamPieceView();
            initView.SetActive(true);
            isInit = true;
            ShowAllDreamPieces();
            backSlider.Activate();
            roomBackgroundImage.sprite = roomDPBackground[0];
        }));
        fadeout.Append(fadeoutScreen.DOFade(0f, 1f).OnComplete(() =>
        {
            fadeoutScreen.gameObject.SetActive(false);
        }));
    }

    public void HideDPUI()
    {
        dpPanel.SetActive(false);
        dpMarker1.dpIconImage.enabled = false;
        dpMarker2.dpIconImage.enabled = false;
        backSlider.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);
        dpMarker1.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        dpMarker2.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        startSlider.arrowSeq.Kill();
        startSlider.GetComponent<RectTransform>().pivot = new Vector2(0, 0.5f);
        // for(int i = 0; i < startSlider.chainArrows.Length; i++)
        // {
        //     startSlider.chainArrows[i].transform.localPosition = startSlider.arrowOriginalPositions[i];
        // }
        RoomPlayer.Inst.isInteractable = true;
        RoomDialogueManager.OnDialogueEnd = null;
    }

    public void StartGame()
    {
        playerStatsSO.courage += playerCharacterSO.personaPiece.courageStat;
        playerStatsSO.wisdom += playerCharacterSO.personaPiece.wisdomStat;
        playerStatsSO.luck += playerCharacterSO.personaPiece.luckStat;
        playerStatsSO.courage += playerCharacterSO.shadowPiece.courageStat;
        playerStatsSO.wisdom += playerCharacterSO.shadowPiece.wisdomStat;
        playerStatsSO.luck += playerCharacterSO.shadowPiece.luckStat;
        SceneChangeManager.Inst.SceneFadeOut("MapScene");
    }

    // void Start()
    // {
    //     InitDPUI();
    // }
}
