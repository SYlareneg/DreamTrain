using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class EncSofaManager : MonoBehaviour
{
    public static EncSofaManager Inst { get; private set; }
    void Awake() => Inst = this;

    [SerializeField] CharacterSO characterSO;
    [SerializeField] StageSO stageSO;
    [SerializeField] GameObject cardDeleteScreen;
    [SerializeField] GameObject cardDeleteList;
    [SerializeField] GameObject deleteCardPrefab;
    [SerializeField] GameObject cardDeleteConfirmScreen;
    [SerializeField] CardUI selectedCardShow;
    public EncCardUI_Delete selectedCard;
    public bool isDeleteCardSelectable;

    public void ShowSofaUI()
    {
        cardDeleteScreen.SetActive(true);
    }

    public void HideSofaUI()
    {
        cardDeleteScreen.SetActive(false);
    }

    public void SofaCardDelete()
    {
        foreach(Transform child in cardDeleteList.transform)
        {
            Destroy(child.gameObject);
        }
            
        List<Item> allCardItems = characterSO.normalCards.Concat(characterSO.personaPiece.cards).Concat(characterSO.shadowPiece.cards).ToList();
        Debug.Log($"총 카드 개수(종류): {allCardItems.Count}");
        foreach (Item item in allCardItems)
        {
            Debug.Log($"아이템: {item.name}, 보유량: {item.num}");
            for (int i = 0; i < item.num; i++)
            {
                var deleteCardObj = Instantiate(deleteCardPrefab, cardDeleteList.transform, false);
                deleteCardObj.transform.SetParent(cardDeleteList.transform);
                EncCardUI_Delete deleteCard = deleteCardObj.GetComponent<EncCardUI_Delete>();
                if (deleteCard != null)
                {
                    deleteCard.Setup(item);
                }
                else
                {
                    Debug.LogError("프리팹에 CardUI_Delete 컴포넌트가 없습니다!");
                }                
            }
        }
        isDeleteCardSelectable = true;
        cardDeleteScreen.SetActive(true);
    }

    public void DeleteCardSelect(EncCardUI_Delete delCard)
    {
        isDeleteCardSelectable = false;
        selectedCardShow.Setup(delCard.item);
        selectedCard = delCard;
        cardDeleteConfirmScreen.SetActive(true);
    }

    public void DeleteCard()
    {
        bool deleteFlag = false;
        List<Item> allCardItems = characterSO.normalCards.Concat(characterSO.personaPiece.cards).Concat(characterSO.shadowPiece.cards).ToList();
        foreach (Item item in allCardItems)
        {
            if (item == selectedCard.item)
            {
                item.num--;
                deleteFlag = true;
                break;
            }
        }
        for(int i = characterSO.normalCards.Count - 1; i >= 0; i--)
        {
            if(characterSO.normalCards[i].num == 0)
            {
                characterSO.normalCards.RemoveAt(i);
            }
        }
        for(int i = characterSO.personaPiece.cards.Count - 1; i >= 0; i--)
        {
            if(characterSO.personaPiece.cards[i].num == 0)
            {
                characterSO.personaPiece.cards.RemoveAt(i);
            }
        }
        for(int i = characterSO.shadowPiece.cards.Count - 1; i >= 0; i--)
        {
            if(characterSO.shadowPiece.cards[i].num == 0)
            {
                characterSO.shadowPiece.cards.RemoveAt(i);
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
        stageSO.sofaUsed = true;
        HideSofaUI();
    }
    
    public void EndDeleteCardConfirm()
    {
        isDeleteCardSelectable = true;
        cardDeleteConfirmScreen.SetActive(false);
    }
}
