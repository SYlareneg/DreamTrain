using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using System.Collections;

public class MapCamera : MonoBehaviour
{
    // 배경 끝 좌표 (카메라의 이동 한계)
    [SerializeField] float fixedX;
    public float minY;
    public float maxY;

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

    IEnumerator Start()
    {
        yield return null;
        transform.position = new Vector3(fixedX, transform.parent.position.y, transform.position.z);
        desiredPosition = transform.position;
        Debug.Log(transform.position);
    }

    void Update()
    {
        if(MapManager.Inst.player_moveable == false)
        {
            desiredPosition.x = fixedX;
            desiredPosition.y = Mathf.Clamp(transform.parent.position.y, minY, maxY);
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
