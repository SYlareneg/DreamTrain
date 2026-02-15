using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ToolPanel_MapScene : MonoBehaviour
{
    bool isShown;
    bool panelMoving;
    [SerializeField] float hideAnchorY = 0.16f;
    [SerializeField] float easeTime = 0.5f;
    RectTransform rect;
    public void TogglePanel()
    {
        if(panelMoving) return;
        panelMoving = true;
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
        panelMoving = true;
        rect.DOMoveY(rect.position.y + rect.rect.height, easeTime).SetEase(Ease.InOutQuad).OnComplete(() => panelMoving = false);
    }

    void showPanel()
    {
        panelMoving = true;
        rect.DOMoveY(rect.position.y - rect.rect.height, easeTime).SetEase(Ease.InOutQuad).OnComplete(() => panelMoving = false);
    }
    
    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        rect = GetComponent<RectTransform>();
        rect.position = new Vector2(rect.position.x, rect.position.y - rect.rect.height);
        isShown = true;
        panelMoving = false;
    }
}
