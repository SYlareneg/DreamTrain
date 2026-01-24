using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Text.RegularExpressions;

public class CardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
    Vector3 originalScale;
    bool tooltipCreated = false;
    [SerializeField] GameObject cardUITooltipPrefab;
    [SerializeField] Transform tooltipPos;
    List<GameObject> activeTooltips = new List<GameObject>();

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

        if(this.item.dreamPieceNum >= 0 && this.item.dreamPieceNum == DataManager.Inst.characterSO.personaPiece.persona.dreamPieceNum)
        {
            cardImg.sprite = DataManager.Inst.characterSO.personaPiece.cardBackgrounds[(int)this.item.rarity];
            typeTMP.color = DataManager.Inst.characterSO.personaPiece.textColors[0];
            nameTMP.color = DataManager.Inst.characterSO.personaPiece.textColors[1];
            textTMP.color = DataManager.Inst.characterSO.personaPiece.textColors[2];
        }
        else if(this.item.dreamPieceNum >= 0 && this.item.dreamPieceNum == DataManager.Inst.characterSO.shadowPiece.shadow.dreamPieceNum)
        {
            cardImg.sprite = DataManager.Inst.characterSO.shadowPiece.cardBackgrounds[(int)this.item.rarity];
            typeTMP.color = DataManager.Inst.characterSO.shadowPiece.textColors[0];
            nameTMP.color = DataManager.Inst.characterSO.shadowPiece.textColors[1];
            textTMP.color = DataManager.Inst.characterSO.shadowPiece.textColors[2];
        }
        else
        {
            cardImg.sprite = rarityTypes[(int)this.item.rarity];
        }
        rarity.sprite = rarityTypes[(int)this.item.rarity];
        // if(rarity.sprite == null) rarity.enabled = false;
        // else rarity.enabled = true;

        Sprite tempSprite = this.item.sprite;
        if(tempSprite != null) character.sprite = tempSprite;

        nameTMP.text = this.item.name;
        costTMP.text = this.item.cost.ToString();
        // if(this.item.cost >= 0 && this.item.cost <= 9)
        // {
        //     cost.sprite = costTypes[this.item.cost];
        //     cost.enabled = true;
        // }
        // else
        // {
        //     cost.enabled = false;
        // }
        
        
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

    void Start()
    {
        originalScale = this.transform.localScale;
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        this.transform.localScale = originalScale * 1.3f;
        if (item != null && textTMP != null && !tooltipCreated)
        {
            int tooltipCount = 0;
            foreach(Keyword keyword in DataManager.Inst.keywordSO.keywords)
            {
                if(textTMP.text.Contains(keyword.word))
                {
                    var keywordTooltipObj = Instantiate(cardUITooltipPrefab, tooltipPos.position, Utils.QI);
                    keywordTooltipObj.transform.SetParent(transform.parent.parent, true);
                    keywordTooltipObj.transform.SetAsLastSibling();
                    activeTooltips.Add(keywordTooltipObj);

                    CardTooltip keywordTooltip = keywordTooltipObj.GetComponent<CardTooltip>();
                    keywordTooltip.SetTooltip(keyword.word, keyword.explanation);
                    tooltipCreated = true;
                    tooltipCount++;
                }
            }
        }

        if(SoundManager.Inst != null && SoundManager.Inst.UISelectSFX != null) GetComponent<AudioSource>().PlayOneShot(SoundManager.Inst.UISelectSFX);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        this.transform.localScale = originalScale;
        if (tooltipCreated)
        {
            foreach(GameObject tooltipObj in activeTooltips)
            {
                Destroy(tooltipObj);
            }
            activeTooltips.Clear();
            tooltipCreated = false;
        }
    }

    void Update()
    {
        if (tooltipCreated)
        {
            Vector3 offset = Vector3.zero;
            for(int i = 0; i < activeTooltips.Count; i++)
            {
                Vector3 screenPoint = tooltipPos.position - offset;
                activeTooltips[i].transform.position = screenPoint;
                offset.y += activeTooltips[i].GetComponent<RectTransform>().rect.height + 10;
            }
        }
    }
}
