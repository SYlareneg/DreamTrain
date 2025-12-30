using UnityEngine;
using DG.Tweening;

public class ToolPanel : MonoBehaviour
{
    bool isShown;
    [SerializeField] float hideAnchorX = 0.16f;
    [SerializeField] float easeTime = 0.5f;
    RectTransform rect;
    public void TogglePanel()
    {
        if (isShown)
        {
            hidePanel();
        }
        else
        {
            showPanel();
        }

        isShown = !isShown;
    }

    void hidePanel()
    {
        DOTween.To(() => rect.anchorMin, x => rect.anchorMin = x, new Vector2(rect.anchorMin.x + hideAnchorX, rect.anchorMin.y), easeTime).SetEase(Ease.InOutQuad);
        DOTween.To(() => rect.anchorMax, x => rect.anchorMax = x, new Vector2(rect.anchorMax.x + hideAnchorX, rect.anchorMax.y), easeTime).SetEase(Ease.InOutQuad);
    }

    void showPanel()
    {
        DOTween.To(() => rect.anchorMin, x => rect.anchorMin = x, new Vector2(rect.anchorMin.x - hideAnchorX, rect.anchorMin.y), easeTime).SetEase(Ease.InOutQuad);
        DOTween.To(() => rect.anchorMax, x => rect.anchorMax = x, new Vector2(rect.anchorMax.x - hideAnchorX, rect.anchorMax.y), easeTime).SetEase(Ease.InOutQuad);
    }
    
    void Start()
    {
        rect = GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.99f, rect.anchorMin.y);
        rect.anchorMax = new Vector2(1.16f, rect.anchorMax.y);
        isShown = false;
    }
}
