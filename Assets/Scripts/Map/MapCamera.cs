using UnityEngine;
using DG.Tweening;

public class MapCamera : MonoBehaviour
{
    // 배경 끝 좌표 (카메라의 이동 한계)
    [SerializeField] float fixedX;
    public float minY;
    public float maxY;
    bool cameraMoveSignal = false;

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
        Debug.Log("MoveCamera called with newY: " + newY);
        Vector3 desiredPosition = transform.position;
        desiredPosition.y = Mathf.Clamp(newY, minY, maxY);
        transform.position = desiredPosition;
    }

    void Update()
    {
        if(MapManager.Inst.player_moveable == false && cameraMoveSignal == false)
        {
            Vector3 desiredPosition = transform.position;
            desiredPosition.y = Mathf.Clamp(transform.parent.position.y, minY, maxY);
            transform.DOLocalMove(desiredPosition - transform.parent.position, 0.5f).OnComplete(() => cameraMoveSignal = false);
            cameraMoveSignal = true;
        }
    }

    void LateUpdate()
    {
        Vector3 desiredPosition = transform.position;
        desiredPosition.x = fixedX;
        desiredPosition.y = Mathf.Clamp(transform.position.y, minY, maxY);

        transform.position = desiredPosition;
    }
}
