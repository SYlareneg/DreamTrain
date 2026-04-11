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
        desiredPosition.x = fixedX;
        desiredPosition.y = Mathf.Clamp(newY, minY, maxY);
        desiredPosition.z = -10;
    }

    IEnumerator Start()
    {
        yield return null;
        transform.position = new Vector3(fixedX, Mathf.Clamp(GameObject.Find("Player").transform.position.y, minY, maxY), -10);
        desiredPosition = transform.position;
        Debug.Log(transform.position);
    }

    void Update()
    {
        if(MapManager.Inst.player_moveable == false)
        {
            desiredPosition.x = fixedX;
            desiredPosition.y = Mathf.Clamp(GameObject.Find("Player").transform.position.y, minY, maxY);
            desiredPosition.z = -10;
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
        desiredPosition.z = -10;

        if(aimPos != desiredPosition)
        {
            if(cameraTween != null) cameraTween.Kill();
            cameraTween = transform.DOMove(desiredPosition, 0.5f);
            aimPos = desiredPosition;
        }
    }
}
