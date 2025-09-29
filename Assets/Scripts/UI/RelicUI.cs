using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RelicUI : MonoBehaviour
{
    [SerializeField] Image backgroundImg;
    [SerializeField] Image relicImg;
    [SerializeField] TMP_Text relicNameTMP;
    [SerializeField] TMP_Text relicTextTMP;

    public RelicItem relicItem;

    public void Setup(RelicItem rItem)
    {
        if (rItem == null)
        {
            this.relicItem = null;
            return;
        }

        this.relicItem = rItem;
        
        relicImg.sprite = rItem.relicSprite;
        relicNameTMP.text = rItem.relicName;
        relicTextTMP.text = rItem.relicTxt;
    }
}
