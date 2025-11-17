using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;  
    public float smoothSpeed = 0.125f; 
    public Vector3 offset; 

    public float minX = 0;
    public float maxX = 10;

    void LateUpdate()
    {
        if (target == null)  return;

        Vector3 desiredPosition = new Vector3(target.position.x + offset.x, transform.position.y, transform.position.z);
        
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        smoothedPosition.x = Mathf.Clamp(smoothedPosition.x, minX, maxX);

        transform.position = smoothedPosition;
    }
}
