using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    [SerializeField] Image cardImg;
    [SerializeField] Image character;
    [SerializeField] Image element;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text costTMP;
    [SerializeField] TMP_Text textTMP;
    [SerializeField] Sprite[] cardTypes;
    [SerializeField] Sprite[] elementTypes;

    public Item item;

    public void Setup(Item item)
    {
        if(item == null)
        {
            this.item = null;
            return;
        }

        this.item = item;

        switch(this.item.type)
        {
            case CardType.Turn:
                cardImg.sprite = cardTypes[0]; break;
            case CardType.Enchant:
                cardImg.sprite = cardTypes[1]; break;
            case CardType.Effect:
                cardImg.sprite = cardTypes[2]; break;
        }

        character.sprite = this.item.sprite;

        switch(this.item.element)
        {
            case CardElement.Fire:
                element.sprite = elementTypes[0]; break;
            case CardElement.Grass:
                element.sprite = elementTypes[1]; break;
            case CardElement.Water:
                element.sprite = elementTypes[2]; break;
        }

        nameTMP.text = this.item.name;
        costTMP.text = this.item.cost.ToString();
        textTMP.text = this.item.text;
    }
}
