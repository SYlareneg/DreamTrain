using UnityEngine;
using TMPro;

public class CardTooltip : MonoBehaviour
{
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text descriptionText;

    public void SetTooltip(string title, string description)
    {
        titleText.text = title;
        descriptionText.text = description;
        this.gameObject.SetActive(true);
    }
}
