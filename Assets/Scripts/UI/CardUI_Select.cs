using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CardUI_Select : CardUI, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData data)
    {
        CardManager.Inst.UnSelectCard(this.gameObject);
    }

    public override void OnPointerEnter(PointerEventData eventData)
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
                    keywordTooltipObj.transform.SetParent(transform, true);
                    keywordTooltipObj.transform.SetAsLastSibling();
                    keywordTooltipObj.transform.localScale *= 0.02777778f;
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

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
    }

    void Update()
    {
        if (tooltipCreated)
        {
            Vector3 offset = Vector3.zero;
            for(int i = 0; i < activeTooltips.Count; i++)
            {
                Vector3 screenPoint = tooltipPos.position - offset * 0.02777778f;
                activeTooltips[i].transform.position = screenPoint;
                offset.y += activeTooltips[i].GetComponent<RectTransform>().rect.height + 10;
                Debug.Log(activeTooltips[i].transform.position);
            }
        }
    }
}
