using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;

public class CardUI : MonoBehaviour
{
    [SerializeField] Image cardImg;
    [SerializeField] Image character;
    [SerializeField] Image type;
    [SerializeField] Image rarity;
    [SerializeField] Image cost;
    [SerializeField] TMP_Text nameTMP;
    [SerializeField] TMP_Text costTMP;
    [SerializeField] TMP_Text typeTMP;
    [SerializeField] TMP_Text textTMP;
    [SerializeField] Sprite[] cardTypes;
    [SerializeField] Sprite[] rarityTypes;
    [SerializeField] Sprite[] costTypes;

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

        type.sprite = cardTypes[(int)this.item.type];
        // if(type.sprite == null) type.enabled = false;
        // else type.enabled = true;
        switch(this.item.type)
        {
            case CardType.Turn:
                typeTMP.text = "회전";
                break;
            case CardType.Enchant:
                typeTMP.text = "부여";
                break;
            case CardType.Skill:
                typeTMP.text = "스킬";
                break;
            case CardType.Dream:
                typeTMP.text = "몽상";
                break;
        }
        rarity.sprite = rarityTypes[(int)this.item.rarity];
        cardImg.sprite = rarityTypes[(int)this.item.rarity];
        // if(rarity.sprite == null) rarity.enabled = false;
        // else rarity.enabled = true;

        Sprite tempSprite = this.item.sprite;
        if(tempSprite != null) character.sprite = tempSprite;

        nameTMP.text = this.item.name;
        costTMP.text = this.item.cost.ToString();
        if(this.item.cost >= 0 && this.item.cost <= 9)
        {
            cost.sprite = costTypes[this.item.cost];
            cost.enabled = true;
        }
        else
        {
            cost.enabled = false;
        }
        
        
        string showText = this.item.text;
        int index = 0;
        if (this.item.cardValues.Count == 0)
        {
            string itemText = Regex.Replace(this.item.text, @"(\d+)(<(피해|수비|회복|특수)>)?", match =>
            {
                ECardValueType tempType = ECardValueType.Default;
                switch(match.Groups[2].Value)
                {
                    case "피해":
                        tempType = ECardValueType.Damage; break;
                    case "수비":
                        tempType = ECardValueType.Shield; break;
                    case "회복":
                        tempType = ECardValueType.Heal; break;
                    case "특수":
                        tempType = ECardValueType.Special; break;
                    default:
                        tempType = ECardValueType.Default; break;
                }
                this.item.cardValues.Add(int.Parse(match.Groups[1].Value));
                this.item.cardValueTypes.Add(tempType);
                index++;
                return match.Value;
            });
            showText = $"{itemText}";
        }
        index = 0;
        showText = Regex.Replace(showText, @"(\d+)(<(피해|수비|회복|특수)>)?", match => 
        {
            return this.item.cardValues[index++].ToString();
        });
        textTMP.text = $"{showText}";
    }

    public void SetBlank()
    {
        character.gameObject.SetActive(false);
        rarity.gameObject.SetActive(false);
        nameTMP.gameObject.SetActive(false);
        costTMP.gameObject.SetActive(false);
        textTMP.gameObject.SetActive(false);
        cardImg.color = Color.gray;
    }

    public void UnsetBlank()
    {
        character.gameObject.SetActive(true);
        //rarity.gameObject.SetActive(true);
        nameTMP.gameObject.SetActive(true);
        //costTMP.gameObject.SetActive(true);
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
