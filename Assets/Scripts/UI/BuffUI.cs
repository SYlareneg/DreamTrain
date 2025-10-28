using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class BuffUI : MonoBehaviour
{
    [SerializeField] Image BuffImg;
    [SerializeField] TMP_Text BuffValue;
    [SerializeField] List<Sprite> BuffSpriteList;
    public ShowBuff buff;
    public void Setup(ShowBuff b)
    {
        this.buff = b;
        BuffImg.sprite = b.icon;
        BuffValue.text = b.val.ToString();

        this.GetComponent<Tooltip>().tooltipTitle = b.name;

        string buffText = Regex.Replace(b.text, @"값", match =>
        {
            string replacement = $"{b.val}";
            return replacement;
        });
        this.GetComponent<Tooltip>().tooltipTxt = $"{buffText}";
    }
}
