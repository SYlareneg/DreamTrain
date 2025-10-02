using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
        if (item == null)
        {
            this.item = null;
            SetBlank();
            return;
        }

        this.item = item;
        UnsetBlank();

        switch (this.item.type)
        {
            case CardType.Turn:
                cardImg.sprite = cardTypes[0]; break;
            case CardType.Enchant:
                cardImg.sprite = cardTypes[1]; break;
            case CardType.Effect:
                cardImg.sprite = cardTypes[2]; break;
        }

        character.sprite = this.item.sprite;

        switch (this.item.element)
        {
            case EPassiveType.Normal:
                element.sprite = elementTypes[0]; break;
            case EPassiveType.Persona:
                element.sprite = elementTypes[1]; break;
            case EPassiveType.Shadow:
                element.sprite = elementTypes[2]; break;
        }

        nameTMP.text = this.item.name;
        costTMP.text = this.item.cost.ToString();
        textTMP.text = this.item.text;
    }

    public void SetBlank()
    {
        character.gameObject.SetActive(false);
        element.gameObject.SetActive(false);
        nameTMP.gameObject.SetActive(false);
        costTMP.gameObject.SetActive(false);
        textTMP.gameObject.SetActive(false);
        cardImg.color = Color.gray;
    }

    public void UnsetBlank()
    {
        character.gameObject.SetActive(true);
        element.gameObject.SetActive(true);
        nameTMP.gameObject.SetActive(true);
        costTMP.gameObject.SetActive(true);
        textTMP.gameObject.SetActive(true);
        cardImg.color = Color.white;
    }

    public void SetAlpha(float alpha)
    {
        Image[] images = this.gameObject.GetComponentsInChildren<Image>(true);
        foreach (Image img in images)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }
}
