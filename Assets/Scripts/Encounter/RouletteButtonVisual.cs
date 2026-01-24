using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 이게 있어야 마우스 이벤트를 감지합니다.

public class RouletteButtonVisual : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Image targetImage;
    private Button btn;

    // 로드할 스프라이트들을 담을 변수
    private Sprite spriteNormal; 
    private Sprite spriteHover;  
    private Sprite spritePressed; 

    void Awake()
    {
        targetImage = GetComponent<Image>();
        btn = GetComponent<Button>();

        spriteNormal = Resources.Load<Sprite>("Encounters/Images/버튼_누르기전_01");
        spriteHover = Resources.Load<Sprite>("Encounters/Images/spinButton_Bright"); 
        spritePressed = Resources.Load<Sprite>("Encounters/Images/버튼_누름_01");
        
        if (targetImage != null && spriteNormal != null)
            targetImage.sprite = spriteNormal;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (btn != null && !btn.interactable) return; 
        if (targetImage != null && spriteHover != null)
            targetImage.sprite = spriteHover;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (btn != null && !btn.interactable) return;
        if (targetImage != null && spriteNormal != null)
            targetImage.sprite = spriteNormal;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (btn != null && !btn.interactable) return;
        if (targetImage != null && spritePressed != null)
            targetImage.sprite = spritePressed;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (btn != null && !btn.interactable) return;
        
        if (targetImage != null)
        {
            if (eventData.pointerEnter == this.gameObject)
            {
                if (spriteHover != null) targetImage.sprite = spriteHover;
            }
            else
            {
                if (spriteNormal != null) targetImage.sprite = spriteNormal;
            }
        }
    }
    
    void OnEnable()
    {
        if (targetImage != null && spriteNormal != null)
            targetImage.sprite = spriteNormal;
    }
}