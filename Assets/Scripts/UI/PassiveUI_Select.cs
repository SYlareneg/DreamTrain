using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PassiveUI_Select : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] Image passiveImage;
    [SerializeField] TMP_Text passiveNameTMP;
    public Passive_Enhanceable passive;
    public bool isSelected;
    Outline outline;

    public void Setup(Passive_Enhanceable p)
    {
        passive = p;
        if (p.isEnhanced)
        {
            passiveImage.sprite = p.enhancedPassive.sprite;
            passiveNameTMP.text = p.enhancedPassive.name;
        }
        else
        {
            passiveImage.sprite = p.sprite;
            passiveNameTMP.text = p.name;
        }
        Select(false, Color.white);
    }

    public void OnPointerClick(PointerEventData data)
    {
        NPCPassiveManager.Inst.SetPassive(this);
    }

    public void Select(bool onSelect, Color color)
    {
        outline = GetComponent<Outline>();
        outline.effectColor = color;
        isSelected = onSelect;
        if (!isSelected)
        {
            outline.enabled = false;
        }
        else
        {
            outline.enabled = true;
        }
    }
}
