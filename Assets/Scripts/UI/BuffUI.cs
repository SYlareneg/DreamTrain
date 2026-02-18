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
    Tooltip tooltip;
    public Vector2 tooltipBasePos;
    public void Setup(ShowBuff b)
    {
        this.buff = b;
        BuffImg.sprite = b.icon;
        BuffValue.text = b.val.ToString();

        tooltip = this.GetComponent<Tooltip>();
        tooltip.tooltipTitle = b.name;

        string buffText = Regex.Replace(b.text, @"값", match =>
        {
            string replacement = $"{b.val}";
            return replacement;
        });
        tooltip.tooltipTxt = $"{buffText}";
    }

    private void Update()
    {
        if(tooltip)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(this.transform.position) - Camera.main.WorldToScreenPoint(Vector3.zero);
            tooltip.tooltipPos = new Vector2(screenPos.x, screenPos.y);
        }
    }
}
