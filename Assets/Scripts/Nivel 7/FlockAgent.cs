using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class FlockAgent : MonoBehaviour
{
    private FlockManager manager;
    private Rigidbody rb;

    [Header("Animación y Combate")]
    public Animator anim;
    public float attackDistance = 1.5f;
    private bool isAttacking = false;

    [Header("Visión y Persecución")]
    public float visionDistance = 8f;
    public LayerMask obstacleLayer;
    [Range(0.1f, 1f)]
    public float chaseSpeedMultiplier = 0.7f; 

    [Header("Patrulla (Retorno)")]
    public float returnForceMultiplier = 5f; 

    public void Initialize(FlockManager flockManager)
    {
        manager = flockManager;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void Start()
    {
        if (rb.linearVelocity == Vector3.zero)
        {
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            rb.linearVelocity = randomDir * Random.Range(manager.MinSpeed, manager.MaxSpeed);
        }
    }

    private void FixedUpdate()
    {
        if (manager == null || manager.GlobalTarget == null || isAttacking) return;

        float distanceToPlayer = Vector3.Distance(transform.position, manager.GlobalTarget.position);

        if (distanceToPlayer <= attackDistance)
        {
            Atacar();
            return;
        }

        Vector3 separation = CalculateSeparation();
        Vector3 alignment = CalculateAlignment();
        Vector3 cohesion = CalculateCohesion();

        Vector3 targetForce = CalculateTargetForce(distanceToPlayer);
        Vector3 boundsForce = CalculateBoundsForce();

        bool isChasing = targetForce != Vector3.zero;

        //Sistema de Caja (Patrullar) Descactivar Cada (Persecución)
        if (isChasing)
        {
            
            boundsForce = Vector3.zero;
        }
        else if (boundsForce != Vector3.zero)
        {
            // Si no te persiguen y están fuera de la caja azul,multiplicamos la fuerza para que vuelvan a la caja

            boundsForce *= returnForceMultiplier;
        }

        Vector3 steering =
            separation * manager.SeparationWeight +
            alignment * manager.AlignmentWeight +
            cohesion * manager.CohesionWeight +
            targetForce * manager.TargetWeight +
            boundsForce * manager.BoundsWeight;

        steering.y = 0f;

        Vector3 acceleration = Vector3.ClampMagnitude(steering, manager.MaxForce);
        Vector3 newVelocity = rb.linearVelocity + acceleration * Time.fixedDeltaTime;

        newVelocity.y = 0f;
        float speed = newVelocity.magnitude;

        
        float currentMaxSpeed = isChasing ? (manager.MaxSpeed * chaseSpeedMultiplier) : manager.MaxSpeed;

        if (speed < manager.MinSpeed)
        {
            newVelocity = newVelocity.normalized * manager.MinSpeed;
        }
        else if (speed > currentMaxSpeed)
        {
            newVelocity = newVelocity.normalized * currentMaxSpeed;
        }

        rb.linearVelocity = newVelocity;

        if (rb.linearVelocity.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity.normalized);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, 8f * Time.fixedDeltaTime));

            if (anim != null) anim.SetFloat("Vel", speed / manager.MaxSpeed);
        }
        else
        {
            if (anim != null) anim.SetFloat("Vel", 0f);
        }
    }

    private void Atacar()
    {
        isAttacking = true;
        rb.linearVelocity = Vector3.zero;

        if (anim != null)
        {
            anim.SetFloat("Vel", 0f);
            anim.SetTrigger("Punch");
        }

        StartCoroutine(EsperarYReiniciar());
    }

    private IEnumerator EsperarYReiniciar()
    {
        yield return new WaitForSeconds(1.5f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private Vector3 CalculateSeparation()
    {
        Vector3 force = Vector3.zero;
        int count = 0;
        for (int i = 0; i < manager.Agents.Count; i++)
        {
            FlockAgent other = manager.Agents[i];
            if (other == this) continue;
            Vector3 offset = transform.position - other.transform.position;
            float distance = offset.magnitude;
            if (distance > 0f && distance < manager.SeparationRadius)
            {
                force += offset.normalized / distance;
                count++;
            }
        }
        return count == 0 ? Vector3.zero : (force / count).normalized;
    }

    private Vector3 CalculateAlignment()
    {
        Vector3 averageVelocity = Vector3.zero;
        int count = 0;
        for (int i = 0; i < manager.Agents.Count; i++)
        {
            FlockAgent other = manager.Agents[i];
            if (other == this) continue;
            if (Vector3.Distance(transform.position, other.transform.position) < manager.NeighborRadius)
            {
                averageVelocity += other.rb.linearVelocity;
                count++;
            }
        }
        return count == 0 || averageVelocity == Vector3.zero ? Vector3.zero : averageVelocity.normalized;
    }

    private Vector3 CalculateCohesion()
    {
        Vector3 center = Vector3.zero;
        int count = 0;
        for (int i = 0; i < manager.Agents.Count; i++)
        {
            FlockAgent other = manager.Agents[i];
            if (other == this) continue;
            if (Vector3.Distance(transform.position, other.transform.position) < manager.NeighborRadius)
            {
                center += other.transform.position;
                count++;
            }
        }
        if (count == 0) return Vector3.zero;
        center /= count;
        Vector3 dirToCenter = center - transform.position;
        return dirToCenter == Vector3.zero ? Vector3.zero : dirToCenter.normalized;
    }

    private Vector3 CalculateTargetForce(float distanceToPlayer)
    {
        if (manager.GlobalTarget == null) return Vector3.zero;
        if (distanceToPlayer > visionDistance) return Vector3.zero;

        Vector3 origin = transform.position + Vector3.up;
        Vector3 targetPos = manager.GlobalTarget.position + Vector3.up;
        Vector3 dirToPlayer = targetPos - origin;

        RaycastHit hit;
        if (Physics.Raycast(origin, dirToPlayer, out hit, distanceToPlayer, obstacleLayer))
        {
            return Vector3.zero;
        }

        Vector3 dir = manager.GlobalTarget.position - transform.position;
        dir.y = 0f;
        return dir.normalized * 5f;
    }

    private Vector3 CalculateBoundsForce()
    {
        Vector3 center = manager.BoundsCenter;
        Vector3 extents = manager.BoundsExtents;
        Vector3 localOffset = transform.position - center;

        bool outsideX = Mathf.Abs(localOffset.x) > extents.x;
        bool outsideZ = Mathf.Abs(localOffset.z) > extents.z;

        if (!outsideX && !outsideZ) return Vector3.zero;

        Vector3 dirToCenter = center - transform.position;
        dirToCenter.y = 0f;
        return dirToCenter == Vector3.zero ? Vector3.zero : dirToCenter.normalized;
    }
}