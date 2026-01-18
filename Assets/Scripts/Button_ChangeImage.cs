using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class Button_ChangeImage : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    Button button;
    Image buttonImage;
    TMP_Text buttonText;
    [SerializeField] Sprite normalSprite;
    [SerializeField] Sprite pressedSprite;
    [SerializeField] TMP_FontAsset buttonFont;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Button Down");
        if(buttonImage != null) buttonImage.sprite = pressedSprite;
        if(buttonText != null) buttonText.color = Color.white;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if(buttonImage != null) buttonImage.sprite = normalSprite;
        if(buttonText != null) buttonText.color = Color.black;
    }

    void Start()
    {
        button = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        buttonText = GetComponentInChildren<TMP_Text>();
        if(buttonImage != null) buttonImage.sprite = normalSprite;
        if(buttonText != null)
        {
            buttonText.color = Color.black;
            buttonText.font = buttonFont;
        }
    }
}
