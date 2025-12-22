using UnityEngine;

public class MapCamera : MonoBehaviour
{
    // 배경 끝 좌표 (카메라의 이동 한계)
    [SerializeField] float fixedY;
    public float minX;
    public float maxX;

    public float GetCameraPosRatio()
    {
        return (transform.position.x - minX) / (maxX - minX);
    }

    public void SetCameraPosRatio(float ratio)
    {
        Vector3 desiredPosition = transform.position;
        desiredPosition.x = minX + (maxX - minX) * ratio;
        transform.position = desiredPosition;
    }

    void Update()
    {
        if(MapManager.Inst.player_moveable == false)
        {
            Vector3 desiredPosition = transform.position;
            desiredPosition.x = transform.parent.position.x;
            desiredPosition.y = transform.parent.position.y;
            transform.position = desiredPosition;
        }
    }

    void LateUpdate()
    {
        Vector3 desiredPosition = transform.position;
        desiredPosition.x = Mathf.Clamp(transform.position.x, minX, maxX);
        desiredPosition.y = fixedY;

        transform.position = desiredPosition;
    }
}
