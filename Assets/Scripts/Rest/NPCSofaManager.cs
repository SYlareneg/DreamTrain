using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class NPCSofaManager : MonoBehaviour
{
    public static NPCSofaManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] GameObject sofaUI;
    [SerializeField] Image fadeoutScreen;
    public Sofa sofa;
    [Header("회복 기능")]
    [SerializeField] float healPercent;
    [SerializeField] CharacterSO characterSO;
    [Header("카드 제거 기능")]
    [SerializeField] ItemSO playerDeckSO;
    [SerializeField] GameObject cardDeleteScreen;
    [SerializeField] GameObject cardDeleteList;
    [SerializeField] GameObject deleteCardPrefab;
    [SerializeField] GameObject cardDeleteConfirmScreen;
    [SerializeField] CardUI selectedCardShow;
    public CardUI_Delete selectedCard;
    public bool isDeleteCardSelectable;

    public void ShowSofaUI()
    {
        sofaUI.SetActive(true);
        PlayerManager.Inst.isLoading = true;
    }

    public void HideSofaUI()
    {
        sofaUI.SetActive(false);
        PlayerManager.Inst.isLoading = false;
    }
    public void SofaHeal()
    {
        fadeoutScreen.color = new Color(Color.black.r, Color.black.g, Color.black.b, 0f);
        fadeoutScreen.gameObject.SetActive(true);
        Sequence healSeq = DOTween.Sequence();
        healSeq.Append(fadeoutScreen.DOFade(1f, 1f).OnComplete(() =>
        {
            PlayerManager.Inst.characterSO.curHealth += (int)(PlayerManager.Inst.characterSO.maxHealth * healPercent);
            if (PlayerManager.Inst.characterSO.curHealth > PlayerManager.Inst.characterSO.maxHealth) PlayerManager.Inst.characterSO.curHealth = PlayerManager.Inst.characterSO.maxHealth;

            sofa.alreadyInteractedSpeech = "다음 정거장에 도착하기 전까지는 쉴 수 없겠어.(소파)";
            sofa.alreadyInteracted = true;
            sofaUI.SetActive(false);
        }));
        healSeq.Append(fadeoutScreen.DOFade(0f, 1f).OnComplete(() =>
        {
            fadeoutScreen.gameObject.SetActive(false);
            PlayerManager.Inst.isLoading = false;
        }));
    }

    public void SofaCardDelete()
    {
        if (cardDeleteScreen.activeSelf == true)
        {
            isDeleteCardSelectable = false;
            cardDeleteScreen.SetActive(false);
        }
        else
        {
            foreach(Transform child in cardDeleteList.transform)
            {
                Destroy(child.gameObject);
            }
            foreach (Item item in playerDeckSO.items)
            {
                for (int i = 0; i < item.num; i++)
                {
                    var deleteCardObj = Instantiate(deleteCardPrefab, cardDeleteList.transform, false);
                    deleteCardObj.transform.SetParent(cardDeleteList.transform);
                    CardUI_Delete deleteCard = deleteCardObj.GetComponent<CardUI_Delete>();
                    deleteCard.Setup(item);
                }
            }
            isDeleteCardSelectable = true;
            cardDeleteScreen.SetActive(true);
        }
    }

    public void DeleteCardSelect(CardUI_Delete delCard)
    {
        isDeleteCardSelectable = false;
        selectedCardShow.Setup(delCard.item);
        selectedCard = delCard;
        cardDeleteConfirmScreen.SetActive(true);
    }

    public void DeleteCard()
    {
        bool deleteFlag = false;
        foreach (Item item in playerDeckSO.items)
        {
            if (item == selectedCard.item)
            {
                item.num--;
                deleteFlag = true;
                break;
            }
        }
        for(int i = playerDeckSO.items.Count - 1; i >= 0; i--)
        {
            if(playerDeckSO.items[i].num == 0)
            {
                // 테스트 목적으로 주석처리
                //playerDeckSO.items.RemoveAt(i);
            }
        }
        if (deleteFlag == false)
        {
            Debug.LogWarning("cannot find card to delete!");
            return;
        }
        Destroy(selectedCard.gameObject);
        EndDeleteCardConfirm();
        SofaCardDelete();
        PlayerManager.Inst.SetPlayerSpeech("다음 정거장에 도착하기 전까지는 쉴 수 없겠어.");
        sofa.alreadyInteracted = true;
        HideSofaUI();
    }
    
    public void EndDeleteCardConfirm()
    {
        isDeleteCardSelectable = true;
        cardDeleteConfirmScreen.SetActive(false);
    }
}
