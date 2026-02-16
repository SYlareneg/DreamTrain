using UnityEngine;

public class PassengerCamera : MonoBehaviour
{
    // 배경 끝 좌표 (카메라의 이동 한계)
    [SerializeField] float minY;
    [SerializeField] float maxY;
    [SerializeField] float minX;
    [SerializeField] float maxX;
    public bool lockFollow = false;

    void Start()
    {
        lockFollow = false;
    }

    void LateUpdate()
    {
        if (lockFollow) return;
        Vector3 desiredPosition = transform.position;
        desiredPosition.x = Mathf.Clamp(transform.parent.position.x, minX, maxX);
        desiredPosition.y = Mathf.Clamp(transform.parent.position.y, minY, maxY);

        transform.position = desiredPosition;
    }
}
