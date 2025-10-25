using UnityEngine.UI;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class BuffUI : MonoBehaviour
{
    [SerializeField] Image BuffImg;
    [SerializeField] TMP_Text BuffValue;
    [SerializeField] List<Sprite> BuffSpriteList;
    public Buff buff;
    public void Setup(Buff b)
    {
        this.buff = b;
        switch(buff.target)
        {
            default:
                BuffImg.sprite = BuffSpriteList[0];
                break;
        }
        BuffValue.text = "";
        if (buff.mul != 1)
        {
            BuffValue.text += "x" + buff.mul.ToString() + " ";
        }
        if (buff.add != 0)
        {
            BuffValue.text = "+" + buff.add.ToString() + " ";
        }
    }
}
