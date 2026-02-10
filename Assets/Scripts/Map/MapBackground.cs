using UnityEngine;

public class MapBackground : MonoBehaviour
{
    Vector2 originalMousePos;
    Vector2 originalCamPos;
    void OnMouseDown()
    {
        originalMousePos = Input.mousePosition;
        if(MapManager.Inst.mapCamera) originalCamPos = MapManager.Inst.mapCamera.transform.position;
    }
    void OnMouseDrag()
    {
        Vector2 newCamPos = originalCamPos;
        newCamPos.x += (originalMousePos.x - Input.mousePosition.x) * 0.01f;
        if(MapManager.Inst.mapCamera) MapManager.Inst.mapCamera.MoveCamera(newCamPos.x);
    }
}
