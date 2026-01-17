using UnityEngine;
using UnityEngine.UI;

public class RoomBackground : MonoBehaviour
{
    Image image;
    RectTransform rectTransform;
    [SerializeField] float scrollSpeed;

    void Start()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        SceneChangeManager.Inst.SceneFadeIn(() => {});
    }

    void FixedUpdate()
    {
        rectTransform.anchoredPosition += scrollSpeed * Vector2.right * Time.fixedDeltaTime;
        if(rectTransform.anchoredPosition.x >= image.sprite.rect.width)
        {
            rectTransform.anchoredPosition = new Vector2(-image.sprite.rect.width, rectTransform.anchoredPosition.y);
        }
    }
}
