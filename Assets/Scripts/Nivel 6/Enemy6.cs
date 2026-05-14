using System.Collections.Generic;
using UnityEngine;

public class EnemyCobarde : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;
    public Rigidbody playerRb;
    public Animator anim;
    public Transform safeZone;

    [Header("Steering (Evade / Seek)")]
    public float maxSpeed = 3.5f;
    public float rotationSpeed = 10f;
    public float panicDistance = 5f; // Distancia para asustarse
    public float stopPanicDistance = 8f; // Distancia para volver a calmarse
    public float fleeSpeedMultiplier = 1.8f;
    public float maxPredictionTime = 1.5f;

    [Header("Condición de Victoria")]
    public float catchDistance = 1.2f;

    [Header("Recompensa")]
    public GameObject llaveParaEscapar;

    [Header("Antenas (Obstacle Avoidance para huir)")]
    public float rayDistance = 3f;
    public LayerMask wallLayer;

    [Header("Pathfinding (Dijkstra)")]
    public LayerMask nodeLayer;
    private List<Node> currentPath = new List<Node>();
    private int currentPathIndex = 0;
    private float pathRecalculateTimer = 0f;

    private Node startNode;
    private Node targetNode;

    private bool isCaught = false;
    private bool isPanicking = false; 

    void Update()
    {
        if (player == null || safeZone == null || isCaught) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        //
        if (distanceToPlayer <= catchDistance)
        {
            GanarNivel();
            return;
        }

        //Panic
        if (distanceToPlayer <= panicDistance)
        {
            isPanicking = true; // Entra en pánico
        }
        else if (distanceToPlayer > stopPanicDistance)
        {
            isPanicking = false; // Se calma si estoy lejos
        }

        
        if (isPanicking)
        {
            HuirDelJugador();
        }
        else
        {
            pathRecalculateTimer -= Time.deltaTime;
            if (pathRecalculateTimer <= 0)
            {
                CalcularRutaDijkstra();
                pathRecalculateTimer = 1f;
            }

            MoverHaciaZonaSegura();
        }
    }

    void GanarNivel()
    {
        isCaught = true;
        anim.SetFloat("Vel", 0f);

        Debug.Log("¡ATRAPASTE AL GUARDIA! ¡Soltó la llave!");

        if (llaveParaEscapar != null)
        {
            llaveParaEscapar.SetActive(true);
            llaveParaEscapar.transform.position = transform.position;
        }

        gameObject.SetActive(false);
    }

    void HuirDelJugador()
    {
        Vector3 dir = SteeringBehaviours.Evade(transform, player, playerRb, maxPredictionTime);
        dir = EvitarObstaculos(dir);

        dir.y = 0;
        transform.position += dir * (maxSpeed * fleeSpeedMultiplier) * Time.deltaTime;

        if (dir != Vector3.zero)
        {
            transform.forward = Vector3.Lerp(transform.forward, dir.normalized, Time.deltaTime * rotationSpeed);
            anim.SetFloat("Vel", 2f);
        }
    }

    void CalcularRutaDijkstra()
    {
        startNode = EncontrarNodoMasCercano(transform.position);
        targetNode = EncontrarNodoMasCercano(safeZone.position);

        if (startNode != null && targetNode != null)
        {
            currentPath = Dijkstra.Run(startNode, IsSatisfied, GetConnections, GetCosts);
            currentPathIndex = 0;
        }
    }

    void MoverHaciaZonaSegura()
    {
        if (currentPath != null && currentPathIndex < currentPath.Count)
        {
            Vector3 targetPos = currentPath[currentPathIndex].transform.position;
            targetPos.y = transform.position.y;

            Vector3 dir = SteeringBehaviours.Seek(transform, targetPos);

            transform.position += dir * maxSpeed * Time.deltaTime;

            if (dir != Vector3.zero)
            {
                transform.forward = Vector3.Lerp(transform.forward, dir.normalized, Time.deltaTime * rotationSpeed);
                anim.SetFloat("Vel", 1f);
            }

            if (Vector3.Distance(transform.position, targetPos) < 1f)
            {
                currentPathIndex++;
            }
        }
        else
        {
            anim.SetFloat("Vel", 0f);
        }
    }

    Vector3 EvitarObstaculos(Vector3 dirOriginal)
    {
        Vector3 dirFinal = dirOriginal;
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up;

        if (Physics.Raycast(origin, transform.forward - transform.right * 0.5f, out hit, rayDistance, wallLayer))
        {
            dirFinal += transform.right * 4f;
        }
        else if (Physics.Raycast(origin, transform.forward + transform.right * 0.5f, out hit, rayDistance, wallLayer))
        {
            dirFinal -= transform.right * 4f;
        }
        else if (Physics.Raycast(origin, transform.forward, out hit, rayDistance, wallLayer))
        {
            dirFinal += hit.normal * 5f;
        }

        return dirFinal.normalized;
    }

    bool IsSatisfied(Node node) { return node == targetNode; }
    List<Node> GetConnections(Node node) { return node.neightbourds; }
    float GetCosts(Node a, Node b) { return b.hasTrap ? 1f : 100f; }

    Node EncontrarNodoMasCercano(Vector3 posicion)
    {
        Collider[] colliders = Physics.OverlapSphere(posicion, 5f, nodeLayer);
        Node closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider col in colliders)
        {
            float dist = Vector3.Distance(posicion, col.transform.position);
            if (dist < minDistance)
            {
                closest = col.GetComponent<Node>();
                minDistance = dist;
            }
        }
        return closest;
    }
}