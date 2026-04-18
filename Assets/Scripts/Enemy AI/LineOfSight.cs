using UnityEngine;

public class LineOfSight : MonoBehaviour
{
    [Header("Configuración de Visión")]
    [SerializeField] private int distance = 8;
    [SerializeField] private int angle = 90;
    [SerializeField] private LayerMask obs;

    [Header("Ajuste de Altura")]
    
    [SerializeField] private float eyeHeight = 1.6f;

    public bool CheckRange(Transform self, Transform target)
    {
        
        Vector3 origin = self.position + Vector3.up * eyeHeight;
        return Vector3.Distance(origin, target.position) < distance;
    }

    public bool CheckAngle(Transform self, Transform target)
    {
        Vector3 origin = self.position + Vector3.up * eyeHeight;
        Vector3 dir = target.position - origin;

        return Vector3.Angle(self.forward, dir) < angle / 2;
    }

    public bool CheckObstacle(Transform self, Transform target)
    {
        
        Vector3 origin = self.position + Vector3.up * eyeHeight;

       
        Vector3 dest = target.position + Vector3.up * 1f;

        Vector3 dir = dest - origin;

        
        return !Physics.Raycast(origin, dir.normalized, dir.magnitude, obs);
    }

    private void OnDrawGizmosSelected()
    {
       
        Vector3 eyePos = transform.position + Vector3.up * eyeHeight;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(eyePos, distance);

        Gizmos.color = Color.red;

        
        Gizmos.DrawRay(eyePos, transform.forward * distance);
        Gizmos.DrawRay(eyePos, Quaternion.Euler(0, angle / 2, 0) * transform.forward * distance);
        Gizmos.DrawRay(eyePos, Quaternion.Euler(0, -angle / 2, 0) * transform.forward * distance);
    }
}