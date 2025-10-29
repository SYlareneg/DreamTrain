using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;  // 따라갈 캐릭터
    public float smoothSpeed = 0.125f;  // 부드럽게 이동하는 속도
    public Vector3 offset;    // 카메라 위치 보정 (필요시)

    // 배경 끝 좌표 (카메라의 이동 한계)
    public float minX;
    public float maxX;

    void LateUpdate()
    {
        if (target == null)  return;

        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, transform.position.y, transform.position.z);
        
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);

        transform.position = smoothedPosition;
    }
}
