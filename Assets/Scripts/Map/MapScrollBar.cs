using UnityEngine;
using UnityEngine.EventSystems;

public class MapScrollBar : MonoBehaviour, IDragHandler
{
    [SerializeField] float leftEnd;
    [SerializeField] float rightEnd;
    RectTransform rect;

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 desiredPosition = rect.localPosition;
        desiredPosition.x = Mathf.Clamp(Input.mousePosition.x - Screen.width / 2, leftEnd, rightEnd);
        rect.localPosition = desiredPosition;

        float ratio = (desiredPosition.x - leftEnd) / (rightEnd - leftEnd);
        MapManager.Inst.mapCamera.SetCameraPosRatio(ratio);
    }

    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        if(rect != null && MapManager.Inst.mapCamera != null && MapManager.Inst.player_moveable == false)
        {
            Vector3 desiredPosition = rect.localPosition;
            desiredPosition.x = leftEnd + (rightEnd - leftEnd) * MapManager.Inst.mapCamera.GetCameraPosRatio();
            rect.localPosition = desiredPosition;
        }
    }
}
