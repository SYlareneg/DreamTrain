using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;

public class MapCamera : MonoBehaviour
{
    // 배경 끝 좌표 (카메라의 이동 한계)
    [SerializeField] float fixedX;
    public float minY;
    public float maxY;
    bool cameraMoveSignal = false;

    Vector3 desiredPosition;

    public float GetCameraPosRatio()
    {
        if(minY == maxY) return 0f;
        return (transform.position.y - minY) / (maxY - minY);
    }

    public void SetCameraPosRatio(float ratio)
    {
        Vector3 desiredPosition = transform.position;
        desiredPosition.y = minY + (maxY - minY) * ratio;
        transform.position = desiredPosition;
    }

    public void MoveCamera(float newY)
    {
        desiredPosition = transform.position;
        desiredPosition.y = Mathf.Clamp(newY, minY, maxY);
    }

    void Update()
    {
        if(MapManager.Inst.player_moveable == false && cameraMoveSignal == false)
        {
            desiredPosition = transform.position;
            desiredPosition.y = Mathf.Clamp(transform.parent.position.y, minY, maxY);
            transform.DOLocalMove(desiredPosition - transform.parent.position, 0.5f).OnComplete(() => cameraMoveSignal = false);
            cameraMoveSignal = true;
        }
        else
        {
            if (Mouse.current == null) return;

            Vector2 scroll = Mouse.current.scroll.ReadValue();
            if (scroll.y != 0)
            {
                MoveCamera(transform.position.y + scroll.y * 1.2f);
            }
        }
    }


    Vector3 aimPos;
    Tween cameraTween;
    void LateUpdate()
    {
        desiredPosition.x = fixedX;
        desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);

        if(aimPos != desiredPosition)
        {
            if(cameraTween != null) cameraTween.Kill();
            cameraTween = transform.DOMove(desiredPosition, 0.2f);
            aimPos = desiredPosition;
        }
    }
}
