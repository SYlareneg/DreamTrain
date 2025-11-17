using UnityEngine;

public class PassengerCamera : MonoBehaviour
{
    // 배경 끝 좌표 (카메라의 이동 한계)
    [SerializeField] float fixedY;
    [SerializeField] float minX;
    [SerializeField] float maxX;

    void LateUpdate()
    {
        Vector3 desiredPosition = transform.position;
        desiredPosition.x = Mathf.Clamp(transform.parent.position.x, minX, maxX);
        desiredPosition.y = fixedY;

        transform.position = desiredPosition;
    }
}
