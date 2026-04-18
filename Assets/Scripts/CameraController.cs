using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 8, -6);
    public float smoothSpeed = 10f; 

    
    void FixedUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.fixedDeltaTime);

        transform.LookAt(target.position + Vector3.up * 1.5f);
    }
}